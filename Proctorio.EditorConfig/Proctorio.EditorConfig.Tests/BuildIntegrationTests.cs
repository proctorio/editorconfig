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
}
