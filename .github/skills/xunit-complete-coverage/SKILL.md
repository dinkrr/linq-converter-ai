---
name: xunit-complete-coverage
description: "Use when writing comprehensive xUnit tests for .NET backend code. Covers test structure, mocking with Moq, happy path + edge cases, async patterns, multi-target testing (net9.0, net10.0), and coverage reporting. Integrates with project conventions from instructions and AGENTS.md."
tools: [execute, read, edit, search, todo]
---

# xUnit Testing with Complete Coverage

Generate rigorous, maintainable unit and integration tests for `Converter.API` following project conventions, xUnit + Moq patterns, and multi-target compatibility.

---

## Workflow Overview

**This skill guides the entire test-writing lifecycle:**

1. **Analyze** — Understand the class/method under test and its dependencies
2. **Plan** — Identify test scenarios (happy path, errors, edge cases, async patterns)
3. **Setup** — Create test class with fixtures and mocks
4. **Write Tests** — Happy path, all exception paths, boundary conditions
5. **Verify** — Run multi-target builds and coverage analysis
6. **Report** — Summarize coverage gaps and recommendations

Use this skill:
- Before implementing new services, controllers, or models (outline test strategy first)
- When adding features to existing code (ensure coverage expansion)
- For code review validation (verify test quality against standards)
- When refactoring (maintain coverage while changing implementation)

---

## Step 1 — Analyze the Code Under Test (CUT)

Load and understand:
1. The class file (e.g., `Converter.API/Services/CodeConversionService.cs`)
2. All dependencies (injected interfaces and HTTP clients)
3. Public methods and their signatures (especially `async Task<T>` patterns)
4. Error paths (exceptions, null inputs, validation)
5. Integration points (Vertex AI service, configuration, external APIs)

**Questions to answer:**
- What does this class do?
- What inputs does it accept (DTOs, primitives, cancellation tokens)?
- What outputs does it return?
- What can go wrong? (null inputs, HTTP failures, timeouts, auth failures)
- Is this class `Scoped`, `Transient`, or `Singleton`? (affects test isolation)
- Does it use `async/await`? (must test cancellation and exception propagation)

---

## Step 2 — Plan Test Scenarios

### Test Categories

**Happy Path (1 test per method)**
- Arrange valid inputs, valid mocks
- Act and Assert success outcome
- Verify response shape and values

**Input Validation & Null Handling**
- Null or empty strings
- Invalid ranges, missing required fields
- Edge case values (empty collections, zero, max values)
- Each parameter tested independently

**Exception & Error Paths**
- `HttpRequestException` from Vertex AI client
- `ArgumentNullException` or `ArgumentException` from validation
- Timeout (cancellation token)
- Configuration errors (missing `ProjectId`, bad model name)
- Expected vs unexpected exceptions

**Async & Cancellation**
- Normal completion
- Cancellation via `CancellationToken`
- Timeout behavior
- Exception propagation (async Task should not swallow exceptions)

**Mocking & Isolation**
- Mock all external dependencies (`IVertexAIService`, `IConfigurationService`, `HttpClient`)
- No real HTTP calls, no real Google Cloud API calls
- Moq `.Verify()` to confirm correct method calls with correct args

### Example Test Plan

**For `CodeConversionService.ConvertCodeAsync(request, cancellationToken)`:**

```
✓ Happy path: valid request → ConvertCodeResponse with linqCode, suggestions
✓ Null request → throws ArgumentNullException
✓ Empty SourceCode → throws ArgumentException or returns error response
✓ Vertex AI returns error → propagates or wraps in service exception
✓ Cancellation token fired → task cancelled gracefully
✓ Response parsing fails → logs error and throws
✓ Configuration invalid → throws on startup (via IConfigurationValidationService)
```

---

## Step 3 — Create Test Class Structure

### Naming & Location

```
Converter.API.Tests/
├── Services/
│   ├── CodeConversionServiceTests.cs      ← mirrors Converter.API/Services/
│   ├── VertexAIServiceTests.cs
│   └── ConfigurationValidationServiceTests.cs
├── Controllers/
│   ├── ExpressionControllerTests.cs       ← mirrors Converter.API/Controllers/
└── Models/
    └── (model unit tests if needed)
```

**File naming:** `{ClassName}Tests.cs` (e.g., `CodeConversionServiceTests.cs`)

### Test Class Template

```csharp
using Moq;
using Xunit;
using Converter.API.Models;
using Converter.API.Services;

namespace Converter.API.Tests.Services;

public class CodeConversionServiceTests
{
    private readonly Mock<IVertexAIService> _vertexAIServiceMock;
    private readonly Mock<IConfigurationValidationService> _configValidationMock;
    private readonly CodeConversionService _sut; // System Under Test

    public CodeConversionServiceTests()
    {
        // Initialize mocks
        _vertexAIServiceMock = new Mock<IVertexAIService>();
        _configValidationMock = new Mock<IConfigurationValidationService>();

        // Inject mocks into SUT
        _sut = new CodeConversionService(
            _vertexAIServiceMock.Object,
            _configValidationMock.Object
        );
    }

    // Test methods follow below...
}
```

