using Microsoft.Identity.Web;
using SpinnerNet.App.Components;
using MudBlazor.Services;
using Microsoft.AspNetCore.Antiforgery;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

var builder = WebApplication.CreateBuilder(args);

// Configure comprehensive logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Configure Azure credentials for Key Vault access
var credential = new DefaultAzureCredential();

// Get configuration from Key Vault
string openAiApiKey = builder.Configuration["OpenAI:ApiKey"] 
    ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    ?? "";
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
            var secret = await secretClient.GetSecretAsync("OpenAI-ApiKey");
            openAiApiKey = secret.Value.Value;
        }
        catch (Exception)
        {
            // Fall back to config/env vars if secret not found
        }
        keyVaultStatus = "✅ Key Vault access successful";
        Console.WriteLine(keyVaultStatus);
    }
}
catch (Exception ex)
{
    keyVaultStatus = $"❌ Key Vault access failed: {ex.Message}";
    Console.WriteLine(keyVaultStatus);
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add SignalR for Blazor Server
builder.Services.AddSignalR();

// Add CORS for SignalR
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://spinnernet-app-3lauxg.azurewebsites.net")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add controllers for authentication endpoints
builder.Services.AddControllers();

// Add MudBlazor services
builder.Services.AddMudServices();

// Configure anti-forgery tokens for .NET 9
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

// Add Azure credentials for Key Vault
builder.Services.AddSingleton<DefaultAzureCredential>();

// Add authentication services
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "OpenIdConnect";
})
.AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors();

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
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map controllers for authentication
app.MapControllers();

logger.LogInformation("🚀 Spinner.Net App ready");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

app.Run();
