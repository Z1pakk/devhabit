using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.CQRS;

namespace Dailo.Infrastructure.Mediator;

public static class MediatorSetup
{
    public static Assembly[] GetHandlerAssemblies(this IServiceCollection services) =>
        services
            .Where(sd => sd.ServiceType == typeof(HandlerAssembly))
            .Select(sd => ((HandlerAssembly)sd.ImplementationInstance!).Value)
            .ToArray();
}
