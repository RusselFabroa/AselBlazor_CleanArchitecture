namespace AselDevBlazor.Domain.Entities.AIModels;

public class TablePayload
{
    public string Type { get; set; } = string.Empty;   // "attendance" | "leave" | "manhour" | "ehr"
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = new();
    public List<Dictionary<string, string>> Rows { get; set; } = new();
    public Dictionary<string, string> Summary { get; set; } = new();
    public bool HasMore { get; set; }
    public int TotalCount { get; set; }
}
