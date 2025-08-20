# Services - Dependency Injection Patterns

## Problem Statement
Properly registering services in Blazor with correct lifetimes, avoiding namespace conflicts, and ensuring services are available where needed.

## Working Solution

### Basic Service Registration
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Scoped services (per connection/circuit in Blazor Server)
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddScoped<IAgeAdaptiveThemeService, AgeAdaptiveThemeService>();

// Singleton services (shared across all users)
builder.Services.AddSingleton<DefaultAzureCredential>();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Transient services (new instance each time)
builder.Services.AddTransient<IEmailService, EmailService>();
```

### Namespace-Specific Registration
```csharp
// When you have naming conflicts, use full namespace
builder.Services.AddScoped<SpinnerNet.App.Services.WebLLM.IWebLLMService, 
                          SpinnerNet.App.Services.WebLLM.WebLLMService>();

// Alternative: Using alias
using WebLLMService = SpinnerNet.App.Services.WebLLM.WebLLMService;
builder.Services.AddScoped<IWebLLMService, WebLLMService>();
```

### Factory Pattern Registration
```csharp
// For complex initialization
builder.Services.AddScoped<IWebLLMService>(provider =>
{
    var jsRuntime = provider.GetRequiredService<IJSRuntime>();
    var logger = provider.GetRequiredService<ILogger<WebLLMService>>();
    var config = provider.GetRequiredService<IConfiguration>();
    
    return new WebLLMService(jsRuntime, logger)
    {
        ModelId = config["WebLLM:ModelId"] ?? "Llama-3.2-1B"
    };
});
```

### Options Pattern
```csharp
// Configure options
builder.Services.Configure<WebLLMOptions>(
    builder.Configuration.GetSection("WebLLM"));

// Service using options
public class WebLLMService : IWebLLMService
{
    private readonly WebLLMOptions _options;
    
    public WebLLMService(IOptions<WebLLMOptions> options)
    {
        _options = options.Value;
    }
}
```

## Service Lifetime Patterns

### Scoped Service Example
```csharp
// Perfect for per-user state in Blazor
public interface IAgeAdaptiveThemeService
{
    MudTheme GetThemeForAge(int age);
    Task<MudTheme> GetThemeAsync(int age, string? preferredTheme = null);
}

public class AgeAdaptiveThemeService : IAgeAdaptiveThemeService
{
    private readonly ILogger<AgeAdaptiveThemeService> _logger;
    private MudTheme? _cachedTheme;
    private int? _cachedAge;

    public AgeAdaptiveThemeService(ILogger<AgeAdaptiveThemeService> logger)
    {
        _logger = logger;
    }

    public MudTheme GetThemeForAge(int age)
    {
        // Cache per circuit/connection
        if (_cachedAge == age && _cachedTheme != null)
        {
            return _cachedTheme;
        }

        _cachedTheme = CreateThemeForAge(age);
        _cachedAge = age;
        return _cachedTheme;
    }
}

// Registration
builder.Services.AddScoped<IAgeAdaptiveThemeService, AgeAdaptiveThemeService>();
```

### Singleton Service Example
```csharp
// Shared across all users - be careful with state!
public interface IModelCacheService
{
    Task<byte[]> GetModelAsync(string modelId);
}

public class ModelCacheService : IModelCacheService
{
    private readonly ConcurrentDictionary<string, byte[]> _cache = new();
    
    public async Task<byte[]> GetModelAsync(string modelId)
    {
        return await _cache.GetOrAddAsync(modelId, 
            async (key) => await DownloadModelAsync(key));
    }
}

// Registration
builder.Services.AddSingleton<IModelCacheService, ModelCacheService>();
```

## Advanced Patterns

### Service Collection Extensions
```csharp
// Extensions/ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSpinnerNetServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Core services
        services.AddScoped<ILocalizationService, LocalizationService>();
        services.AddScoped<IAgeAdaptiveThemeService, AgeAdaptiveThemeService>();
        
        // WebLLM services
        services.AddScoped<IWebLLMService, WebLLMService>();
        services.Configure<WebLLMOptions>(configuration.GetSection("WebLLM"));
        
        // Feature services
        services.AddScoped<IPersonaService, PersonaService>();
        
        return services;
    }
}

