# Blazor Production App

A production-ready **Blazor Server** application template with clean architecture, modern UI components, and Azure deployment support.

## 🏗️ Architecture

**Blazor Server** was chosen for this template because:

- ✅ **Better for data-heavy applications** - Direct server-side database access with minimal latency
- ✅ **Real-time capabilities** - Built-in SignalR for live updates
- ✅ **Smaller initial payload** - Faster first load compared to WebAssembly
- ✅ **Server-side security** - Business logic stays on the server
- ✅ **Ideal for Azure App Service** - Simple deployment with existing .NET hosting

### Project Structure

```
BlazorProductionApp/
├── src/
│   ├── BlazorApp.Web/              # Blazor Server UI (Presentation Layer)
│   │   ├── Components/
│   │   │   ├── Layout/             # MudBlazor layouts
│   │   │   └── Pages/              # Razor pages/components
│   │   ├── Styles/                 # Tailwind CSS source
│   │   ├── wwwroot/                # Static assets
│   │   ├── Program.cs              # Application entry point & DI configuration
│   │   ├── appsettings.json        # Configuration
│   │   ├── package.json            # Node dependencies (Tailwind)
│   │   └── tailwind.config.js      # Tailwind configuration
│   │
│   ├── BlazorApp.Application/      # Business Logic Layer
│   │   ├── Services/               # Application services
│   │   ├── DTOs/                   # Data transfer objects
│   │   └── DependencyInjection.cs  # Service registration
│   │
│   ├── BlazorApp.Domain/           # Domain Layer (Core)
│   │   ├── Entities/               # Domain entities
│   │   └── Interfaces/             # Repository interfaces
│   │
│   └── BlazorApp.Infrastructure/   # Infrastructure Layer
│       ├── Data/                   # Database connection factory
│       ├── Repositories/           # Dapper repository implementations
│       └── DependencyInjection.cs  # Infrastructure registration
│
├── database/
│   └── schema.sql                  # SQL Server database schema
│
└── BlazorApp.sln                   # Solution file
```

