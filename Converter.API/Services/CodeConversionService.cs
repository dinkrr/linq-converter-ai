using Converter.API.Models;

namespace Converter.API.Services;

public class CodeConversionService : ICodeConversionService
{
    private readonly IVertexAIService _vertexAIService;
    private readonly ILogger<CodeConversionService> _logger;

    public CodeConversionService(IVertexAIService vertexAIService, ILogger<CodeConversionService> logger)
    {
        _vertexAIService = vertexAIService;
        _logger = logger;
    }

    public async Task<ConvertCodeResponse> ConvertCSharpToLinqAsync(string csharpCode, ConversionOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(csharpCode))
        {
            return new ConvertCodeResponse
            {
                LinqCode = "",
                Success = false,
                Message = "Empty C# code provided"
            };
        }

        _logger.LogInformation("Converting C# code to LINQ using CodeConverter Agent. Code length: {Length}", csharpCode.Length);

        var response = await _vertexAIService.ConvertCSharpToLinqAsync(csharpCode, options);

        _logger.LogInformation("CodeConverter Agent conversion completed. Success: {Success}", response.Success);

        return response;
    }

    public async Task<ExplainCodeResponse> ExplainCSharpCodeAsync(string csharpCode, bool includeLinqEquivalent = true)
    {
        if (string.IsNullOrWhiteSpace(csharpCode))
        {
            return new ExplainCodeResponse
            {
                Explanation = "",
                Success = false,
                Message = "Empty C# code provided"
            };
        }

        _logger.LogInformation("Explaining C# code using CodeExplainer Agent. Code length: {Length}", csharpCode.Length);

        var response = await _vertexAIService.ExplainCSharpCodeAsync(csharpCode, includeLinqEquivalent);

        _logger.LogInformation("CodeExplainer Agent explanation completed. Success: {Success}", response.Success);

        return response;
    }
}