using AselDevBlazor.Application.Common;
using AselDevBlazor.Application.Common.Interfaces.AuthServices;
using AselDevBlazor.Application.Features.Auth.DTOs;
using AselDevBlazor.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace AselDevBlazor.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(IAuthService authService, ILogger<AuthController> logger, SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            Log.Information("Login attempt for email: {Email}", dto.Email);


            if (user == null || !user.IsActive)
                return Unauthorized("Invalid credentials");

            var result = await _signInManager.PasswordSignInAsync(
                user,
                dto.Password,
                isPersistent: false,
                lockoutOnFailure: false);

            if (!result.Succeeded)
                return Unauthorized("Invalid credentials");

            return Ok(new { message = "Login success" });
        }

        [HttpPost("loginv2")]
        public async Task<IActionResult> Loginv2([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            Log.Information("Login attempt for email: {Email}", dto.Email);


            if (user == null || !user.IsActive)
                return Unauthorized("Invalid credentials");

            var result = await _signInManager.PasswordSignInAsync(
                user,
                dto.Password,
                isPersistent: false,
                lockoutOnFailure: false);

            if (!result.Succeeded)
                return Unauthorized("Invalid credentials");

            return Ok(new { message = "Login success" });
        }

        // 🔥 VERSION 2 — JWT LOGIN (API STYLE)
        [HttpPost("LoginVersion2")]
        public async Task<ActionResult<ServiceResponse<AuthResponseDto>>> LoginVersion2([FromBody] LoginDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new ServiceResponse<AuthResponseDto>("Invalid request", 400));

                var result = await _authService.LoginAsync(dto);

                // 🔷 Return proper HTTP status
                if (!result.Success)
                {
                    _logger.LogWarning("LoginVersion2 failed: {Message}", result.Message);
                    return StatusCode(result.StatusCode, result);
                }

                _logger.LogInformation("LoginVersion2 success: {Email}", dto.Email);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoginVersion2 exception");

                return StatusCode(500, new ServiceResponse<AuthResponseDto>(
                    "Internal server error", 500));
            }
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var response = await _authService.RegisterAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            var response = await _authService.AssignRoleAsync(userId, role);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("create-role")]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            var response = await _authService.CreateRoleAsync(roleName);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }
    }
}
