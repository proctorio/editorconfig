using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

namespace Proctorio.EditorConfig.Tests;

[TestClass]
public class EditorConfigValidationTests
{
    private const string TemplatesPath = "Templates";

    [TestMethod]
    public void EditorConfigBase_HasNoMalformedRules()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "editorconfig.base");
        string content = File.ReadAllText(filePath);
        var lines = content.Split('\n');

        // Act - Look for common malformations
        var invalidLines = new List<string>();
        var rulePattern = new Regex(@"^[a-z_]+\.[A-Z0-9]+\s*=\s*.+$|^[a-z_]+\s*=\s*.+$", RegexOptions.IgnoreCase);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            
            // Skip empty lines, comments, and section headers
            if (string.IsNullOrWhiteSpace(trimmed) || 
                trimmed.StartsWith("#") || 
                trimmed.StartsWith("["))
            {
                continue;
            }

            // Check if it's a valid rule format (key = value)
            if (!trimmed.Contains("="))
            {
                invalidLines.Add(trimmed);
            }
        }

        // Assert
        Assert.AreEqual(0, invalidLines.Count, 
            $"Found {invalidLines.Count} potentially malformed rules: {string.Join(", ", invalidLines)}");
    }

    [TestMethod]
    public void EditorConfigBase_AllRulesHaveValidSeverity()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "editorconfig.base");
        string content = File.ReadAllText(filePath);
        var lines = content.Split('\n');

        var validSeverities = new[] { "none", "silent", "suggestion", "warning", "error" };
        var invalidRules = new List<string>();

        // Act - Check all severity assignments
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            
            if (trimmed.Contains(".severity =") || trimmed.Contains("severity="))
            {
                var parts = trimmed.Split('=');
                if (parts.Length >= 2)
                {
                    var severity = parts[1].Trim().ToLower();
                    
                    if (!validSeverities.Contains(severity))
                    {
                        invalidRules.Add(trimmed);
                    }
                }
            }
        }

        // Assert
        Assert.AreEqual(0, invalidRules.Count, 
            $"Found rules with invalid severity: {string.Join(", ", invalidRules)}");
    }

    [TestMethod]
    public void EditorConfigBase_AllStyleRulesHaveValidSeverity()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "editorconfig.base");
        string content = File.ReadAllText(filePath);
        var lines = content.Split('\n');

        var validSeverities = new[] { "none", "silent", "suggestion", "warning", "error", "true", "false" };
        var invalidRules = new List<string>();

        // Act - Check all style rules with :severity suffix
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            
            // Match patterns like "csharp_style_something = value:severity"
            if (trimmed.Contains("csharp_") || trimmed.Contains("dotnet_"))
            {
                var parts = trimmed.Split('=');
                if (parts.Length >= 2 && parts[1].Contains(":"))
                {
                    var valueParts = parts[1].Split(':');
                    if (valueParts.Length >= 2)
                    {
                        var severity = valueParts[1].Trim().ToLower();
                        
                        if (!validSeverities.Contains(severity))
                        {
                            invalidRules.Add(trimmed);
                        }
                    }
                }
            }
        }

        // Assert
        Assert.AreEqual(0, invalidRules.Count, 
            $"Found style rules with invalid severity: {string.Join(", ", invalidRules)}");
    }

    [TestMethod]
    public void ConfigureAwaitRules_HaveValidSeverities()
    {
        // Arrange
        var validSeverities = new[] { "none", "silent", "suggestion", "warning", "error" };
        var filesToCheck = new[] 
        { 
            "no-configureawait.rules", 
            "require-configureawait.rules" 
        };

        // Act & Assert
        foreach (var file in filesToCheck)
        {
            string filePath = Path.Combine(TemplatesPath, file);
            string content = File.ReadAllText(filePath);
            var lines = content.Split('\n');

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                
                if (trimmed.Contains(".severity ="))
                {
                    var parts = trimmed.Split('=');
                    if (parts.Length >= 2)
                    {
                        var severity = parts[1].Trim().ToLower();
                        
                        Assert.IsTrue(validSeverities.Contains(severity), 
                            $"Invalid severity '{severity}' in {file}: {trimmed}");
                    }
                }
            }
        }
    }

    [TestMethod]
    public void EditorConfigBase_NoDuplicateRuleDefinitions()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "editorconfig.base");
        string content = File.ReadAllText(filePath);
        var lines = content.Split('\n');

        var currentSection = "";
        var sectionRules = new Dictionary<string, Dictionary<string, int>>();
        var duplicates = new List<string>();

        // Act - Track rule definitions per section
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            
            // Track section changes
            if (trimmed.StartsWith("["))
            {
                currentSection = trimmed;
                if (!sectionRules.ContainsKey(currentSection))
                {
                    sectionRules[currentSection] = new Dictionary<string, int>();
                }
                continue;
            }

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
            {
                continue;
            }

            if (trimmed.Contains("="))
            {
                var key = trimmed.Split('=')[0].Trim();
                
                if (!sectionRules.ContainsKey(currentSection))
                {
                    sectionRules[currentSection] = new Dictionary<string, int>();
                }
                
                var rules = sectionRules[currentSection];
                if (rules.ContainsKey(key))
                {
                    rules[key]++;
                    if (rules[key] == 2) // Only add once
                    {
                        duplicates.Add($"{key} in section {currentSection}");
                    }
                }
                else
                {
                    rules[key] = 1;
                }
            }
        }

        // Assert
        Assert.AreEqual(0, duplicates.Count, 
            $"Found duplicate rule definitions within the same section: {string.Join(", ", duplicates)}");
    }

    [TestMethod]
    public void EditorConfigBase_HasRequiredSections()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "editorconfig.base");
        string content = File.ReadAllText(filePath);

        // Act & Assert
        Assert.IsTrue(content.Contains("[*.cs]"), "Should contain [*.cs] section");
        Assert.IsTrue(content.Contains("[*.{cs,vb}]") || content.Contains("Naming"), 
            "Should contain naming conventions section");
    }

    [TestMethod]
    public void EditorConfigBase_EndsWithNewline()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "editorconfig.base");
        string content = File.ReadAllText(filePath);

        // Act & Assert
        Assert.IsTrue(content.EndsWith("\n") || content.EndsWith("\r\n"), 
            "EditorConfig file should end with a newline");
    }

    [TestMethod]
    public void NoConfigureAwaitRules_ContainsRequiredRules()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "no-configureawait.rules");
        string content = File.ReadAllText(filePath);

        // Act & Assert
        Assert.IsTrue(content.Contains("CA2007"), "Should contain CA2007 rule");
        Assert.IsTrue(content.Contains("RCS1090"), "Should contain RCS1090 rule");
    }

    [TestMethod]
    public void RequireConfigureAwaitRules_ContainsRequiredRules()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "require-configureawait.rules");
        string content = File.ReadAllText(filePath);

        // Act & Assert
        Assert.IsTrue(content.Contains("CA2007"), "Should contain CA2007 rule");
        Assert.IsTrue(content.Contains("RCS1090"), "Should contain RCS1090 rule");
    }
}
