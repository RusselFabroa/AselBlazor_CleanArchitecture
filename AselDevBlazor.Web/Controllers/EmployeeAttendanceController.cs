using AselDevBlazor.Application.Common.Interfaces;
using AselDevBlazor.Domain.Entities;
using AselDevBlazor.Domain.Entities.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AselDevBlazor.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeAttendanceController : ControllerBase
    {
        private readonly ILogger<EmployeeAttendanceController> _logger;
        private readonly IEmpAttendanceService _attendanceService;
        public EmployeeAttendanceController(ILogger<EmployeeAttendanceController> logger, IEmpAttendanceService empAttendanceService)
        {
            _logger = logger;
            _attendanceService = empAttendanceService;
        }

        [HttpGet]
        public async Task<ActionResult<APIResponse<List<EmpAttendanceData>>>> GetAll()
        {
            try
            {
                var records = await _attendanceService.GetAllAsync();
                var response = new APIResponse<List<EmpAttendanceData>>(records.Data, "Succesfully Retrieved!", 200);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching attendance records.");
                var errorResponse = new APIResponse<List<EmpAttendanceData>>("Failed to retrieve attendance records.", 500);
                return Ok(errorResponse);
            }
        }

        [HttpPost("PostAttendance")]
        public async Task<ActionResult<APIResponse<EmpAttendanceData>>> PostAttendance(EmpAttendanceData data)
        {
            try
            {
                var createdRecord = await _attendanceService.CreateAsync(data);
                var response = new APIResponse<EmpAttendanceData>(createdRecord.Data, "Attendance record created successfully!", 201);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating attendance record.{ex.Message}");
                var errorResponse = new APIResponse<EmpAttendanceData>($"Failed to create attendance record. {ex.Message}", 500);
                return Ok(errorResponse);
            }
        }
    }
}
