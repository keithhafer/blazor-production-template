using System.Data;

namespace BlazorApp.Infrastructure.Data;

/// <summary>
/// Factory interface for creating database connections
/// Abstraction allows easy swapping between different database providers
/// </summary>
public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}
