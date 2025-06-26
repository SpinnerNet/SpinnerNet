using SpinnerNet.Shared.Localization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Antiforgery;
using System.Globalization;
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
// Get configuration from environment variables or Key Vault
string cosmosConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? Environment.GetEnvironmentVariable("COSMOS_DB_CONNECTION_STRING")
    ?? "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="; // Cosmos DB Emulator - safe for development only
string keyVaultStatus = "Using local configuration";

try
{
    var keyVaultname = builder.Configuration["Azure:KeyVault:Name"];
    if (!string.IsNullOrEmpty(keyVaultName))
    {
        var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
        var secretClient = new SecretClient(keyVaultUri, credential);
    
        var secret = await secretClient.GetSecretAsync("CosmosDb-ConnectionString");
        cosmosConnectionString = secret.Value.Value;
        keyVaultStatus = "✅ Key Vault access successful";
        Console.WriteLine(keyVaultStatus);
    }
}
catch (Exception ex)
{
    keyVaultStatus = $"❌ Key Vault access failed: {ex.Message}";
    Console.WriteLine(keyVaultStatus);
}

// Configure localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "de", "en" };
    options.SetDefaultCulture("de")
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
    
    options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());
});
builder.Services.AddScoped<ILocalizationService, LocalizationService>();

// Configure routing options
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// Add services to the container
builder.Services.AddRazorPages();

// Configure anti-forgery tokens for .NET 9
builder.Services.AddAntiforgery(options =>
{
    options.Headername = "X-CSRF-TOKEN";
    options.Cookie.name = "__SpinnerNet-CSRF";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddHttpContextAccessor();

// Add Azure credentials for Key Vault only
builder.Services.AddSingleton<DefaultAzureCredential>();

// Configure authentication for public site
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorizationCore();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 Spinner.Net application starting...");

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseRequestLocalization();

// Add health check endpoint
app.MapGet("/health", () => $"OK - Bamberger Spinnerei Public Site is running! Key Vault: {keyVaultStatus}");

// Add anti-forgery token endpoint for JavaScript/AJAX requests
app.MapGet("/antiforgery/token", (IAntiforgery antiforgery, HttpContext context) =>
{
    var tokens = antiforgery.GetAndStoreTokens(context);
    return Results.Ok(new { token = tokens.RequestToken });
});

logger.LogInformation("🗺️ Configuring endpoint mappings...");

app.MapRazorPages();

logger.LogInformation("🚀 Bamberger Spinnerei public website ready");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

app.Run();


