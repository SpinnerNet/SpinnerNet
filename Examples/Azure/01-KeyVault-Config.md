# Azure KeyVault Integration Example

This example demonstrates how to securely manage API keys and secrets using Azure KeyVault with DefaultAzureCredential for enterprise-grade security in the hybrid AI architecture.

## Overview

Azure KeyVault provides enterprise-grade secret management that ensures:
- **Zero secrets in code** - All sensitive data stored securely
- **Managed identity integration** - Seamless Azure authentication  
- **Audit logging** - Complete access tracking
- **Role-based access** - Granular permission control
- **Rotation support** - Automated secret lifecycle management

## Key Components

### 1. Azure KeyVault Configuration

**File:** `Program.cs` (KeyVault setup)

```csharp
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SpinnerNet.Core.Services;

namespace SpinnerNet.App
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Azure KeyVault client with DefaultAzureCredential
            await ConfigureAzureKeyVaultAsync(builder);

            var app = builder.Build();
            
            // Configure the HTTP request pipeline
            ConfigureMiddleware(app);
            
            app.Run();
        }

        /// <summary>
        /// Configures Azure KeyVault integration with proper error handling
        /// </summary>
        private static async Task ConfigureAzureKeyVaultAsync(WebApplicationBuilder builder)
        {
            try
            {
                // Get KeyVault name from configuration
                var keyVaultName = builder.Configuration["Azure:KeyVault:Name"];
                if (string.IsNullOrEmpty(keyVaultName))
                {
                    throw new InvalidOperationException("Azure:KeyVault:Name not configured");
                }

                var keyVaultUri = $"https://{keyVaultName}.vault.azure.net/";
                Console.WriteLine($"🔐 Connecting to Azure KeyVault: {keyVaultUri}");

                // Create KeyVault client using DefaultAzureCredential
                // This supports multiple authentication methods:
                // 1. Environment variables (local development)
                // 2. Managed Identity (Azure deployment)
                // 3. Azure CLI (developer workstation)
                // 4. Visual Studio (IDE integration)
                var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    // Exclude interactive browser credential for production
                    ExcludeInteractiveBrowserCredential = !builder.Environment.IsDevelopment(),
                    // Add logging for credential chain debugging
                    Diagnostics = { IsLoggingContentEnabled = builder.Environment.IsDevelopment() }
                });

                var keyVaultClient = new SecretClient(new Uri(keyVaultUri), credential);
                
                // Register KeyVault client for dependency injection
                builder.Services.AddSingleton(keyVaultClient);

                // Test KeyVault connectivity and retrieve OpenAI API key
                await ConfigureAIServicesAsync(builder, keyVaultClient);

                Console.WriteLine("✅ Azure KeyVault integration configured successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to configure Azure KeyVault: {ex.Message}");
                
                // In production, fail fast if KeyVault is not accessible
                if (!builder.Environment.IsDevelopment())
                {
                    throw new InvalidOperationException(
                        "Azure KeyVault is required for production deployment. " +
                        "Ensure Managed Identity is configured and has appropriate permissions.", ex);
                }
                
                // In development, allow fallback to environment variables
                Console.WriteLine("⚠️ Falling back to environment variables for development");
                await ConfigureAIServicesFromEnvironmentAsync(builder);
            }
        }

        /// <summary>
        /// Configures AI services using secrets from Azure KeyVault
        /// </summary>
        private static async Task ConfigureAIServicesAsync(
            WebApplicationBuilder builder, 
            SecretClient keyVaultClient)
        {
            try
            {
                // Retrieve OpenAI API key from KeyVault
                Console.WriteLine("🔑 Retrieving OpenAI API key from KeyVault...");
                var openAiKeySecret = await keyVaultClient.GetSecretAsync("OpenAI-API-Key");
                var openAiApiKey = openAiKeySecret.Value.Value;

                if (string.IsNullOrEmpty(openAiApiKey))
                {
                    throw new InvalidOperationException("OpenAI API key is empty in KeyVault");
                }

                // Configure Semantic Kernel with OpenAI
                var kernelBuilder = Kernel.CreateBuilder();
                
                kernelBuilder.AddOpenAIChatCompletion(
                    modelId: "gpt-4o-mini",
                    apiKey: openAiApiKey,
                    orgId: null, // Optional: retrieve from KeyVault if needed
                    serviceId: "openai-chat"
                );

                var kernel = kernelBuilder.Build();
                
                // Register Semantic Kernel services
                builder.Services.AddSingleton(kernel);
                builder.Services.AddScoped<AIOrchestrationService>();
                
                Console.WriteLine("✅ AI services configured with KeyVault secrets");

                // Optionally retrieve other secrets
                await RetrieveAdditionalSecretsAsync(builder, keyVaultClient);
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                throw new InvalidOperationException(
                    "OpenAI-API-Key secret not found in KeyVault. " +
                    "Please ensure the secret exists and access permissions are configured correctly.", ex);
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 403)
            {
                throw new InvalidOperationException(
                    "Access denied to KeyVault. " +
                    "Please ensure the application's Managed Identity has 'Key Vault Secrets User' role.", ex);
            }
        }

        /// <summary>
        /// Retrieves additional secrets that might be needed
        /// </summary>
        private static async Task RetrieveAdditionalSecretsAsync(
            WebApplicationBuilder builder,
            SecretClient keyVaultClient)
        {
            try
            {
                // Example: Retrieve Cosmos DB connection string
                var cosmosSecret = await keyVaultClient.GetSecretAsync("CosmosDB-ConnectionString");
                builder.Configuration["ConnectionStrings:CosmosDB"] = cosmosSecret.Value.Value;
                
                // Example: Retrieve application insights key
                var appInsightsSecret = await keyVaultClient.GetSecretAsync("ApplicationInsights-Key");
                builder.Configuration["ApplicationInsights:InstrumentationKey"] = appInsightsSecret.Value.Value;
                
                Console.WriteLine("✅ Additional secrets retrieved from KeyVault");
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                // Optional secrets - don't fail if not found
                Console.WriteLine($"⚠️ Optional secret not found: {ex.Message}");
            }
        }

        /// <summary>
        /// Fallback configuration using environment variables (development only)
        /// </summary>
        private static async Task ConfigureAIServicesFromEnvironmentAsync(WebApplicationBuilder builder)
        {
            var openAiApiKey = builder.Configuration["OPENAI_API_KEY"] ?? 
                              Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            if (!string.IsNullOrEmpty(openAiApiKey))
            {
                var kernelBuilder = Kernel.CreateBuilder();
                kernelBuilder.AddOpenAIChatCompletion("gpt-4o-mini", openAiApiKey);
                var kernel = kernelBuilder.Build();
                
                builder.Services.AddSingleton(kernel);
                builder.Services.AddScoped<AIOrchestrationService>();
                
                Console.WriteLine("✅ AI services configured with environment variables");
            }
            else
            {
                throw new InvalidOperationException(
                    "No AI service configuration found. " +
                    "Please ensure OpenAI API key is available in Azure KeyVault or environment variables.");
            }
        }

        /// <summary>
        /// Configures HTTP middleware pipeline
        /// </summary>
        private static void ConfigureMiddleware(WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAntiforgery();

            app.UseAuthentication();
            app.UseAuthorization();

            // Map SignalR hubs
            app.MapHub<SpinnerNet.App.Hubs.AIHub>("/aihub");
            
            app.MapRazorComponents<App>()
               .AddInteractiveServerRenderMode();
        }
    }
}
```

