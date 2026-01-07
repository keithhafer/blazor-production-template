using BlazorApp.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorApp.Application;

/// <summary>
/// Extension methods for configuring Application services
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<IProductService, ProductService>();
        
        return services;
    }
}
