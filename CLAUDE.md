# Spinner.Net Development Guide

## Project Overview

**Spinner.Net** is a digital community platform developed by **Peter Paul Clinton** for **Bamberger Spinnerei**. The project is now split into two clean, focused applications:

### 🌐 SpinnerNet.Web (Public Site)
Clean Razor Pages website for **Bamberger Spinnerei** marketing and public content:
- Fast, SEO-optimized public site
- Routes: `/`, `/about`, `/contact`, `/beta`, `/app` (redirect)
- No Blazor complexity - pure Razor Pages/MVC
- Deployed at: https://spinnernet-3lauxg.azurewebsites.net

### 🚀 SpinnerNet.App (Interactive Platform)  
Clean Blazor Hybrid application for the **Spinner.Net** interactive platform:
- Pure MudBlazor setup following official documentation
- Interactive features: AI companions, time banking, community tools
- Clean architecture with proper provider setup
- Local development ready

**Current Status**: Architecture cleaned and separated - Public site deployed ✅, Blazor app ready for features ✅

### Core Principles
- **Community First**: You are not alone - together we are stronger than any system
- **Digital & Personal Connection**: Online community that strengthens real-world relationships  
- **User-First Data Sovereignty**: Users control where every piece of data is stored and processed
- **Local-First AI**: Critical processing happens locally, cloud enhancement is optional
- **Privacy by Design**: AI assistance without surveillance or data exploitation
- **Room-Based Organization**: Communication organized by context (private/business/commercial)
- **AI Support Features**: Technology that serves the community, including buddy companions

## Architecture Patterns

### Vertical Slice Architecture (from LichtFlow)
Each feature is self-contained in a single file with:
```csharp
public static class FeatureName
{
    public record Command : IRequest<Result> { }     // Input
    public record Result { }                         // Output  
    public class Validator : AbstractValidator<Command> { }  // Validation
    public class Handler : IRequestHandler<Command, Result> { }  // Logic
    [ApiController] public class Endpoint : ControllerBase { }  // HTTP API
}
```

### Semantic Kernel Integration (from LichtFlow)
- **Never use direct HTTP calls** to AI APIs - always use Semantic Kernel
- **IAiService interfaces** for all AI functionality
- **Local SQLite vector storage** for privacy-first embeddings
- **Plugin system** for domain-specific AI capabilities

### Data Sovereignty Framework
```csharp
// Every data operation respects user preferences
var dataLocation = await _dataSovereignty.GetDataLocationAsync(userId, "email");
var repository = _dataSovereignty.GetRepositoryForLocation(dataLocation);
```

## Technology Stack

### Core Technologies
- **Frontend**: Blazor Server + MudBlazor
- **Backend**: ASP.NET Core 9.0 + MediatR + CQRS
- **Database**: Cosmos DB with local SQLite for development
- **AI**: Semantic Kernel + OpenAI integration
- **Vector Storage**: SQLite (local) + Cosmos DB (cloud)

### Key Dependencies
```xml
<PackageReference Include="Microsoft.SemanticKernel" Version="1.0.1" />
<PackageReference Include="MediatR" Version="12.4.1" />
<PackageReference Include="FluentValidation" Version="11.9.2" />
<PackageReference Include="Microsoft.Azure.Cosmos" Version="3.42.0" />
<PackageReference Include="Microsoft.SemanticKernel.Connectors.Sqlite" Version="1.0.1" />
```

## File Structure
```
SpinnerNet.Core/Features/
├── Users/RegisterUser.cs
├── Personas/CreatePersona.cs  
├── Buddies/CreateBuddy.cs
├── Email/ConnectEmailAccount.cs
├── Rooms/CreateRoom.cs
└── DataSovereignty/SetDataResidencyPreference.cs
```

## Development Commands

### Build & Test
```bash
# Build solution
dotnet build ./src/SpinnerNet.sln

# Run tests  
dotnet test ./src/SpinnerNet.Tests/SpinnerNet.Tests.csproj

# Run application
dotnet run --project ./src/SpinnerNet.Web/SpinnerNet.Web.csproj
```

### Database Setup
```bash
# Start Cosmos DB Emulator (Windows)
# OR use connection string for cloud Cosmos DB

# Cosmos DB Local Emulator:
# AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==
```

