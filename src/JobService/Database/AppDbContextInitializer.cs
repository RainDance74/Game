using Microsoft.EntityFrameworkCore;

namespace JobService.Database;

public static class InitialiserExtensions
{
    public static async Task InitializeDatabaseAsync(this IHost host)
    {
        using IServiceScope scope = host.Services.CreateScope();

        AppDbContextInitializer initializer = scope.ServiceProvider.GetRequiredService<AppDbContextInitializer>();

        await initializer.InitializeAsync();
    }
}

public class AppDbContextInitializer(
    ILogger<AppDbContextInitializer> logger, AppDbContext context)
{
    private readonly ILogger<AppDbContextInitializer> _logger = logger;
    private readonly AppDbContext _context = context;

    public async Task InitializeAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }
}
