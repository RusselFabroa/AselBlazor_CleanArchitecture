using AselDevBlazor.Domain.Entities.AIModels.QueryResult;

namespace AselDevBlazor.Domain.Entities.AIModels;

public class ChatEntry
{
    public int Id { get; set; } 
    public string Role { get; set; } = string.Empty; // "system", "user", "assistant"
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsStreaming { get; set; } = false;
    public bool IsError { get; set; } = false;

    // ── Populated when AI finds multiple name matches ─────────────────
    public List<EmployeeCandidate>? Candidates { get; set; }

    // ── Universal table — works for any intent ────────────────────────
    public TablePayload? Table { get; set; }
}
