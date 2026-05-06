using AselDevBlazor.Application;
using AselDevBlazor.Infrastructure;
using AselDevBlazor.Infrastructure.Auth;
using AselDevBlazor.Infrastructure.Data;
using AselDevBlazor.Web.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using MudBlazor.Services;
using Serilog;
using System.Reflection;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Net;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting AselDevBlazor application...");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ──
    builder.Services.AddSerilog((services, config) =>
        config.ReadFrom.Configuration(builder.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext());

    // ── Blazor ──
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    // ── MudBlazor ──
    builder.Services.AddMudServices();

    // ── Controllers ──
    builder.Services.AddControllers();

    // ── Swagger ──
    // ── Swagger ──
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "AselDev Enterprise API",
            Version = "v1",
            Description = "Enterprise API for AselDev services"
        });

        // ── JWT Authentication ──
        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "Enter JWT token like: Bearer {your token}",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,   // ✅ FIX (more correct than ApiKey)
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    });

    // ── Cascading Auth ──
    builder.Services.AddCascadingAuthenticationState();

    // ── Application + Infrastructure ──
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);


    // ════════════════════════════════════
    var app = builder.Build();
    // ════════════════════════════════════



    // ── Auto migrate + seed ──
    if (app.Configuration.GetValue("Database:RunStartupTasks", true))
    {
        try
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Database migration failed");
            throw;
        }

        using (var scope = app.Services.CreateScope())
        {
            await IdentitySeeder.SeedAsync(scope.ServiceProvider);
        }
    }
    else
    {
        Log.Information("Database startup tasks are disabled.");
    }

    // ── Middleware pipeline ──
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "AselDev API v1");
            c.RoutePrefix = "swagger";
        });
    }
    else
    {
        app.UseExceptionHandler("/system-error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();    // ← must be before UseAuthorization
    app.UseAuthorization();
    app.UseAntiforgery();

    app.MapControllers();
    app.MapGet("/system-error", () => Results.Content(
        BuildSystemErrorPage(
            "Runtime Error",
            "An unhandled error occurred while processing the request. Check the application logs for the full exception details.",
            app.Environment.EnvironmentName,
            app.Environment.ContentRootPath),
        "text/html"));

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    var server = app.Services.GetRequiredService<IServer>();
    var addresses = server.Features.Get<IServerAddressesFeature>();

    if (addresses != null)
    {
        foreach (var address in addresses.Addresses)
        {
            Log.Information("Now listening on: {Address}", address);
        }
    }

    Log.Information("AselDevBlazor started successfully.");
    app.Run();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("========== FATAL ERROR ==========");
    Console.WriteLine(ex.ToString());
    Console.WriteLine("=================================");
    Console.ResetColor();
    Console.ResetColor();

    Log.Fatal(ex, "Application terminated unexpectedly.");
    await RunStartupErrorPageAsync(args, ex);
}
finally
{
    Log.CloseAndFlush();
}

static async Task RunStartupErrorPageAsync(string[] args, Exception startupException)
{
    try
    {
        var fallbackBuilder = WebApplication.CreateBuilder(args);
        fallbackBuilder.WebHost.UseSetting(WebHostDefaults.PreventHostingStartupKey, "true");

        var fallbackApp = fallbackBuilder.Build();
        var html = BuildSystemErrorPage(
            "Startup Error",
            startupException.ToString(),
            fallbackApp.Environment.EnvironmentName,
            fallbackApp.Environment.ContentRootPath);

        fallbackApp.MapGet("/", () => Results.Content(html, "text/html"));
        fallbackApp.MapGet("/startup-error", () => Results.Content(html, "text/html"));

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Startup fallback UI is running. Open the configured application URL to view the error.");
        Console.ResetColor();

        await fallbackApp.RunAsync();
    }
    catch (Exception fallbackException)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("========== FALLBACK UI ERROR ==========");
        Console.WriteLine(fallbackException.ToString());
        Console.WriteLine("=======================================");
        Console.ResetColor();
    }
}