### 2. Development Configuration

**File:** `appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Azure.Identity": "Information"
    }
  },
  "Azure": {
    "KeyVault": {
      "Name": "kv-spinnernet-3lauxg"
    }
  },
  "AllowedHosts": "*"
}
```

### 3. Production Configuration

**File:** `appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Azure": {
    "KeyVault": {
      "Name": "kv-spinnernet-3lauxg"
    }
  },
  "AllowedHosts": "*"
}
```

### 4. KeyVault Secret Management Service

**File:** `Core/Services/KeyVaultSecretService.cs`

```csharp
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;

namespace SpinnerNet.Core.Services
{
    /// <summary>
    /// Service for managing KeyVault secrets with caching and error handling
    /// </summary>
    public class KeyVaultSecretService
    {
        private readonly SecretClient _secretClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<KeyVaultSecretService> _logger;
        private static readonly TimeSpan DefaultCacheExpiry = TimeSpan.FromMinutes(15);

        public KeyVaultSecretService(
            SecretClient secretClient,
            IMemoryCache cache,
            ILogger<KeyVaultSecretService> logger)
        {
            _secretClient = secretClient;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a secret from KeyVault with caching
        /// </summary>
        public async Task<string> GetSecretAsync(string secretName, bool useCache = true)
        {
            var cacheKey = $"keyvault-secret-{secretName}";

            // Check cache first
            if (useCache && _cache.TryGetValue(cacheKey, out string cachedValue))
            {
                _logger.LogDebug("Retrieved secret {SecretName} from cache", secretName);
                return cachedValue;
            }

            try
            {
                _logger.LogInformation("Retrieving secret {SecretName} from KeyVault", secretName);
                
                var secret = await _secretClient.GetSecretAsync(secretName);
                var secretValue = secret.Value.Value;

                // Cache the secret
                if (useCache)
                {
                    _cache.Set(cacheKey, secretValue, DefaultCacheExpiry);
                    _logger.LogDebug("Cached secret {SecretName} for {ExpiryMinutes} minutes", 
                        secretName, DefaultCacheExpiry.TotalMinutes);
                }

                return secretValue;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogError("Secret {SecretName} not found in KeyVault", secretName);
                throw new InvalidOperationException($"Secret '{secretName}' not found in KeyVault", ex);
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 403)
            {
                _logger.LogError("Access denied to secret {SecretName} in KeyVault", secretName);
                throw new UnauthorizedAccessException($"Access denied to secret '{secretName}' in KeyVault", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving secret {SecretName} from KeyVault", secretName);
                throw;
            }
        }

        /// <summary>
        /// Sets a secret in KeyVault (requires appropriate permissions)
        /// </summary>
        public async Task SetSecretAsync(string secretName, string secretValue)
        {
            try
            {
                _logger.LogInformation("Setting secret {SecretName} in KeyVault", secretName);
                
                await _secretClient.SetSecretAsync(secretName, secretValue);
                
                // Invalidate cache
                var cacheKey = $"keyvault-secret-{secretName}";
                _cache.Remove(cacheKey);
                
                _logger.LogInformation("Secret {SecretName} set successfully in KeyVault", secretName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting secret {SecretName} in KeyVault", secretName);
                throw;
            }
        }

        /// <summary>
        /// Retrieves multiple secrets efficiently
        /// </summary>
        public async Task<Dictionary<string, string>> GetSecretsAsync(params string[] secretNames)
        {
            var tasks = secretNames.Select(async name => new
            {
                Name = name,
                Value = await GetSecretAsync(name)
            });

            var results = await Task.WhenAll(tasks);
            
            return results.ToDictionary(r => r.Name, r => r.Value);
        }

        /// <summary>
        /// Checks if a secret exists without retrieving its value
        /// </summary>
        public async Task<bool> SecretExistsAsync(string secretName)
        {
            try
            {
                await _secretClient.GetSecretAsync(secretName);
                return true;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                return false;
            }
        }

        /// <summary>
        /// Lists all secret names (for administrative purposes)
        /// </summary>
        public async Task<List<string>> ListSecretNamesAsync()
        {
            try
            {
                var secretNames = new List<string>();
                
                await foreach (var secretProperties in _secretClient.GetPropertiesOfSecretsAsync())
                {
                    secretNames.Add(secretProperties.Name);
                }
                
                _logger.LogInformation("Found {SecretCount} secrets in KeyVault", secretNames.Count);
                return secretNames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing secrets from KeyVault");
                throw;
            }
        }

        /// <summary>
        /// Clears the secret cache
        /// </summary>
        public void ClearCache()
        {
            if (_cache is MemoryCache memoryCache)
            {
                // Clear all cached secrets
                var field = typeof(MemoryCache).GetField("_coherentState", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var coherentState = field?.GetValue(memoryCache);
                var entriesCollection = coherentState?.GetType()
                    .GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var entries = (IDictionary?)entriesCollection?.GetValue(coherentState);
                
                if (entries != null)
                {
                    var keysToRemove = new List<object>();
                    foreach (DictionaryEntry entry in entries)
                    {
                        if (entry.Key.ToString()?.StartsWith("keyvault-secret-") == true)
                        {
                            keysToRemove.Add(entry.Key);
                        }
                    }
                    
                    foreach (var key in keysToRemove)
                    {
                        _cache.Remove(key);
                    }
                }
            }
            
            _logger.LogInformation("KeyVault secret cache cleared");
        }
    }
}
```