### Fixture Builders (Optional, for Complex Objects)

```csharp
public static class CodeConversionRequestBuilder
{
    public static CodeConversionRequest Create(
        string sourceCode = "var x = items.Where(i => i.IsActive).Select(i => i.Id).ToList();",
        ConversionOptions? options = null
    ) => new(sourceCode, options ?? ConversionOptions.Default);

    public static CodeConversionRequest CreateWithNullCode() =>
        new(null!, ConversionOptions.Default);

    public static CodeConversionRequest CreateWithEmptyCode() =>
        new(string.Empty, ConversionOptions.Default);
}
```

---

## Step 4 — Write Individual Tests

### Pattern: AAA (Arrange, Act, Assert)

```csharp
[Fact]
public async Task ConvertCodeAsync_WithValidRequest_ReturnsConversionResponse()
{
    // Arrange
    var request = new CodeConversionRequest(
        "var x = items.Where(i => i.IsActive).ToList();",
        ConversionOptions.Default
    );
    var expectedResponse = new ConversionResponse(
        success: true,
        linqCode: "items.Where(i => i.IsActive).ToList()",
        suggestions: new[] { "Use .Any() instead of .Count() > 0" }
    );

    _vertexAIServiceMock
        .Setup(s => s.ConvertCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedResponse);

    // Act
    var result = await _sut.ConvertCodeAsync(request, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.True(result.Success);
    Assert.Equal(expectedResponse.LinqCode, result.LinqCode);
    _vertexAIServiceMock.Verify(
        s => s.ConvertCodeAsync(request.SourceCode, It.IsAny<CancellationToken>()),
        Times.Once
    );
}
```

### Input Validation Tests

```csharp
[Fact]
public async Task ConvertCodeAsync_WithNullRequest_ThrowsArgumentNullException()
{
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentNullException>(
        () => _sut.ConvertCodeAsync(null!, CancellationToken.None)
    );
}

[Fact]
public async Task ConvertCodeAsync_WithEmptySourceCode_ThrowsArgumentException()
{
    // Arrange
    var request = new CodeConversionRequest(string.Empty, ConversionOptions.Default);

    // Act & Assert
    var ex = await Assert.ThrowsAsync<ArgumentException>(
        () => _sut.ConvertCodeAsync(request, CancellationToken.None)
    );
    Assert.Contains("SourceCode", ex.Message);
}
```

### Exception Handling Tests

```csharp
[Fact]
public async Task ConvertCodeAsync_WhenVertexAIThrowsException_PropagatesWrapped()
{
    // Arrange
    var request = new CodeConversionRequest("var x = items.ToList();", ConversionOptions.Default);
    var innerException = new HttpRequestException("API timeout");

    _vertexAIServiceMock
        .Setup(s => s.ConvertCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(innerException);

    // Act & Assert
    var ex = await Assert.ThrowsAsync<HttpRequestException>(
        () => _sut.ConvertCodeAsync(request, CancellationToken.None)
    );
    Assert.Equal(innerException.Message, ex.Message);
}
```

### Async & Cancellation Tests

```csharp
[Fact]
public async Task ConvertCodeAsync_WithCancelledToken_ThrowsOperationCanceledException()
{
    // Arrange
    var request = new CodeConversionRequest("var x = items.ToList();", ConversionOptions.Default);
    var cts = new CancellationTokenSource();
    cts.Cancel();

    _vertexAIServiceMock
        .Setup(s => s.ConvertCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new OperationCanceledException());

    // Act & Assert
    await Assert.ThrowsAsync<OperationCanceledException>(
        () => _sut.ConvertCodeAsync(request, cts.Token)
    );
}
```

### Controller Tests (Integration)

```csharp
using Microsoft.AspNetCore.Mvc;

namespace Converter.API.Tests.Controllers;

public class ExpressionControllerTests
{
    private readonly Mock<ICodeConversionService> _codeConversionMock;
    private readonly ExpressionController _sut;

    public ExpressionControllerTests()
    {
        _codeConversionMock = new Mock<ICodeConversionService>();
        _sut = new ExpressionController(_codeConversionMock.Object);
    }

    [Fact]
    public async Task ConvertAsync_WithValidRequest_ReturnsOkWithResponse()
    {
        // Arrange
        var request = new CodeConversionRequest("var x = items.ToList();", ConversionOptions.Default);
        var serviceResponse = new ConversionResponse(
            success: true,
            linqCode: "items.ToList()",
            suggestions: new string[] { }
        );

        _codeConversionMock
            .Setup(s => s.ConvertCodeAsync(request, CancellationToken.None))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _sut.ConvertAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task ConvertAsync_WithNullRequest_ReturnsBadRequest()
    {
        // Act
        var result = await _sut.ConvertAsync(null!);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }
}
```

---

## Step 5 — Run Tests & Verify Coverage

### Run Tests Locally

