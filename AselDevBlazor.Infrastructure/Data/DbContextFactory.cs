// Infrastructure/Data/DbContextFactory.cs
using AselDevBlazor.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace AselDevBlazor.Infrastructure.Data;

public class DbContextFactory : IDbContextFactory
{
    private readonly IConfiguration _config;

    public DbContextFactory(IConfiguration config)
    {
        _config = config;
    }

   public DbContext CreateDbContext(string connectionName)
    {
        var section          = _config.GetSection($"DynamicConnectionStrings:{connectionName}");
        var provider         = section["Provider"];
        var connectionString = section["ConnectionString"];

        if (string.IsNullOrWhiteSpace(provider))
            throw new InvalidOperationException($"Provider missing for '{connectionName}'.");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"ConnectionString missing for '{connectionName}'.");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        switch (provider.ToLower().Trim())
        {
            case "sqlserver":
            //case "mssql":
            //    optionsBuilder.UseSqlServer(connectionString);
            //    break;
            case "mysql":
                optionsBuilder.UseMySql(connectionString,
                    ServerVersion.AutoDetect(connectionString));
                break;
            case "postgresql":
            //case "postgres":
            //    optionsBuilder.UseNpgsql(connectionString);
            //    break;
            //case "sqlite":
            //    optionsBuilder.UseSqlite(connectionString);
            //    break;
            default:
                throw new NotSupportedException($"Provider '{provider}' not supported.");
        }

        Log.Information("DbContext created — Connection: {ConnectionName} | Provider: {Provider}",
            connectionName, provider);

        return new AppDbContext(optionsBuilder.Options);
    }

   
}