## Azure Deployment

### Current Azure Resources
- **Resource Group**: `rg-spinnernet-proto`
- **Public Site (SpinnerNet.Web)**: `spinnernet-3lauxg` → https://spinnernet-3lauxg.azurewebsites.net
- **Blazor App (SpinnerNet.App)**: `spinnernet-app-3lauxg` → https://spinnernet-app-3lauxg.azurewebsites.net  
- **Cosmos DB**: `cosmos-spinnernet-3lauxg`
- **Key Vault**: `kv-spinnernet-3lauxg`
- **Primary Domain**: `bamberger-spinnerei.org` (to be configured)

### Deployment Status (Last Updated: 2025-06-18)
- ✅ **Architecture Separation**: Clean split between public site and Blazor app
- ✅ **SpinnerNet.Web (Public)**: Razor Pages site with localization, status 200
- ✅ **SpinnerNet.App (Blazor)**: Pure MudBlazor application, status 200
- ✅ **MudBlazor Integration**: Properly configured with Material Design components
- ✅ **Cross-linking**: Public site navigation points to Blazor app
- ✅ **Health Checks**: Both applications operational
- ✅ **Authentication Ready**: Cookie-based auth configured for both
- ⏳ **Custom Domain**: bamberger-spinnerei.org to be configured
- ⏳ **Feature Implementation**: Ready for user registration and AI personas

### DEPLOYMENT INSTRUCTIONS - TWO APPLICATIONS

**ARCHITECTURE**: We now have two separate applications that must be deployed independently.

## Deploy Public Site (SpinnerNet.Web)

#### Step 1: Navigate to Project Root
```bash
cd /Users/peterclinton/Desktop/repos/LichtFlow/Spinner.Net-Public/src
pwd  # Should show: /Users/peterclinton/Desktop/repos/LichtFlow/Spinner.Net-Public/src
```

#### Step 2: Clean and Publish Public Site
```bash
rm -rf publish/ && rm -f deployment.zip
dotnet publish SpinnerNet.Web/SpinnerNet.Web.csproj -c Release -o publish --self-contained false
ls -la publish/SpinnerNet.Web.dll  # Verify exists
```

#### Step 3: Deploy Public Site
```bash
cd publish && zip -r ../deployment.zip . && cd ..
az webapp deploy --resource-group rg-spinnernet-proto --name spinnernet-3lauxg --src-path deployment.zip --type zip
rm -rf publish/ && rm -f deployment.zip  # Cleanup
```

## Deploy Blazor App (SpinnerNet.App)

#### Step 1: Clean and Publish Blazor App
```bash
rm -rf publish/ && rm -f deployment.zip
dotnet publish SpinnerNet.App/SpinnerNet.App.csproj -c Release -o publish --self-contained false
ls -la publish/SpinnerNet.App.dll  # Verify exists
```

#### Step 2: Deploy Blazor App
```bash
cd publish && zip -r ../deployment.zip . && cd ..
az webapp deploy --resource-group rg-spinnernet-proto --name spinnernet-app-3lauxg --src-path deployment.zip --type zip
rm -rf publish/ && rm -f deployment.zip  # Cleanup
```

## Verify Both Deployments
```bash
# Test public site
curl -s -o /dev/null -w "%{http_code}" https://spinnernet-3lauxg.azurewebsites.net/
# Expected: 200

# Test public site health
curl -s https://spinnernet-3lauxg.azurewebsites.net/health
# Expected: "OK - Bamberger Spinnerei Public Site is running! Key Vault: ✅"

# Test Blazor app
curl -s -o /dev/null -w "%{http_code}" https://spinnernet-app-3lauxg.azurewebsites.net/
# Expected: 200

# Test MudBlazor functionality
curl -s -o /dev/null -w "%{http_code}" https://spinnernet-app-3lauxg.azurewebsites.net/test
# Expected: 200

# Verify MudBlazor CSS loads properly
curl -s https://spinnernet-app-3lauxg.azurewebsites.net/test | grep "MudBlazor.min.css"
# Expected: <link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet">
```

### TROUBLESHOOTING

