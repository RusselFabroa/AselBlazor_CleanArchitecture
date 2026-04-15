using AselDevBlazor.Domain.Entities.AIModels;
using OpenAI.Chat;



namespace AselDevBlazor.Application.Common.Interfaces.AI;   

public interface IAIChatService
{
    IAsyncEnumerable<string> StreamResponseAsync(List<ChatMessage> messages);
    List<ChatMessage> InitializeConversation(string? systemOverride = null);


    // ── Intent detection — now accepts history for context ───────────
    Task<ChatIntent> DetectIntentAsync(string userMessage, List<ChatMessage>? conversationHistory = null);

    // ── Final response using real DB data ────────────────────────────
    IAsyncEnumerable<string> BuildResponseAsync(List<ChatMessage> history, ChatContextPayload payload);
}