## Azure Deployment Configuration

### 1. Managed Identity Setup

**Azure CLI Commands:**

```bash
# Create managed identity for the app service
az identity create \
    --resource-group rg-spinnernet-proto \
    --name spinnernet-app-identity

# Get the principal ID
PRINCIPAL_ID=$(az identity show \
    --resource-group rg-spinnernet-proto \
    --name spinnernet-app-identity \
    --query principalId \
    --output tsv)

# Assign managed identity to app service
az webapp identity assign \
    --resource-group rg-spinnernet-proto \
    --name spinnernet-app-3lauxg \
    --identities /subscriptions/{subscription-id}/resourcegroups/rg-spinnernet-proto/providers/Microsoft.ManagedIdentity/userAssignedIdentities/spinnernet-app-identity

# Grant KeyVault access to managed identity
az keyvault set-policy \
    --name kv-spinnernet-3lauxg \
    --resource-group rg-spinnernet-proto \
    --object-id $PRINCIPAL_ID \
    --secret-permissions get list
```

### 2. KeyVault Secret Creation

```bash
# Create OpenAI API key secret
az keyvault secret set \
    --vault-name kv-spinnernet-3lauxg \
    --name "OpenAI-API-Key" \
    --value "your-openai-api-key-here"

# Create Cosmos DB connection string secret
az keyvault secret set \
    --vault-name kv-spinnernet-3lauxg \
    --name "CosmosDB-ConnectionString" \
    --value "your-cosmos-connection-string"

# Verify secrets exist
az keyvault secret list \
    --vault-name kv-spinnernet-3lauxg \
    --query "[].name" \
    --output table
```

