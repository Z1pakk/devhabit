namespace SharedKernel.Persistence;

public interface IAppDbContextBase
{
    string Schema { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task ExecuteTransactionalAsync(
        Func<Task> action,
        CancellationToken cancellationToken = default
    );

    Task<T> ExecuteTransactionalAsync<T>(
        Func<Task<T>> action,
        CancellationToken cancellationToken = default
    );
}
