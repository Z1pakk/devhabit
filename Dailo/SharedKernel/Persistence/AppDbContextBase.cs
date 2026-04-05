using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SharedKernel.Persistence.Conventions;
using SharedKernel.Persistence.Interceptors;
using SharedKernel.User;

namespace SharedKernel.Persistence;

public abstract class AppDbContextBase(
    DbContextOptions options,
    ICurrentUserService? currentUserService = null,
    TimeProvider? timeProvider = null
) : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new VersionInterceptor());

        // Only add audit interceptor if dependencies are available (runtime scenario)
        if (currentUserService is not null && timeProvider is not null)
        {
            optionsBuilder.AddInterceptors(new AuditInterceptor(currentUserService, timeProvider));
        }

        base.OnConfiguring(optionsBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // Order matters: Pluralization must run before SnakeCaseNaming
        configurationBuilder.Conventions.Add(_ => new TablePluralizationConvention());
        configurationBuilder.Conventions.Add(_ => new DefaultStringLengthConvention());
        configurationBuilder.Conventions.Add(_ => new SoftDeleteConvention());
    }

    public async Task ExecuteTransactionalAsync(
        Func<Task> action,
        CancellationToken cancellationToken = default
    )
    {
        var strategy = CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken
            );
            try
            {
                await action();

                await SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public Task<T> ExecuteTransactionalAsync<T>(
        Func<Task<T>> action,
        CancellationToken cancellationToken = default
    )
    {
        var strategy = CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken
            );
            try
            {
                var result = await action();

                await SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    private IExecutionStrategy CreateExecutionStrategy() => Database.CreateExecutionStrategy();
}
