# Azure Deployment Guide - Spinner.Net Sprint 1

## Overview

This guide covers complete Azure deployment for Sprint 1 production infrastructure. You'll deploy Azure Cosmos DB, Azure OpenAI, Azure SignalR, and Azure App Service to host the Spinner.Net platform.

**Goal**: Week 4 production deployment with all Sprint 1 features operational on Azure.

## Prerequisites

### 1. Azure Account Setup

**Required:**
```bash
# Azure CLI
https://docs.microsoft.com/en-us/cli/azure/install-azure-cli

# Azure subscription with sufficient credits/budget
# Estimated Sprint 1 costs: $50-100/month
```

**Login and verify:**
```bash
az login
az account show
az account list --output table
```

### 2. Resource Planning

**Sprint 1 Azure Resources:**
- **Resource Group**: `rg-spinnernet-prod`
- **Cosmos DB**: `cosmos-spinnernet-prod`
- **Azure OpenAI**: `openai-spinnernet-prod`
- **SignalR Service**: `signalr-spinnernet-prod`
- **App Service Plan**: `asp-spinnernet-prod`
- **App Service**: `app-spinnernet-prod`
- **Key Vault**: `kv-spinnernet-prod`

## Step 1: Create Resource Group

### Create Base Infrastructure

```bash
# Set variables
LOCATION="West Europe"
RESOURCE_GROUP="rg-spinnernet-prod"

# Create resource group
az group create \
  --name $RESOURCE_GROUP \
  --location "$LOCATION"
```

### Set Default Resource Group

```bash
# Configure defaults to simplify commands
az configure --defaults group=$RESOURCE_GROUP location="$LOCATION"
```

## Step 2: Azure Cosmos DB Setup

### Create Cosmos DB Account

```bash
# Cosmos DB optimized for Sprint 1
az cosmosdb create \
  --name "cosmos-spinnernet-prod" \
  --kind GlobalDocumentDB \
  --default-consistency-level Session \
  --enable-free-tier false \
  --locations regionName="$LOCATION" failoverPriority=0 isZoneRedundant=false
```

### Create Database and Containers

```bash
# Create database
az cosmosdb sql database create \
  --account-name "cosmos-spinnernet-prod" \
  --name "SpinnerNetProd"

# Create containers with proper partitioning
az cosmosdb sql container create \
  --account-name "cosmos-spinnernet-prod" \
  --database-name "SpinnerNetProd" \
  --name "Users" \
  --partition-key-path "/userId" \
  --throughput 400

az cosmosdb sql container create \
  --account-name "cosmos-spinnernet-prod" \
  --database-name "SpinnerNetProd" \
  --name "Personas" \
  --partition-key-path "/userId" \
  --throughput 400

az cosmosdb sql container create \
  --account-name "cosmos-spinnernet-prod" \
  --database-name "SpinnerNetProd" \
  --name "Tasks" \
  --partition-key-path "/userId" \
  --throughput 400

az cosmosdb sql container create \
  --account-name "cosmos-spinnernet-prod" \
  --database-name "SpinnerNetProd" \
  --name "Conversations" \
  --partition-key-path "/userId" \
  --throughput 400

az cosmosdb sql container create \
  --account-name "cosmos-spinnernet-prod" \
  --database-name "SpinnerNetProd" \
  --name "Communications" \
  --partition-key-path "/userId" \
  --throughput 400
```

### Get Connection String

```bash
# Get primary connection string
az cosmosdb keys list \
  --name "cosmos-spinnernet-prod" \
  --type connection-strings \
  --query "connectionStrings[0].connectionString" \
  --output tsv
```

**Save this connection string - you'll need it for configuration.**

## Step 3: Azure OpenAI Service

### Create OpenAI Resource

```bash
# Azure OpenAI (requires approval in most regions)
az cognitiveservices account create \
  --name "openai-spinnernet-prod" \
  --kind OpenAI \
  --sku S0 \
  --location "East US" \
  --custom-domain "openai-spinnernet-prod"
```

### Deploy Models

**Via Azure Portal (Recommended):**
1. Go to Azure OpenAI Studio: https://oai.azure.com
2. Navigate to "Deployments"
3. Deploy these models:

```yaml
Models to Deploy:
- gpt-4o-mini:
    deployment_name: "gpt-4o-mini"
    version: "latest"
    scale_type: "Standard"
    capacity: 10

- text-embedding-3-small:
    deployment_name: "text-embedding-3-small"
    version: "latest"
    scale_type: "Standard"
    capacity: 10
```

