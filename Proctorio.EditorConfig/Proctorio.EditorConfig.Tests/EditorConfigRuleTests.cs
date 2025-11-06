using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Proctorio.EditorConfig.Tests;

[TestClass]
public class EditorConfigRuleTests
{
    private const string TemplatesPath = "Templates";

    [TestMethod]
    public void EditorConfigBase_ContainsVarRules()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "editorconfig.base");
        string content = File.ReadAllText(filePath);

        // Act & Assert - Check that var rules are present
        Assert.IsTrue(content.Contains("csharp_style_var_elsewhere"), "Should contain var_elsewhere rule");
        Assert.IsTrue(content.Contains("csharp_style_var_for_built_in_types"), "Should contain var_for_built_in_types rule");
        Assert.IsTrue(content.Contains("csharp_style_var_when_type_is_apparent"), "Should contain var_when_type_is_apparent rule");
    }

    [TestMethod]
    public void EditorConfigBase_DisablesImplicitVar()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "editorconfig.base");
        string content = File.ReadAllText(filePath);

        // Act & Assert - Check that IDE0007 (use var) is disabled
        Assert.IsTrue(content.Contains("dotnet_diagnostic.IDE0007.severity = none"), 
            "IDE0007 should be disabled to prevent 'use var' suggestions");
    }

    [TestMethod]
    public void EditorConfigBase_EnablesExplicitType()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "editorconfig.base");
        string content = File.ReadAllText(filePath);

        // Act & Assert - Check that IDE0008 (use explicit type) is enabled
        Assert.IsTrue(content.Contains("dotnet_diagnostic.IDE0008.severity = warning"), 
            "IDE0008 should be enabled to enforce explicit types");
    }

    [TestMethod]
    public void EditorConfigBase_ContainsNamingConventions()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "editorconfig.base");
        string content = File.ReadAllText(filePath);

        // Act & Assert
        Assert.IsTrue(content.Contains("dotnet_naming_rule.interface_should_be_begins_with_i"), 
            "Should contain interface naming rule");
        Assert.IsTrue(content.Contains("dotnet_naming_rule.private_fields_should_be_camelcase_with_underscore"), 
            "Should contain private field naming rule");
        Assert.IsTrue(content.Contains("dotnet_naming_style.underscore_camelcase.required_prefix = _"), 
            "Private fields should require underscore prefix");
    }

    [TestMethod]
    public void EditorConfigBase_ContainsSpacingRules()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "editorconfig.base");
        string content = File.ReadAllText(filePath);

        // Act & Assert
        Assert.IsTrue(content.Contains("csharp_space_around_binary_operators"), 
            "Should contain spacing rules for binary operators");
        Assert.IsTrue(content.Contains("csharp_space_after_cast"), 
            "Should contain spacing rules for casts");
        Assert.IsTrue(content.Contains("csharp_space_after_keywords_in_control_flow_statements"), 
            "Should contain spacing rules for control flow");
    }

    [TestMethod]
    public void EditorConfigBase_ContainsNewLinePreferences()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "editorconfig.base");
        string content = File.ReadAllText(filePath);

        // Act & Assert
        Assert.IsTrue(content.Contains("csharp_new_line_before_open_brace"), 
            "Should contain new line rules for braces");
        Assert.IsTrue(content.Contains("csharp_new_line_before_else"), 
            "Should contain new line rules for else");
        Assert.IsTrue(content.Contains("csharp_new_line_before_catch"), 
            "Should contain new line rules for catch");
    }

    [TestMethod]
    public void EditorConfigBase_ContainsPatternMatchingRules()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "editorconfig.base");
        string content = File.ReadAllText(filePath);

        // Act & Assert
        Assert.IsTrue(content.Contains("csharp_style_pattern_matching_over_as_with_null_check"), 
            "Should contain pattern matching rule for 'as' with null check");
        Assert.IsTrue(content.Contains("csharp_style_pattern_matching_over_is_with_cast_check"), 
            "Should contain pattern matching rule for 'is' with cast");
    }

    [TestMethod]
    public void EditorConfigBase_ContainsReadonlyRule()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "editorconfig.base");
        string content = File.ReadAllText(filePath);

        // Act & Assert
        Assert.IsTrue(content.Contains("IDE0044"), "Should contain IDE0044 (readonly modifier) rule");
        Assert.IsTrue(content.Contains("dotnet_style_readonly_field"), "Should contain readonly field preference");
    }

    [TestMethod]
    public void EditorConfigBase_DisablesConditionalExpressions()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "editorconfig.base");
        string content = File.ReadAllText(filePath);

        // Act & Assert - Should prefer explicit if/else over ternary for assignment/return
        Assert.IsTrue(content.Contains("dotnet_diagnostic.IDE0045.severity = none"), 
            "IDE0045 should be disabled (prefer explicit over ternary for assignment)");
        Assert.IsTrue(content.Contains("dotnet_diagnostic.IDE0046.severity = none"), 
            "IDE0046 should be disabled (prefer explicit over ternary for return)");
    }

    [TestMethod]
    public void EditorConfigBase_ContainsCodeAnalysisRules()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "editorconfig.base");
        string content = File.ReadAllText(filePath);

        // Act & Assert
        Assert.IsTrue(content.Contains("CA1031"), "Should contain CA1031 rule");
        Assert.IsTrue(content.Contains("CA1062"), "Should contain CA1062 rule");
        Assert.IsTrue(content.Contains("CA1716"), "Should contain CA1716 rule");
        Assert.IsTrue(content.Contains("CA1822"), "Should contain CA1822 rule");
        Assert.IsTrue(content.Contains("CA2016"), "Should contain CA2016 rule (CancellationToken)");
    }

    [TestMethod]
    public void NoConfigureAwaitRules_DisablesCA2007()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "no-configureawait.rules");
        string content = File.ReadAllText(filePath);

        // Act & Assert
        Assert.IsTrue(content.Contains("dotnet_diagnostic.CA2007.severity = none"), 
            "CA2007 should be disabled when ConfigureAwait is not required");
    }

    [TestMethod]
    public void NoConfigureAwaitRules_DisablesRCS1090()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "no-configureawait.rules");
        string content = File.ReadAllText(filePath);

        // Act & Assert
        Assert.IsTrue(content.Contains("dotnet_diagnostic.RCS1090.severity = none"), 
            "RCS1090 should be disabled when ConfigureAwait is not required");
    }

    [TestMethod]
    public void RequireConfigureAwaitRules_EnablesCA2007AsError()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "require-configureawait.rules");
        string content = File.ReadAllText(filePath);

        // Act & Assert
        Assert.IsTrue(content.Contains("dotnet_diagnostic.CA2007.severity = error"), 
            "CA2007 should be error when ConfigureAwait is required");
    }

    [TestMethod]
    public void RequireConfigureAwaitRules_EnablesRCS1090AsError()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "require-configureawait.rules");
        string content = File.ReadAllText(filePath);

        // Act & Assert
        Assert.IsTrue(content.Contains("dotnet_diagnostic.RCS1090.severity = error"), 
            "RCS1090 should be error when ConfigureAwait is required");
    }

    [TestMethod]
    public void EditorConfigBase_SpecifiesCSharpFileSection()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "editorconfig.base");
        string content = File.ReadAllText(filePath);

        // Act & Assert
        Assert.IsTrue(content.Contains("[*.cs]"), "Should have [*.cs] section");
    }

    [TestMethod]
    public void EditorConfigBase_SpecifiesIndentationForCSharp()
    {
        // Arrange
        string filePath = Path.Combine(TemplatesPath, "editorconfig.base");
        string content = File.ReadAllText(filePath);

        // Act & Assert
        Assert.IsTrue(content.Contains("indent_style = space") || content.Contains("indent_size"), 
            "Should specify indentation preferences");
    }
}
