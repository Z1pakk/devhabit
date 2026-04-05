using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedKernel.Persistence;

namespace Dailo.Infrastructure.Database;

public static class DatabaseExtensions
{
    /// <summary>
    /// Register all seeders as a hosted service.
    /// </summary>
    public static IHostApplicationBuilder AddDatabaseSeeding(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHostedService<DatabaseSeedingService>();
        return builder;
    }
}

internal sealed class DatabaseSeedingService(IServiceProvider services) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var seeders = scope.ServiceProvider.GetServices<IDataSeeder>();

        foreach (var seeder in seeders)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await seeder.SeedAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
