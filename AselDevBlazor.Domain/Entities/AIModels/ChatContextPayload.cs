
using TurnstileManagementSystem.Data.Models.AIModels;

namespace AselDevBlazor.Domain.Entities.AIModels;

public class ChatContextPayload
{
    // ── Detected intent info ─────────────────────────────────────────
    public List<string> Intents { get; set; } = new();
    public string RawUserMessage { get; set; } = string.Empty;

    // ── eHR data ─────────────────────────────────────────────────────
    public List<HREmployeeList>? Employees { get; set; }

    // ── Attendance data ───────────────────────────────────────────────
    public List<AttendanceRecord>? AttendanceRecords { get; set; }

    // ── Attendance summary (pre-computed for AI context) ──────────────
    public AttendanceSummaryForAI? AttendanceSummaryF { get; set; }


    // ── Status ───────────────────────────────────────────────────────
    public bool HasData => Employees?.Any() == true;
    // || AttendanceRecords?.Any() == true
    // || LeaveRecords?.Any() == true
    // || ManhourRecords?.Any() == true;

    public string? ErrorMessage { get; set; }


    // ── Pre-computed attendance summary passed to AI ──────────────────────
    // Keeps token usage low — AI gets counts, not raw rows
    public class AttendanceSummaryForAI
    {
        
        public string DateFrom { get; set; } = string.Empty;
        public string DateTo { get; set; } = string.Empty;
        public int TotalDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LateDays { get; set; }
        public int OnLeaveDays { get; set; }
        public int TotalRegularHours { get; set; }
        public int TotalOvertimeHours { get; set; }
        public List<AttendanceDayEntry> DailyEntries { get; set; } = new();
        public bool HasMoreRecords { get; set; }
        public int TotalRecordCount { get; set; }

    }

    public class AttendanceDayEntry
    {
        public string Name { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? TimeIn { get; set; }
        public string? TimeOut { get; set; }
        public int? RegularHours { get; set; }
        public int? Overtime { get; set; }

        public string? LeaveCode { get; set; }
        public int? DayType { get; set; }
    }
}
