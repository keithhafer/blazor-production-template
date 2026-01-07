using BlazorApp.Domain.Interfaces;
using BlazorApp.Infrastructure.Data;
using BlazorApp.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorApp.Infrastructure;

/// <summary>
/// Extension methods for configuring Infrastructure services
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Register database connection factory
        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
        
        // Register repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        
        return services;
    }
}