static string BuildSystemErrorPage(
    string title,
    string detail,
    string environmentName,
    string contentRootPath)
{
    var encodedTitle = WebUtility.HtmlEncode(title);
    var encodedDetail = WebUtility.HtmlEncode(detail);
    var encodedEnvironment = WebUtility.HtmlEncode(environmentName);
    var encodedContentRoot = WebUtility.HtmlEncode(contentRootPath);
    var encodedTimestamp = WebUtility.HtmlEncode(DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz"));

    return $$"""
<!doctype html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>{{encodedTitle}} - AselDevBlazor</title>
    <style>
        :root {
            --blue: #0046ad;
            --blue-dark: #002d72;
            --line: #cdd8eb;
            --text: #172033;
            --muted: #536174;
            --danger: #b42318;
            --danger-bg: #fff1f0;
        }

        * {
            box-sizing: border-box;
        }

        body {
            margin: 0;
            background: #f7faff;
            color: var(--text);
            font-family: Arial, Helvetica, sans-serif;
            line-height: 1.5;
        }

        main {
            display: grid;
            min-height: 100vh;
            place-items: center;
            padding: 24px;
        }

        .error-shell {
            width: min(100%, 980px);
            background: #ffffff;
            border: 1px solid var(--line);
            border-top: 6px solid var(--danger);
            border-radius: 2px;
            box-shadow: 0 18px 42px rgba(23, 32, 51, .08);
        }

        .header {
            border-bottom: 1px solid var(--line);
            padding: 22px 24px;
        }

        .eyebrow {
            color: var(--danger);
            font-size: 12px;
            font-weight: 800;
            letter-spacing: .08em;
            margin-bottom: 8px;
            text-transform: uppercase;
        }

        h1 {
            color: var(--blue-dark);
            font-size: clamp(28px, 4vw, 42px);
            line-height: 1.1;
            margin: 0;
        }

        .body {
            display: grid;
            gap: 16px;
            padding: 20px 24px 24px;
        }

        .meta {
            display: grid;
            gap: 10px;
            grid-template-columns: repeat(3, minmax(0, 1fr));
        }

        .meta div,
        pre {
            background: #f8fbff;
            border: 1px solid var(--line);
            border-radius: 2px;
        }

        .meta div {
            padding: 12px;
        }

        .meta strong {
            color: var(--blue-dark);
            display: block;
            font-size: 12px;
            margin-bottom: 4px;
            text-transform: uppercase;
        }

        .meta span {
            color: var(--muted);
            overflow-wrap: anywhere;
        }

        pre {
            color: var(--danger);
            max-height: 460px;
            margin: 0;
            overflow: auto;
            padding: 16px;
            white-space: pre-wrap;
        }

        .hint {
            background: var(--danger-bg);
            border-left: 5px solid var(--danger);
            color: #7a271a;
            padding: 12px 14px;
        }

        @media (max-width: 760px) {
            .meta {
                grid-template-columns: 1fr;
            }

            main {
                padding: 12px;
            }
        }
    </style>
</head>
<body>
    <main>
        <section class="error-shell">
            <div class="header">
                <div class="eyebrow">Application Diagnostics</div>
                <h1>{{encodedTitle}}</h1>
            </div>
            <div class="body">
                <div class="hint">
                    The normal application could not complete startup. This fallback page is intentionally shown so setup issues are visible on new devices.
                </div>
                <div class="meta">
                    <div><strong>Environment</strong><span>{{encodedEnvironment}}</span></div>
                    <div><strong>Content Root</strong><span>{{encodedContentRoot}}</span></div>
                    <div><strong>Timestamp</strong><span>{{encodedTimestamp}}</span></div>
                </div>
                <pre>{{encodedDetail}}</pre>
            </div>
        </section>
    </main>
</body>
</html>
""";
}
