using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Template.Infrastructure.DbService;

public class SqliteContext<T>(FileInfo location) : DbContext where T : class
{
    internal DbSet<T> Result { get; set; }
    internal FileInfo Location { get; set; } = location;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // Ensure the directory exists before trying to create the db file
        if (!Location.Directory!.Exists)
            Location.Directory.Create();

        options.UseSqlite($"Data Source={Location.FullName}");
    }
}
