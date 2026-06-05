using Converter.API.Models;

namespace Converter.API.Services;

public interface IVertexAIService
{
    Task<VertexAIResponse> GenerateTextAsync(VertexAIRequest request);

    Task<ConvertCodeResponse> ConvertCSharpToLinqAsync(string csharpCode, ConversionOptions? options = null);

    Task<ExplainCodeResponse> ExplainCSharpCodeAsync(string csharpCode, bool includeLinqEquivalent = true);

    CodeAnalysis AnalyzeCSharpCode(string csharpCode);
}