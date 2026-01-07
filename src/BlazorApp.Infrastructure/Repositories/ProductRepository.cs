using BlazorApp.Domain.Entities;
using BlazorApp.Domain.Interfaces;
using BlazorApp.Infrastructure.Data;
using Dapper;

namespace BlazorApp.Infrastructure.Repositories;

/// <summary>
/// Dapper implementation of Product repository
/// Demonstrates production-ready patterns: async, cancellation tokens, proper resource disposal
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProductRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, Name, Description, Price, StockQuantity, Category, CreatedAt, UpdatedAt, IsActive
            FROM Products
            WHERE IsActive = 1
            ORDER BY Name";

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QueryAsync<Product>(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, Name, Description, Price, StockQuantity, Category, CreatedAt, UpdatedAt, IsActive
            FROM Products
            WHERE Id = @Id";

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Product>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<int> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO Products (Name, Description, Price, StockQuantity, Category, CreatedAt, IsActive)
            VALUES (@Name, @Description, @Price, @StockQuantity, @Category, @CreatedAt, @IsActive);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleAsync<int>(
            new CommandDefinition(sql, product, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Products
            SET Name = @Name,
                Description = @Description,
                Price = @Price,
                StockQuantity = @StockQuantity,
                Category = @Category,
                UpdatedAt = @UpdatedAt,
                IsActive = @IsActive
            WHERE Id = @Id";

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var affectedRows = await connection.ExecuteAsync(
            new CommandDefinition(sql, product, cancellationToken: cancellationToken));
        
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        // Soft delete - set IsActive to false
        const string sql = @"
            UPDATE Products
            SET IsActive = 0,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var affectedRows = await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id, UpdatedAt = DateTime.UtcNow }, cancellationToken: cancellationToken));
        
        return affectedRows > 0;
    }

    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, Name, Description, Price, StockQuantity, Category, CreatedAt, UpdatedAt, IsActive
            FROM Products
            WHERE IsActive = 1
                AND (Name LIKE @SearchTerm OR Description LIKE @SearchTerm OR Category LIKE @SearchTerm)
            ORDER BY Name";

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QueryAsync<Product>(
            new CommandDefinition(sql, new { SearchTerm = $"%{searchTerm}%" }, cancellationToken: cancellationToken));
    }
}
