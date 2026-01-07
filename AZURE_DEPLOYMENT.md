# Azure Deployment Guide

Complete guide for deploying the Blazor Production App to Azure App Service and Azure SQL Database.

## Prerequisites

- Azure subscription ([Free trial available](https://azure.microsoft.com/free/))
- Azure CLI installed ([Install guide](https://docs.microsoft.com/cli/azure/install-azure-cli))
- .NET 8.0 SDK
- Node.js 18+ (for Tailwind CSS build)

## Architecture Overview

```
┌─────────────────────────────────────────────┐
│         Azure App Service (Linux)            │
│  ┌─────────────────────────────────────┐   │
│  │   Blazor Server Application         │   │
│  │   - .NET 8.0 Runtime                │   │
│  │   - MudBlazor + Tailwind CSS        │   │
│  │   - Dapper Data Access              │   │
│  └─────────────┬───────────────────────┘   │
└────────────────┼───────────────────────────┘
                 │ Secure Connection
                 │ (Connection String)
                 ▼
┌────────────────────────────────────────────┐
│       Azure SQL Database                   │
│  ┌────────────────────────────────────┐   │
│  │   BlazorAppDb                      │   │
│  │   - Products table                 │   │
│  │   - Stored procedures              │   │
│  └────────────────────────────────────┘   │
└────────────────────────────────────────────┘
```

## Step-by-Step Deployment

### 1. Login to Azure

```bash
# Login to your Azure account
az login

# Set your subscription (if you have multiple)
az account list --output table
az account set --subscription "Your Subscription Name"
```

### 2. Create Resource Group

```bash
# Create a resource group in your preferred region
az group create \
  --name rg-blazorapp-prod \
  --location eastus

# Verify creation
az group show --name rg-blazorapp-prod
```

Available regions:

- `eastus` - East US
- `westus2` - West US 2
- `westeurope` - West Europe
- `southeastasia` - Southeast Asia

### 3. Create Azure SQL Server and Database

```bash
# Create SQL Server
az sql server create \
  --name sql-blazorapp-prod-001 \
  --resource-group rg-blazorapp-prod \
  --location eastus \
  --admin-user sqladmin \
  --admin-password "YourStrong!Password123"

# Configure firewall to allow Azure services
az sql server firewall-rule create \
  --resource-group rg-blazorapp-prod \
  --server sql-blazorapp-prod-001 \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Create database
az sql db create \
  --name BlazorAppDb \
  --resource-group rg-blazorapp-prod \
  --server sql-blazorapp-prod-001 \
  --service-objective S0 \
  --backup-storage-redundancy Local

# Get connection string
az sql db show-connection-string \
  --client ado.net \
  --name BlazorAppDb \
  --server sql-blazorapp-prod-001
```

**Note the connection string** - you'll need it later. Replace `<username>` and `<password>` with your credentials.

### 4. Run Database Schema

**Option A: Using Azure Data Studio**

1. Download [Azure Data Studio](https://docs.microsoft.com/sql/azure-data-studio/download)
2. Connect to: `sql-blazorapp-prod-001.database.windows.net`
3. Open `database/schema.sql`
4. Execute the script

**Option B: Using sqlcmd**

```bash
# Install sqlcmd if not available
# macOS: brew install sqlcmd
# Windows: Included with SQL Server tools

sqlcmd -S sql-blazorapp-prod-001.database.windows.net \
       -d BlazorAppDb \
       -U sqladmin \
       -P "YourStrong!Password123" \
       -i database/schema.sql
```

**Option C: Using Azure Portal**

1. Go to Azure Portal → SQL databases → BlazorAppDb
2. Click "Query editor (preview)"
3. Login with SQL authentication
4. Paste and run the contents of `database/schema.sql`

### 5. Create App Service Plan

```bash
# Create Linux App Service Plan (B1 tier)
az appservice plan create \
  --name plan-blazorapp-prod \
  --resource-group rg-blazorapp-prod \
  --location eastus \
  --sku B1 \
  --is-linux

# For production, consider P1V2 or higher:
# --sku P1V2
```

Pricing tiers:

- **B1**: Basic, ~$13/month (development/testing)
- **S1**: Standard, ~$70/month (small production)
- **P1V2**: Premium, ~$85/month (production recommended)

### 6. Create Web App

```bash
# Create Web App with .NET 8.0 runtime
az webapp create \
  --name blazorapp-prod-001 \
  --resource-group rg-blazorapp-prod \
  --plan plan-blazorapp-prod \
  --runtime "DOTNETCORE:8.0"

# Enable HTTPS only
az webapp update \
  --name blazorapp-prod-001 \
  --resource-group rg-blazorapp-prod \
  --https-only true

# Configure logging
az webapp log config \
  --name blazorapp-prod-001 \
  --resource-group rg-blazorapp-prod \
  --application-logging azureblobstorage \
  --level information
```

### 7. Configure Application Settings

```bash
# Set connection string (replace with your actual connection string)
az webapp config connection-string set \
  --name blazorapp-prod-001 \
  --resource-group rg-blazorapp-prod \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:sql-blazorapp-prod-001.database.windows.net,1433;Initial Catalog=BlazorAppDb;Persist Security Info=False;User ID=sqladmin;Password=YourStrong!Password123;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Set environment to Production
az webapp config appsettings set \
  --name blazorapp-prod-001 \
  --resource-group rg-blazorapp-prod \
  --settings ASPNETCORE_ENVIRONMENT=Production
```

### 8. Build and Deploy Application

#### Option A: Deploy from VS Code

1. Install [Azure App Service extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azureappservice)
2. Build Tailwind CSS:
   ```bash
   cd src/BlazorApp.Web
   npm install
   npm run build:css
   ```
3. In VS Code:
   - Open Azure extension
   - Find your Web App under your subscription
   - Right-click on `BlazorApp.Web` folder → "Deploy to Web App"
   - Select your Web App

#### Option B: Deploy using Azure CLI

```bash
# Navigate to project root
cd BlazorProductionApp

# Install npm dependencies and build Tailwind
cd src/BlazorApp.Web
npm install
npm run build:css
cd ../..

# Publish the application
dotnet publish src/BlazorApp.Web/BlazorApp.Web.csproj \
  -c Release \
  -o ./publish

# Create deployment package
cd publish
zip -r ../deploy.zip .
cd ..

# Deploy to Azure
az webapp deployment source config-zip \
  --name blazorapp-prod-001 \
  --resource-group rg-blazorapp-prod \
  --src deploy.zip

# Clean up
rm deploy.zip
rm -rf publish
```

#### Option C: Deploy using GitHub Actions

Create `.github/workflows/azure-deploy.yml`:

```yaml
name: Deploy to Azure App Service

on:
  push:
    branches: [main]
  workflow_dispatch:

env:
  AZURE_WEBAPP_NAME: blazorapp-prod-001
  AZURE_WEBAPP_PACKAGE_PATH: "./publish"
  DOTNET_VERSION: "8.0.x"
  NODE_VERSION: "18.x"

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: ${{ env.NODE_VERSION }}

      - name: Install npm dependencies
        run: |
          cd src/BlazorApp.Web
          npm install

      - name: Build Tailwind CSS
        run: |
          cd src/BlazorApp.Web
          npm run build:css

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --configuration Release --no-restore

      - name: Publish
        run: dotnet publish src/BlazorApp.Web/BlazorApp.Web.csproj -c Release -o ${{ env.AZURE_WEBAPP_PACKAGE_PATH }}

      - name: Deploy to Azure Web App
        uses: azure/webapps-deploy@v2
        with:
          app-name: ${{ env.AZURE_WEBAPP_NAME }}
          publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
          package: ${{ env.AZURE_WEBAPP_PACKAGE_PATH }}
```

Get publish profile:

```bash
az webapp deployment list-publishing-profiles \
  --name blazorapp-prod-001 \
  --resource-group rg-blazorapp-prod \
  --xml
```

Add this as a secret `AZURE_WEBAPP_PUBLISH_PROFILE` in your GitHub repository.

### 9. Verify Deployment

```bash
# Open the app in browser
az webapp browse \
  --name blazorapp-prod-001 \
  --resource-group rg-blazorapp-prod

# Or directly navigate to:
# https://blazorapp-prod-001.azurewebsites.net
```

### 10. Monitor and Troubleshoot

```bash
# Stream logs
az webapp log tail \
  --name blazorapp-prod-001 \
  --resource-group rg-blazorapp-prod

# View log files
az webapp log download \
  --name blazorapp-prod-001 \
  --resource-group rg-blazorapp-prod \
  --log-file logs.zip
```

## Post-Deployment Configuration

### Enable Application Insights

```bash
# Create Application Insights
az monitor app-insights component create \
  --app blazorapp-insights \
  --location eastus \
  --resource-group rg-blazorapp-prod

# Get instrumentation key
INSTRUMENTATION_KEY=$(az monitor app-insights component show \
  --app blazorapp-insights \
  --resource-group rg-blazorapp-prod \
  --query instrumentationKey -o tsv)

# Configure Web App to use Application Insights
az webapp config appsettings set \
  --name blazorapp-prod-001 \
  --resource-group rg-blazorapp-prod \
  --settings APPINSIGHTS_INSTRUMENTATIONKEY=$INSTRUMENTATION_KEY
```

### Configure Custom Domain

```bash
# Map custom domain
az webapp config hostname add \
  --webapp-name blazorapp-prod-001 \
  --resource-group rg-blazorapp-prod \
  --hostname www.yourdomain.com

# Enable managed SSL certificate (free)
az webapp config ssl bind \
  --name blazorapp-prod-001 \
  --resource-group rg-blazorapp-prod \
  --certificate-thumbprint auto \
  --ssl-type SNI
```

### Auto-Scaling Configuration

```bash
# Create autoscale rule
az monitor autoscale create \
  --resource-group rg-blazorapp-prod \
  --resource /subscriptions/{subscription-id}/resourceGroups/rg-blazorapp-prod/providers/Microsoft.Web/serverfarms/plan-blazorapp-prod \
  --name autoscale-blazorapp \
  --min-count 1 \
  --max-count 5 \
  --count 2

# Scale out when CPU > 70%
az monitor autoscale rule create \
  --resource-group rg-blazorapp-prod \
  --autoscale-name autoscale-blazorapp \
  --scale out 1 \
  --condition "Percentage CPU > 70 avg 5m"
```

## Cost Optimization

| Resource         | Tier     | Monthly Cost (approx) |
| ---------------- | -------- | --------------------- |
| App Service (B1) | Basic    | $13                   |
| Azure SQL (S0)   | Standard | $15                   |
| **Total**        |          | **~$28/month**        |

### Production Recommendations:

- **App Service**: P1V2 (~$85/month) for better performance
- **Azure SQL**: S1 or higher (~$30/month) for production workloads
- **Application Insights**: Free tier (5 GB/month free)

## Security Best Practices

### 1. Use Managed Identity (Recommended)

Avoid storing database credentials in connection strings:

```bash
# Enable system-assigned managed identity
az webapp identity assign \
  --name blazorapp-prod-001 \
  --resource-group rg-blazorapp-prod

# Get the identity's principal ID
PRINCIPAL_ID=$(az webapp identity show \
  --name blazorapp-prod-001 \
  --resource-group rg-blazorapp-prod \
  --query principalId -o tsv)

# Grant SQL access to managed identity
# (Run this in Azure SQL using SQL Admin)
# CREATE USER [blazorapp-prod-001] FROM EXTERNAL PROVIDER;
# ALTER ROLE db_datareader ADD MEMBER [blazorapp-prod-001];
# ALTER ROLE db_datawriter ADD MEMBER [blazorapp-prod-001];
```

Update connection string to use managed identity:

```
Server=tcp:sql-blazorapp-prod-001.database.windows.net,1433;Database=BlazorAppDb;Authentication=Active Directory Managed Identity;
```

### 2. Key Vault for Secrets

```bash
# Create Key Vault
az keyvault create \
  --name kv-blazorapp-prod \
  --resource-group rg-blazorapp-prod \
  --location eastus

# Store connection string
az keyvault secret set \
  --vault-name kv-blazorapp-prod \
  --name "SqlConnectionString" \
  --value "Server=tcp:sql-blazorapp-prod-001.database.windows.net,1433;..."

# Grant Web App access to Key Vault
az keyvault set-policy \
  --name kv-blazorapp-prod \
  --object-id $PRINCIPAL_ID \
  --secret-permissions get list
```

Reference in App Service:

```bash
az webapp config appsettings set \
  --name blazorapp-prod-001 \
  --resource-group rg-blazorapp-prod \
  --settings DefaultConnection="@Microsoft.KeyVault(SecretUri=https://kv-blazorapp-prod.vault.azure.net/secrets/SqlConnectionString/)"
```

## Disaster Recovery

### Backup Strategy

```bash
# Enable automated backups
az webapp config backup create \
  --resource-group rg-blazorapp-prod \
  --webapp-name blazorapp-prod-001 \
  --backup-name daily-backup \
  --container-url "<storage-container-sas-url>"

# Configure SQL Database backup retention
az sql db ltr-policy set \
  --resource-group rg-blazorapp-prod \
  --server sql-blazorapp-prod-001 \
  --database BlazorAppDb \
  --weekly-retention P4W \
  --monthly-retention P12M
```

## Cleanup Resources

```bash
# Delete entire resource group (removes all resources)
az group delete \
  --name rg-blazorapp-prod \
  --yes \
  --no-wait
```

## Troubleshooting

### Application won't start

```bash
# Check logs
az webapp log tail --name blazorapp-prod-001 --resource-group rg-blazorapp-prod

# Common issues:
# 1. Missing Tailwind CSS build → Run npm run build:css before publishing
# 2. Connection string not set → Verify with: az webapp config connection-string list
# 3. Wrong .NET version → Ensure runtime is set to DOTNETCORE:8.0
```

### Database connection errors

```bash
# Test connection from App Service
az webapp ssh --name blazorapp-prod-001 --resource-group rg-blazorapp-prod

# Inside SSH:
# apt-get update && apt-get install -y netcat
# nc -zv sql-blazorapp-prod-001.database.windows.net 1433
```

### Performance issues

- Enable Application Insights for diagnostics
- Check App Service Plan scaling
- Verify database DTUs aren't maxed out
- Review SQL query performance in Azure Portal

## Additional Resources

- [Azure App Service Documentation](https://docs.microsoft.com/azure/app-service/)
- [Azure SQL Database Documentation](https://docs.microsoft.com/azure/azure-sql/)
- [Blazor Deployment Guide](https://docs.microsoft.com/aspnet/core/blazor/host-and-deploy/)
- [Azure Pricing Calculator](https://azure.microsoft.com/pricing/calculator/)

---

**Need Help?** Open an issue in the repository or consult [Azure Support](https://azure.microsoft.com/support/).
