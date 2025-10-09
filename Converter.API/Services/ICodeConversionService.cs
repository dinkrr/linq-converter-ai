using Converter.API.Models;
using System.Text.RegularExpressions;

namespace Converter.API.Services;

public interface ICodeConversionService
{
    Task<ConvertCodeResponse> ConvertCSharpToLinqAsync(string csharpCode, ConversionOptions? options = null);
    Task<ExplainCodeResponse> ExplainCSharpCodeAsync(string csharpCode, bool includeLinqEquivalent = true);
}
