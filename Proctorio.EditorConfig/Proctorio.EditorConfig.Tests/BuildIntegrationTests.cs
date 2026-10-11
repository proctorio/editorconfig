using System.Diagnostics;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Proctorio.EditorConfig.Tests;

[TestClass]
public class BuildIntegrationTests
{
    private const string BuildPath = "Build";

    [TestMethod]
    public void PropsFile_Exists()
    {
        // Arrange
        string filePath = Path.Combine(BuildPath, "Proctorio.EditorConfig.NuGet.Package.Internal.props");

        // Act & Assert
        Assert.IsTrue(File.Exists(filePath), $"Props file not found: {filePath}");
    }

    [TestMethod]
    public void PropsFile_IsValidXml()
    {
        // Arrange
        string filePath = Path.Combine(BuildPath, "Proctorio.EditorConfig.NuGet.Package.Internal.props");

        // Act
        var content = File.ReadAllText(filePath);

        // Assert
        Assert.IsTrue(content.Contains("<Project"), "Props file should be valid MSBuild XML");
        Assert.IsFalse(string.IsNullOrWhiteSpace(content), "Props file should not be empty");
    }

    [TestMethod]
    public void PropsFile_ReferencesEditorConfigFile()
    {
        // Arrange
        string filePath = Path.Combine(BuildPath, "Proctorio.EditorConfig.NuGet.Package.Internal.props");

        // Act
        var content = File.ReadAllText(filePath);

        // Assert
        Assert.IsTrue(content.Contains(".editorconfig") || content.Contains("editorconfig"), 
            "Props file should reference .editorconfig");
    }

    private const string PropsFile = "Proctorio.EditorConfig.NuGet.Package.Internal.props";
    private const string TargetsFile = "Proctorio.EditorConfig.NuGet.Package.Internal.targets";

    [TestMethod]
    public void TargetsFile_Exists()
    {
        // Arrange
        string filePath = Path.Combine(BuildPath, TargetsFile);

        // Act & Assert
        Assert.IsTrue(File.Exists(filePath), $"Targets file not found: {filePath}");
    }

    [TestMethod]
    public void PackageProject_PacksPropsAndTargetsIntoBuildAndBuildTransitive()
    {
        // Arrange
        string csproj = File.ReadAllText(FindRepoFile("Proctorio.EditorConfig.NuGet.Package", "Proctorio.EditorConfig.NuGet.Package.csproj"));

        // Act & Assert
        foreach (string extension in new[] { "props", "targets" })
        {
            foreach (string folder in new[] { "build", "buildTransitive" })
            {
                Assert.IsTrue(
                    csproj.Contains($"Include=\"build\\Proctorio.EditorConfig.{extension}\" Pack=\"true\" PackagePath=\"{folder}\\\""),
                    $"Proctorio.EditorConfig.{extension} should be packed into {folder}/");
            }
        }
    }

