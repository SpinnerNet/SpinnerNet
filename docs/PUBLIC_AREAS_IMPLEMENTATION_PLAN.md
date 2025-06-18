# Public Areas Implementation Plan

## Executive Summary

The current Azure deployment broke because of routing conflicts between public Razor Pages and the Blazor app. This plan provides a clean separation architecture that supports multiple public websites per persona with custom domain forwarding.

## Immediate Fix (Today)

### 1. Deploy Routing Fix
```bash
cd Spinner.Net-Public/src
dotnet publish SpinnerNet.Web/SpinnerNet.Web.csproj -c Release -o publish
cd publish
zip -r ../spinnernet-fix.zip .
cd ..
az webapp deploy --resource-group rg-spinnernet-proto \
  --name spinnernet-3lauxg \
  --src-path spinnernet-fix.zip \
  --type zip
```

### 2. Verify Current Structure Works
- Public pages: `/`, `/about`, `/contact`, `/beta`, `/support`
- Blazor app: `/app/*`
- Static assets: `/css/*`, `/js/*`, `/images/*`

## Phase 1: Public Areas Foundation (Week 1)

### 1. Create SpinnerNet.PublicAreas Project

```bash
cd Spinner.Net-Public/src
dotnet new classlib -n SpinnerNet.PublicAreas
dotnet sln add SpinnerNet.PublicAreas/SpinnerNet.PublicAreas.csproj
```

### 2. Core Models
```csharp
// SpinnerNet.PublicAreas/Models/PublicSite.cs
namespace SpinnerNet.PublicAreas.Models;

public class PublicSite
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PersonaId { get; set; }
    public string Slug { get; set; }
    public string Title { get; set; }
    public string? CustomDomain { get; set; }
    public string Theme { get; set; } = "default";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<PublicPage> Pages { get; set; } = new();
    public Dictionary<string, object> Settings { get; set; } = new();
}

public class PublicPage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SiteId { get; set; }
    public string Slug { get; set; }
    public string Title { get; set; }
    public string Content { get; set; } = "";
    public string Layout { get; set; } = "default";
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; } = true;
    public Dictionary<string, object> MetaData { get; set; } = new();
}
```

### 3. Routing Middleware
```csharp
// SpinnerNet.PublicAreas/Middleware/PublicAreaRoutingMiddleware.cs
public class PublicAreaRoutingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IPublicAreaService _publicAreaService;

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.TrimStart('/') ?? "";
        
        // Check if this is a persona site request
        if (path.StartsWith("p/"))
        {
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 2)
            {
                var slug = segments[1];
                var site = await _publicAreaService.GetSiteBySlugAsync(slug);
                
                if (site != null && site.IsActive)
                {
                    context.Items["PublicSite"] = site;
                    context.Items["PublicPath"] = string.Join("/", segments.Skip(2));
                    await _publicAreaService.RenderSiteAsync(context, site);
                    return;
                }
            }
        }
        
        await _next(context);
    }
}
```

### 4. Update Program.cs
```csharp
// Add public areas services
builder.Services.AddScoped<IPublicAreaService, PublicAreaService>();
builder.Services.AddScoped<IPublicAreaRepository, PublicAreaRepository>();

// Configure middleware pipeline
app.UseWhen(context => 
    !context.Request.Path.StartsWithSegments("/app") &&
    !context.Request.Path.StartsWithSegments("/api"),
    publicApp =>
    {
        publicApp.UseMiddleware<CustomDomainMiddleware>();
        publicApp.UseMiddleware<PublicAreaRoutingMiddleware>();
    });

// Keep existing Blazor routing
app.MapWhen(context => 
    context.Request.Path.StartsWithSegments("/app"),
    blazorApp =>
    {
        blazorApp.UseRouting();
        blazorApp.MapBlazorHub();
        blazorApp.MapFallbackToPage("/app/{*path:nonfile}", "/_Host");
    });
```

## Phase 2: Site Management UI (Week 2)

