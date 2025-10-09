namespace Converter.API.Models;

public class CodeAnalysis
{
    public string Complexity { get; set; } = string.Empty;

    public string[] DetectedPatterns { get; set; } = Array.Empty<string>();

    public string[] PotentialIssues { get; set; } = Array.Empty<string>();

    public string[] RequiredNamespaces { get; set; } = Array.Empty<string>();

    public int EstimatedPerformanceGain { get; set; } = 0;
}