namespace Converter.API.Services;

public interface IConfigurationValidationService
{
    (bool IsValid, string ErrorMessage) ValidateVertexAIConfiguration();
}