### 3. App Service Configuration

```bash
# Configure app service to use managed identity
az webapp config appsettings set \
    --resource-group rg-spinnernet-proto \
    --name spinnernet-app-3lauxg \
    --settings \
        "Azure__KeyVault__Name=kv-spinnernet-3lauxg" \
        "AZURE_CLIENT_ID=$(az identity show --resource-group rg-spinnernet-proto --name spinnernet-app-identity --query clientId --output tsv)"
```

## Local Development Setup

### 1. Environment Variables

**File:** `.env` (local development)

```bash
# Azure KeyVault configuration
AZURE_KEYVAULT_NAME=kv-spinnernet-3lauxg

# Azure CLI authentication (for local development)
AZURE_TENANT_ID=your-tenant-id
AZURE_CLIENT_ID=your-client-id
AZURE_CLIENT_SECRET=your-client-secret

# Fallback for development
OPENAI_API_KEY=your-local-openai-key
```

### 2. Azure CLI Login

```bash
# Login to Azure CLI for local development
az login

# Set subscription if needed
az account set --subscription "your-subscription-id"

# Verify access to KeyVault
az keyvault secret show \
    --vault-name kv-spinnernet-3lauxg \
    --name "OpenAI-API-Key" \
    --query "value" \
    --output tsv
```

### 3. Visual Studio Configuration

**File:** `Properties/launchSettings.json`

```json
{
  "profiles": {
    "SpinnerNet.App": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7299;http://localhost:5299",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "AZURE_KEYVAULT_NAME": "kv-spinnernet-3lauxg"
      }
    }
  }
}
```

## Security Best Practices

### 1. Principle of Least Privilege

```bash
# Create custom KeyVault role with minimal permissions
az role definition create --role-definition '{
    "Name": "SpinnerNet KeyVault Reader",
    "Description": "Read-only access to SpinnerNet secrets",
    "Actions": [],
    "DataActions": [
        "Microsoft.KeyVault/vaults/secrets/getSecret/action"
    ],
    "AssignableScopes": ["/subscriptions/{subscription-id}/resourceGroups/rg-spinnernet-proto"]
}'

# Assign the custom role
az role assignment create \
    --assignee $PRINCIPAL_ID \
    --role "SpinnerNet KeyVault Reader" \
    --scope "/subscriptions/{subscription-id}/resourceGroups/rg-spinnernet-proto/providers/Microsoft.KeyVault/vaults/kv-spinnernet-3lauxg"
```

### 2. Network Security

```bash
# Configure KeyVault firewall (production)
az keyvault network-rule add \
    --name kv-spinnernet-3lauxg \
    --resource-group rg-spinnernet-proto \
    --ip-address "your-app-service-outbound-ip"

# Enable private endpoint (enterprise)
az network private-endpoint create \
    --resource-group rg-spinnernet-proto \
    --name kv-spinnernet-3lauxg-pe \
    --vnet-name your-vnet \
    --subnet your-subnet \
    --private-connection-resource-id "/subscriptions/{subscription-id}/resourceGroups/rg-spinnernet-proto/providers/Microsoft.KeyVault/vaults/kv-spinnernet-3lauxg" \
    --group-id vault \
    --connection-name kv-connection
```