**Error: "Either 'deployment.zip' is not a valid local file path"**
- Ensure you're in `/Users/peterclinton/Desktop/repos/LichtFlow/Spinner.Net-Public/src`
- Run `ls -la deployment.zip` to verify zip exists
- If missing, re-run publish and zip commands

**Error: "No such file or directory: SpinnerNet.Web/SpinnerNet.Web.csproj"**
- Run `pwd` - you must be in the src/ directory
- Run `ls SpinnerNet.Web/` and `ls SpinnerNet.App/` to verify projects exist

**MudBlazor components not rendering in Blazor app**
- Check CSS loads: `curl -s https://spinnernet-app-3lauxg.azurewebsites.net/test | grep MudBlazor`
- Verify providers in MainLayout.razor (not App.razor)
- Check browser dev tools for 404 errors on MudBlazor assets

**Public site /app redirect not working**
- Verify navigation links point to: `https://spinnernet-app-3lauxg.azurewebsites.net`
- Check _Layout.cshtml navbar link is updated
- Test: `curl -s https://spinnernet-3lauxg.azurewebsites.net/app | grep "Launch Spinner.Net App"`

**Deployment succeeds but 500 errors**
- **Public Site Logs**: https://spinnernet-3lauxg.scm.azurewebsites.net/api/logs/docker
- **Blazor App Logs**: https://spinnernet-app-3lauxg.scm.azurewebsites.net/api/logs/docker
- Common issue: Missing environment variables or Key Vault access

### PROJECT STRUCTURE REFERENCE
```
/Users/peterclinton/Desktop/repos/LichtFlow/Spinner.Net-Public/src/  ← YOU MUST BE HERE
├── SpinnerNet.Web/          ← Public Razor Pages site
├── SpinnerNet.App/          ← Clean Blazor app with MudBlazor  
├── SpinnerNet.Shared/       ← Shared libraries
├── publish/                 ← Created by dotnet publish (temp)
└── deployment.zip          ← Created by zip command (temp)

Deployment Targets:
- SpinnerNet.Web → spinnernet-3lauxg (Public site)
- SpinnerNet.App → spinnernet-app-3lauxg (Blazor platform)
```

## AI Integration Patterns

### Buddy AI Service
```csharp
public interface IBuddyAiService
{
    Task<BuddyResponse> ChatWithBuddyAsync(string message, BuddyContext context);
    IAsyncEnumerable<string> StreamBuddyResponseAsync(string message, BuddyContext context);
    Task<EmailSummary> SummarizeEmailAsync(EmailMessage email, BuddyPersonality personality);
}
```

### Privacy-Aware AI Processing
```csharp
// Route AI operations based on user privacy preferences
var aiPreference = await _dataSovereignty.GetAiProcessingPreferenceAsync(userId);
var aiService = aiPreference == AiProcessingPreference.Local ? _localAi : _cloudAi;
var response = await aiService.ProcessWithPrivacyAsync(request, context);
```

### Semantic Kernel Setup
```csharp
// Service registration (Program.cs)
services.AddSemanticKernel(options =>
{
    options.AddOpenAITextGeneration("gpt-4o-mini", openAiApiKey);
    options.AddOpenAITextEmbeddingGeneration("text-embedding-3-small", openAiApiKey);
});

// Local vector memory
var memoryStore = SqliteMemoryStore.ConnectAsync("Data/SpinnerNet-vector.db");
services.AddSingleton<ISemanticTextMemory>(memory);
```

## Data Models

### Core Documents (Cosmos DB)
- **UserDocument**: User registration, preferences, data sovereignty settings
- **PersonaDocument**: User personas with language, cultural, and UI preferences  
- **BuddyDocument**: AI buddy configuration, personality, capabilities
- **EmailDocument**: Email messages with room assignment and AI analysis
- **BuddyMemoryDocument**: Buddy learning data and interaction patterns

### Data Residency Options
```csharp
public enum DataResidencyPreference
{
    Local = 0,          // Local device/private cloud
    EU = 1,             // European Union
    US = 2,             // United States  
    UserRegion = 3,     // Auto-detect user region
    Hybrid = 4          // Sensitive=local, general=cloud
}
```

## Testing Patterns

