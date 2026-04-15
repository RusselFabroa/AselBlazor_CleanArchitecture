

using AselDevBlazor.Domain.Entities.AIModels;
using AselDevBlazor.Domain.Entities.AIModels.QueryResult;

namespace AselDevBlazor.Application.Common.Interfaces.AI;

public interface IChatBotDataService
{
    // ── eHR ──────────────────────────────────────────────────────────
    Task<EHRQueryResult> GetEmployeesAsync(ChatIntentParameter parameters);

    Task<EHRQueryResult> GetEmployeesAsyncImproved(ChatIntentParameter parameters);

    // ── Attendance ───────────────────────────────────────────────────
    Task<AttendanceQueryResult> GetAttendanceAsync(ChatIntentParameter parameters);

    // ── Attendance ───────────────────────────────────────────────────
    // Task<AttendanceQueryResult> GetAttendanceAsync(ChatIntentParameter parameters);

    // ── Leave ────────────────────────────────────────────────────────
    // Task<LeaveQueryResult> GetLeaveAsync(ChatIntentParameter parameters);

    // ── Manhour ──────────────────────────────────────────────────────
    // Task<ManhourQueryResult> GetManhourAsync(ChatIntentParameter parameters);
}
