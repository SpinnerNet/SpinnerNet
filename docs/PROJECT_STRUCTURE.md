# Project Structure - Spinner.Net

## Overview

Spinner.Net follows a modular, vertical slice architecture with clear separation of concerns. The project structure supports the Zeitsparkasse™ time management platform with AI buddy companions, data sovereignty, and multi-language support.

## Root Directory Structure

```
Spinner.Net-Public/
├── README.md                          # Project overview and quick start
├── LICENSE                            # MIT License
├── CHANGELOG.md                       # Version history and changes
├── .gitignore                        # Git ignore patterns
├── .github/                          # GitHub Actions and templates
│   ├── workflows/                    # CI/CD pipelines
│   └── ISSUE_TEMPLATE/              # Issue templates
├── docs/                            # Documentation
│   ├── ARCHITECTURE.md              # System architecture
│   ├── VERTICAL_SLICE_ARCHITECTURE.md # Development patterns
│   ├── DATA_MODEL_DESIGN.md         # Database design
│   ├── PROJECT_STRUCTURE.md         # This file
│   ├── DEVELOPMENT.md               # Dev environment setup
│   ├── GETTING_STARTED.md           # Quick start guide
│   ├── TECHNICAL_FAQ.md             # Technical Q&A
│   ├── API_SPECIFICATION.md         # API documentation
│   ├── EMAIL_INTEGRATION.md         # Email system design
│   ├── BAMBERGER_SPINNEREI.md       # Business concept
│   ├── VISION.md                    # Product vision
│   ├── CONTRIBUTION_WORKFLOW.md     # Contributing guidelines
│   ├── GITHUB_SETUP_GUIDE.md        # GitHub configuration
│   └── planning/                    # Planning documents
│       ├── SPRINT_1_CONSOLIDATED_PLAN.md
│       ├── SPRINT_1_VERTICAL_SLICE_PLAN.md
│       ├── MULTI_SPRINT_MASTER_PLAN.md
│       ├── VERTICAL_SLICE_INTEGRATION_SUMMARY.md
│       └── IMPLEMENTATION_SUMMARY.md
├── examples/                        # Code examples and demos
│   ├── curl-examples/              # API usage examples
│   ├── integration-examples/       # Integration samples
│   └── deployment-examples/        # Deployment configurations
├── src/                            # Source code
└── Azure/                          # Azure deployment configurations
    ├── main.bicep                  # Infrastructure as Code
    ├── deploy.sh                   # Deployment script
    ├── deploy.ps1                  # PowerShell deployment
    └── README.md                   # Deployment guide
```

## Source Code Structure

