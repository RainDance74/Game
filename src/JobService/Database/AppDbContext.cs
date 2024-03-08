using System.Reflection;

using JobService.Database.Models;

using Microsoft.EntityFrameworkCore;

namespace JobService.Database;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Job> Jobs => Set<Job>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
