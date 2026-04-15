

namespace AselDevBlazor.Domain.Entities.AIModels;

public class ChatSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public string EmployeeId { get; set; } = string.Empty;
    public List<ChatEntry> Entries { get; set; } = new();  // ← was Messages
    public DateTime StartedAt { get; set; } = DateTime.Now;
    public DateTime LastActivityAt { get; set; } = DateTime.Now;
    public int TotalTokensUsed { get; set; } = 0;
}
