using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using SpinnerNet.App.Components;
using MudBlazor.Services;
using Microsoft.AspNetCore.Antiforgery;
// using Microsoft.AspNetCore.ResponseCompression; // Temporarily disabled
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using SpinnerNet.Shared.Localization;
using SpinnerNet.Core.Extensions;
using SpinnerNet.Core.Data.CosmosDb;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.Logging.Console;

var builder = WebApplication.CreateBuilder(args);

// Configure comprehensive logging with OpenTelemetry Azure Monitor
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.FormatterName = ConsoleFormatterNames.Simple;
});
builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
    options.SingleLine = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});
builder.Logging.AddDebug();

// Set log levels for debugging
if (builder.Environment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
    builder.Logging.AddFilter("SpinnerNet", LogLevel.Debug);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
    builder.Logging.AddFilter("SpinnerNet", LogLevel.Information);
}

builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore.Components", LogLevel.Information); // Enable Blazor logging

// Add OpenTelemetry with Azure Monitor (Microsoft's recommended approach)
builder.Services.AddOpenTelemetry()
    .UseAzureMonitor(options =>
    {
        // Connection string will be set from Key Vault or environment
        if (!string.IsNullOrEmpty(builder.Configuration["ApplicationInsights:ConnectionString"]))
        {
            options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
        }
    });

// Configure Azure credentials for Key Vault access
var credential = new DefaultAzureCredential();

// Get configuration from Key Vault
string openAiApiKey = builder.Configuration["OpenAI:ApiKey"] 
    ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    ?? "";
string azureAdClientId = builder.Configuration["AzureAd:ClientId"] ?? "";
string azureAdClientSecret = builder.Configuration["AzureAd:ClientSecret"] ?? "";
string keyVaultStatus = "Using local configuration";

try
{
    var keyVaultName = builder.Configuration["Azure:KeyVault:Name"];
    if (!string.IsNullOrEmpty(keyVaultName))
    {
        var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
        var secretClient = new SecretClient(keyVaultUri, credential);
    
        try
        {
            // Get OpenAI API key
            var openAiSecret = await secretClient.GetSecretAsync("OpenAI-ApiKey");
            openAiApiKey = openAiSecret.Value.Value;
            
            // Get Azure AD Client ID
            var azureAdClientIdSecret = await secretClient.GetSecretAsync("AzureAd-ClientId");
            azureAdClientId = azureAdClientIdSecret.Value.Value;
            
            // Get Azure AD Client Secret
            var azureAdClientSecretValue = await secretClient.GetSecretAsync("AzureAd-ClientSecret");
            azureAdClientSecret = azureAdClientSecretValue.Value.Value;
            
            // Get Cosmos DB connection string
            try
            {
                var cosmosConnectionSecret = await secretClient.GetSecretAsync("CosmosDb-ConnectionString");
                builder.Configuration["CosmosDb:ConnectionString"] = cosmosConnectionSecret.Value.Value;
                builder.Configuration["CosmosDb:DatabaseName"] = "SpinnerNet";
            }
            catch (Exception ex)
            {
                var cosmosLogger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
                cosmosLogger.LogWarning(ex, "Cosmos DB connection string not found in Key Vault");
                // Use development connection string if available
                builder.Configuration["CosmosDb:ConnectionString"] = builder.Configuration.GetConnectionString("CosmosDb") ?? "";
                builder.Configuration["CosmosDb:DatabaseName"] = "SpinnerNet";
            }
            
            // Get Application Insights connection string for OpenTelemetry
            try
            {
                var appInsightsSecret = await secretClient.GetSecretAsync("ApplicationInsights-ConnectionString");
                builder.Configuration["ApplicationInsights:ConnectionString"] = appInsightsSecret.Value.Value;
            }
            catch (Exception ex)
            {
                var aiLogger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
                aiLogger.LogWarning(ex, "Application Insights connection string not found in Key Vault");
                // Use environment variable or config file fallback
                builder.Configuration["ApplicationInsights:ConnectionString"] = 
                    Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING") ?? 
                    builder.Configuration["ApplicationInsights:ConnectionString"] ?? "";
            }
        }
        catch (Exception ex)
        {
            var secretLogger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
            secretLogger.LogWarning(ex, "Key Vault secret retrieval failed");
            // Fall back to config/env vars if secrets not found
        }
        keyVaultStatus = "✅ Key Vault access successful";
        var kvLogger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
        kvLogger.LogInformation(keyVaultStatus);
    }
}
catch (Exception ex)
{
    keyVaultStatus = $"❌ Key Vault access failed: {ex.Message}";
    var kvErrorLogger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
    kvErrorLogger.LogError(ex, "Key Vault access failed");
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add response compression for SignalR (recommended by Microsoft)  
// Temporarily disabled due to Azure startup issues
// builder.Services.AddResponseCompression(opts =>
// {
//     opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
//         ["application/octet-stream"]);
// });

// Add SignalR for Blazor Server with Azure-friendly configuration
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Blazor Server handles its own SignalR connection - no manual CORS needed for /_blazor
// Only add CORS if we have custom SignalR hubs later
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });
}

