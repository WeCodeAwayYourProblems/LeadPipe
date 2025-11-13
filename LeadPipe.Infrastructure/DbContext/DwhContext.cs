using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.DbContext;

internal class DwhContext<T>(string connectionStr) : DbContext where T : class
{
    internal DbSet<T> Result { get; set; }
    internal string ConnectionString { get; set; } = connectionStr;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseMySQL(ConnectionString);
    }
}
