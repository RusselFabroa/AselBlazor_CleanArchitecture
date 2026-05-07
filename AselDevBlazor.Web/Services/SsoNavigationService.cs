using AselDevBlazor.Application.Features.Auth;
using Microsoft.AspNetCore.WebUtilities;

namespace AselDevBlazor.Web.Services
{
    public class SsoNavigationService
    {
        private readonly SsoSettings _settings;

        public SsoNavigationService(IConfiguration configuration)
        {
            _settings = configuration.GetSection("Sso").Get<SsoSettings>() ?? new SsoSettings();
        }

        public SsoSettings Settings => _settings;

        public string BuildLoginUrl(string returnUrl)
        {
            var loginUrl = string.IsNullOrWhiteSpace(_settings.LoginUrl)
                ? "/login"
                : _settings.LoginUrl;

            return QueryHelpers.AddQueryString(loginUrl, "urlReturn", returnUrl);
        }
    }
}
