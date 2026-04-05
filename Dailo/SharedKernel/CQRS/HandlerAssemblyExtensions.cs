using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace SharedKernel.CQRS;

/// <summary>
/// Marker record for mediator handler assemblies.
/// </summary>
/// <param name="Value"></param>
public sealed record HandlerAssembly(Assembly Value);

public static class HandlerAssemblyExtensions
{
    /// <summary>
    /// Adds a handler assembly to the DI container to register all handlers in the assembly.
    /// </summary>
    public static IServiceCollection AddHandlerAssembly<TMarker>(this IServiceCollection services)
    {
        services.AddSingleton(new HandlerAssembly(typeof(TMarker).Assembly));
        return services;
    }
}