### Unit Test Structure
```csharp
[TestMethod]
public async Task Handle_ValidCommand_ReturnsSuccess()
{
    // Arrange
    var command = new CreateBuddy.Command { UserId = "user123", BuddyName = "Assistant" };
    
    // Act  
    var result = await _handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.Success.Should().BeTrue();
}
```

### Database Testing (SQLite In-Memory)
```csharp
var connection = new SqliteConnection("DataSource=:memory:");
connection.Open();
var options = new DbContextOptionsBuilder<TestDbContext>()
    .UseSqlite(connection)
    .Options;
```

## Configuration

### Environment Variables
```bash
# Required
OPENAI_API_KEY=your_openai_api_key

# Optional
COSMOS_DB_CONNECTION_STRING=your_cosmos_connection
COSMOS_DB_DATABASE_NAME=SpinnerNetDb
```

### appsettings.json
```json
{
  "OpenAI": {
    "ApiKey": "",
    "ModelName": "gpt-4o-mini"
  },
  "CosmosDb": {
    "ConnectionString": "",
    "DatabaseName": "SpinnerNetDb"
  }
}
```

## Key Development Rules

### Code Style
- **PascalCase** for classes, interfaces, public members
- **Interface prefix**: `IAiService`, `IBuddyService`
- **Async suffix**: `CreateBuddyAsync`, `ProcessEmailAsync`
- **No XML comments** unless specifically requested

### Architecture Rules
- **Never use DbContext directly** in UI components
- **Always use vertical slice pattern** for new features
- **Respect data sovereignty** in every data operation
- **Privacy-first AI processing** - check user preferences first
- **Local-first functionality** - core features work offline

### AI Integration Rules
- **Never make direct HTTP calls** to AI APIs
- **Always use Semantic Kernel** for AI operations
- **Check user AI preferences** before cloud processing
- **Store AI interactions** based on user data sovereignty settings

### Community-Focused Messaging Guidelines
- **Personal & Direct**: Use "du" (informal you) in German, warm personal tone in all languages
- **Community Over Individual**: Emphasize connection, mutual support, "you are not alone"
- **Inclusive & Welcoming**: Everyone belongs, regardless of background or skills
- **Empowering**: Focus on what people can achieve together, not just technology features
- **Human-Centered**: Technology serves the community, not the other way around
- **Examples**:
  - ❌ "Our AI buddy system helps users optimize productivity"
  - ✅ "Du bist nicht allein - wir unterstützen uns gegenseitig und wachsen zusammen"
  - ❌ "Advanced features for registered members"
  - ✅ "Gemeinsam erschaffen wir einen Raum, wo jeder gehört wird und mitwirken kann"

## Current Sprint: Foundation - Public Areas Complete ✅

**Status**: Public areas deployed and working on Azure with status 200

**Completed**:
- ✅ **Public Areas Foundation**: Homepage, About, Contact, Beta, Support pages
- ✅ **Azure Deployment**: Working deployment with proper PageModels
- ✅ **Routing Architecture**: Clean separation between public pages (/\*) and Blazor app (/app/\*)
- ✅ **Branding**: Updated with Peter Paul Clinton and Bamberger Spinnerei

**Next Sprint: Multi-tenant Public Areas**:
- **Week 1**: Implement PublicAreas engine with routing middleware
- **Week 2**: Site management UI for persona-based public sites  
- **Week 3**: Custom domain support (bamberger-spinnerei.org → /p/peter-clinton)
- **Week 4**: Theme system and content management

**Future Sprints**:
- User/Persona system with data sovereignty
- AI persona interview system
- Basic time tracking for ZeitCoin foundation
- Email connection and room organization

## Architecture Documentation

- **Multi-tenant Architecture**: `/docs/architecture/MULTI_TENANT_PUBLIC_AREAS_ARCHITECTURE.md`
- **Implementation Plan**: `/docs/architecture/PUBLIC_AREAS_IMPLEMENTATION_PLAN.md`

## Getting Started

1. **Clone repository** and ensure .NET 9.0 is installed
2. **Set OpenAI API key** in environment variables
3. **Start Cosmos DB Emulator** or configure cloud connection
4. **Run solution** and navigate to onboarding wizard
5. **Create user → persona → buddy → connect email**

**Current Focus**: Build multi-tenant public areas system for persona-based websites with custom domain support.