### 1. Create Management Features
```csharp
// SpinnerNet.Core/Features/PublicSites/CreatePublicSite.cs
public static class CreatePublicSite
{
    public record Command : IRequest<Result>
    {
        public string PersonaId { get; init; }
        public string Title { get; init; }
        public string Slug { get; init; }
        public string? Description { get; init; }
    }
    
    public record Result
    {
        public bool Success { get; init; }
        public string? SiteId { get; init; }
        public string? Error { get; init; }
    }
    
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Slug)
                .NotEmpty()
                .Matches(@"^[a-z0-9-]+$")
                .WithMessage("Slug can only contain lowercase letters, numbers, and hyphens");
        }
    }
    
    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly IPublicAreaRepository _repository;
        
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            // Check if slug is already taken
            var existing = await _repository.GetBySlugAsync(request.Slug);
            if (existing != null)
                return new Result { Success = false, Error = "Slug already in use" };
            
            var site = new PublicSite
            {
                PersonaId = request.PersonaId,
                Title = request.Title,
                Slug = request.Slug
            };
            
            // Add default home page
            site.Pages.Add(new PublicPage
            {
                Slug = "home",
                Title = "Home",
                Content = $"# Welcome to {request.Title}\n\nThis is your new public site."
            });
            
            await _repository.CreateAsync(site);
            
            return new Result { Success = true, SiteId = site.Id };
        }
    }
}
```

### 2. Blazor Management Component
```razor
@* SpinnerNet.Components/Components/PublicSites/SiteManager.razor *@
@page "/app/sites"

<MudContainer>
    <MudText Typo="Typo.h4">My Public Sites</MudText>
    
    <MudButton Variant="Variant.Filled" Color="Color.Primary" 
               OnClick="ShowCreateDialog">
        Create New Site
    </MudButton>
    
    <MudGrid Class="mt-4">
        @foreach (var site in Sites)
        {
            <MudItem xs="12" sm="6" md="4">
                <MudCard>
                    <MudCardContent>
                        <MudText Typo="Typo.h6">@site.Title</MudText>
                        <MudText Typo="Typo.body2">/@site.Slug</MudText>
                        @if (!string.IsNullOrEmpty(site.CustomDomain))
                        {
                            <MudText Typo="Typo.caption">@site.CustomDomain</MudText>
                        }
                    </MudCardContent>
                    <MudCardActions>
                        <MudButton Variant="Variant.Text" 
                                   Href="@($"/p/{site.Slug}")" 
                                   Target="_blank">View</MudButton>
                        <MudButton Variant="Variant.Text" 
                                   OnClick="@(() => EditSite(site.Id))">Edit</MudButton>
                    </MudCardActions>
                </MudCard>
            </MudItem>
        }
    </MudGrid>
</MudContainer>
```

## Phase 3: Custom Domain Support (Week 3)

### 1. Domain Middleware
```csharp
// SpinnerNet.PublicAreas/Middleware/CustomDomainMiddleware.cs
public class CustomDomainMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IPublicAreaService _publicAreaService;
    private readonly IMemoryCache _cache;

    public async Task InvokeAsync(HttpContext context)
    {
        var host = context.Request.Host.Host;
        
        // Skip for main domain
        if (host.Contains("bamberger-spinnerei.org") || 
            host.Contains("azurewebsites.net") || 
            host == "localhost")
        {
            await _next(context);
            return;
        }
        
        // Check cache first
        if (_cache.TryGetValue<PublicSite>($"domain:{host}", out var cachedSite))
        {
            context.Items["PublicSite"] = cachedSite;
            context.Items["IsCustomDomain"] = true;
            await _publicAreaService.RenderSiteAsync(context, cachedSite);
            return;
        }
        
        // Look up site by domain
        var site = await _publicAreaService.GetSiteByDomainAsync(host);
        if (site != null && site.IsActive)
        {
            _cache.Set($"domain:{host}", site, TimeSpan.FromMinutes(5));
            context.Items["PublicSite"] = site;
            context.Items["IsCustomDomain"] = true;
            await _publicAreaService.RenderSiteAsync(context, site);
            return;
        }
        
        await _next(context);
    }
}
```

