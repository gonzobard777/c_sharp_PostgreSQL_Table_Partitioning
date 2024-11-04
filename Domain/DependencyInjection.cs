using Domain.Contract;
using Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomainDependencies(this IServiceCollection services)
    {
        services.AddScoped<IPuansonService, PuansonService>();
        // services.AddScoped<PuansonService>();

        return services;
    }
}