    [TestMethod]
    public void Props_ImportedTwice_AddsEachPackageReferenceOnce()
    {
        // Arrange - a direct PackageReference gets the props from build/ and from buildTransitive/
        string dir = NewTempDir();
        try
        {
            string first = CopyBuildFile(dir, "build", PropsFile);
            string second = CopyBuildFile(dir, "buildTransitive", PropsFile);
            string project = Path.Combine(dir, "Consumer.csproj");
            File.WriteAllText(project, ConsumerProject("Library", props: new[] { first, second }, targets: Array.Empty<string>()));

            // Act
            string json = RunDotnet(dir, "msbuild", project, "-nologo", "-getItem:PackageReference");

            // Assert
            using JsonDocument doc = JsonDocument.Parse(json);
            List<string> ids = doc.RootElement.GetProperty("Items").GetProperty("PackageReference").EnumerateArray()
                .Select(e => e.GetProperty("Identity").GetString()!).ToList();
            Assert.IsTrue(ids.Contains("Microsoft.CodeAnalysis.NetAnalyzers"), "The props should add the analyzer packages");
            CollectionAssert.AreEquivalent(ids.Distinct().ToList(), ids, $"Duplicate PackageReference items (NU1504): {string.Join(", ", ids)}");
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [TestMethod]
    public void Props_ImportedTwice_AppendsNoWarnOnce()
    {
        // Arrange
        string dir = NewTempDir();
        try
        {
            string first = CopyBuildFile(dir, "build", PropsFile);
            string second = CopyBuildFile(dir, "buildTransitive", PropsFile);
            string project = Path.Combine(dir, "Consumer.csproj");
            File.WriteAllText(project, ConsumerProject("Library", props: new[] { first, second }, targets: Array.Empty<string>()));

            // Act
            string noWarn = RunDotnet(dir, "msbuild", project, "-nologo", "-getProperty:NoWarn").Trim();

            // Assert
            Assert.AreEqual(1, noWarn.Split(';').Count(w => w == "CA1014"), $"NoWarn was '{noWarn}'");
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [DataTestMethod]
    [DataRow("Library", "", "Consumer", true)]
    [DataRow("Library", "<IsPackable>true</IsPackable>", "Consumer", true)]
    [DataRow("Exe", "", "Consumer", false)]
    [DataRow("WinExe", "", "Consumer", false)]
    [DataRow("Library", "<IsPackable>false</IsPackable>", "Consumer", false)]
    [DataRow("Library", "<IsTestProject>true</IsTestProject>", "Consumer", false)]
    [DataRow("Library", "", "Consumer.Tests", false)]
    [DataRow("Library", "", "Consumer.IntegrationTests", false)]
    [DataRow("Library", "<ItemGroup><PackageReference Include=\"Microsoft.NET.Test.Sdk\" Version=\"17.11.1\" /></ItemGroup>", "Consumer", false)]
    public void Targets_ReportMissingDocsOnlyForPackableLibraries(string outputType, string extra, string name, bool expectsCs1591)
    {
        // Arrange
        string dir = NewTempDir();
        try
        {
            string props = CopyBuildFile(dir, "build", PropsFile);
            string targets = CopyBuildFile(dir, "build", TargetsFile);
            string project = Path.Combine(dir, name + ".csproj");
            File.WriteAllText(project, ConsumerProject(outputType, new[] { props }, new[] { targets }, extra));

            // Act - run only the suppression target, then read the resulting NoWarn
            string noWarn = RunDotnet(dir, "msbuild", project, "-nologo", "-t:ProctorioEditorConfigSuppressMissingDocs", "-getProperty:NoWarn").Trim();

            // Assert
            bool suppressed = noWarn.Split(';').Contains("CS1591");
            Assert.AreEqual(!expectsCs1591, suppressed, $"{outputType} {name} {extra}: NoWarn was '{noWarn}'");
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [DataTestMethod]
    [DataRow("Library", "", "Lib", true)]
    [DataRow("Exe", "", "App", false)]
    [DataRow("Library", "", "Lib.Tests", false)]
    public void Build_Cs1591ReportedOnlyForPackableLibraries(string outputType, string extra, string name, bool expectsCs1591)
    {
        // Arrange - a real compile of an undocumented public type, with the documentation file switched on as the props do
        string dir = NewTempDir();
        try
        {
            string targets = CopyBuildFile(dir, "build", TargetsFile);
            File.WriteAllText(Path.Combine(dir, "Types.cs"), "namespace Sample { public class Undocumented { public int Value { get; set; } } }\n"
                + (outputType == "Exe" ? "namespace Sample { public static class Program { public static void Main() { } } }\n" : string.Empty));
            string project = Path.Combine(dir, name + ".csproj");
            File.WriteAllText(project, ConsumerProject(outputType, Array.Empty<string>(), new[] { targets }, extra, generateDocs: true));

            // Act
            string output = RunDotnet(dir, "build", project, "-nologo", "--no-incremental", "-p:UseAppHost=false");

            // Assert
            Assert.AreEqual(expectsCs1591, output.Contains("warning CS1591"), output);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [TestMethod]
    public void Build_NonCorrectnessAnalyzersAreSuggestionsAndCorrectnessAnalyzersStayWarnings()
    {
        // Arrange - the composed .editorconfig next to the props, imported before the project body as a package would be
        string dir = NewTempDir();
        try
        {
            string props = CopyBuildFile(dir, "build", PropsFile);
            string editorConfig = File.ReadAllText(Path.Combine("Templates", "editorconfig.base")) + Environment.NewLine
                + File.ReadAllText(Path.Combine("Templates", "no-configureawait.rules"));
            File.WriteAllText(Path.Combine(dir, ".editorconfig"), editorConfig);
            // The props copy the package's content/Rules/.editorconfig into the project folder before building
            Directory.CreateDirectory(Path.Combine(dir, "content", "Rules"));
            File.WriteAllText(Path.Combine(dir, "content", "Rules", ".editorconfig"), editorConfig);
            File.WriteAllText(Path.Combine(dir, "Types.cs"), @"
using System;
using System.Collections.Generic;
namespace Sample
{
    public class Under_Score
    {
        public List<int> Items { get; set; } = new List<int>();
        public string Format(int x) { return string.Format(""{0}"", x); }
        public void Throw() { throw new ArgumentException(""message"", ""nope""); }
    }
}
");
            string project = Path.Combine(dir, "Sample.csproj");
            // The props also add analyzer PackageReferences, which this offline test cannot restore; the analyzers it needs ship with the SDK.
            File.WriteAllText(project, ConsumerProject("Library", new[] { props }, Array.Empty<string>(), "<ItemGroup><PackageReference Remove=\"@(PackageReference)\" /></ItemGroup>"));

            // Act
            string output = RunDotnet(dir, "build", project, "-nologo", "--no-incremental");

            // Assert
            Assert.IsFalse(output.Contains("warning CA1707"), "CA1707 (Naming) should be a suggestion\n" + output);
            Assert.IsFalse(output.Contains("warning CA1305"), "CA1305 (Globalization) should be a suggestion\n" + output);
            Assert.IsFalse(output.Contains("warning CA1002"), "CA1002 (Design) should be a suggestion\n" + output);
            Assert.IsTrue(output.Contains("warning CA2227"), "CA2227 (Usage) should stay a warning\n" + output);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [TestMethod]
    public void Props_SetCategoryAnalysisModes()
    {
        // Arrange
        string content = File.ReadAllText(Path.Combine(BuildPath, PropsFile));

        // Act & Assert
        Assert.IsFalse(content.Contains("<AnalysisMode>AllEnabledByDefault</AnalysisMode>"), "AllEnabledByDefault hard-sets every CA rule and defeats the category severities");
        Assert.IsTrue(content.Contains("<AnalysisMode>Default</AnalysisMode>"));
        foreach (string category in new[] { "Reliability", "Security", "Usage" })
        {
            Assert.IsTrue(content.Contains($"<AnalysisMode{category}>All</AnalysisMode{category}>"), category);
        }
    }

    private static string ConsumerProject(string outputType, string[] props, string[] targets, string extra = "", bool generateDocs = false)
    {
        string importProps = string.Join(Environment.NewLine, props.Select(p => $"  <Import Project=\"{p}\" />"));
        string importTargets = string.Join(Environment.NewLine, targets.Select(t => $"  <Import Project=\"{t}\" />"));
        string docs = generateDocs ? "<GenerateDocumentationFile>true</GenerateDocumentationFile>" : string.Empty;

        // Same order MSBuild uses for a package: .props before the project body, .targets after it.
        return $@"<Project>
  <Import Sdk=""Microsoft.NET.Sdk"" Project=""Sdk.props"" />
{importProps}
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <OutputType>{outputType}</OutputType>
    {docs}
    {(extra.StartsWith("<ItemGroup") ? string.Empty : extra)}
  </PropertyGroup>
  {(extra.StartsWith("<ItemGroup") ? extra : string.Empty)}
  <Import Sdk=""Microsoft.NET.Sdk"" Project=""Sdk.targets"" />
{importTargets}
</Project>";
    }

    private static string NewTempDir()
    {
        string dir = Path.Combine(Path.GetTempPath(), "pec-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static string CopyBuildFile(string dir, string folder, string fileName)
    {
        string target = Path.Combine(dir, folder);
        Directory.CreateDirectory(target);
        string destination = Path.Combine(target, fileName);
        File.Copy(Path.Combine(BuildPath, fileName), destination, overwrite: true);
        return destination;
    }

    private static string FindRepoFile(string projectDir, string fileName)
    {
        string? dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            string candidate = Path.Combine(dir, projectDir, fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = Path.GetDirectoryName(dir);
        }

        throw new FileNotFoundException($"{projectDir}/{fileName} not found above {AppContext.BaseDirectory}");
    }

    private static string RunDotnet(string workingDirectory, params string[] args)
    {
        ProcessStartInfo info = new("dotnet")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        foreach (string arg in args)
        {
            info.ArgumentList.Add(arg);
        }

        info.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        info.Environment["DOTNET_NOLOGO"] = "1";
        using Process process = Process.Start(info)!;
        Task<string> stdout = process.StandardOutput.ReadToEndAsync();
        Task<string> stderr = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(240_000))
        {
            process.Kill(entireProcessTree: true);
            Assert.Fail("dotnet " + string.Join(' ', args) + " timed out");
        }

        return stdout.Result + stderr.Result;
    }
}
