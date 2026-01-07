using BlazorApp.Application.DTOs;

namespace BlazorApp.Application.Services;

/// <summary>
/// Service interface for Product business logic
/// </summary>
public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default);
    Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateProductAsync(ProductDto product, CancellationToken cancellationToken = default);
    Task<bool> UpdateProductAsync(ProductDto product, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm, CancellationToken cancellationToken = default);
}
