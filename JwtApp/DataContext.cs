using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using JwtApp.Models;

namespace JwtApp;

public class DataContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<JwtToken> JwtTokens { get; set; }

    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    #region Create Database

    public static void CreateDatabase(WebApplication application)
    {
        using (var scope = application.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<DataContext>();

            if (context != null)
            {
                var exists = (context.GetService<IDatabaseCreator>() as RelationalDatabaseCreator)?.Exists() ?? false;

                if (false == exists)
                {
                    context.Database.EnsureCreated();
                } 
            }
        }
    }

    #endregion

    // public string Path { get; }

    // public DataContext()
    // {
    //     var folder = Environment.SpecialFolder.LocalApplicationData;
    //     var path = Environment.GetFolderPath(folder);

    //     this.Path = System.IO.Path.Join(path, "db.sqlite");

    //     System.Console.WriteLine(this.Path);
    // }

    // protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={this.Path}");
}