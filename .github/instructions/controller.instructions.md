---
description: "Use when creating, editing, or reviewing ASP.NET Core API controllers. Covers naming, routing, XML documentation, dependency injection, validation, and error handling conventions for *Controller.cs files."
applyTo: "**/*Controller.cs"
---

# ASP.NET Core Controller Conventions

## Class Structure

- Inherit from `ControllerBase` (not `Controller` — no view support needed).
- Apply `[ApiController]` and `[Route("api/[controller]")]` attributes.
- Name classes `<Resource>Controller` — the `[controller]` route token strips the suffix automatically.
- Declare injected services as `private readonly I<Service> _<service>` and assign in the constructor.

```csharp
[ApiController]
[Route("api/[controller]")]
public class CodeConversionController : ControllerBase
{
    private readonly ICodeConversionService _codeConversionService;

    public CodeConversionController(ICodeConversionService codeConversionService)
    {
        _codeConversionService = codeConversionService;
    }
}
```

## XML Documentation

Every public class and every public action method **must** have full XML documentation:

- Class: `<summary>` describing the resource/feature exposed.
- Action methods: `<summary>`, `<param>` for each parameter, and `<returns>` listing all possible HTTP responses as a `<list type="bullet">`.

```csharp
/// <summary>
/// Converts imperative C# code to LINQ expressions.
/// </summary>
/// <param name="request">The conversion request containing source code and options.</param>
/// <returns>
/// <list type="bullet">
///   <item><description><c>200 OK</c> with a <see cref="ConvertCodeResponse"/>.</description></item>
///   <item><description><c>400 Bad Request</c> when <paramref name="request"/> is null or empty.</description></item>
///   <item><description><c>500 Internal Server Error</c> on unexpected errors.</description></item>
/// </list>
/// </returns>
```

## Action Method Signatures

- **POST / mutating actions**: `async Task<ActionResult<TResponse>>` with `[HttpPost("verb")]`.
- **GET / read-only info endpoints**: `IActionResult` with `[HttpGet("noun")]`.
- Use descriptive method names (e.g., `ConvertCSharpToLinq`, `ExplainCSharpCode`, `GetSupportedPatterns`).

## Input Validation

Validate at the top of every action that accepts a request body before any service call:

```csharp
if (request == null || string.IsNullOrWhiteSpace(request.CSharpCode))
{
    return BadRequest(new ConvertCodeResponse
    {
        Success = false,
        Message = "C# code is required for conversion"
    });
}
```

- Return a typed response object — not a string — inside `BadRequest(...)`.
- Set `Success = false` and populate `Message` with a human-readable reason.

## Error Handling

Wrap service calls in `try/catch` and return `StatusCode(500, ...)` with the same typed response model:

```csharp
try
{
    var response = await _codeConversionService.ConvertCSharpToLinqAsync(request.CSharpCode, request.Options);
    return Ok(response);
}
catch (Exception ex)
{
    return StatusCode(500, new ConvertCodeResponse
    {
        Success = false,
        Message = $"Internal server error: {ex.Message}"
    });
}
```

- Do **not** let exceptions propagate out of action methods.
- Never expose stack traces; only `ex.Message` is acceptable.

## GET Utility Endpoints (health, metadata)

Anonymous objects are acceptable for read-only, schema-free responses:

```csharp
[HttpGet("health")]
public IActionResult Health()
{
    return Ok(new
    {
        status = "healthy",
        service = "C# to LINQ Converter",
        timestamp = DateTime.UtcNow,
        version = "2.0.0"
    });
}
```

## Response Models

- POST endpoints must use typed response models defined in `Models/ApiModels.cs`.
- Response models should include at minimum `Success` (bool) and `Message` (string) fields.
- Do **not** use anonymous objects for POST responses.

## Namespace

Always place controllers in `Converter.API.Controllers`:

```csharp
namespace Converter.API.Controllers;
```
