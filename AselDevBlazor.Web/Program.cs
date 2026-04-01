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
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }

    using (var scope = app.Services.CreateScope())
    {
        await IdentitySeeder.SeedAsync(scope.ServiceProvider);
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
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();    // ← must be before UseAuthorization
    app.UseAuthorization();
    app.UseAntiforgery();

    app.MapControllers();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    Log.Information("AselDevBlazor started successfully.");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"FATAL ERROR: {ex}");
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}