## 🚀 Quick Start

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (LTS)
- [Node.js 18+](https://nodejs.org/) (for Tailwind CSS)
- [SQL Server](https://www.microsoft.com/sql-server) or [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads)
- [VS Code](https://code.visualstudio.com/) with [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)

### 1. Clone and Setup

```bash
cd BlazorProductionApp
dotnet restore
```

### 2. Install Tailwind CSS Dependencies

```bash
cd src/BlazorApp.Web
npm install
```

### 3. Setup Database

**Option A: SQL Server LocalDB (Windows)**

```bash
# Connection string is already configured in appsettings.json
# Run the schema script:
sqlcmd -S "(localdb)\mssqllocaldb" -i ../../database/schema.sql
```

**Option B: SQL Server (Docker)**

```bash
# Start SQL Server in Docker
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong!Password" \
   -p 1433:1433 --name sql_server \
   -d mcr.microsoft.com/mssql/server:2022-latest

# Create database and run schema
docker exec -it sql_server /opt/mssql-tools/bin/sqlcmd \
   -S localhost -U SA -P "YourStrong!Password" \
   -i /database/schema.sql
```

**Option C: Azure SQL Database**

```bash
# Update connection string in appsettings.Production.json or environment variables
# Run schema.sql using Azure Data Studio or SQL Server Management Studio
```

### 4. Build Tailwind CSS

```bash
cd src/BlazorApp.Web
npm run build:css
```

For development with auto-rebuild:

```bash
npm run watch:css
```

### 5. Run the Application

```bash
cd src/BlazorApp.Web
dotnet run
```

Navigate to: `https://localhost:5001`

## 🎨 Technology Stack

| Technology        | Purpose           | Version                           |
| ----------------- | ----------------- | --------------------------------- |
| **.NET**          | Framework         | 8.0 LTS                           |
| **Blazor Server** | UI Framework      | Interactive server-side rendering |
| **MudBlazor**     | Component Library | Material Design components        |
| **Tailwind CSS**  | Utility-First CSS | 3.4+ (prefixed with `tw-`)        |
| **Dapper**        | Micro-ORM         | High-performance data access      |
| **SQL Server**    | Database          | Compatible with Azure SQL         |

## 🎯 Key Features

### Clean Architecture

- **Domain Layer**: Core business entities and interfaces
- **Application Layer**: Business logic and DTOs
- **Infrastructure Layer**: Data access with Dapper
- **Presentation Layer**: Blazor components and pages

### MudBlazor + Tailwind CSS Integration

- **No conflicts**: Tailwind uses `tw-` prefix
- **Preflight disabled**: Preserves MudBlazor's base styles
- **Custom theme**: Complementary color palette
- **Component classes**: Reusable utility combinations

Example:

```razor
<MudCard Class="tw-card-hover">
    <div class="tw-flex tw-gap-4 tw-items-center">
        <span class="tw-text-gradient">Styled Text</span>
    </div>
</MudCard>
```

### Dapper Repository Pattern

- **Interface-based**: Easy to mock for testing
- **Connection factory**: Swap database providers easily
- **Async/await**: All operations are asynchronous
- **Proper disposal**: Using `await using` for connections

Example repository usage:

```csharp
public class ProductRepository : IProductRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<Product>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }
}
```

### Dependency Injection

All layers register services through extension methods:

```csharp
builder.Services.AddApplication();     // Application services
builder.Services.AddInfrastructure();  // Repositories & DB factory
builder.Services.AddMudServices();     // MudBlazor
```

## ☁️ Azure Deployment

### Configuration

The app is configured for environment-based settings:

**Development** (`appsettings.Development.json`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BlazorAppDb;..."
  }
}
```

**Production** (`appsettings.Production.json`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "${SQL_CONNECTION_STRING}"
  }
}
```

### Deploy to Azure App Service

#### 1. Create Azure Resources

```bash
# Login to Azure
az login

# Create resource group
az group create --name rg-blazorapp --location eastus

# Create App Service Plan (Linux)
az appservice plan create \
  --name plan-blazorapp \
  --resource-group rg-blazorapp \
  --sku B1 \
  --is-linux

# Create Web App
az webapp create \
  --name blazorapp-prod-001 \
  --resource-group rg-blazorapp \
  --plan plan-blazorapp \
  --runtime "DOTNETCORE:8.0"

# Create Azure SQL Database
az sql server create \
  --name sql-blazorapp-prod \
  --resource-group rg-blazorapp \
  --location eastus \
  --admin-user sqladmin \
  --admin-password "YourStrong!Password123"

az sql db create \
  --name BlazorAppDb \
  --resource-group rg-blazorapp \
  --server sql-blazorapp-prod \
  --service-objective S0
```

#### 2. Configure Connection String

```bash
# Get SQL connection string
az sql db show-connection-string \
  --server sql-blazorapp-prod \
  --name BlazorAppDb \
  --client ado.net

# Set as App Service configuration
az webapp config connection-string set \
  --name blazorapp-prod-001 \
  --resource-group rg-blazorapp \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="<connection-string-from-above>"
```

#### 3. Deploy Application

**Option A: Using VS Code Azure Extension**

1. Install [Azure App Service extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azureappservice)
2. Right-click on `BlazorApp.Web` → Deploy to Web App
3. Select your App Service

**Option B: Using Azure CLI**

```bash
cd src/BlazorApp.Web

# Publish the app
dotnet publish -c Release -o ./publish

# Create deployment package
cd publish
zip -r ../deploy.zip .

# Deploy to Azure
az webapp deployment source config-zip \
  --name blazorapp-prod-001 \
  --resource-group rg-blazorapp \
  --src ../deploy.zip
```

#### 4. Run Database Migrations

Connect to Azure SQL Database and run `database/schema.sql`:

```bash
# Using Azure Data Studio or SQL Server Management Studio
# Or use sqlcmd:
sqlcmd -S sql-blazorapp-prod.database.windows.net \
       -d BlazorAppDb \
       -U sqladmin \
       -P "YourStrong!Password123" \
       -i database/schema.sql
```

### Environment Variables in Azure

Azure App Service automatically maps:

- **Connection Strings** → `IConfiguration["ConnectionStrings:DefaultConnection"]`
- **App Settings** → `IConfiguration["SettingName"]`

No code changes needed!

## 🛠️ Development Workflow

### VS Code Setup

Recommended extensions:

- [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
- [Tailwind CSS IntelliSense](https://marketplace.visualstudio.com/items?itemName=bradlc.vscode-tailwindcss)
- [Azure App Service](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azureappservice)

### Running in Development

Terminal 1 - Run app:

```bash
cd src/BlazorApp.Web
dotnet watch run
```

Terminal 2 - Watch Tailwind:

```bash
cd src/BlazorApp.Web
npm run watch:css
```

### Building for Production

```bash
# Build Tailwind CSS (minified)
cd src/BlazorApp.Web
npm run build:css

# Build .NET app
cd ../..
dotnet build -c Release

# Publish
dotnet publish src/BlazorApp.Web -c Release -o ./publish
```

## 📁 Adding New Features

### 1. Add a New Entity

**Domain/Entities/Order.cs**:

```csharp
public class Order
{
    public int Id { get; set; }
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
}
```

### 2. Create Repository Interface

**Domain/Interfaces/IOrderRepository.cs**:

```csharp
public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAllAsync(CancellationToken ct = default);
    // ... other methods
}
```

### 3. Implement Repository with Dapper

**Infrastructure/Repositories/OrderRepository.cs**:

```csharp
public class OrderRepository : IOrderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public OrderRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken ct = default)
    {
        await using var conn = await _connectionFactory.CreateConnectionAsync(ct);
        return await conn.QueryAsync<Order>(
            new CommandDefinition("SELECT * FROM Orders", cancellationToken: ct));
    }
}
```

### 4. Register in DI

**Infrastructure/DependencyInjection.cs**:

```csharp
services.AddScoped<IOrderRepository, OrderRepository>();
```

### 5. Create Service Layer

**Application/Services/OrderService.cs**:

```csharp
public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync(CancellationToken ct = default)
    {
        var orders = await _repository.GetAllAsync(ct);
        return orders.Select(MapToDto);
    }
}
```

### 6. Create Blazor Page

**Components/Pages/Orders.razor**:

```razor
@page "/orders"
@inject IOrderService OrderService

<MudTable Items="@_orders">
    <!-- Table implementation -->
</MudTable>
```

## 🔐 Security Best Practices

- ✅ **Connection strings** stored in configuration, not code
- ✅ **Parameterized queries** with Dapper (SQL injection protection)
- ✅ **HTTPS** enforced in production
- ✅ **HSTS** enabled for production
- ✅ **Azure Managed Identity** support (optional enhancement)

## 📊 Performance Optimizations

- ✅ **Dapper**: High-performance data access (faster than EF Core)
- ✅ **Async/await**: Non-blocking I/O operations
- ✅ **Connection pooling**: Automatic with SqlConnection
- ✅ **Response caching**: Can be added via middleware
- ✅ **Tailwind purging**: Minified CSS with unused classes removed

## 🧪 Testing

Add test projects:

```bash
dotnet new xunit -n BlazorApp.Tests
dotnet sln add BlazorApp.Tests/BlazorApp.Tests.csproj
```

Mock repositories for unit testing:

```csharp
var mockRepo = new Mock<IProductRepository>();
mockRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(products);

var service = new ProductService(mockRepo.Object);
```

## 📝 License

This is a template project. Use it for any purpose.

## 🤝 Contributing

This template demonstrates production-ready patterns. Contributions welcome!

---

**Built with ❤️ using .NET 8.0, Blazor Server, MudBlazor, Tailwind CSS, and Dapper**