// Usage in Program.cs
builder.Services.AddSpinnerNetServices(builder.Configuration);
```

### Conditional Registration
```csharp
// Register based on environment
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IEmailService, MockEmailService>();
}
else
{
    builder.Services.AddScoped<IEmailService, SendGridEmailService>();
}

// Feature flag based
if (builder.Configuration.GetValue<bool>("Features:EnableWebLLM"))
{
    builder.Services.AddScoped<IWebLLMService, WebLLMService>();
}
else
{
    builder.Services.AddScoped<IWebLLMService, MockWebLLMService>();
}
```

### Generic Repository Pattern
```csharp
// Generic repository registration
builder.Services.AddScoped(typeof(ICosmosRepository<>), typeof(CosmosRepository<>));

// Specific implementations
builder.Services.AddScoped<ICosmosRepository<UserDocument>, CosmosRepository<UserDocument>>();
builder.Services.AddScoped<ICosmosRepository<PersonaDocument>, CosmosRepository<PersonaDocument>>();
```

## Common Errors & Solutions

### ❌ Error: Service Not Registered
```csharp
// Component tries to inject unregistered service
@inject IWebLLMService WebLLMService  // InvalidOperationException
```

### ✅ Solution: Register in Program.cs
```csharp
builder.Services.AddScoped<IWebLLMService, WebLLMService>();
```

### ❌ Error: Wrong Lifetime
```csharp
// Singleton depending on Scoped
public class SingletonService
{
    // WRONG: Can't inject scoped into singleton
    public SingletonService(IScopedService scoped) { }
}
```

### ✅ Solution: Use Factory or IServiceProvider
```csharp
public class SingletonService
{
    private readonly IServiceProvider _provider;
    
    public SingletonService(IServiceProvider provider)
    {
        _provider = provider;
    }
    
    public void DoWork()
    {
        using var scope = _provider.CreateScope();
        var scoped = scope.ServiceProvider.GetRequiredService<IScopedService>();
        // Use scoped service
    }
}
```

### ❌ Error: Circular Dependency
```csharp
// ServiceA depends on ServiceB, ServiceB depends on ServiceA
public ServiceA(IServiceB b) { }
public ServiceB(IServiceA a) { }  // Circular dependency!
```

### ✅ Solution: Refactor or Use Factory
```csharp
// Option 1: Extract shared functionality
public interface ISharedService { }
public class ServiceA(ISharedService shared) { }
public class ServiceB(ISharedService shared) { }

// Option 2: Lazy resolution
public class ServiceA
{
    private readonly Lazy<IServiceB> _serviceB;
    public ServiceA(IServiceProvider provider)
    {
        _serviceB = new Lazy<IServiceB>(() => 
            provider.GetRequiredService<IServiceB>());
    }
}
```

## Testing Services

### Unit Testing with DI
```csharp
[Test]
public void ServiceRegistration_ShouldResolveCorrectly()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddScoped<IAgeAdaptiveThemeService, AgeAdaptiveThemeService>();
    services.AddLogging();
    
    var provider = services.BuildServiceProvider();
    
    // Act
    var service = provider.GetService<IAgeAdaptiveThemeService>();
    
    // Assert
    Assert.NotNull(service);
    Assert.IsType<AgeAdaptiveThemeService>(service);
}
```

## Best Practices

1. **Use interfaces** for all services (testability)
2. **Avoid ServiceLocator pattern** (use constructor injection)
3. **Keep services focused** (single responsibility)
4. **Be careful with scoped services** in background tasks
5. **Log service initialization** for debugging
6. **Use options pattern** for configuration
7. **Create service extensions** for complex registration

## Performance Considerations

- **Scoped**: New instance per request/circuit (minimal overhead)
- **Transient**: New instance every time (can be expensive)
- **Singleton**: One instance forever (memory considerations)
- **Factory delegates**: Add slight overhead but enable flexibility

## Related Patterns
- [02-Async-Lifecycle.md] - Service disposal patterns
- [03-Event-Handling.md] - Service event systems
- [04-State-Management.md] - Managing service state
- [../Azure/03-KeyVault-Config.md] - Configuration patterns

## Production Checklist
- ✅ All services registered with correct lifetime
- ✅ No circular dependencies
- ✅ Configuration properly injected
- ✅ Logging available in all services
- ✅ Services are testable (use interfaces)
- ✅ Async disposal implemented where needed