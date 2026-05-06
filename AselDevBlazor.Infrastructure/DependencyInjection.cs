//using AselDevBlazor.Infrastructure.Logging;
//using AselDevBlazor.Application.Common.Interfaces;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Serilog;
//using AselDevBlazor.Infrastructure.Data;
//using AselDevBlazor.Application.Features.Attendance.Services;
//using AselDevBlazor.Application.Features.Temperature;

//public static class DependencyInjection
//{
//    public static IServiceCollection AddInfrastructure(
//        this IServiceCollection services,
//        IConfiguration configuration)
//    {
//        // ... your existing registrations ...

//        // Serilog
//        Log.Logger = LoggingConfiguration.CreateLogger(configuration);
//        services.AddLogging(loggingBuilder =>
//            loggingBuilder.AddSerilog(dispose: true));

//        services.AddScoped<IDbContextFactory, DbContextFactory>();
//        services.AddScoped<IEmpAttendanceService, EmpAttendanceService>();
//        services.AddScoped<ITemperatureServices, TemperatureService>();
//        return services;
//    }
//}


using AselDevBlazor.Application.Common.Interfaces;

using AselDevBlazor.Application.Common.Interfaces.AuthServices;

using AselDevBlazor.Application.Features.Auth;

using AselDevBlazor.Domain.Entities;

using AselDevBlazor.Infrastructure.Auth;
using AselDevBlazor.Infrastructure.Data;

using AselDevBlazor.Infrastructure.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Pomelo.EntityFrameworkCore.MySql;
using Serilog;
using System.Text;

namespace AselDevBlazor.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── 1. Serilog ──
        Log.Logger = LoggingConfiguration.CreateLogger(configuration);
        services.AddLogging(loggingBuilder =>
            loggingBuilder.AddSerilog(dispose: true));

        // ── 2. Database — dynamic provider ──
        var provider = configuration["DynamicConnectionStrings:DefaultConnection:Provider"];
        var connectionString = configuration["DynamicConnectionStrings:DefaultConnection:ConnectionString"];

        if (string.IsNullOrWhiteSpace(provider))
            throw new InvalidOperationException("Database provider is not configured.");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string is not configured.");

        services.AddDbContext<AppDbContext>(options =>
        {
            switch (provider.ToLower().Trim())
            {
                case "mysql":
                    options.UseMySql(connectionString,
                        new MySqlServerVersion(new Version(8, 0, 0)));
                    break;

                case "sqlserver":
                //case "mssql":
                //    options.UseSqlServer(connectionString, sql =>
                //        sql.EnableRetryOnFailure(maxRetryCount: 3));
                //    break;

                case "postgresql":
                //case "postgres":
                //    options.UseNpgsql(connectionString, pg =>
                //        pg.EnableRetryOnFailure(maxRetryCount: 3));
                //    break;

                //case "sqlite":
                //    options.UseSqlite(connectionString);
                //    break;

                default:
                    throw new NotSupportedException(
                        $"Provider '{provider}' is not supported.");
            }

            Log.Information("Database configured — Provider: {Provider}", provider);
        });


        // ── 3. Identity ──
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // ── 4. JWT ──
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings not configured.");

        var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };
        });



        services.AddAuthorization();


        // ── Auth State Provider ──
        services.AddScoped<JwtAuthStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(
            sp => sp.GetRequiredService<JwtAuthStateProvider>());

       
        services.AddScoped<IAuthService, AuthService>();

        // ── 5. DbContext Factory ──
        services.AddSingleton<IDbContextFactory, DbContextFactory>();

        
      
        services.AddScoped<IAuthGuardService, AuthGuardService>();

     

        return services;
    }
}
