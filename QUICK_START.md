# Getting Started - Quick Reference

## 🚀 Immediate Setup (5 minutes)

### 1. Open in VS Code

```bash
cd ~/BlazorProductionApp
code .
```

### 2. Install Tailwind CSS

```bash
cd src/BlazorApp.Web
npm install
npm run build:css
```

### 3. Build and Run

```bash
cd ../..
dotnet run --project src/BlazorApp.Web
```

Navigate to: **https://localhost:5001**

---

## 📊 What You'll See

Without database setup, you'll see:

- ✅ **Home Page** - Working with all features showcased
- ⚠️ **Products Page** - Will show connection error (expected until database is configured)
- ✅ **Counter/Weather** - Standard Blazor demo pages

---

## 🗄️ Database Setup (Optional - to see Products page work)

### Quick Start with Docker

```bash
# Start SQL Server
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong!Password" \
   -p 1433:1433 --name sql_blazor \
   -d mcr.microsoft.com/mssql/server:2022-latest

# Wait 10 seconds for SQL Server to start
sleep 10

# Create database and run schema
docker exec -i sql_blazor /opt/mssql-tools/bin/sqlcmd \
   -S localhost -U SA -P "YourStrong!Password" \
   -Q "CREATE DATABASE BlazorAppDb"

# Run schema script
docker exec -i sql_blazor /opt/mssql-tools/bin/sqlcmd \
   -S localhost -U SA -P "YourStrong!Password" \
   -d BlazorAppDb < database/schema.sql
```

Connection string is already configured in `appsettings.Development.json`.

---

## 🎨 VS Code Development

### Recommended Extensions

The project includes extension recommendations. Accept the prompt to install:

- C# Dev Kit
- Tailwind CSS IntelliSense
- Azure App Service

### Run Tasks

Press `Cmd+Shift+P` (macOS) or `Ctrl+Shift+P` (Windows/Linux):

- **Tasks: Run Build Task** - Build the solution
- **Tasks: Run Task > watch** - Run with hot reload
- **Tasks: Run Task > watch:tailwind** - Watch Tailwind CSS changes

### Debug

Press `F5` to launch with debugger attached.

---

## 📂 Project Structure Overview

```
BlazorProductionApp/
├── src/
│   ├── BlazorApp.Web/           # ← START HERE (Presentation)
│   ├── BlazorApp.Application/   # Business logic
│   ├── BlazorApp.Domain/        # Core entities
│   └── BlazorApp.Infrastructure/ # Data access (Dapper)
├── database/
│   └── schema.sql               # Database setup
├── README.md                    # Full documentation
├── AZURE_DEPLOYMENT.md          # Azure deployment guide
└── QUICK_START.md               # This file
```

---

## 🔑 Key Features Demonstrated

1. **Clean Architecture** - Proper layer separation
2. **Blazor Server** - Real-time with SignalR
3. **MudBlazor** - Material Design components
4. **Tailwind CSS** - Utility-first styling (prefixed with `tw-`)
5. **Dapper** - High-performance data access
6. **Repository Pattern** - Testable, swappable data access
7. **Azure Ready** - Environment-based configuration

---

## 📖 Next Steps

1. **Explore the Code**

   - [src/BlazorApp.Web/Components/Pages/Home.razor](src/BlazorApp.Web/Components/Pages/Home.razor) - Homepage with Tailwind examples
   - [src/BlazorApp.Infrastructure/Repositories/ProductRepository.cs](src/BlazorApp.Infrastructure/Repositories/ProductRepository.cs) - Dapper implementation

2. **Read Full Documentation**

   - [README.md](README.md) - Complete project documentation
   - [AZURE_DEPLOYMENT.md](AZURE_DEPLOYMENT.md) - Step-by-step Azure deployment

3. **Customize**
   - Update `tailwind.config.js` - Add your brand colors
   - Modify MudBlazor theme in `Components/Layout/MainLayout.razor`
   - Add new entities following the existing pattern

---

## ❓ Common Questions

**Q: Why Blazor Server instead of WebAssembly?**  
A: Better for database-heavy apps, faster initial load, server-side security, easier Azure deployment.

**Q: Can I swap the database?**  
A: Yes! Just implement a new connection factory (PostgreSQL, MySQL, etc.) - see `SqlConnectionFactory.cs`.

**Q: How does Tailwind not conflict with MudBlazor?**  
A: Tailwind uses `tw-` prefix and preflight is disabled. See `tailwind.config.js`.

**Q: Where do I add authentication?**  
A: Add ASP.NET Core Identity or Azure AD. Update `Program.cs` with auth middleware.

---

## 🆘 Troubleshooting

**Build Errors**

```bash
dotnet clean
dotnet restore
dotnet build
```

**Tailwind Not Working**

```bash
cd src/BlazorApp.Web
npm install
npm run build:css
```

**Database Connection Failed**

- Check connection string in `appsettings.Development.json`
- Ensure SQL Server is running
- Verify database exists: `BlazorAppDb`

---

## 📞 Support

- Open an issue in the repository
- Check existing documentation in `README.md`
- Review Azure deployment guide: `AZURE_DEPLOYMENT.md`

---

**Happy Coding! 🎉**
