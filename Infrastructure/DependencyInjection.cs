using Domain.Contract;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
    {
        services.AddDbContext<MyDbContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}