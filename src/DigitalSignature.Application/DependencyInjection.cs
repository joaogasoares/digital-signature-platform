using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalSignature.Application;

public static class DependencyInjection
{
    public static readonly Assembly Assembly = typeof(DependencyInjection).Assembly;

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
