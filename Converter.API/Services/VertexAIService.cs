using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using Converter.API.Models;
using Newtonsoft.Json;
using System.Text;

namespace Converter.API.Services;

public class VertexAIService : IVertexAIService
{
    #region Constants
    private const string CloudPlatformScope = "https://www.googleapis.com/auth/cloud-platform";
    private const string ApplicationJsonMediaType = "application/json";
    private const int MaxPerformanceGain = 30;
    private const int ComplexCodeLineThreshold = 15;
    private const int ModerateCodeLineThreshold = 7;
    private const int ComplexLoopThreshold = 2;
    #endregion

    #region Fields
    private readonly VertexAIConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly ILogger<VertexAIService> _logger;
    private readonly Dictionary<string, string> _agentPrompts;
    #endregion

    #region Constructor
    public VertexAIService(IOptions<VertexAIConfiguration> config, HttpClient httpClient, ILogger<VertexAIService> logger)
    {
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _agentPrompts = InitializeAgentPrompts();

        ValidateConfiguration();
        _logger.LogInformation("VertexAI Service initialized with custom agent implementation");
    }
    #endregion

    #region Configuration Validation
    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_config.ProjectId))
            throw new InvalidOperationException("VertexAI ProjectId is required");

        if (string.IsNullOrWhiteSpace(_config.Location))
            throw new InvalidOperationException("VertexAI Location is required");

        if (string.IsNullOrWhiteSpace(_config.ModelName))
            throw new InvalidOperationException("VertexAI ModelName is required");
    }
    #endregion

    #region Agent Prompt Initialization
    private Dictionary<string, string> InitializeAgentPrompts()
    {
        var prompts = new Dictionary<string, string>();

        prompts["CodeConverter"] = GetCodeConverterPrompt();
        prompts["CodeExplainer"] = GetCodeExplainerPrompt();
        prompts["PerformanceAnalyzer"] = GetPerformanceAnalyzerPrompt();

        return prompts;
    }

    private string GetCodeConverterPrompt()
    {
        return @"You are an expert C# developer and LINQ specialist. Your role is to convert traditional C# code (loops, conditionals, etc.) into equivalent LINQ expressions.

AGENT RESPONSIBILITIES:
- Focus exclusively on code conversion tasks
- Ensure functional equivalence between original and converted code
- Optimize for readability and performance
- Provide clean, compilable LINQ code

KEY CONVERSION GUIDELINES:
1. Convert for/foreach loops to LINQ methods (Select, Where, SelectMany, etc.)
2. Convert conditional logic to Where clauses or ternary operators
3. Convert aggregation loops to LINQ aggregation methods (Sum, Count, Average, etc.)
4. Preserve the original logic and functionality exactly
5. Include necessary using statements
6. Prefer method syntax over query syntax unless specified otherwise
7. Add helpful comments explaining complex conversions

RESPONSE FORMAT: Return only the converted LINQ code without explanations unless specifically requested.";
    }

    private string GetCodeExplainerPrompt()
    {
        return @"You are an expert C# developer and educator. Your role is to analyze and explain C# code to help developers understand complex programming concepts.

AGENT RESPONSIBILITIES:
- Provide comprehensive code analysis and explanations
- Identify key programming concepts and patterns
- Suggest improvements and best practices
- Educate developers on LINQ conversion opportunities

ANALYSIS FRAMEWORK:
1. Code structure and flow analysis
2. Key programming concepts identification
3. Performance implications assessment
4. Best practices evaluation
5. LINQ conversion opportunities
6. Code complexity analysis

RESPONSE FORMAT: Provide clear, educational explanations with sections and bullet points. Be thorough but concise.";
    }

    private string GetPerformanceAnalyzerPrompt()
    {
        return @"You are a performance optimization specialist for C# code. Your role is to analyze code for performance bottlenecks and suggest LINQ optimizations.

AGENT RESPONSIBILITIES:
- Identify performance bottlenecks in traditional C# code
- Suggest LINQ alternatives that improve performance
- Analyze memory usage patterns
- Recommend optimization strategies

ANALYSIS AREAS:
1. Loop efficiency and complexity
2. Memory allocation patterns
3. LINQ deferred execution opportunities
4. Collection manipulation efficiency
5. Parallel processing potential

RESPONSE FORMAT: Provide performance metrics, bottleneck identification, and optimization recommendations.";
    }
    #endregion

    #region Public Methods
    public async Task<VertexAIResponse> GenerateTextAsync(VertexAIRequest request)
    {
        try
        {
            var accessToken = await GetAccessTokenAsync();
            var apiUrl = BuildApiUrl();

            var requestPayload = BuildRequestPayload(request);
            var jsonContent = JsonConvert.SerializeObject(requestPayload, Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, ApplicationJsonMediaType)
            };

            httpRequest.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode
                ? ProcessSuccessResponse(responseContent)
                : ProcessErrorResponse(response, responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Vertex AI API");
            return new VertexAIResponse
            {
                Content = "",
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<ConvertCodeResponse> ConvertCSharpToLinqAsync(string csharpCode, ConversionOptions? options = null)
    {
        var systemInstruction = _agentPrompts["CodeConverter"];
        var agentContext = "\n[AGENT MODE: CodeConverter Agent Active]";
        _logger.LogInformation("Using CodeConverter agent for LINQ conversion");

        var optionsText = BuildConversionOptionsText(options);
        var prompt = BuildConversionPrompt(csharpCode, optionsText, agentContext);

        var request = new VertexAIRequest
        {
            Prompt = prompt,
            SystemInstruction = systemInstruction
        };

        var aiResponse = await GenerateTextAsync(request);

        return aiResponse.Success
            ? CreateSuccessfulConversionResponse(aiResponse, csharpCode)
            : CreateFailedConversionResponse(aiResponse, csharpCode);
    }

    public async Task<ExplainCodeResponse> ExplainCSharpCodeAsync(string csharpCode, bool includeLinqEquivalent = true)
    {
        var systemInstruction = _agentPrompts["CodeExplainer"];
        var agentContext = "\n[AGENT MODE: CodeExplainer Agent Active]";
        _logger.LogInformation("Using CodeExplainer agent for code analysis");

        var prompt = BuildExplanationPrompt(csharpCode, includeLinqEquivalent, agentContext);

        var request = new VertexAIRequest
        {
            Prompt = prompt,
            SystemInstruction = systemInstruction
        };

        var aiResponse = await GenerateTextAsync(request);

        return await CreateExplanationResponse(aiResponse, csharpCode, includeLinqEquivalent);
    }
    #endregion

    #region Private Helper Methods
    private async Task<string> GetAccessTokenAsync()
    {
        try
        {
            GoogleCredential credential;

            if (!string.IsNullOrEmpty(_config.ServiceAccountKeyPath) && File.Exists(_config.ServiceAccountKeyPath))
            {
                credential = GoogleCredential.FromFile(_config.ServiceAccountKeyPath);
            }
            else
            {
                credential = await GoogleCredential.GetApplicationDefaultAsync();
            }

            var scopedCredential = credential.CreateScoped(CloudPlatformScope);
            var accessToken = await scopedCredential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            return accessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get access token for Vertex AI");
            throw new InvalidOperationException("Failed to authenticate with Google Cloud. Please check your credentials.", ex);
        }
    }

    private string BuildApiUrl()
    {
        return $"https://{_config.Location}-aiplatform.googleapis.com/v1/projects/{_config.ProjectId}/locations/{_config.Location}/publishers/google/models/{_config.ModelName}:generateContent";
    }

    private object BuildRequestPayload(VertexAIRequest request)
    {
        return new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = request.Prompt }
                    }
                }
            },
            systemInstruction = !string.IsNullOrEmpty(request.SystemInstruction) ? new
            {
                parts = new[]
                {
                    new { text = request.SystemInstruction }
                }
            } : null,
            generationConfig = new
            {
                temperature = _config.Temperature,
                topP = _config.TopP,
                topK = _config.TopK,
                maxOutputTokens = _config.MaxOutputTokens
            }
        };
    }

    private VertexAIResponse ProcessSuccessResponse(string responseContent)
    {
        dynamic? parsedResponse = JsonConvert.DeserializeObject(responseContent);

        string content = "";
        int tokensUsed = 0;

        if (parsedResponse?.candidates != null)
        {
            var candidates = parsedResponse.candidates as Newtonsoft.Json.Linq.JArray;
            if (candidates != null && candidates.Count > 0)
            {
                var candidate = candidates[0];
                var contentParts = candidate?["content"]?["parts"] as Newtonsoft.Json.Linq.JArray;
                if (contentParts != null && contentParts.Count > 0)
                {
                    content = contentParts[0]?["text"]?.ToString() ?? "";
                }
            }
        }

        // Extract token usage if available
        if (parsedResponse?.usageMetadata?.totalTokenCount != null)
        {
            if (int.TryParse(parsedResponse.usageMetadata.totalTokenCount.ToString(), out int tokens))
            {
                tokensUsed = tokens;
            }
        }

        return new VertexAIResponse
        {
            Content = content,
            Success = true,
            TokensUsed = tokensUsed
        };
    }

    private VertexAIResponse ProcessErrorResponse(HttpResponseMessage response, string responseContent)
    {
        _logger.LogError("Vertex AI API request failed with status {StatusCode}: {ResponseContent}",
            response.StatusCode, responseContent);

        return new VertexAIResponse
        {
            Content = "",
            Success = false,
            ErrorMessage = $"API request failed: {response.StatusCode} - {responseContent}"
        };
    }
    #endregion

    #region Conversion Helper Methods
    private static string BuildConversionOptionsText(ConversionOptions? options)
    {
        if (options == null) return "";

        return $@"
Conversion preferences:
- Method syntax preferred: {options.PreferMethodSyntax}
- Optimize for performance: {options.OptimizePerformance}";
    }

    private static string BuildConversionPrompt(string csharpCode, string optionsText, string agentContext)
    {
        return $@"Convert this C# code to LINQ equivalent:

```csharp
{csharpCode}
```

{optionsText}{agentContext}

Converted LINQ code:";
    }

    private ConvertCodeResponse CreateSuccessfulConversionResponse(VertexAIResponse aiResponse, string csharpCode)
    {
        var analysis = AnalyzeCSharpCode(csharpCode);
        var suggestions = GenerateConversionSuggestions(csharpCode);

        return new ConvertCodeResponse
        {
            LinqCode = CleanCodeResponse(aiResponse.Content),
            Success = true,
            Message = "C# code converted to LINQ successfully using custom converter agent",
            Analysis = analysis,
            Suggestions = suggestions
        };
    }

    private ConvertCodeResponse CreateFailedConversionResponse(VertexAIResponse aiResponse, string csharpCode)
    {
        _logger.LogError("Code converter agent conversion failed: {Error}", aiResponse.ErrorMessage);

        return new ConvertCodeResponse
        {
            LinqCode = $"// Code converter agent failed: {aiResponse.ErrorMessage}\n// Original code:\n{csharpCode}",
            Success = false,
            Message = $"Code converter agent failed: {aiResponse.ErrorMessage}",
            Analysis = AnalyzeCSharpCode(csharpCode),
            Suggestions = GenerateConversionSuggestions(csharpCode)
        };
    }
    #endregion

    #region Explanation Helper Methods
    private static string BuildExplanationPrompt(string csharpCode, bool includeLinqEquivalent, string agentContext)
    {
        return $@"Analyze and explain this C# code in detail:

```csharp
{csharpCode}
```

Please provide:
1. **Code Analysis**: What does this code do?
2. **Structure Breakdown**: Key components and flow
3. **Performance Considerations**: Efficiency and optimization opportunities
4. **Best Practices**: Code quality observations
{(includeLinqEquivalent ? "5. **LINQ Equivalent**: How this could be written using LINQ" : "")}
6. **Learning Points**: Key concepts demonstrated{agentContext}

Provide a comprehensive but concise analysis.";
    }

    private async Task<ExplainCodeResponse> CreateExplanationResponse(VertexAIResponse aiResponse, string csharpCode, bool includeLinqEquivalent)
    {
        var analysis = AnalyzeCSharpCode(csharpCode);
        string? linqEquivalent = null;

        if (includeLinqEquivalent)
        {
            var linqResponse = await ConvertCSharpToLinqAsync(csharpCode);
            if (linqResponse.Success)
            {
                linqEquivalent = linqResponse.LinqCode;
            }
        }

        if (aiResponse.Success)
        {
            return new ExplainCodeResponse
            {
                Explanation = aiResponse.Content,
                Success = true,
                Message = "C# code explained successfully using custom code analyzer agent",
                Analysis = analysis,
                LinqEquivalent = linqEquivalent
            };
        }
        else
        {
            _logger.LogError("Code explainer agent analysis failed: {Error}", aiResponse.ErrorMessage);

            return new ExplainCodeResponse
            {
                Explanation = $"Code explainer agent failed to analyze the code: {aiResponse.ErrorMessage}\n\nBasic Analysis:\n- Code Length: {csharpCode.Length} characters\n- Complexity: {analysis.Complexity}\n- Detected Patterns: {string.Join(", ", analysis.DetectedPatterns)}",
                Success = false,
                Message = $"Code explainer agent failed: {aiResponse.ErrorMessage}",
                Analysis = analysis,
                LinqEquivalent = linqEquivalent
            };
        }
    }
    #endregion

    #region Code Analysis Methods
    public CodeAnalysis AnalyzeCSharpCode(string csharpCode)
    {
        var patterns = new List<string>();
        var issues = new List<string>();
        var namespaces = new List<string> { "System.Linq" };

        // Detect common patterns
        if (csharpCode.Contains("for(") || csharpCode.Contains("foreach"))
            patterns.Add("Loops");
        if (csharpCode.Contains("if("))
            patterns.Add("Conditional Logic");
        if (csharpCode.Contains("List<") || csharpCode.Contains("Array"))
            patterns.Add("Collection Operations");
        if (csharpCode.Contains(".Add(") || csharpCode.Contains(".Remove("))
            patterns.Add("Collection Modification");

        // Detect potential issues
        if (csharpCode.Contains("foreach") && csharpCode.Contains("if"))
            issues.Add("Nested loop with condition - could be simplified with LINQ Where");
        if (csharpCode.Split('\n').Length > 20)
            issues.Add("Complex method - consider breaking into smaller LINQ operations");

        return new CodeAnalysis
        {
            Complexity = DetermineComplexity(csharpCode),
            DetectedPatterns = patterns.ToArray(),
            PotentialIssues = issues.ToArray(),
            RequiredNamespaces = namespaces.ToArray(),
            EstimatedPerformanceGain = CalculatePerformanceGain(csharpCode)
        };
    }

    private static string DetermineComplexity(string csharpCode)
    {
        var lines = csharpCode.Split('\n').Length;
        var loopCount = csharpCode.Split(new[] { "for", "foreach", "while" }, StringSplitOptions.None).Length - 1;

        if (lines > ComplexCodeLineThreshold || loopCount > ComplexLoopThreshold)
            return "Complex";

        if (lines > ModerateCodeLineThreshold || loopCount > 0)
            return "Moderate";

        return "Simple";
    }

    private static string[] GenerateConversionSuggestions(string csharpCode)
    {
        var suggestions = new List<string>();

        if (csharpCode.Contains("foreach"))
            suggestions.Add("Consider using Select() for transformations");
        if (csharpCode.Contains("if") && csharpCode.Contains("foreach"))
            suggestions.Add("Use Where() to filter before processing");
        if (csharpCode.Contains(".Count") || csharpCode.Contains(".Length"))
            suggestions.Add("LINQ Count() might be more expressive");
        if (csharpCode.Contains("sum") || csharpCode.Contains("total"))
            suggestions.Add("Consider using Sum() aggregation method");

        return suggestions.ToArray();
    }

    private static int CalculatePerformanceGain(string csharpCode)
    {
        var gain = 0;

        if (csharpCode.Contains("foreach") && csharpCode.Contains("if"))
            gain += 15; // Filtering with Where can be more efficient
        if (csharpCode.Contains("for") && csharpCode.Contains("Add"))
            gain += 10; // Select with ToList() can be optimized
        if (csharpCode.Split('\n').Length > 10)
            gain += 5; // Cleaner LINQ code

        return Math.Min(gain, MaxPerformanceGain);
    }

    private static string CleanCodeResponse(string code)
    {
        var cleaned = code.Trim();

        // Remove markdown code blocks if present
        if (cleaned.StartsWith("```"))
        {
            var lines = cleaned.Split('\n');
            cleaned = string.Join('\n', lines.Skip(1).Take(lines.Length - 2));
        }

        return cleaned.Trim();
    }
    #endregion
}
