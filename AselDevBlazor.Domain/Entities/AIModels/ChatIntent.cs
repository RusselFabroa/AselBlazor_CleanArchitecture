namespace AselDevBlazor.Domain.Entities.AIModels;

public class ChatIntent
{
    public List<string> Intents { get; set; } = new List<string>();
    public ChatIntentParameter Parameters { get; set; } = new ChatIntentParameter();
    public bool IsHrRelated { get; set; } = false;

    public bool NeedsClarification { get; set; } = false;
    public string? ClarificationQuestion { get; set; }
    public string RawUserMessage { get; set; } = string.Empty;
}
