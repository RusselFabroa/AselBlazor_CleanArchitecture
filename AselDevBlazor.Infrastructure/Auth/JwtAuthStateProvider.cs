using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AselDevBlazor.Infrastructure.Auth;

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    private readonly ILogger<JwtAuthStateProvider> _logger;
    private ClaimsPrincipal _anonymous =>
        new(new ClaimsIdentity());

    public JwtAuthStateProvider(IJSRuntime js, ILogger<JwtAuthStateProvider> logger)
    {
        _js = js;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            string? token;

            try
            {
                token = await _js.InvokeAsync<string?>(
                    "aselAuth.getToken",
                    TimeSpan.FromSeconds(3));
            }
            catch (InvalidOperationException)
            {
                return new AuthenticationState(_anonymous);
            }
            catch (JSException)
            {
                return new AuthenticationState(_anonymous);
            }
            catch (TaskCanceledException)
            {
                return new AuthenticationState(_anonymous);
            }

            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(_anonymous);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            if (jwt.ValidTo.ToUniversalTime() < DateTime.UtcNow)
            {
                await RemoveTokenAsync();
                return new AuthenticationState(_anonymous);
            }

            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(jwt.Claims, "jwt"));

            return new AuthenticationState(principal);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read authentication state from token storage.");
            return new AuthenticationState(_anonymous);
        }
    }

    public async Task SetTokenAsync(string token)
    {
        try
        {
            await _js.InvokeVoidAsync("aselAuth.setToken", token);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(jwt.Claims, "jwt"));

            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(principal)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store authentication token.");
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await _js.InvokeAsync<string?>(
                "aselAuth.getToken",
                TimeSpan.FromSeconds(3));
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "No authentication token is available.");
            return null;
        }
    }

    public async Task RemoveTokenAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("aselAuth.removeToken");
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to remove authentication token from browser storage.");
        }

        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(_anonymous)));
    }
}
