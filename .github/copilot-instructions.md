# GitHub Copilot Custom Instructions for LINQ Converter AI

## Project Context
This is a .NET 9 Web API that converts traditional C# code to LINQ expressions 
using Google Vertex AI. The frontend is Angular 17+.

## Coding Standards
- Use file-scoped namespaces (namespace X; not namespace X { })
- Use primary constructors where applicable
- Prefer record types for DTOs and immutable data
- Use async/await for all I/O operations
- Always include CancellationToken in async methods
- Use ILogger<T> for logging, never Console.WriteLine

## Naming Conventions
- Interfaces: Prefix with 'I' (e.g., ICodeConversionService)
- Async methods: Suffix with 'Async' (e.g., ConvertAsync)
- Private fields: Prefix with '_' (e.g., _logger)
- Constants: PascalCase (e.g., MaxRetryCount)

## Error Handling
- Use Result pattern for expected failures
- Throw exceptions only for unexpected errors
- Always log exceptions with context
- Return meaningful error messages to API clients

## Testing Requirements
- Use xUnit for unit tests
- Use Moq for mocking
- Follow Arrange-Act-Assert pattern
- Test names: MethodName_Scenario_ExpectedResult

## LINQ Preferences
- Prefer method syntax over query syntax
- Use meaningful variable names in lambdas (not single letters)
- Avoid multiple enumerations - use .ToList() when needed
- Consider performance for large collections
