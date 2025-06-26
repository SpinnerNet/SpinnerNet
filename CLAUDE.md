# Spinner.Net Development Guide

**Two-app architecture**: Public Razor + Authenticated Blazor hybrid

## 🏗️ Apps
- **SpinnerNet.Web**: Public Razor Pages (anonymous users)
- **SpinnerNet.App**: Blazor hybrid with MudBlazor (registered users only)

## 🔐 Azure (rg-spinnernet-proto)
- Web: spinnernet-3lauxg.azurewebsites.net  
- App: spinnernet-app-3lauxg.azurewebsites.net
- KeyVault: kv-spinnernet-3lauxg

## ✅ DONE - SPRINT 1 COMPLETE
- Azure AD multi-tenant auth (personal + work accounts)
- Authentication endpoints: /Account/login, /Account/logout
- User registration with Cosmos DB (using direct PascalCase properties)
- Basic persona creation workflow via interview flow
- Clean build with zero errors (achieved via strategic feature commenting)
- Successfully deployed to Azure: https://spinnernet-app-3lauxg.azurewebsites.net

## 🏃 CURRENT STATE - SPRINT 1 MVP
**ACTIVE FEATURES (Working & Deployed)**:
- User registration via Azure AD authentication 
- Interview flow for persona creation (4-step process)
- Basic dashboard navigation
- Cosmos DB storage with Microsoft NoSQL patterns

**COMMENTED OUT (Future Sprints)**:
- Tasks management (TaskDocument, CreateTask, CompleteTask)
- Goals system (GoalDocument, CreateGoal, LinkTaskToGoal) 
- AI Buddies (BuddyDocument, CreateBuddy, ChatWithBuddy)
- Analytics dashboard (GetUserAnalytics)
- Data sovereignty preferences
- Entity Framework integration (using Cosmos DB only)

## 🚨 CRITICAL RULES

**SECRETS**: NEVER in code! KeyVault/env vars only
```json
"AzureAd": { "ClientId": "", "Domain": "" }  // Empty in appsettings!
```

**MUDBLAZOR**: Follow official docs exactly - no custom CSS overrides
**TEXT**: ALL in .resx files (XML-encoded: & → &amp;)
**ARCHITECTURE**: Vertical slice - one file per feature  
**AI**: Only via Semantic Kernel, never direct HTTP
**COSMOS DB**: Direct PascalCase properties (Microsoft NoSQL pattern), NO JsonPropertyName attributes

## 🌍 LOCALIZATION SETUP

**CRITICAL**: NO hardcoded strings in Blazor components!

### Blazor Component Pattern
```razor
@inject ILocalizationService LocalizationService

<h1>@LocalizationService.GetString("Splash_Welcome")</h1>
<p>@LocalizationService.GetString("Splash_Loading")</p>
```

### Program.cs Registration
```csharp
using SpinnerNet.Shared.Localization;

// Add localization services
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
```

### .resx Files (SpinnerNet.Shared/Resources/)
- **Strings.resx**: English (default)
- **Strings.de.resx**: German
- **Strings.fr.resx**: French  
- **Strings.es.resx**: Spanish

### XML Encoding Required
```xml
<!-- WRONG -->
<value>Innovation & Community</value>

<!-- CORRECT -->  
<value>Innovation &amp; Community</value>
```

### Usage Examples
```razor
<!-- Simple text -->
@LocalizationService.GetString("Nav_Home")

<!-- With parameters -->
@LocalizationService.GetString("Welcome_User", userName)

<!-- PageTitle -->
<PageTitle>@LocalizationService.GetString("Splash_Welcome") - Spinner.Net</PageTitle>
```

## 🗄️ COSMOS DB PATTERNS

### Active Document Types (Sprint 1)
```csharp
// UserDocument.cs - User registration data
public class UserDocument : CosmosDocument
{
    public string Id { get; set; }               // Required by Cosmos
    public string Type { get; set; } = "user";  // Document type
    public string UserId { get; set; }           // Unique user identifier
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public DateTime BirthDate { get; set; }
    public int Age { get; set; }
    public bool IsMinor { get; set; }
    // ... other properties
}

// PersonaDocument.cs - User personality profiles
public class PersonaDocument : CosmosDocument  
{
    public string Id { get; set; }               // "persona_{UserId}_{PersonaId}"
    public string Type { get; set; } = "persona";
    public string UserId { get; set; }           // Links to UserDocument
    public string PersonaId { get; set; }        // Unique persona ID
    public string DisplayName { get; set; }
    public string Personality { get; set; }
    public string PrimaryUse { get; set; }
    public bool IsDefault { get; set; }
    // ... other properties
}
```

### Microsoft NoSQL Pattern Rules
1. **NO JsonPropertyName attributes** - Cosmos DB SDK v3 uses System.Text.Json, ignores Newtonsoft attributes
2. **Direct PascalCase properties** - Property names become JSON field names directly
3. **Partition strategy**: Use `Type` property for logical partitioning
4. **ID convention**: `{type}_{userid}_{entityid}` for easy querying

### Commented Out Documents (Future Sprints)
- `TaskDocument.cs` - Task management
- `GoalDocument.cs` - Goal tracking  
- `BuddyDocument.cs` - AI companions
- `ConversationDocument.cs` - Chat history
- `EmailDocument.cs` - Email templates
- `BuddyMemoryDocument.cs` - AI memory

## 🚀 Deploy SpinnerNet.App
```bash
cd src && dotnet publish SpinnerNet.App/*.csproj -c Release -o publish
cd publish && zip -r ../deployment.zip . && cd ..
az webapp deploy -g rg-spinnernet-proto -n spinnernet-app-3lauxg --src-path deployment.zip --type zip
```

## 🔧 Build & Test
- **Clean build**: `dotnet build` (0 errors achieved)
- **Deploy status**: ✅ Working at https://spinnernet-app-3lauxg.azurewebsites.net
- **Test flow**: Register → Interview (4 steps) → Persona creation → Dashboard

## 📋 Sprint 1 Architecture Decisions
- **Database**: Cosmos DB only (Entity Framework commented out)
- **Authentication**: Azure AD multi-tenant
- **Features**: Minimal viable - user registration + persona creation only
- **Error strategy**: Comment out non-essential features rather than fix
- **Deployment**: Single Blazor app with MudBlazor UI