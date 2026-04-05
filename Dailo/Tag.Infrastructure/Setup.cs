using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tag.Application.Persistence;
using Tag.Infrastructure.Database;

namespace Tag.Infrastructure;

public static class Setup
{
    public const string TagDbConnectionString = "TagPostgresConnectionString";

    public static IServiceCollection AddTagModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString = configuration.GetConnectionString(TagDbConnectionString);

        services.AddDbContext<ITagDbContext, TagDbContext>(opt =>
            opt.UseNpgsql(
                    connectionString,
                    b =>
                    {
                        b.MigrationsAssembly(AssemblyReference.Assembly)
                            .MigrationsHistoryTable(
                                HistoryRepository.DefaultTableName,
                                TagSchema.NAME
                            );
                        // Enable retry on failure for transient errors
                        b.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorCodesToAdd: null
                        );

                        // Set command timeout for long-running queries
                        b.CommandTimeout(60);
                    }
                )
                .UseSnakeCaseNamingConvention()
        );

        return services;
    }
}
