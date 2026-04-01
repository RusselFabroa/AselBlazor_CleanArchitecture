using AselDevBlazor.Application.Common.Interfaces.AuthServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AselDevBlazor.Infrastructure.Auth
{
    public class AuthGuardService : IAuthGuardService
    {
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly NavigationManager _navManager;
        private readonly ILogger<AuthGuardService> _logger;

        public AuthGuardService(
            AuthenticationStateProvider authStateProvider,
            NavigationManager navManager,
            ILogger<AuthGuardService> logger)
        {
            _authStateProvider = authStateProvider;
            _navManager = navManager;
            _logger = logger;
        }

        public async Task<bool> EnsureAuthorizedAsync(
            List<string>? roles = null,
            string? urlReturn = null)
        {
            try
            {
                // ── 1. Get token ──
                var jwtProvider = (JwtAuthStateProvider)_authStateProvider;
                var token = await jwtProvider.GetTokenAsync();

                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogWarning("AuthGuard — No token found, redirecting to login");
                    var returnUrl = urlReturn ?? _navManager.Uri;
                    _navManager.NavigateTo(
                        $"/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
                    return false;
                }

                // ── 2. Get auth state ──
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (user?.Identity == null || !user.Identity.IsAuthenticated)
                {
                    _logger.LogWarning("AuthGuard — User not authenticated, redirecting to login");
                    var returnUrl = urlReturn ?? _navManager.Uri;
                    _navManager.NavigateTo(
                        $"/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
                    return false;
                }

                // ── 3. Role check — only if roles provided ──
                if (roles != null && roles.Any())
                {
                    bool isInAnyRole = roles.Any(role =>
                        user.IsInRole(role.Trim()));

                    if (!isInAnyRole)
                    {
                        _logger.LogWarning(
                            "AuthGuard — User {User} not in required roles: {Roles}",
                            user.Identity.Name,
                            string.Join(", ", roles));

                        _navManager.NavigateTo("/unauthorized");
                        return false;
                    }
                }

                _logger.LogInformation(
                    "AuthGuard — Access granted: {User}",
                    user.Identity.Name);

                return true; // ✅ Authorized
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AuthGuard — Unexpected error");
                _navManager.NavigateTo("/login");
                return false;
            }
        }
    }
}
