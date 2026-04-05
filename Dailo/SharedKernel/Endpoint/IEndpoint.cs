using Mediator;

namespace SharedKernel.Endpoint;

public interface IEndpoint<in TRequest, TResponse> : IEndpointBase
{
    Task<TResponse> HandleAsync(
        TRequest request,
        ISender sender,
        CancellationToken cancellationToken = default
    );
}