**Via CLI (Alternative):**
```bash
# Get API key first
OPENAI_KEY=$(az cognitiveservices account keys list \
  --name "openai-spinnernet-prod" \
  --query "key1" --output tsv)

# Deploy models (requires REST API calls)
# Use Azure Portal method instead - it's more reliable
```

### Get API Configuration

```bash
# Get endpoint and key
az cognitiveservices account show \
  --name "openai-spinnernet-prod" \
  --query "properties.endpoint" --output tsv

az cognitiveservices account keys list \
  --name "openai-spinnernet-prod" \
  --query "key1" --output tsv
```

## Step 4: Azure SignalR Service

### Create SignalR Service

```bash
# SignalR for real-time buddy conversations
az signalr create \
  --name "signalr-spinnernet-prod" \
  --sku Standard_S1 \
  --unit-count 1 \
  --service-mode Default
```

### Get Connection String

```bash
# Get SignalR connection string
az signalr key list \
  --name "signalr-spinnernet-prod" \
  --query "primaryConnectionString" --output tsv
```

## Step 5: Azure Key Vault

### Create Key Vault

```bash
# Key Vault for secrets management
az keyvault create \
  --name "kv-spinnernet-prod" \
  --enabled-for-deployment true \
  --enabled-for-template-deployment true
```

### Store Secrets

```bash
# Store all connection strings and keys
az keyvault secret set \
  --vault-name "kv-spinnernet-prod" \
  --name "CosmosDbConnectionString" \
  --value "YOUR_COSMOS_CONNECTION_STRING"

az keyvault secret set \
  --vault-name "kv-spinnernet-prod" \
  --name "OpenAIApiKey" \
  --value "YOUR_OPENAI_API_KEY"

az keyvault secret set \
  --vault-name "kv-spinnernet-prod" \
  --name "SignalRConnectionString" \
  --value "YOUR_SIGNALR_CONNECTION_STRING"
```

## Step 6: App Service Setup

### Create App Service Plan

```bash
# App Service Plan for hosting
az appservice plan create \
  --name "asp-spinnernet-prod" \
  --sku B1 \
  --is-linux true
```

### Create App Service

```bash
# Web App with .NET 9
az webapp create \
  --name "app-spinnernet-prod" \
  --plan "asp-spinnernet-prod" \
  --runtime "DOTNETCORE:9.0"
```

### Configure App Settings

```bash
# Configure application settings
az webapp config appsettings set \
  --name "app-spinnernet-prod" \
  --settings \
    WEBSITES_ENABLE_APP_SERVICE_STORAGE=false \
    ASPNETCORE_ENVIRONMENT=Production \
    WEBSITE_RUN_FROM_PACKAGE=1
```

### Enable Managed Identity

```bash
# Enable system-assigned managed identity
az webapp identity assign \
  --name "app-spinnernet-prod"

# Get the identity principal ID
PRINCIPAL_ID=$(az webapp identity show \
  --name "app-spinnernet-prod" \
  --query "principalId" --output tsv)

# Grant Key Vault access
az keyvault set-policy \
  --name "kv-spinnernet-prod" \
  --object-id $PRINCIPAL_ID \
  --secret-permissions get list
```

## Step 7: Application Configuration

### Production Configuration

**Create `appsettings.Production.json`:**
```json
{
  "ConnectionStrings": {
    "CosmosDb": "",
    "DatabaseName": "SpinnerNetProd",
    "SignalR": ""
  },
  "OpenAI": {
    "ApiKey": "",
    "Endpoint": "https://openai-spinnernet-prod.openai.azure.com/",
    "DeploymentName": "gpt-4o-mini",
    "EmbeddingDeploymentName": "text-embedding-3-small",
    "ApiVersion": "2024-06-01"
  },
  "LocalLLM": {
    "Enabled": false,
    "BaseUrl": "",
    "ModelName": ""
  },
  "AI": {
    "PreferLocal": false,
    "UseCloudForComplexTasks": true,
    "MaxTokensLocal": 0,
    "MaxTokensCloud": 4096
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "SpinnerNet": "Information",
      "Microsoft.AspNetCore": "Warning"
    },
    "ApplicationInsights": {
      "LogLevel": {
        "Default": "Information"
      }
    }
  },
  "AllowedHosts": "*"
}
```

### Key Vault Integration

