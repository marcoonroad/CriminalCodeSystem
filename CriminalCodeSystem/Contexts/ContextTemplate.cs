using System;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;

namespace CriminalCodeSystem.Contexts;

public partial class ContextTemplate : DbContext
{
    protected string ConnectionString { get; set; }

    protected ContextTemplate()
    {
        ConnectionString = UserConfig.Credentials["MYSQL_DB_CONNECTION"] ?? "";
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            bool forcedInMemoryStorage = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FORCE_IN_MEMORY_STORAGE"));
            if (string.IsNullOrEmpty(ConnectionString) || forcedInMemoryStorage) {
                options.UseInMemoryDatabase("CriminalCodeSystem");
                return;
            }

            var serverVersion = new MySqlServerVersion(ServerVersion.AutoDetect(ConnectionString));
            
            options
                .UseMySql(ConnectionString, serverVersion)
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableDetailedErrors();
        }
    }

    protected override void OnModelCreating(ModelBuilder model)
    {

    }
}