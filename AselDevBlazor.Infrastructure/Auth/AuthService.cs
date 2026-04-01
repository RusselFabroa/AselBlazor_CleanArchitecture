using AselDevBlazor.Application.Common;
using AselDevBlazor.Application.Common.Interfaces.AuthServices;
using AselDevBlazor.Application.Features.Auth;
using AselDevBlazor.Application.Features.Auth.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AselDevBlazor.Infrastructure.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config,
            ILogger<AuthService> logger,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
            _logger = logger;
            _signInManager = signInManager;
        }

        public async Task<ServiceResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
        {
            try
            {
                _logger.LogInformation("Login attempt: {Email}", dto.Email);

                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user is null || !user.IsActive)
                {
                    _logger.LogWarning("Login failed — not found: {Email}", dto.Email);
                    return new ServiceResponse<AuthResponseDto>("Invalid email or password.", 401);
                }

                var isValid = await _userManager.CheckPasswordAsync(user, dto.Password);
                if (!isValid)
                {
                    _logger.LogWarning("Login failed — wrong password: {Email}", dto.Email);
                    return new ServiceResponse<AuthResponseDto>("Invalid email or password.", 401);
                }

                var roles = await _userManager.GetRolesAsync(user);
                var token = GenerateJwtToken(user, roles);

                _logger.LogInformation("Login successful: {Email}", dto.Email);

                return new ServiceResponse<AuthResponseDto>(new AuthResponseDto
                {
                    Token = token.Token,
                    Email = user.Email!,
                    FullName = user.FullName,
                    Role = roles.FirstOrDefault() ?? "User",
                    ExpiresAt = token.ExpiresAt
                }, "Login successful", 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoginAsync failed: {Email}", dto.Email);
                return new ServiceResponse<AuthResponseDto>($"Login Error: {ex.Message}", 500);
            }
        }

        public async Task<ServiceResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto)
        {
            try
            {
                var exists = await _userManager.FindByEmailAsync(dto.Email);
                if (exists is not null)
                    return new ServiceResponse<AuthResponseDto>("Email already registered.", 400);

                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FullName = dto.FullName,
                    Department = dto.Department,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    _logger.LogWarning("Register failed: {Errors}", string.Join(", ", errors));
                    return new ServiceResponse<AuthResponseDto>("Registration failed.", 400);
                }

                var role = dto.Role ?? "User";
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));

                await _userManager.AddToRoleAsync(user, role);

                var roles = await _userManager.GetRolesAsync(user);
                var token = GenerateJwtToken(user, roles);

                _logger.LogInformation("User registered: {Email}", dto.Email);

                return new ServiceResponse<AuthResponseDto>(new AuthResponseDto
                {
                    Token = token.Token,
                    Email = user.Email!,
                    FullName = user.FullName,
                    Role = role,
                    ExpiresAt = token.ExpiresAt
                }, "Registration successful", 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RegisterAsync failed: {Email}", dto.Email);
                return new ServiceResponse<AuthResponseDto>($"Error: {ex.Message}", 500);
            }
        }

        public async Task<ServiceResponse> AssignRoleAsync(string userId, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user is null)
                    return ServiceResponse.NotFound("User not found.");

                // Remove existing roles first
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));

                await _userManager.AddToRoleAsync(user, role);

                _logger.LogInformation("Role assigned — User: {UserId} | Role: {Role}", userId, role);
                return ServiceResponse.Ok($"Role '{role}' assigned.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AssignRoleAsync failed");
                return ServiceResponse.Error($"Error: {ex.Message}");
            }
        }

        public async Task<ServiceResponse> CreateRoleAsync(string roleName)
        {
            try
            {
                if (await _roleManager.RoleExistsAsync(roleName))
                    return ServiceResponse.Error($"Role '{roleName}' already exists.");

                await _roleManager.CreateAsync(new IdentityRole(roleName));
                _logger.LogInformation("Role created: {Role}", roleName);
                return ServiceResponse.Error($"Role '{roleName}' created.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateRoleAsync failed");
                return ServiceResponse.Error($"Error: {ex.Message}");
            }
        }

        // ── JWT Generator ──
        private (string Token, DateTime ExpiresAt) GenerateJwtToken(
            ApplicationUser user, IEnumerable<string> roles)
        {
            var jwtSettings = _config.GetSection("JwtSettings").Get<JwtSettings>()!;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryInMinutes);

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email,          user.Email!),
            new Claim(ClaimTypes.Name,           user.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}
