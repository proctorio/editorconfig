using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Proctorio.EditorConfig.Tests;

[TestClass]
public class TemplateFileTests
{
    private const string TemplatesPath = "Templates";

    [TestMethod]
    public void EditorConfigBaseFile_Exists()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "editorconfig.base");

        // Act & Assert
        Assert.IsTrue(File.Exists(filePath), $"Template file not found: {filePath}");
    }

    [TestMethod]
    public void NoConfigureAwaitRulesFile_Exists()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "no-configureawait.rules");

        // Act & Assert
        Assert.IsTrue(File.Exists(filePath), $"Template file not found: {filePath}");
    }

    [TestMethod]
    public void RequireConfigureAwaitRulesFile_Exists()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "require-configureawait.rules");

        // Act & Assert
        Assert.IsTrue(File.Exists(filePath), $"Template file not found: {filePath}");
    }

    [TestMethod]
    public void EditorConfigBaseFile_IsNotEmpty()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "editorconfig.base");

        // Act
        string content = File.ReadAllText(filePath);

        // Assert
        Assert.IsFalse(string.IsNullOrWhiteSpace(content), "Base template file should not be empty");
        Assert.IsTrue(content.Length > 100, "Base template file should contain substantial content");
    }

    [TestMethod]
    public void NoConfigureAwaitRules_IsNotEmpty()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "no-configureawait.rules");

        // Act
        string content = File.ReadAllText(filePath);

        // Assert
        Assert.IsFalse(string.IsNullOrWhiteSpace(content), "No-ConfigureAwait rules file should not be empty");
    }

    [TestMethod]
    public void RequireConfigureAwaitRules_IsNotEmpty()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "require-configureawait.rules");

        // Act
        string content = File.ReadAllText(filePath);

        // Assert
        Assert.IsFalse(string.IsNullOrWhiteSpace(content), "Require-ConfigureAwait rules file should not be empty");
    }
}