```
src/
├── SpinnerNet.sln                  # Visual Studio solution file
├── CLAUDE.md                       # Development instructions for Claude
├── SpinnerNet.Core/                # Core application logic
│   ├── SpinnerNet.Core.csproj     # Project file
│   ├── Data/                      # Data access layer
│   │   ├── CosmosDb/              # Cosmos DB implementations
│   │   │   ├── ICosmosRepository.cs
│   │   │   ├── CosmosRepository.cs
│   │   │   ├── CosmosDbConnectionManager.cs
│   │   │   ├── CosmosDbServiceExtensions.cs
│   │   │   ├── CosmosQueryBuilder.cs
│   │   │   └── CosmosUnitOfWork.cs
│   │   └── SpinnerNetDbContext.cs # Entity Framework context (if needed)
│   ├── Extensions/                # Service extensions
│   │   └── ServiceCollectionExtensions.cs
│   ├── Features/                  # Vertical slices
│   │   ├── Users/                 # User management
│   │   │   ├── RegisterUser.cs    # User registration slice
│   │   │   └── GetUser.cs         # User retrieval slice
│   │   ├── PersonaInterview/      # AI persona discovery
│   │   │   ├── StartPersonaInterview.cs
│   │   │   ├── ProcessInterviewResponse.cs
│   │   │   └── CompletePersonaInterview.cs
│   │   ├── Tasks/                 # Zeitsparkasse task management
│   │   │   ├── CreateTask.cs      # Natural language task creation
│   │   │   ├── UpdateTask.cs      # Task editing and completion
│   │   │   ├── GetUserTasks.cs    # Task retrieval
│   │   │   ├── CreateGoal.cs      # Goal creation
│   │   │   └── LinkTaskToGoal.cs  # Task-goal relationships
│   │   ├── Buddies/               # AI companion system
│   │   │   ├── CreateBuddy.cs     # Buddy creation
│   │   │   ├── ChatWithBuddy.cs   # Real-time conversation
│   │   │   └── GetBuddyContext.cs # Context-aware responses
│   │   ├── DataSovereignty/       # Privacy and data control
│   │   │   ├── SetDataResidencyPreference.cs
│   │   │   ├── GetDataLocations.cs
│   │   │   └── ExportUserData.cs
│   │   ├── AI/                    # AI processing
│   │   │   ├── RouteToAiProvider.cs
│   │   │   ├── ProcessNaturalLanguage.cs
│   │   │   └── GenerateAiResponse.cs
│   │   └── Email/                 # Email integration
│   │       ├── ConnectEmailAccount.cs
│   │       ├── SyncEmails.cs
│   │       └── EmailToTask.cs
│   └── Identity/                  # Authentication
│       └── ApplicationUser.cs     # User identity model
├── SpinnerNet.Shared/             # Shared models and contracts
│   ├── SpinnerNet.Shared.csproj  # Project file
│   ├── Models/                    # Data models
│   │   ├── CosmosDb/              # Cosmos DB documents
│   │   │   ├── UserDocument.cs    # User data model
│   │   │   ├── PersonaDocument.cs # User persona model
│   │   │   ├── TaskDocument.cs    # Task data model
│   │   │   ├── BuddyDocument.cs   # AI buddy model
│   │   │   ├── BuddyMemoryDocument.cs # AI memory model
│   │   │   └── EmailDocument.cs   # Email data model
│   │   ├── User.cs                # User DTO
│   │   └── UserPersona.cs         # Persona DTO
│   └── DTOs/                      # Data transfer objects
│       ├── OnboardingRequest.cs   # Onboarding flow DTOs
│       └── PersonaUIConfiguration.cs # TypeLeap UI config
├── SpinnerNet.Personas/           # Persona management service
│   ├── SpinnerNet.Personas.csproj # Project file
│   └── Services/                  # Persona services
│       ├── IPersonaService.cs     # Interface
│       └── PersonaService.cs      # Implementation
├── SpinnerNet.Web/                # Blazor Server web application
│   ├── SpinnerNet.Web.csproj     # Project file
│   ├── Program.cs                 # Application entry point
│   ├── appsettings.json          # Configuration
│   ├── appsettings.Development.json # Dev configuration
│   ├── Components/                # Blazor components
│   │   ├── App.razor             # Root component
│   │   ├── Layout/               # Layout components
│   │   │   └── MainLayout.razor  # Main layout
│   │   ├── Account/              # Authentication UI
│   │   │   └── RedirectToLogin.razor
│   │   ├── Onboarding/           # User onboarding
│   │   │   └── OnboardingWizard.razor
│   │   └── Pages/                # Page components
│   │       └── Home.razor        # Home page
│   ├── Pages/                    # Razor pages (if needed)
│   │   ├── Shared/               # Shared layouts
│   │   │   └── _Layout.cshtml    # HTML layout
│   │   └── _Host.cshtml          # Host page
│   └── wwwroot/                  # Static files
│       └── css/                  # Stylesheets
│           └── app.css           # Application styles
└── SpinnerNet.Tests/             # Unit and integration tests
    ├── SpinnerNet.Tests.csproj   # Test project file
    ├── Features/                 # Feature tests (by slice)
    │   ├── Users/                # User feature tests
    │   │   ├── RegisterUserTests.cs
    │   │   └── GetUserTests.cs
    │   ├── Tasks/                # Task feature tests
    │   │   ├── CreateTaskTests.cs
    │   │   └── UpdateTaskTests.cs
    │   ├── Buddies/              # Buddy feature tests
    │   │   ├── CreateBuddyTests.cs
    │   │   └── ChatWithBuddyTests.cs
    │   └── Integration/          # Integration tests
    │       ├── ApiIntegrationTests.cs
    │       └── CosmosDbIntegrationTests.cs
    ├── Helpers/                  # Test utilities
    │   ├── TestFixtures.cs       # Common test setup
    │   ├── MockDataBuilder.cs    # Test data builders
    │   └── CosmosDbTestHelper.cs # Cosmos DB test utilities
    └── TestData/                 # Test data files
        ├── SampleUsers.json      # Sample user data
        ├── SampleTasks.json      # Sample task data
        └── SampleConversations.json # Sample conversations
```

## Module Breakdown

### 1. **SpinnerNet.Core** - Application Logic
**Purpose**: Contains all business logic organized as vertical slices

**Key Features**:
- Vertical slice architecture with MediatR
- Direct Cosmos DB data access
- Multi-language support
- Data sovereignty implementation

**Dependencies**:
- MediatR for CQRS
- FluentValidation for input validation
- Azure Cosmos DB SDK
- Microsoft.Extensions.Logging

### 2. **SpinnerNet.Shared** - Common Models
**Purpose**: Shared data models and contracts

**Key Features**:
- Cosmos DB document models
- Data transfer objects
- Shared interfaces and contracts

**Dependencies**:
- System.Text.Json for serialization
- System.ComponentModel.DataAnnotations

### 3. **SpinnerNet.Personas** - Persona Management
**Purpose**: Specialized service for persona operations

**Key Features**:
- Persona creation and management
- TypeLeap UI configuration
- Cultural adaptation logic

**Dependencies**:
- SpinnerNet.Shared
- Microsoft.Extensions.DependencyInjection

### 4. **SpinnerNet.Web** - Web Application
**Purpose**: Blazor Server web interface

**Key Features**:
- Server-side Blazor components
- Real-time SignalR integration
- Progressive Web App (PWA) support
- Responsive design with MudBlazor

