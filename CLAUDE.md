# Spinner.Net Development Guide

**Two-app architecture**: Public Razor + Authenticated Blazor hybrid

## 🏗️ Apps
- **SpinnerNet.Web**: Public Razor Pages (anonymous users)
- **SpinnerNet.App**: Blazor hybrid with MudBlazor (registered users only)

## 🔐 Azure (rg-spinnernet-proto)

### ⚠️ DEVELOPMENT PAUSED - SERVICES STOPPED (2025-09-01)
**To minimize costs, the following services have been stopped:**
- ✅ **Web App**: spinnernet-3lauxg.azurewebsites.net - **STOPPED**
- ✅ **App Service**: spinnernet-app-3lauxg.azurewebsites.net - **STOPPED**
- ⚠️ **App Service Plan**: asp-spinnernet-3lauxg - **Still on B1 tier (~$50/month)**
  - Cannot downgrade to Free tier due to x64 configuration
  - Consider deleting apps and recreating on Free tier if needed

**Low/No-cost services still running:**
- ✅ **Cosmos DB**: cosmos-spinnernet-3lauxg - **Serverless (pay-per-request, no idle costs)**
- ✅ **SignalR**: signalr-spinnernet-3lauxg - **Free tier (F1)**
- ✅ **KeyVault**: kv-spinnernet-3lauxg - **Minimal cost (~$0.03/month)**
- ✅ **Application Insights**: ai-spinnernet-3lauxg, spinnernet-app-insights - **Low cost**

### 🔄 To Resume Development
```bash
# Start web apps
az webapp start --name spinnernet-3lauxg --resource-group rg-spinnernet-proto
az webapp start --name spinnernet-app-3lauxg --resource-group rg-spinnernet-proto
```

### 💰 For Complete Cost Elimination
```bash
# Option 1: Delete entire resource group (removes everything)
az group delete --name rg-spinnernet-proto --yes --no-wait

# Option 2: Delete just the App Service Plan after removing apps
az webapp delete --name spinnernet-3lauxg --resource-group rg-spinnernet-proto
az webapp delete --name spinnernet-app-3lauxg --resource-group rg-spinnernet-proto  
az appservice plan delete --name asp-spinnernet-3lauxg --resource-group rg-spinnernet-proto --yes
```

### 🔄 Project Awareness & Context & Research
- **Always read `PLANNING.md`** at the start of a new conversation to understand the project's architecture, goals, style, and constraints.
- **Check `TASK.md`** before starting a new task. If the task isn’t listed, add it with a brief description and today's date.
- **Use consistent naming conventions, file structure, and architecture patterns** as described in `PLANNING.md`.
- **LLM Models** - Always look for the models page from the documentation links mentioned below and find the model that is mentioned in the initial.md - do not change models, find the exact model name to use in the code.
- **Always scrape around 30-100 pages in total when doing research**
- **Take my tech as sacred truth, for example if I say a model name then research that model name for LLM usage - don't assume from your own knowledge at any point** 
- **For Maximum efficiency, whenever you need to perform multiple independent operations, such as research, invole all relevant tools simultaneously, rather that sequentially.**
- 
### 🧱 Code Structure & Modularity
- **Never create a file longer than 500 lines of code.** If a file approaches this limit, refactor by splitting it into modules or helper files.
- **When creating AI prompts do not hardcode examples but make everything dynamic or based off the context of what the prompt is for**
- **Agents should be designed as intelligent human beings** by giving them decision making, ability to do detailed research using Jina, and not just your basic propmts that generate absolute shit. This is absolutely vital.

### 🧠 AI Behavior Rules
- **Never assume missing context. Ask questions if uncertain.**
- **Never hallucinate libraries or functions** – only use known, verified .net packages.
- **Always confirm file paths and module names** exist before referencing them in code or tests.
- **Never delete or overwrite existing code** unless explicitly instructed to or if part of a task from `TASK.md`.

### Design
- Stick to the design system inside Designsystem.md - must be adhered to at all times for building any new features.

## 🏃 CURRENT STATE - SPRINT 1 MVP
**ACTIVE FEATURES (Working & Deployed)**:
- User registration via Azure AD authentication 
- Interview flow for persona creation
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
**AI**: WebLLM client-side + TypeLeap real-time interface (ultra-low latency)
**COSMOS DB**: Direct PascalCase properties (Microsoft NoSQL pattern), NO JsonPropertyName attributes
**SCREENSHOTS**: Save to /tmp/ folder, delete immediately after usage to avoid clutter

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
## 📚 DOCUMENTATION LOOKUP PRIORITY - CRITICAL

When you need to look up documentation for any library, framework, or technology:

1. **CONTEXT7 MCP FIRST**: ALWAYS use `mcp__context7__resolve-library-id` and `mcp__context7__get-library-docs` tools before anything else
   - These tools provide up-to-date, curated documentation with code snippets
   - Much faster and more reliable than web search
   - Example: For MudBlazor docs, use context7 MCP instead of searching the web

2. **WebFetch Last Resort**: For specific documentation URLs when needed
   - Use when you have a direct URL to fetch
   - Good for specific pages or sections

3. **WebSearch Fallback**: Only use web search if context7 MCP doesn't have the library
   - Use when context7 returns no results or library not available
   - Good for finding official documentation sites

**CRITICAL RULE**: Never use WebSearch without first attempting context7 MCP documentation lookup! This saves time and provides better results.