// Add controllers for authentication endpoints (with anti-forgery configuration)
builder.Services.AddControllers(options =>
{
    // Disable anti-forgery for API controllers by default
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute());
});

// Add MudBlazor services
builder.Services.AddMudServices();

// Configure anti-forgery tokens for .NET 9 (skip for API endpoints)
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "__SpinnerNet-App-CSRF";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Add required services
builder.Services.AddHttpContextAccessor();

// Add localization services
builder.Services.AddScoped<ILocalizationService, LocalizationService>();

// Add Azure credentials for Key Vault
builder.Services.AddSingleton<DefaultAzureCredential>();

// Add Core services (MediatR, FluentValidation, etc.)
builder.Services.AddVerticalSliceServices();

// Add Cosmos DB services
builder.Services.AddCosmosDb(builder.Configuration);

// Configure Azure AD settings dynamically
builder.Configuration["AzureAd:ClientId"] = azureAdClientId;
builder.Configuration["AzureAd:ClientSecret"] = azureAdClientSecret;

// Add authentication services
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "OpenIdConnect";
})
.AddMicrosoftIdentityWebApp(options =>
{
    builder.Configuration.GetSection("AzureAd").Bind(options);
    options.ClientId = azureAdClientId;
    options.ClientSecret = azureAdClientSecret;
});

// Add Razor Pages for authentication UI
builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

builder.Services.AddAuthorizationCore();

// Add authentication services for Blazor
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 Spinner.Net App starting...");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Always enable developer exception page for troubleshooting
app.UseDeveloperExceptionPage();

// Add response compression before static files (Microsoft recommendation)
// Temporarily disabled due to Azure startup issues
// app.UseResponseCompression();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Only use CORS in development
if (app.Environment.IsDevelopment())
{
    app.UseCors();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Add health check endpoint
app.MapGet("/health", () => $"OK - Spinner.Net App is running! Key Vault: {keyVaultStatus}");

// Add anti-forgery token endpoint for JavaScript/AJAX requests
app.MapGet("/antiforgery/token", (IAntiforgery antiforgery, HttpContext context) =>
{
    var tokens = antiforgery.GetAndStoreTokens(context);
    return Results.Ok(new { token = tokens.RequestToken });
});

logger.LogInformation("🗺️ Configuring Blazor component mappings...");

app.MapStaticAssets();

// Map Blazor components - Blazor Server handles its own SignalR hub automatically
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
    
// Note: No need to manually map /_blazor hub - Blazor Server does this automatically

// Map controllers and Razor Pages for authentication
app.MapControllers();
app.MapRazorPages();

logger.LogInformation("🚀 Spinner.Net App ready");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

app.Run();
