using TurnstileManagementSystem.Data.Models.AIModels;

namespace AselDevBlazor.Domain.Entities.AIModels.QueryResult;

public class AttendanceQueryResult
{
    public bool Success { get; set; }
    public List<AttendanceRecord> Records { get; set; } = new();
    public int TotalCount { get; set; }
    public string? ErrorMessage { get; set; }

    
    public bool HasData => Success && Records.Count > 0;

    // ── Summary stats for AI context ─────────────────────────────────
    public int PresentCount => Records.Count(r => r.IsPresent);
    public int AbsentCount => Records.Count(r => r.IsAbsent);
    public int LateCount => Records.Count(r => r.IsLate);
    public int OnLeaveCount => Records.Count(r => r.Status == "On Leave");

    public static AttendanceQueryResult Ok(List<AttendanceRecord> records) => new()
    {
        Success = true,
        Records = records,
        TotalCount = records.Count
    };

    public static AttendanceQueryResult Empty() => new()
    {
        Success = true,
        Records = new(),
        TotalCount = 0
    };

    public static AttendanceQueryResult Fail(string error) => new()
    {
        Success = false,
        ErrorMessage = error
    };
}
