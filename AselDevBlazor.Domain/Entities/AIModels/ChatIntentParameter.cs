using System.Drawing;

namespace AselDevBlazor.Domain.Entities.AIModels;

public class ChatIntentParameter
{
    public string? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? Department { get; set; }
    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }
    public string? Keyword { get; set; }
}