### 3. Audit and Monitoring

```bash
# Enable KeyVault logging
az monitor diagnostic-settings create \
    --resource "/subscriptions/{subscription-id}/resourceGroups/rg-spinnernet-proto/providers/Microsoft.KeyVault/vaults/kv-spinnernet-3lauxg" \
    --name "keyvault-audit" \
    --logs '[{"category":"AuditEvent","enabled":true}]' \
    --workspace "/subscriptions/{subscription-id}/resourceGroups/rg-spinnernet-proto/providers/Microsoft.OperationalInsights/workspaces/your-workspace"
```

## Testing and Validation

### 1. Integration Tests

```csharp
[Test]
public async Task KeyVaultService_ShouldRetrieveSecret()
{
    // Arrange
    var keyVaultService = GetKeyVaultService();
    
    // Act
    var secret = await keyVaultService.GetSecretAsync("OpenAI-API-Key");
    
    // Assert
    Assert.That(secret, Is.Not.Null.And.Not.Empty);
    Assert.That(secret.StartsWith("sk-"), Is.True);
}

[Test]
public async Task KeyVaultService_ShouldHandleNotFoundSecret()
{
    // Arrange
    var keyVaultService = GetKeyVaultService();
    
    // Act & Assert
    var ex = await Assert.ThrowsAsync<InvalidOperationException>(
        () => keyVaultService.GetSecretAsync("NonExistentSecret"));
    
    Assert.That(ex.Message, Contains.Substring("not found"));
}
```

### 2. Health Checks

```csharp
// Add KeyVault health check
builder.Services.AddHealthChecks()
    .AddAzureKeyVault(options =>
    {
        options.VaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
        options.Credential = new DefaultAzureCredential();
    });

app.MapHealthChecks("/health/keyvault");
```

### 3. Monitoring

```csharp
// Add custom metrics for KeyVault operations
public class KeyVaultMetrics
{
    private readonly IMetrics _metrics;
    
    public KeyVaultMetrics(IMetrics metrics)
    {
        _metrics = metrics;
    }
    
    public void RecordSecretRetrieval(string secretName, bool success, TimeSpan duration)
    {
        _metrics.CreateCounter<int>("keyvault_secret_retrievals_total")
            .Add(1, new("secret_name", secretName), new("success", success.ToString()));
            
        _metrics.CreateHistogram<double>("keyvault_secret_retrieval_duration_ms")
            .Record(duration.TotalMilliseconds, new("secret_name", secretName));
    }
}
```

## Troubleshooting

### Common Issues

1. **Authentication Failures**
   ```
   Azure.Identity.AuthenticationFailedException: No credentials available
   ```
   **Solution:** Ensure DefaultAzureCredential can find valid credentials (Azure CLI, Managed Identity, etc.)

2. **Permission Denied**
   ```
   Azure.RequestFailedException: Access denied (403)
   ```
   **Solution:** Verify managed identity has "Key Vault Secrets User" role

3. **Secret Not Found**
   ```
   Azure.RequestFailedException: Not found (404)
   ```
   **Solution:** Check secret name spelling and ensure it exists in KeyVault

### Diagnostic Commands

```bash
# Test KeyVault connectivity
az keyvault secret show --vault-name kv-spinnernet-3lauxg --name "OpenAI-API-Key"

# Check managed identity assignment
az webapp identity show --resource-group rg-spinnernet-proto --name spinnernet-app-3lauxg

# Verify KeyVault permissions
az keyvault show --name kv-spinnernet-3lauxg --query "properties.accessPolicies"
```

## Results

✅ **Zero secrets in code** - All sensitive data in KeyVault  
✅ **Managed identity authentication** - Seamless Azure integration  
✅ **Role-based access control** - Granular permissions  
✅ **Audit logging** - Complete access tracking  
✅ **Production-ready security** - Enterprise-grade secret management  

**Live Demo:** https://spinnernet-app-3lauxg.azurewebsites.net/ai-interview-hybrid

The Azure KeyVault integration ensures enterprise-grade security for the hybrid AI architecture, with zero secrets exposed in code and comprehensive audit capabilities.