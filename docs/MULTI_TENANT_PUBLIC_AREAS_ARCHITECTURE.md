# Multi-Tenant Public Areas Architecture for Spinner.Net

## Overview
This document outlines a scalable architecture for hosting multiple public-facing websites/areas within the Spinner.Net ecosystem, where each persona can create virtual rooms or public sites with custom domain forwarding.

## Architecture Design

### 1. Core Components

```
┌─────────────────────────────────────────────────────────────┐
│                     Azure App Service                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────┐    ┌──────────────────┐              │
│  │  Public Router  │    │  Spinner.Net     │              │
│  │   (Gateway)     │    │  Core App        │              │
│  └────────┬────────┘    └────────┬─────────┘              │
│           │                      │                          │
│  ┌────────▼────────┐    ┌───────▼────────┐                │
│  │  Public Areas   │    │   Protected    │                │
│  │   Engine        │    │     Areas      │                │
│  └────────┬────────┘    └────────────────┘                │
│           │                                                 │
│  ┌────────▼───────────────────────────┐                   │
│  │        Shared Data Layer           │                   │
│  │    (Cosmos DB / SQLite)           │                   │
│  └────────────────────────────────────┘                   │
└─────────────────────────────────────────────────────────────┘
```

### 2. URL Structure & Routing

#### Primary Domain Structure
- `bamberger-spinnerei.org/` - Main public landing page
- `bamberger-spinnerei.org/app/*` - Protected Spinner.Net application
- `bamberger-spinnerei.org/p/{persona-slug}/*` - Public persona sites
- `bamberger-spinnerei.org/r/{room-id}/*` - Public virtual rooms

#### Custom Domain Forwarding
- `peterclinton.de` → `bamberger-spinnerei.org/p/peter-clinton`
- `bamberg-photos.de` → `bamberger-spinnerei.org/p/peter-clinton/bamberg`
- `workshop-xyz.com` → `bamberger-spinnerei.org/r/workshop-abc123`

### 3. Implementation Strategy

#### Phase 1: Core Infrastructure (Week 1)

**A. Public Areas Engine**
```csharp
namespace SpinnerNet.PublicAreas;

public interface IPublicAreaService
{
    Task<PublicSite?> GetSiteBySlugAsync(string slug);
    Task<PublicSite?> GetSiteByDomainAsync(string domain);
    Task<bool> RenderSiteAsync(HttpContext context, PublicSite site);
}

public class PublicSite
{
    public string Id { get; set; }
    public string PersonaId { get; set; }
    public string Slug { get; set; }
    public string Title { get; set; }
    public string? CustomDomain { get; set; }
    public string Theme { get; set; }
    public List<PublicPage> Pages { get; set; }
    public Dictionary<string, object> Settings { get; set; }
}
```

**B. Routing Middleware**
```csharp
// Program.cs
app.UseWhen(context => 
    !context.Request.Path.StartsWithSegments("/app") &&
    !context.Request.Path.StartsWithSegments("/api"),
    publicApp =>
    {
        publicApp.UseMiddleware<PublicAreaRoutingMiddleware>();
        publicApp.UseMiddleware<CustomDomainMiddleware>();
    });

// Blazor app only handles /app routes
app.MapWhen(context => 
    context.Request.Path.StartsWithSegments("/app"),
    blazorApp =>
    {
        blazorApp.UseRouting();
        blazorApp.MapBlazorHub();
        blazorApp.MapFallbackToPage("/app/{*path:nonfile}", "/_Host");
    });
```

#### Phase 2: Multi-Tenant Data Architecture (Week 2)

**A. Database Schema**
```sql
-- Public Sites
CREATE TABLE PublicSites (
    Id NVARCHAR(50) PRIMARY KEY,
    PersonaId NVARCHAR(50) NOT NULL,
    Slug NVARCHAR(100) UNIQUE NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    CustomDomain NVARCHAR(255) UNIQUE NULL,
    Theme NVARCHAR(50) DEFAULT 'default',
    Settings NVARCHAR(MAX), -- JSON
    CreatedAt DATETIME2,
    UpdatedAt DATETIME2,
    IsActive BIT DEFAULT 1
);

-- Public Pages
CREATE TABLE PublicPages (
    Id NVARCHAR(50) PRIMARY KEY,
    SiteId NVARCHAR(50) NOT NULL,
    Slug NVARCHAR(100) NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Content NVARCHAR(MAX), -- HTML/Markdown
    Layout NVARCHAR(50) DEFAULT 'default',
    MetaData NVARCHAR(MAX), -- JSON
    SortOrder INT DEFAULT 0,
    IsPublished BIT DEFAULT 1,
    CONSTRAINT UK_SiteId_Slug UNIQUE (SiteId, Slug)
);

-- Domain Mappings
CREATE TABLE DomainMappings (
    Domain NVARCHAR(255) PRIMARY KEY,
    TargetType NVARCHAR(50) NOT NULL, -- 'site', 'room', 'page'
    TargetId NVARCHAR(50) NOT NULL,
    SslEnabled BIT DEFAULT 1,
    CreatedAt DATETIME2
);
```

