namespace Converter.API.Models;

public class VertexAIConfiguration
{
    public string ProjectId { get; set; } = string.Empty;

    public string Location { get; set; } = "us-central1";

    public string ModelName { get; set; } = "gemini-1.5-flash-001";

    public string ServiceAccountKeyPath { get; set; } = string.Empty;

    public int MaxOutputTokens { get; set; } = 1024;

    public double Temperature { get; set; } = 0.7;

    public double TopP { get; set; } = 0.9;

    public int TopK { get; set; } = 40;

    // Agent Framework Settings (always enabled)

    public int MaxAgentRounds { get; set; } = 3;

    public bool EnableMultiAgentConversation { get; set; } = false;

    public string AgentWorkingDirectory { get; set; } = string.Empty;
}

public class VertexAIRequest
{
    public string Prompt { get; set; } = string.Empty;

    public string SystemInstruction { get; set; } = string.Empty;
}

public class VertexAIResponse
{
    public string Content { get; set; } = string.Empty;

    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public int TokensUsed { get; set; }
}