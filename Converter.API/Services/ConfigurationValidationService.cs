using Converter.API.Models;
using Microsoft.Extensions.Options;

namespace Converter.API.Services;

public class ConfigurationValidationService : IConfigurationValidationService
{
    private readonly VertexAIConfiguration _config;
    private readonly ILogger<ConfigurationValidationService> _logger;

    public ConfigurationValidationService(
        IOptions<VertexAIConfiguration> config,
        ILogger<ConfigurationValidationService> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public (bool IsValid, string ErrorMessage) ValidateVertexAIConfiguration()
    {
        var errors = new List<string>();

        // Validate required fields
        if (string.IsNullOrWhiteSpace(_config.ProjectId))
        {
            errors.Add("ProjectId is required");
        }

        if (string.IsNullOrWhiteSpace(_config.Location))
        {
            errors.Add("Location is required");
        }

        if (string.IsNullOrWhiteSpace(_config.ModelName))
        {
            errors.Add("ModelName is required");
        }

        // Validate numeric ranges
        if (_config.Temperature < 0.0 || _config.Temperature > 1.0)
        {
            errors.Add("Temperature must be between 0.0 and 1.0");
        }

        if (_config.TopP < 0.0 || _config.TopP > 1.0)
        {
            errors.Add("TopP must be between 0.0 and 1.0");
        }

        if (_config.TopK < 1)
        {
            errors.Add("TopK must be greater than 0");
        }

        if (_config.MaxOutputTokens < 1 || _config.MaxOutputTokens > 8192)
        {
            errors.Add("MaxOutputTokens must be between 1 and 8192");
        }

        // Validate service account key path if provided
        if (!string.IsNullOrEmpty(_config.ServiceAccountKeyPath))
        {
            if (!File.Exists(_config.ServiceAccountKeyPath))
            {
                errors.Add($"Service account key file not found: {_config.ServiceAccountKeyPath}");
            }
            else
            {
                try
                {
                    var content = File.ReadAllText(_config.ServiceAccountKeyPath);
                    if (!content.Contains("private_key") || !content.Contains("client_email"))
                    {
                        errors.Add("Service account key file appears to be invalid");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Cannot read service account key file: {ex.Message}");
                }
            }
        }

        // Validate supported models
        var supportedModels = new[]
        {
            "gemini-1.5-flash-001",
            "gemini-1.5-pro-001",
            "gemini-1.0-pro-001",
            "text-bison-001",
            "text-bison-002"
        };

        if (!supportedModels.Contains(_config.ModelName))
        {
            _logger.LogWarning("Using unsupported model: {ModelName}. Supported models: {SupportedModels}",
                _config.ModelName, string.Join(", ", supportedModels));
        }

        if (errors.Any())
        {
            var errorMessage = "Vertex AI configuration validation failed:\n" + string.Join("\n", errors.Select(e => $"- {e}"));
            _logger.LogError("Configuration validation failed: {Errors}", errorMessage);
            return (false, errorMessage);
        }

        _logger.LogInformation("Vertex AI configuration validation successful");
        return (true, "Configuration is valid");
    }
}