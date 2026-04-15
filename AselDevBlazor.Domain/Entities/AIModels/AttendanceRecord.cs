namespace TurnstileManagementSystem.Data.Models.AIModels;
public class AttendanceRecord
{
    public string? EmployeeId { get; set; }
    public string? FullName { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? Shift { get; set; }
    public DateTime? LogDate { get; set; }
    public DateTime? TimeIn { get; set; }
    public DateTime? TimeOut { get; set; }
    public int? RegularHours { get; set; }
    public int? Overtime { get; set; }
    public int? LeaveHours { get; set; }
    public string? LeaveCode { get; set; }

    public int? DayType { get; set; }

    // ── Derived status ───────────────────────────────────────────────
    public string Status
    {
        get
        {
            // On leave
            if (!string.IsNullOrWhiteSpace(LeaveCode) && LeaveHours > 0)
                return "On Leave";

            // Absent — no login and no leave
            if (TimeIn is null && (LeaveHours is null or 0))
                return "Inactive";

            // Late — login after shift start + 5 min grace
            if (TimeIn is not null && Shift is not null)
            {
                var shiftStart = ParseShiftStart(Shift, LogDate);
                if (shiftStart.HasValue && TimeIn > shiftStart.Value.AddMinutes(5))
                    return "Late";
            }

            return "Active";
        }
    }

    public bool IsLate => Status == "Late";
    public bool IsAbsent => Status == "Inactive";
    public bool IsPresent => Status == "Active";

    // ── Parse shift start time from shift code ────────────────────────
    // Adjust this logic to match your actual shift codes
    private static DateTime? ParseShiftStart(string shift, DateTime? logDate)
    {
        if (logDate is null) return null;

        // Common shift patterns — extend as needed
        var shiftMap = new Dictionary<string, TimeSpan>(StringComparer.OrdinalIgnoreCase)
        {
            { "DAY",   new TimeSpan(8, 0, 0) },
            { "MID",   new TimeSpan(14, 0, 0) },
            { "NIGHT", new TimeSpan(22, 0, 0) },
            { "AM",    new TimeSpan(6, 0, 0) },
            { "PM",    new TimeSpan(14, 0, 0) },
        };

        // Try exact match first
        foreach (var key in shiftMap.Keys)
        {
            if (shift.Contains(key, StringComparison.OrdinalIgnoreCase))
                return logDate.Value.Date + shiftMap[key];
        }

        // Try to parse time directly from shift string e.g. "0800", "08:00"
        var cleaned = shift.Replace(":", "").Trim();
        if (cleaned.Length == 4 && int.TryParse(cleaned, out var t))
        {
            var h = t / 100;
            var m = t % 100;
            return logDate.Value.Date + new TimeSpan(h, m, 0);
        }

        return null;
    }
}
