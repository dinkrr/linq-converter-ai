namespace Converter.API.Models;

public class ConvertCodeRequest
{
    public string CSharpCode { get; set; } = string.Empty;

    public ConversionOptions? Options { get; set; }
}

public class ConvertCodeResponse
{
    public string LinqCode { get; set; } = string.Empty;

    public bool Success { get; set; }

    public string? Message { get; set; }

    public CodeAnalysis? Analysis { get; set; }

    public string[]? Suggestions { get; set; }
}

public class ExplainCodeRequest
{
    public string CSharpCode { get; set; } = string.Empty;

    public bool IncludeLinqEquivalent { get; set; } = true;
}

public class ExplainCodeResponse
{
    public string Explanation { get; set; } = string.Empty;

    public bool Success { get; set; }

    public string? Message { get; set; }

    public CodeAnalysis? Analysis { get; set; }

    public string? LinqEquivalent { get; set; }
}