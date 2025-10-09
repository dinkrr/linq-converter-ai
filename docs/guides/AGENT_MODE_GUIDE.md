# Custom Agent-Based Architecture Guide

The VertexAI Service uses a **custom agent-based** approach exclusively, providing specialized AI agents for different code conversion and analysis tasks. This system is always active and provides consistent, domain-specific responses.

## Overview

This service has been designed from the ground up to use a custom agent-based processing architecture exclusively. All code conversion and explanation tasks are handled by specialized agents that provide consistent, domain-specific responses.

## Configuration

The service is now always in "custom agent mode" and requires minimal configuration:

```json
{
  "VertexAI": {
    "ProjectId": "csharp-linq-converter",
    "Location": "us-central1",
    "ModelName": "gemini-2.0-flash-lite-001",
    "Temperature": 0.3,
    "MaxOutputTokens": 2048,
    "TopP": 0.9,
    "TopK": 40,
    "MaxAgentRounds": 3,
    "EnableMultiAgentConversation": false,
    "AgentWorkingDirectory": ""
  }
}
```

### Configuration Options

| Property | Description | Default |
|----------|-------------|---------|
| `MaxAgentRounds` | Maximum conversation rounds for multi-agent scenarios | `3` |
| `EnableMultiAgentConversation` | Allow agents to collaborate on complex tasks | `false` |
| `AgentWorkingDirectory` | Working directory for agent operations | `""` |

## Available Agents

### 1. CodeConverter Agent
**Purpose**: Convert traditional C# code to LINQ expressions

**Specializations**:
- Loop to LINQ method conversion (for/foreach → Select, Where, etc.)
- Conditional logic optimization (if statements → Where clauses)
- Aggregation operations (manual loops → Sum, Count, Average)
- Performance-focused transformations
- Method syntax preference with proper using statements

**Usage**: Automatically activated when calling `ConvertCSharpToLinqAsync()`

### 2. CodeExplainer Agent
**Purpose**: Analyze and explain C# code for educational purposes

**Specializations**:
- Comprehensive code structure analysis
- Performance implications assessment
- Best practices identification
- LINQ conversion opportunity detection
- Educational explanations with clear sections

**Usage**: Automatically activated when calling `ExplainCSharpCodeAsync()`

### 3. PerformanceAnalyzer Agent
**Purpose**: Analyze code for performance bottlenecks and optimization opportunities

**Specializations**:
- Loop efficiency analysis
- Memory usage pattern identification
- Parallel processing opportunities
- LINQ deferred execution optimization
- Performance metrics and recommendations

**Usage**: Available for future implementation and multi-agent conversations

## Key Features

### ✅ Always-On Custom Agent System
- No configuration switches needed
- Consistent agent-based responses
- Specialized prompts for each task type

### ✅ Simplified Architecture
- Removed dual-mode complexity
- Eliminated fallback mechanisms
- Streamlined codebase

### ✅ Enhanced Agent Prompts
- Domain-specific system instructions
- Consistent response formatting
- Clear agent responsibilities

### ✅ Improved Error Handling
- Agent-specific error messages
- Graceful degradation within agent context
- Detailed logging for debugging

## Example Usage

```csharp
// CodeConverter Agent will handle this automatically
var conversionResult = await vertexAIService.ConvertCSharpToLinqAsync(csharpCode);

// Response message will indicate the agent used
Console.WriteLine(conversionResult.Message); 
// Output: "C# code converted to LINQ successfully using custom converter agent"

// CodeExplainer Agent will handle this automatically  
var explanationResult = await vertexAIService.ExplainCSharpCodeAsync(csharpCode);

// Response message will indicate the agent used
Console.WriteLine(explanationResult.Message);
// Output: "C# code explained successfully using custom code analyzer agent"
```

## Agent Response Format

### CodeConverter Agent
- Returns clean, compilable LINQ code
- Includes necessary using statements
- Provides method syntax preference
- Adds explanatory comments for complex conversions

### CodeExplainer Agent
- Structured analysis with clear sections
- Educational explanations with bullet points
- Performance considerations
- Best practices recommendations
- LINQ conversion opportunities

## Error Handling

When an agent fails to process a request, the service provides:
- Agent-specific error messages
- Basic code analysis fallback
- Preserved analysis metadata
- Clear indication of failure reason

Example error response:
```
"Code converter agent failed: API timeout"
"Code explainer agent failed: Invalid API key"
```

## Migration Notes

This application was built with a custom agent-based architecture from the beginning:

1. ✅ **Architecture**: Custom agent-based processing is the core design principle
2. ✅ **Behavior**: All requests use specialized agents automatically
3. ✅ **Responses**: Consistent agent-powered responses with clear attribution
4. ✅ **Performance**: Optimized execution path with agent specialization

## Future Enhancements

- **Multi-Agent Conversations**: Enable agents to collaborate on complex tasks
- **Custom Agent Creation**: Support for domain-specific agent templates
- **Agent Performance Metrics**: Detailed analytics and optimization insights
- **Advanced Agent Framework**: Integration with emerging agent orchestration platforms