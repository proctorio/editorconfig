# Proctorio.EditorConfig

Standardized C# code style rules for all Proctorio projects.

## Install

```bash
dotnet add package Proctorio.EditorConfig
```

That's it. Rules apply automatically on build.

## Severities

Correctness analyzers (Reliability, Security and Usage categories, for example CA2xxx) and compiler nullability warnings are warnings. Style, naming, documentation, design, maintainability, performance and globalization analyzers are suggestions: they show in the IDE and do not appear as build warnings. Underscore test names (CA1707) and namespace style are therefore fine.

CS1591 (missing XML comment) is only reported for packable libraries. Executables, test projects and projects with `<IsPackable>false</IsPackable>` have it suppressed.

## 2.2.0: correct severities on the first build

The package now also ships its rules as a global analyzer config, `Proctorio.EditorConfig.globalconfig`, generated from the same `.editorconfig` and added to the build automatically. The compiler reads it before anything is compiled, so a fresh checkout (every CI build) gets the right severities on its very first build. Before, the severities came only from the `.editorconfig` copied into your project folder during the build, and a project that did not commit that copy compiled once without it.

You no longer need to commit the copied `.editorconfig`, and a repo that gitignores it works. The copy is still written for IDE formatting and is unchanged.

If a project tracks an OLD copied `.editorconfig` (from a 1.x or 2.0.x package), that file wins over the global config because path-based `.editorconfig` files take precedence. Update it by building once with 2.2.0 and committing the new copy, or delete it.

## What This Enforces

The first three examples below are IDE suggestions, so they do not fail or warn in the pipeline. The last one, CA2016, is a Reliability rule and is still a build warning.

**Explicit types** - No `var`. Ever.
```csharp
// ❌ Suggestion
var user = GetUser();

// ✅ Do this
User user = GetUser();
```

**Private field naming** - Must use underscore prefix.
```csharp
// ❌ Suggestion
private string userName;

// ✅ Correct
private string _userName;
```

**Readonly fields** - If a field doesn't change, mark it readonly.
```csharp
// ❌ Suggestion if never reassigned
private IUserRepository _repo;

// ✅ Better
private readonly IUserRepository _repo;
```

**CancellationToken forwarding** - Pass tokens through async calls.
```csharp
// ❌ Warning (CA2016)
await DoWorkAsync();

// ✅ Pass it along
await DoWorkAsync(cancellationToken);
```

## What This Suggests

These show as suggestions, not warnings:

- Use pattern matching over `as`/`is` checks
- Use object/collection initializers
- Use null coalescing (`??`) and null propagation (`?.`)
- Use throw expressions
- Mark static members when possible
- Convert `typeof(T)` to `nameof(T)` where valid

## What This Doesn't Enforce

**Ternary operators** - Feel free to use them (or don't).
**Exception handling** - CA1031 is off. Catch what you need.
**Null checks** - CA1062 is off. Not every public method needs null guards.
**Localization** - CA1303 is off. String literals are fine.

## Formatting

**Braces** - Always required, even for single statements.
```csharp
// ❌ No
if (condition)
    DoSomething();

// ✅ Yes
if (condition)
{
    DoSomething();
}
```

**Brace style** - Allman (new line).
```csharp
if (condition)
{
    // code
}
```

**Indentation** - 4 spaces for C#, tabs for XML/config.

**Spacing**
- Space after keywords: `if (x)`
- No space after cast: `(int)value`
- Space around binary operators: `x + y`

## Naming Conventions

| Type | Format | Example |
|------|--------|---------|
| Interfaces | `I` + PascalCase | `IUserRepository` |
| Classes, Methods, Properties | PascalCase | `UserService`, `GetUser()` |
| Private fields | `_camelCase` | `_userName` |
| Private constants | PascalCase | `MaxRetries` |

## ConfigureAwait

The default package doesn't require `.ConfigureAwait(false)`. If you need that, use the `-RequireConfigureAwait` variant (when available).

## IDE Setup

**Visual Studio / Rider** - Works out of the box after package install.

**VS Code** - Install [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit), then add the package.

## Testing

This package has 34 automated tests covering rule syntax, formatting, and build integration.

```bash
dotnet test
```