using Microsoft.AspNetCore.Mvc;
using Converter.API.Models;
using Converter.API.Services;

namespace Converter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CodeConversionController : ControllerBase
{
    private readonly ICodeConversionService _codeConversionService;

    public CodeConversionController(ICodeConversionService codeConversionService)
    {
        _codeConversionService = codeConversionService;
    }

    [HttpPost("convert")]
    public async Task<ActionResult<ConvertCodeResponse>> ConvertCSharpToLinq([FromBody] ConvertCodeRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.CSharpCode))
        {
            return BadRequest(new ConvertCodeResponse
            {
                LinqCode = "",
                Success = false,
                Message = "C# code is required for conversion"
            });
        }

        try
        {
            var response = await _codeConversionService.ConvertCSharpToLinqAsync(request.CSharpCode, request.Options);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ConvertCodeResponse
            {
                LinqCode = "",
                Success = false,
                Message = $"Internal server error: {ex.Message}"
            });
        }
    }

    [HttpPost("explain")]
    public async Task<ActionResult<ExplainCodeResponse>> ExplainCSharpCode([FromBody] ExplainCodeRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.CSharpCode))
        {
            return BadRequest(new ExplainCodeResponse
            {
                Explanation = "",
                Success = false,
                Message = "C# code is required for explanation"
            });
        }

        try
        {
            var response = await _codeConversionService.ExplainCSharpCodeAsync(request.CSharpCode, request.IncludeLinqEquivalent);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ExplainCodeResponse
            {
                Explanation = "",
                Success = false,
                Message = $"Internal server error: {ex.Message}"
            });
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            service = "C# to LINQ Converter",
            features = new[]
            {
                "AI-powered code conversion",
                "Intelligent code analysis",
                "LINQ optimization suggestions",
                "Performance improvement estimates"
            },
            timestamp = DateTime.UtcNow,
            version = "2.0.0"
        });
    }

    /// <summary>
    /// Validates whether the provided C# code contains patterns suitable for LINQ conversion.
    /// </summary>
    /// <param name="request">The validation request containing the C# source code.</param>
    /// <returns>
    /// <list type="bullet">
    ///   <item><description><c>200 OK</c> with a <see cref="ValidateCodeResponse"/> indicating suitability.</description></item>
    ///   <item><description><c>400 Bad Request</c> when <paramref name="request"/> is null or empty.</description></item>
    ///   <item><description><c>500 Internal Server Error</c> on unexpected errors.</description></item>
    /// </list>
    /// </returns>
    [HttpPost("validate")]
    public async Task<ActionResult<ValidateCodeResponse>> ValidateForLinqConversion([FromBody] ValidateCodeRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.CSharpCode))
        {
            return BadRequest(new ValidateCodeResponse
            {
                CanConvert = false,
                Success = false,
                Message = "C# code is required for validation"
            });
        }

        try
        {
            var response = await _codeConversionService.ValidateForLinqAsync(request.CSharpCode);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ValidateCodeResponse
            {
                CanConvert = false,
                Success = false,
                Message = $"Internal server error: {ex.Message}"
            });
        }
    }

    [HttpGet("patterns")]
    public IActionResult GetSupportedPatterns()
    {
        var patterns = new
        {
            supportedPatterns = new[]
            {
                new
                {
                    name = "Foreach to Select",
                    description = "Convert foreach loops that transform collections to LINQ Select",
                    example = new
                    {
                        csharp = "foreach(var item in list) { result.Add(item.Transform()); }",
                        linq = "var result = list.Select(item => item.Transform()).ToList();"
                    }
                },
                new
                {
                    name = "Filtering with Where",
                    description = "Convert if conditions inside loops to LINQ Where clauses",
                    example = new
                    {
                        csharp = "foreach(var item in list) { if(item.IsValid) result.Add(item); }",
                        linq = "var result = list.Where(item => item.IsValid).ToList();"
                    }
                },
                new
                {
                    name = "Aggregation Operations",
                    description = "Convert manual sum/count operations to LINQ aggregation methods",
                    example = new
                    {
                        csharp = "int sum = 0; foreach(var item in list) sum += item.Value;",
                        linq = "var sum = list.Sum(item => item.Value);"
                    }
                }
            }
        };

        return Ok(patterns);
    }
}