**Update `Program.cs`:**
```csharp
// Production configuration with Key Vault
if (builder.Environment.IsProduction())
{
    var keyVaultUrl = "https://kv-spinnernet-prod.vault.azure.net/";
    
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new DefaultAzureCredential());
}

// Cosmos DB with production connection
services.AddSingleton<CosmosClient>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("CosmosDb");
    
    return new CosmosClient(connectionString, new CosmosClientOptions
    {
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    });
});

// SignalR with Azure service
services.AddSignalR()
    .AddAzureSignalR(configuration.GetConnectionString("SignalR"));
```

## Step 8: CI/CD Pipeline Setup

### GitHub Actions Deployment

**Create `.github/workflows/azure-deploy.yml`:**
```yaml
name: Deploy to Azure

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore dependencies
      run: dotnet restore src/SpinnerNet.sln
    
    - name: Build
      run: dotnet build src/SpinnerNet.sln --no-restore --configuration Release
    
    - name: Test
      run: dotnet test src/SpinnerNet.Tests/SpinnerNet.Tests.csproj --no-build --configuration Release
    
    - name: Publish
      run: dotnet publish src/SpinnerNet.Web/SpinnerNet.Web.csproj --configuration Release --output ./publish
    
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'app-spinnernet-prod'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

### Get Publish Profile

```bash
# Download publish profile
az webapp deployment list-publishing-profiles \
  --name "app-spinnernet-prod" \
  --xml
```

**Add to GitHub Secrets:**
1. Go to GitHub repository → Settings → Secrets
2. Add `AZURE_WEBAPP_PUBLISH_PROFILE` with the XML content

## Step 9: SSL and Custom Domain

### Configure SSL

```bash
# Enable HTTPS only
az webapp update \
  --name "app-spinnernet-prod" \
  --https-only true

# Enable HTTP/2
az webapp config set \
  --name "app-spinnernet-prod" \
  --http20-enabled true
```

### Custom Domain (Optional)

```bash
# Add custom domain (if you have one)
az webapp config hostname add \
  --webapp-name "app-spinnernet-prod" \
  --hostname "your-domain.com"

# Configure SSL certificate
az webapp config ssl bind \
  --name "app-spinnernet-prod" \
  --certificate-thumbprint "YOUR_CERT_THUMBPRINT" \
  --ssl-type SNI
```

## Step 10: Monitoring and Logging

### Application Insights

```bash
# Create Application Insights
az monitor app-insights component create \
  --app "appinsights-spinnernet-prod" \
  --location "$LOCATION" \
  --application-type web

# Get instrumentation key
INSTRUMENTATION_KEY=$(az monitor app-insights component show \
  --app "appinsights-spinnernet-prod" \
  --query "instrumentationKey" --output tsv)

# Configure in App Service
az webapp config appsettings set \
  --name "app-spinnernet-prod" \
  --settings \
    APPINSIGHTS_INSTRUMENTATIONKEY=$INSTRUMENTATION_KEY \
    APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=$INSTRUMENTATION_KEY"
```

### Log Streaming

```bash
# Enable logging
az webapp log config \
  --name "app-spinnernet-prod" \
  --application-logging filesystem \
  --detailed-error-messages true \
  --failed-request-tracing true \
  --web-server-logging filesystem

# Stream logs in real-time
az webapp log tail --name "app-spinnernet-prod"
```

## Step 11: Deployment and Testing

### Deploy Application

```bash
# Build and publish locally for first deployment
cd src
dotnet publish SpinnerNet.Web/SpinnerNet.Web.csproj \
  --configuration Release \
  --output ../publish

# Deploy via ZIP
az webapp deployment source config-zip \
  --name "app-spinnernet-prod" \
  --src "../publish.zip"
```

### Verify Deployment

**Check application status:**
```bash
# Get app URL
az webapp show --name "app-spinnernet-prod" --query "defaultHostName" --output tsv

# Check app status
az webapp show --name "app-spinnernet-prod" --query "state" --output tsv
```

**Test endpoints:**
```bash
# Health check
curl https://app-spinnernet-prod.azurewebsites.net/health

# API test
curl https://app-spinnernet-prod.azurewebsites.net/api/users/register \
  -X POST \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","displayName":"Test User","password":"Test123!"}'