### 2. Domain Configuration
```csharp
// SpinnerNet.Core/Features/PublicSites/ConfigureCustomDomain.cs
public static class ConfigureCustomDomain
{
    public record Command : IRequest<Result>
    {
        public string SiteId { get; init; }
        public string Domain { get; init; }
        public string PersonaId { get; init; }
    }
    
    public class Handler : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            // Verify ownership
            var site = await _repository.GetByIdAsync(request.SiteId);
            if (site?.PersonaId != request.PersonaId)
                return new Result { Success = false, Error = "Unauthorized" };
            
            // Check if domain is already used
            var existing = await _repository.GetByDomainAsync(request.Domain);
            if (existing != null && existing.Id != request.SiteId)
                return new Result { Success = false, Error = "Domain already in use" };
            
            // Generate verification code
            var verificationCode = GenerateVerificationCode();
            
            // Store pending verification
            await _verificationService.CreateVerificationAsync(new DomainVerification
            {
                Domain = request.Domain,
                SiteId = request.SiteId,
                VerificationCode = verificationCode,
                Method = "DNS_TXT"
            });
            
            return new Result 
            { 
                Success = true, 
                VerificationCode = verificationCode,
                Instructions = $"Add TXT record: spinnerei-verify={verificationCode}"
            };
        }
    }
}
```

## Phase 4: Theme System (Week 4)

### 1. Theme Structure
```
/wwwroot/themes/
├── default/
│   ├── theme.json
│   ├── layouts/
│   │   ├── default.cshtml
│   │   └── landing.cshtml
│   ├── components/
│   │   ├── header.cshtml
│   │   └── footer.cshtml
│   └── assets/
│       ├── styles.css
│       └── theme.js
├── photography/
├── workshop/
└── minimal/
```

### 2. Theme Renderer
```csharp
// SpinnerNet.PublicAreas/Services/ThemeRenderer.cs
public class ThemeRenderer : IThemeRenderer
{
    public async Task<string> RenderPageAsync(PublicSite site, PublicPage page, HttpContext context)
    {
        var theme = await _themeService.GetThemeAsync(site.Theme);
        var layout = await LoadLayoutAsync(theme, page.Layout);
        
        var model = new PageViewModel
        {
            Site = site,
            Page = page,
            Content = await _markdownProcessor.ProcessAsync(page.Content),
            IsCustomDomain = context.Items.ContainsKey("IsCustomDomain"),
            CurrentUrl = context.Request.GetDisplayUrl()
        };
        
        return await _razorEngine.RenderAsync(layout, model);
    }
}
```

## Deployment Strategy

### 1. Azure Configuration Updates
```json
// appsettings.Production.json
{
  "PublicAreas": {
    "EnableCustomDomains": true,
    "CdnUrl": "https://spinnernet-cdn.azureedge.net",
    "MaxSitesPerPersona": 10,
    "ThemesPath": "wwwroot/themes"
  }
}
```

### 2. Azure Front Door Rules
```bicep
// deploy.bicep additions
resource frontDoor 'Microsoft.Cdn/profiles@2021-06-01' = {
  name: 'fd-spinnernet'
  location: 'global'
  sku: {
    name: 'Standard_AzureFrontDoor'
  }
  
  resource endpoint 'endpoints' = {
    name: 'spinnernet-endpoint'
    properties: {
      hostName: 'bamberger-spinnerei.azurefd.net'
    }
  }
  
  resource route 'routes' = {
    name: 'custom-domain-route'
    properties: {
      customDomains: [
        {
          id: customDomain.id
        }
      ]
      originGroup: {
        id: originGroup.id
      }
      patterns: [
        '/*'
      ]
    }
  }
}
```

## Testing Plan

### 1. Local Testing
```bash
# Add hosts file entries
echo "127.0.0.1 peterclinton.local" >> /etc/hosts
echo "127.0.0.1 bamberg.local" >> /etc/hosts

# Test with custom domains locally
dotnet run --urls="http://peterclinton.local:5000;http://bamberg.local:5000"
```

### 2. Integration Tests
```csharp
[Fact]
public async Task CustomDomain_RoutesToCorrectSite()
{
    // Arrange
    var client = _factory.WithWebHostBuilder(builder =>
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IPublicAreaService, MockPublicAreaService>();
        });
    }).CreateClient();
    
    // Act
    client.DefaultRequestHeaders.Host = "peterclinton.de";
    var response = await client.GetAsync("/");
    
    // Assert
    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadAsStringAsync();
    Assert.Contains("Peter Clinton Photography", content);
}
```

## Success Metrics

1. **Performance**: Public pages load in <200ms
2. **Scalability**: Support 1000+ sites per instance
3. **Security**: Complete isolation between public/private
4. **SEO**: Perfect Lighthouse scores for public sites

## Next Steps

1. Deploy routing fix immediately
2. Implement Phase 1 this week
3. Test with 3 example sites
4. Gather feedback before Phase 2