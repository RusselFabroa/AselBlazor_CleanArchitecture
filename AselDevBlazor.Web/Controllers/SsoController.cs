using AselDevBlazor.Application.Common;
using AselDevBlazor.Application.Common.Interfaces.AuthServices;
using AselDevBlazor.Application.Features.Auth;
using AselDevBlazor.Application.Features.Auth.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AselDevBlazor.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SsoController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly JwtSettings _jwtSettings;
        private readonly SsoSettings _ssoSettings;
        private readonly ILogger<SsoController> _logger;

        public SsoController(
            IAuthService authService,
            IConfiguration configuration,
            ILogger<SsoController> logger)
        {
            _authService = authService;
            _jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
                ?? new JwtSettings();
            _ssoSettings = configuration.GetSection("Sso").Get<SsoSettings>() ?? new SsoSettings();
            _logger = logger;
        }

        [HttpGet(".well-known")]
        [AllowAnonymous]
        public ActionResult<SsoConfigurationDto> GetConfiguration()
        {
            if (!_ssoSettings.IsIdentityProvider)
                return NotFound();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            return Ok(new SsoConfigurationDto
            {
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                TokenEndpoint = $"{baseUrl}/api/sso/token",
                UserInfoEndpoint = $"{baseUrl}/api/sso/me"
            });
        }

        [HttpPost("token")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResponse<AuthResponseDto>>> Token([FromBody] LoginDto dto)
        {
            if (!_ssoSettings.IsIdentityProvider)
                return NotFound();

            if (dto is null)
                return BadRequest(new ServiceResponse<AuthResponseDto>("Invalid request.", 400));

            var response = await _authService.LoginAsync(dto);
            if (!response.Success)
            {
                _logger.LogWarning("SSO token request failed for {LoginId}: {Message}",
                    dto.UsernameOrEmployeeId,
                    response.Message);

                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }

        [HttpGet("me")]
        [Authorize]
        public ActionResult<ServiceResponse<SsoUserDto>> Me()
        {
            var user = User;

            var roles = user.FindAll(ClaimTypes.Role)
                .Select(role => role.Value)
                .Distinct()
                .ToList();

            var dto = new SsoUserDto
            {
                UserId = FindClaimValue(user, ClaimTypes.NameIdentifier, JwtRegisteredClaimNames.Sub),
                EmployeeId = FindClaimValue(user, "employee_id"),
                UserName = FindClaimValue(user, "preferred_username", "employee_id"),
                FullName = FindClaimValue(user, ClaimTypes.Name, JwtRegisteredClaimNames.Name),
                Email = FindClaimValue(user, ClaimTypes.Email, JwtRegisteredClaimNames.Email),
                Department = FindClaimValue(user, "department"),
                Roles = roles
            };

            return Ok(new ServiceResponse<SsoUserDto>(dto, "Authenticated user", 200));
        }

        private static string FindClaimValue(ClaimsPrincipal user, params string[] claimTypes)
        {
            foreach (var claimType in claimTypes)
            {
                var value = user.FindFirst(claimType)?.Value;
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return string.Empty;
        }
    }
}