```

### Smoke Testing

**Sprint 1 end-to-end test:**
1. **User Registration**: Create account via API
2. **Persona Interview**: Start and complete interview
3. **Task Creation**: Create task via natural language
4. **Buddy Chat**: Send message to AI buddy
5. **Real-time**: Test SignalR connection

## Security Configuration

### Network Security

```bash
# Configure allowed IPs (optional)
az webapp config access-restriction add \
  --name "app-spinnernet-prod" \
  --rule-name "AllowOfficeIP" \
  --action Allow \
  --ip-address "YOUR_OFFICE_IP/32" \
  --priority 100

# Enable Always On (for production)
az webapp config set \
  --name "app-spinnernet-prod" \
  --always-on true
```

### API Rate Limiting

**Configure in application:**
```csharp
// Rate limiting middleware in Program.cs
services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("ApiLimit", configure =>
    {
        configure.PermitLimit = 100;
        configure.Window = TimeSpan.FromMinutes(1);
    });
});
```

## Cost Optimization

### Resource Scaling

```bash
# Scale down for development
az appservice plan update \
  --name "asp-spinnernet-prod" \
  --sku B1

# Scale up for production load
az appservice plan update \
  --name "asp-spinnernet-prod" \
  --sku S1

# Auto-scaling (for higher tiers)
az monitor autoscale create \
  --resource-group $RESOURCE_GROUP \
  --resource "asp-spinnernet-prod" \
  --resource-type Microsoft.Web/serverFarms \
  --name "autoscale-spinnernet" \
  --min-count 1 \
  --max-count 3 \
  --count 1
```

### Cosmos DB Optimization

```bash
# Adjust throughput based on usage
az cosmosdb sql container throughput update \
  --account-name "cosmos-spinnernet-prod" \
  --database-name "SpinnerNetProd" \
  --name "Users" \
  --throughput 400

# Enable serverless (for development)
# Note: Choose between provisioned and serverless at creation time
```

## Troubleshooting

### Common Issues

**1. Application won't start:**
```bash
# Check application logs
az webapp log tail --name "app-spinnernet-prod"

# Check configuration
az webapp config appsettings list --name "app-spinnernet-prod"
```

**2. Cosmos DB connection issues:**
```bash
# Verify connection string
az cosmosdb keys list --name "cosmos-spinnernet-prod" --type connection-strings

# Check firewall rules
az cosmosdb network-rule list --name "cosmos-spinnernet-prod"
```

**3. OpenAI API errors:**
```bash
# Check deployment status
az cognitiveservices account deployment list --name "openai-spinnernet-prod"

# Verify quota
# Check in Azure Portal → OpenAI → Quotas
```

**4. SignalR connection issues:**
```bash
# Check service status
az signalr show --name "signalr-spinnernet-prod" --query "provisioningState"

# Test connection
# Use SignalR connection tester tool
```

### Health Checks

**Implement health checks:**
```csharp
// In Program.cs
services.AddHealthChecks()
    .AddCosmosDb(connectionString: cosmosConnectionString)
    .AddAzureSignalR(connectionString: signalRConnectionString)
    .AddCheck<OpenAIHealthCheck>("openai");

app.MapHealthChecks("/health");
```

## Production Checklist

### Pre-Deployment
- [ ] All Azure resources created and configured
- [ ] Connection strings stored in Key Vault
- [ ] Application Insights configured
- [ ] SSL/HTTPS enabled
- [ ] Managed Identity configured
- [ ] CI/CD pipeline working

### Post-Deployment
- [ ] Health checks passing
- [ ] All Sprint 1 endpoints responding
- [ ] Cosmos DB containers populated
- [ ] OpenAI integration working
- [ ] SignalR real-time chat functional
- [ ] Logs flowing to Application Insights
- [ ] Performance metrics baseline established

### Cost Monitoring
- [ ] Azure Cost Management alerts configured
- [ ] Resource usage monitoring enabled
- [ ] Auto-scaling rules appropriate
- [ ] Unused resources identified and removed

---

## Quick Reference

**Resource URLs:**
- App Service: `https://app-spinnernet-prod.azurewebsites.net`
- Cosmos DB: `https://cosmos-spinnernet-prod.documents.azure.com`
- Key Vault: `https://kv-spinnernet-prod.vault.azure.net`
- Application Insights: `https://portal.azure.com` → Application Insights

**Essential Commands:**
```bash
# Check deployment status
az webapp show --name "app-spinnernet-prod" --query "state"

# Stream logs
az webapp log tail --name "app-spinnernet-prod"

# Restart app
az webapp restart --name "app-spinnernet-prod"

# Scale resources
az appservice plan update --name "asp-spinnernet-prod" --sku S1
```

**Sprint 1 production deployment ready! 🚀**