---
description: "Use when writing, editing, or reviewing C# and .NET code. Covers naming conventions, async/await patterns, null handling, LINQ, dependency injection, error handling, disposable resources, collections, and modern C# features."
applyTo: "**/*.cs"
---

# C# and .NET Best Practices

## Naming Conventions

- **PascalCase**: classes, records, structs, interfaces, enums, public properties, methods, constants, namespaces, events.
- **camelCase with `_` prefix**: private fields (`_myField`).
- **camelCase**: local variables, method parameters.
- **`I` prefix**: interfaces (`ICodeConversionService`).
- **`Async` suffix**: all `async` methods (`ConvertCodeAsync`).
- **`T` prefix**: generic type parameters (`TRequest`, `TResponse`).

```csharp
// Good
private readonly ICodeConversionService _codeConversionService;

public async Task<ConvertCodeResponse> ConvertCodeAsync(string sourceCode) { }
```

## Async / Await

- Mark every I/O-bound method `async Task<T>` (or `async Task` for void). Never use `void` for async (except event handlers).
- Always `await` tasks — do not `.Result` or `.Wait()` (causes deadlocks in ASP.NET Core).
- Pass `CancellationToken` through the call chain and honour it.
- Use `ConfigureAwait(false)` in library/service code (not controller code).

```csharp
// Good
public async Task<string> GetDataAsync(CancellationToken cancellationToken = default)
{
    return await _httpClient.GetStringAsync(url, cancellationToken).ConfigureAwait(false);
}

// Bad — blocks the thread and can deadlock
public string GetData() => _httpClient.GetStringAsync(url).Result;
```

## Null Handling

- Enable nullable reference types (`<Nullable>enable</Nullable>` in .csproj) and keep it enabled.
- Use `??` for null-coalescing, `?.` for null-conditional access, `??=` for lazy assignment.
- Prefer the null-forgiving operator `!` only when you have verified nullability — document why.
- Validate public-facing inputs early and throw `ArgumentNullException.ThrowIfNull(param)` (or return `BadRequest` in controllers).

```csharp
// Good
public void Process(string code)
{
    ArgumentNullException.ThrowIfNull(code);
    var trimmed = code?.Trim() ?? string.Empty;
}
```

## LINQ

- Prefer method syntax (`.Where().Select()`) over query syntax for consistency and composability.
- Keep chains readable: one method per line when the chain has 3+ operations.
- Avoid multiple enumeration — materialise with `.ToList()` or `.ToArray()` when a sequence is consumed more than once.
- Never call `.First()` on an unknown sequence; use `.FirstOrDefault()` and null-check the result.
- Use `.Any()` instead of `.Count() > 0` for existence checks.

```csharp
// Good
var results = items
    .Where(x => x.IsActive)
    .OrderBy(x => x.Name)
    .Select(x => x.Id)
    .ToList();

bool hasItems = results.Any();

// Bad
bool hasItems = results.Count() > 0;
```

## Dependency Injection

- Always define an interface (`IFooService`) and register both interface and implementation.
- Inject dependencies via the constructor; avoid `ServiceLocator` or `IServiceProvider` directly in business logic.
- Register services with the correct lifetime:
  - `AddScoped` — per HTTP request (default for services with EF/HTTP state)
  - `AddTransient` — stateless, cheap-to-create helpers
  - `AddSingleton` — truly stateless, thread-safe services (e.g., caches, config wrappers)
- Use `AddHttpClient<IFoo, Foo>()` for services that own an `HttpClient`.

```csharp
// Program.cs
builder.Services.AddScoped<ICodeConversionService, CodeConversionService>();
builder.Services.AddHttpClient<IVertexAIService, VertexAIService>();
```

## Error Handling and Logging

- Catch specific exceptions before `Exception`.
- Log errors with structured context (`_logger.LogError(ex, "Failed to convert code for request {RequestId}", requestId)`).
- Do not swallow exceptions silently — always log or rethrow.
- Use `ILogger<T>` (injected) — never `Console.Write` in production code.
- Let unhandled infrastructure exceptions propagate to middleware rather than catching them in every method.

```csharp
try
{
    var result = await _service.ConvertAsync(request, cancellationToken);
    return result;
}
catch (HttpRequestException ex)
{
    _logger.LogError(ex, "HTTP call to Vertex AI failed for request {RequestId}", request.Id);
    throw;
}
```

## Disposable Resources

- Implement `IDisposable` / `IAsyncDisposable` for types that own unmanaged or `IDisposable` resources.
- Always wrap disposable objects in `using` declarations (C# 8+).
- Do not dispose objects you don't own (e.g., `HttpClient` injected via DI).

```csharp
using var stream = new FileStream(path, FileMode.Open);
await stream.ReadAsync(buffer, cancellationToken);
```

## Collections

- Use the most specific interface for parameters (`IReadOnlyList<T>`, `IEnumerable<T>`) and the concrete type for local variables.
- Return `IReadOnlyList<T>` or `IEnumerable<T>` from public methods — never expose mutable internal collections.
- Initialise known-size collections with capacity: `new List<T>(count)`.
- Prefer `ImmutableArray<T>` or records for data-transfer objects that should not mutate.

```csharp
// Good
public IReadOnlyList<ConversionPattern> GetPatterns() => _patterns.AsReadOnly();

// Bad
public List<ConversionPattern> GetPatterns() => _patterns;
```

## Modern C# Features

### Records
Use `record` for immutable data-transfer objects and value-equality semantics.

```csharp
public record ConversionRequest(string SourceCode, ConversionOptions Options);
```

### Pattern Matching
Prefer `switch` expressions and `is` patterns over chains of `if`/`else if` for type-based branching.

```csharp
return response switch
{
    { Success: true } => Ok(response),
    { Success: false, Message: var msg } => BadRequest(msg),
    _ => StatusCode(500)
};
```

### String Interpolation and Raw Literals
Prefer `$"..."` over string concatenation; use raw string literals (`"""..."""`) for multi-line content such as JSON templates or prompts.

### Expression-bodied Members
Use `=>` only for single-expression members; do not compress multi-step logic onto one line.

## File and Class Organisation

- One primary type per file; file name matches the type name.
- Order members: constants → static fields → instance fields → constructors → public properties → public methods → private/protected methods.
- Apply `sealed` to classes not intended for inheritance.
- Avoid `partial` classes unless required by a code generator (e.g., EF migrations, source generators).

## Security

- Never log, serialise, or return secrets, tokens, or connection strings.
- Sanitise inputs that will be included in AI prompts or external API calls to prevent prompt injection.
- Use parameterised queries or ORMs — never string-interpolate user input into SQL.
- Store credentials in environment variables or secret managers, not in `appsettings.json`.