**Dependencies**:
- Microsoft.AspNetCore.Blazor.Server
- MudBlazor for UI components
- Microsoft.AspNetCore.SignalR

### 5. **SpinnerNet.Tests** - Testing Suite
**Purpose**: Comprehensive testing coverage

**Key Features**:
- Unit tests for each vertical slice
- Integration tests for API endpoints
- Cosmos DB integration testing
- Test data builders and fixtures

**Dependencies**:
- xUnit for testing framework
- FluentAssertions for readable assertions
- Microsoft.AspNetCore.Mvc.Testing
- Testcontainers for database testing

## Configuration Structure

### Application Settings
```json
{
  "ConnectionStrings": {
    "CosmosDb": "AccountEndpoint=...",
    "Redis": "localhost:6379"
  },
  "AzureCosmosDb": {
    "DatabaseName": "SpinnerNet",
    "Containers": {
      "Users": "Users",
      "Personas": "Personas", 
      "Tasks": "Tasks",
      "Conversations": "Conversations",
      "Communications": "Communications"
    }
  },
  "AI": {
    "Providers": {
      "OpenAI": {
        "ApiKey": "...",
        "Model": "gpt-4"
      },
      "Local": {
        "Endpoint": "http://localhost:11434",
        "Models": ["llama3", "mistral", "phi3"]
      }
    },
    "DefaultProvider": "Local",
    "FallbackProvider": "OpenAI"
  },
  "DataSovereignty": {
    "DefaultLocation": "local",
    "SupportedRegions": ["europe", "us", "asia"],
    "LocalStoragePath": "./data"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "SpinnerNet": "Debug"
    }
  }
}
```

## Build and Deployment

### Build Scripts
```bash
# Build solution
dotnet build src/SpinnerNet.sln

# Run tests
dotnet test src/SpinnerNet.Tests/

# Run web application
dotnet run --project src/SpinnerNet.Web/

# Package for deployment
dotnet publish src/SpinnerNet.Web/ -c Release -o ./publish
```

### Docker Support
```dockerfile
# Development
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Copy and restore
COPY src/ ./src/
RUN dotnet restore src/SpinnerNet.sln

# Build and publish
RUN dotnet publish src/SpinnerNet.Web/ -c Release -o /app/publish

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SpinnerNet.Web.dll"]
```

## Development Workflow

### 1. **Adding New Features**
```bash
# Create new vertical slice
touch src/SpinnerNet.Core/Features/{Area}/{FeatureName}.cs

# Add corresponding test
touch src/SpinnerNet.Tests/Features/{Area}/{FeatureName}Tests.cs

# Update service registration if needed
# Edit src/SpinnerNet.Core/Extensions/ServiceCollectionExtensions.cs
```

### 2. **Database Changes**
```bash
# Add new document model
touch src/SpinnerNet.Shared/Models/CosmosDb/{EntityName}Document.cs

# Update repository interfaces if needed
# Run database migration scripts
```

### 3. **Testing New Features**
```bash
# Run specific feature tests
dotnet test --filter "Category={FeatureName}"

# Run integration tests
dotnet test --filter "Category=Integration"

# Run all tests
dotnet test src/SpinnerNet.Tests/
```

## IDE Configuration

### Visual Studio Code
```json
{
  "recommendations": [
    "ms-dotnettools.csharp",
    "ms-dotnettools.vscode-dotnet-runtime",
    "humao.rest-client",
    "ms-azuretools.vscode-bicep"
  ],
  "settings": {
    "dotnet.completion.showCompletionItemsFromUnimportedNamespaces": true,
    "omnisharp.enableEditorConfigSupport": true
  }
}
```

### EditorConfig
```ini
root = true

[*]
indent_style = space
indent_size = 4
end_of_line = crlf
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

[*.{cs,csx}]
csharp_new_line_before_open_brace = all
csharp_indent_case_contents = true
```

## Security Considerations

### 1. **Code Organization**
- Sensitive configuration in environment variables
- No hardcoded secrets in source code
- API keys stored in Azure Key Vault

### 2. **Data Access**
- User-scoped queries with partition keys
- Input validation in all vertical slices
- Cosmos DB connection string encryption

### 3. **Authentication**
- JWT token-based authentication
- Role-based authorization
- Secure cookie configuration

## Performance Considerations

### 1. **Cosmos DB Optimization**
- Efficient query patterns with partition keys
- Proper indexing for common queries
- Bulk operations for batch processing

### 2. **Caching Strategy**
- In-memory caching for user sessions
- Redis caching for frequently accessed data
- CDN for static assets

### 3. **Blazor Performance**
- Server-side rendering for SEO
- SignalR for real-time updates
- Lazy loading for components

## Monitoring and Logging

### 1. **Application Insights**
- Performance monitoring
- Exception tracking
- Custom telemetry

### 2. **Structured Logging**
- Correlation IDs for request tracking
- Feature-specific logging contexts
- Log aggregation with ELK stack

This project structure provides a solid foundation for the Spinner.Net platform while maintaining clean separation of concerns, testability, and scalability for future growth.