using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AselDevBlazor.Domain.Entities.AIModels.Ollama;


/// <summary>
/// Represents a single message in a conversation.
/// Role must be: "system" | "user" | "assistant"
/// </summary>
public class OllamaChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// The request payload sent to the Ollama /api/chat endpoint.
/// </summary>
public class OllamaChatRequest
{
    public string Model { get; set; } = "qwen3:7b";
    public string? SystemMessage { get; set; }
    public List<OllamaChatMessage> ChatHistory { get; set; } = new();
    public string UserMessage { get; set; } = string.Empty;
    public double Temperature { get; set; } = 0.6;
    public int TopK { get; set; } = 20;
    public double TopP { get; set; } = 0.95;
    public double RepeatPenalty { get; set; } = 1.0;
    public int NumPredict { get; set; } = 512;
    public int NumCtx { get; set; } = 4096;
    public bool Think { get; set; } = false;
}