**B. Vertical Slices for Public Areas**
```csharp
// Features/PublicAreas/CreatePublicSite.cs
public static class CreatePublicSite
{
    public record Command : IRequest<Result>
    {
        public string PersonaId { get; init; }
        public string Title { get; init; }
        public string Slug { get; init; }
        public string Theme { get; init; } = "default";
    }
    
    public class Handler : IRequestHandler<Command, Result>
    {
        // Implementation
    }
}

// Features/PublicAreas/ConfigureCustomDomain.cs
public static class ConfigureCustomDomain
{
    public record Command : IRequest<Result>
    {
        public string SiteId { get; init; }
        public string Domain { get; init; }
    }
}
```

#### Phase 3: Theme & Content Management (Week 3)

**A. Theme System**
```
/wwwroot/themes/
├── default/
│   ├── styles.css
│   ├── layout.html
│   └── components/
├── photography/
├── workshop/
└── minimal/
```

**B. Dynamic Content Rendering**
```csharp
public class PublicAreaRenderer
{
    public async Task<IHtmlContent> RenderPageAsync(PublicPage page, PublicSite site)
    {
        var theme = await _themeService.GetThemeAsync(site.Theme);
        var layout = await _layoutEngine.LoadLayoutAsync(theme, page.Layout);
        
        return await _templateEngine.RenderAsync(layout, new
        {
            Site = site,
            Page = page,
            Content = await _contentProcessor.ProcessAsync(page.Content)
        });
    }
}
```

#### Phase 4: Domain Management & SSL (Week 4)

**A. Azure Configuration**
- Use Azure Front Door for domain routing
- Automatic SSL certificate management
- CDN integration for static assets

**B. Domain Verification**
```csharp
public class DomainVerificationService
{
    public async Task<bool> VerifyDomainOwnershipAsync(string domain, string verificationCode)
    {
        // Check TXT record or file verification
        var dnsRecords = await _dnsClient.GetTxtRecordsAsync(domain);
        return dnsRecords.Contains($"spinnerei-verify={verificationCode}");
    }
}
```

### 4. Security Considerations

1. **Public vs Private Separation**
   - Public areas run in separate process/container
   - No shared authentication cookies
   - API calls require explicit tokens

2. **Content Security**
   - Sanitize all user-generated content
   - CSP headers for each public site
   - Rate limiting per domain

3. **Domain Verification**
   - DNS TXT record verification
   - Email confirmation to domain owner
   - Automatic SSL provisioning

### 5. Performance Optimization

1. **Caching Strategy**
   - CloudFlare/Azure CDN for static assets
   - Redis cache for rendered pages
   - 5-minute cache for dynamic content

2. **Database Optimization**
   - Indexed lookups on slug/domain
   - Partitioned tables by PersonaId
   - Read replicas for public content

### 6. Migration Path

#### Step 1: Fix Current Deployment (Immediate)
```bash
# Deploy the routing fix
az webapp deploy --resource-group rg-spinnernet-proto \
  --name spinnernet-3lauxg \
  --src-path spinnernet-public-pages-fix.zip \
  --type zip
```

#### Step 2: Implement Public Areas Engine (Week 1)
- Create PublicAreas project
- Implement routing middleware
- Basic theme support

#### Step 3: Enable Multi-Site Creation (Week 2)
- Database schema updates
- Admin UI for site management
- Basic page editor

#### Step 4: Custom Domain Support (Week 3-4)
- Azure Front Door configuration
- Domain verification system
- SSL automation

### 7. Example User Flow

1. **Peter creates photography site:**
   - Login to Spinner.Net
   - Navigate to "My Public Sites"
   - Create site with slug "peter-photography"
   - Add pages: Gallery, About, Contact
   - Configure custom domain "peterclinton.de"

2. **Visitor accesses site:**
   - Visits peterclinton.de
   - Azure Front Door routes to bamberger-spinnerei.org
   - PublicAreaMiddleware detects domain
   - Renders photography site theme
   - No access to private Spinner.Net data

### 8. Future Enhancements

1. **Visual Site Builder**
   - Drag-drop page builder
   - Component marketplace
   - Custom CSS/JS injection

2. **Analytics & Insights**
   - Visitor tracking per site
   - Conversion tracking
   - A/B testing support

3. **Monetization**
   - Payment integration
   - Subscription management
   - Digital product sales

4. **API Access**
   - Public API for site data
   - Webhook integrations
   - Third-party extensions

## Implementation Priority

1. **Immediate**: Fix routing conflict (DONE)
2. **Week 1**: Basic public areas engine
3. **Week 2**: Multi-site management
4. **Week 3**: Theme system
5. **Week 4**: Custom domains

This architecture provides clean separation while maintaining flexibility for future growth.