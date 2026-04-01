using AselDevBlazor.Application.Common.Interfaces;
using AselDevBlazor.Domain.Entities;
using AselDevBlazor.Domain.Entities.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AselDevBlazor.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemperatureController : ControllerBase
    {
        private readonly ILogger<TemperatureController> _logger;
        private readonly ITemperatureServices _tempService;
        public TemperatureController(ILogger<TemperatureController> logger, ITemperatureServices tempService)
        {
            _logger = logger;

            _tempService = tempService;
        }

        [HttpPost("PostTemperature")]
        public async Task<ActionResult<APIResponse<TemperatureData>>> PostTemperature(TemperatureData data)
        {
            try
            {
                //Philppine time is UTC+8, so we can set the timestamp to UTC time and it will be consistent regardless of where the server is located.
                data.Timestamp = DateTime.UtcNow.AddHours(8);
                var createdRecord = await _tempService.CreateAsync(data);
                var response = new APIResponse<TemperatureData>(createdRecord.Data, "Temperature record created successfully!", 201);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating temperature record.{ex.Message}");
                var errorResponse = new APIResponse<TemperatureData>($"Failed to create temperature record. {ex.Message}", 500);
                return Ok(errorResponse);
            }

        }
    }
}
