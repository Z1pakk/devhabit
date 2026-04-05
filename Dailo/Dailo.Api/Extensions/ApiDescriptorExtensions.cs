using System.Reflection;

namespace Dailo.Api.Extensions;

public static class ApiDescriptorExtensions
{
    public static bool IsOpenApiExecution(this WebApplicationBuilder? builder)
    {
        return builder != null
            && Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";
    }
}
