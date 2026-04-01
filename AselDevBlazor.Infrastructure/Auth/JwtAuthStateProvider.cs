using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AselDevBlazor.Infrastructure.Auth;
public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    private ClaimsPrincipal _anonymous =>
        new ClaimsPrincipal(new ClaimsIdentity());

    public JwtAuthStateProvider(IJSRuntime js)
    {
        _js = js;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            string? token = null;

            try
            {
                token = await _js.InvokeAsync<string?>(
                    "aselAuth.getToken",
                    TimeSpan.FromSeconds(3));
            }
            catch (InvalidOperationException)
            {
                // JS not available during prerendering
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

            // ── Check expiry ──
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
        catch
        {
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
            Console.WriteLine($"SetTokenAsync error: {ex.Message}");
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
        catch
        {
            return null;
        }
    }

    public async Task RemoveTokenAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("aselAuth.removeToken");
        }
        catch { }

        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(_anonymous)));
    }
}

