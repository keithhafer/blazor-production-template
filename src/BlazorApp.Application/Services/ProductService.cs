using BlazorApp.Application.DTOs;
using BlazorApp.Domain.Entities;
using BlazorApp.Domain.Interfaces;

namespace BlazorApp.Application.Services;

/// <summary>
/// Service implementation for Product business logic
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);
        return products.Select(MapToDto);
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        return product != null ? MapToDto(product) : null;
    }

    public async Task<int> CreateProductAsync(ProductDto productDto, CancellationToken cancellationToken = default)
    {
        var product = MapToEntity(productDto);
        product.CreatedAt = DateTime.UtcNow;
        product.IsActive = true;
        
        return await _productRepository.CreateAsync(product, cancellationToken);
    }

    public async Task<bool> UpdateProductAsync(ProductDto productDto, CancellationToken cancellationToken = default)
    {
        var product = MapToEntity(productDto);
        product.UpdatedAt = DateTime.UtcNow;
        
        return await _productRepository.UpdateAsync(product, cancellationToken);
    }

    public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _productRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.SearchAsync(searchTerm, cancellationToken);
        return products.Select(MapToDto);
    }

    // Pure mapping functions
    private static ProductDto MapToDto(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        StockQuantity = product.StockQuantity,
        Category = product.Category,
        CreatedAt = product.CreatedAt,
        UpdatedAt = product.UpdatedAt,
        IsActive = product.IsActive
    };

    private static Product MapToEntity(ProductDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Description = dto.Description,
        Price = dto.Price,
        StockQuantity = dto.StockQuantity,
        Category = dto.Category,
        CreatedAt = dto.CreatedAt,
        UpdatedAt = dto.UpdatedAt,
        IsActive = dto.IsActive
    };
}