```bash
# Single target
dotnet test Converter.API.Tests/Converter.API.Tests.csproj

# Multi-target (net9.0 + net10.0)
dotnet test Converter.API.Tests/Converter.API.Tests.csproj --framework net9.0
dotnet test Converter.API.Tests/Converter.API.Tests.csproj --framework net10.0

# Verbose output
dotnet test Converter.API.Tests/Converter.API.Tests.csproj --verbosity detailed

# Stop on first failure
dotnet test Converter.API.Tests/Converter.API.Tests.csproj --no-build --stop-on-failure
```

### Collect Coverage

```bash
dotnet test Converter.API.Tests/Converter.API.Tests.csproj \
  --collect:"XPlat Code Coverage" \
  --results-directory="./CoverageResults"
```

**Output:** Coverage report in `CoverageResults/*/coverage.cobertura.xml`

### Analyze Coverage

- **Line coverage**: % of lines executed
- **Branch coverage**: % of conditional branches taken
- **Target**: ≥80% overall; ≥90% for critical paths (services, validation)
- **Uncovered lines**: Review and decide: test it or mark as unreachable (`[ExcludeFromCodeCoverage]`)

---

## Step 6 — Follow Project Conventions

### Apply Instruction Files

Before finalizing tests, verify:
- **[csharp-dotnet.instructions.md](../../instructions/csharp-dotnet.instructions.md)**: Async/await patterns, null handling, naming (PascalCase classes, camelCase methods)
- **[controller.instructions.md](../../instructions/controller.instructions.md)** (for controller tests): Response shapes, status codes, validation

### Key Test Patterns

**Async Methods** — Always use `async Task`:
```csharp
[Fact]
public async Task MyMethodAsync_Scenario_Outcome()
{
    var result = await _sut.MyMethodAsync();
    Assert.NotNull(result);
}
```

**Null Validation** — Throw early:
```csharp
[Fact]
public async Task MyMethodAsync_WithNullInput_ThrowsArgumentNullException()
{
    await Assert.ThrowsAsync<ArgumentNullException>(
        () => _sut.MyMethodAsync(null!)
    );
}
```

**Mocking HttpClient Calls**:
```csharp
_httpClientMock
    .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK) 
    { 
        Content = new StringContent("{}") 
    });
```

**Verify Exact Method Calls**:
```csharp
_mock.Verify(
    s => s.MethodAsync(
        It.Is<string>(x => x == "expected"),
        It.IsAny<CancellationToken>()
    ),
    Times.Once
);
```

---

## Step 7 — Integration with Code Generator Agent

This skill is designed to work **before** the `code-generator` agent:

1. **Plan Phase**: Use this skill to design tests first (TDD)
2. **Generate Phase**: `code-generator` implements both code and tests together
3. **Review Phase**: Verify `code-generator` output matches your test strategy

**Prompt example for `code-generator`:**

> "Use the xunit-complete-coverage skill to plan tests for the new GetPatternsAsync endpoint. Then implement the endpoint and tests together."

---

## Test Organization Checklist

- [ ] Test class mirrors source directory structure
- [ ] Test file named `{ClassName}Tests.cs`
- [ ] Constructor initializes all mocks and SUT
- [ ] Happy path test exists (1 per public method)
- [ ] Null/empty input tests for each parameter
- [ ] Exception path tests for all exception types
- [ ] Async cancellation tests (if method is `async Task`)
- [ ] Mock `.Verify()` calls confirm correct behavior
- [ ] All test names follow `Method_Scenario_Outcome` pattern
- [ ] No hardcoded test data (use builders or factories)
- [ ] Multi-target builds pass (net9.0 and net10.0)
- [ ] Coverage ≥80% overall; ≥90% for critical code
- [ ] No `[Skip]` attributes without documented reason

---

## Common Pitfalls

| Issue | Solution |
|-------|----------|
| `NullReferenceException` in setup | Initialize all mocks in constructor |
| Test passes locally, fails in CI | Ensure timezone/culture independence; avoid hardcoded dates |
| Flaky async tests | Always pass `CancellationToken.None` explicitly; don't rely on defaults |
| Multiple test assertions fail | Use separate tests for separate concerns (one Assert.Throws per test) |
| Coverage not reported | Install ReportGenerator: `dotnet tool install -g reportgenerator` |
| Tests slow to run | Reduce Sleep/delay; mock external calls; use Fact instead of Theory when parameters are static |

---

## Related Workflows

- **Before Testing**: Use `code-planner` agent to plan implementation, then add test scenarios
- **During Implementation**: Use `code-generator` agent with approved test strategy
- **After Implementation**: Use `Code Reviewer` agent to audit test coverage

---

## Example Prompts to Try This Skill

> "Create comprehensive xUnit tests for CodeConversionService covering all happy path, exception, and cancellation scenarios."

> "Plan and implement tests for a new controller endpoint following xunit-complete-coverage conventions."

> "Analyze coverage gaps in the test suite and generate missing test cases for critical paths."

