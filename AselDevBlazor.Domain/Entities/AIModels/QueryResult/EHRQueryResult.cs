

namespace AselDevBlazor.Domain.Entities.AIModels.QueryResult;
public class EmployeeCandidate
{
    public string EmployeeId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
}
 
public class EHRQueryResult
{
    public bool Success { get; set; }
    public List<HREmployeeList> Employees { get; set; } = new();
    public int TotalCount { get; set; }
    public string? ErrorMessage { get; set; }
    public bool HasData => Success && Employees.Count > 0;
 
    // ── Disambiguation ───────────────────────────────────────────────
    public bool NeedsDisambiguation { get; set; }
    public List<EmployeeCandidate> Candidates { get; set; } = new();
 
    public static EHRQueryResult Ok(List<HREmployeeList> employees) => new()
    {
        Success    = true,
        Employees  = employees,
        TotalCount = employees.Count
    };
 
    public static EHRQueryResult Disambiguate(List<EmployeeCandidate> candidates) => new()
    {
        Success              = true,
        NeedsDisambiguation  = true,
        Candidates           = candidates,
        TotalCount           = candidates.Count
    };
 
    public static EHRQueryResult Empty() => new()
    {
        Success    = true,
        Employees  = new(),
        TotalCount = 0
    };
 
    public static EHRQueryResult Fail(string error) => new()
    {
        Success      = false,
        ErrorMessage = error
    };
}