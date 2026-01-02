# Proctorio.EditorConfig

Standardized C# code style rules for all Proctorio projects.

## Install

```bash
dotnet add package Proctorio.EditorConfig
```

That's it. Rules apply automatically on build.

## What This Enforces

**Explicit types** - No `var`. Ever.
```csharp
// ❌ This will warn
var user = GetUser();

// ✅ Do this
User user = GetUser();
```

**Private field naming** - Must use underscore prefix.
```csharp
// ❌ Warning
private string userName;

// ✅ Correct
private string _userName;
```

**Readonly fields** - If a field doesn't change, mark it readonly.
```csharp
// ❌ Warning if never reassigned
private IUserRepository _repo;

// ✅ Better
private readonly IUserRepository _repo;
```

**CancellationToken forwarding** - Pass tokens through async calls.
```csharp
// ❌ Warning
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