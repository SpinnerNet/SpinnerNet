# Getting Started with Spinner.Net

This guide will help you get started with the Spinner.Net project. It covers the basics of setting up your development environment and making your first contribution.

## Prerequisites

You'll need the following tools installed:

- **.NET SDK**: Version 9.0 or newer
- **Node.js**: Version 20.0.0 or newer
- **Git**: Latest version
- **IDE**: Visual Studio 2022 or Visual Studio Code

## Setup Steps

### 1. Clone the Repository

```bash
git clone https://github.com/spinner-net/spinner-net.git
cd spinner-net
```

### 2. Restore Dependencies

```bash
# Restore .NET dependencies
dotnet restore

# Restore frontend dependencies
cd src/Spinner.Net.Web
npm install
cd ../..
```

### 3. Run the Database Migrations

```bash
dotnet ef database update --project src/Spinner.Net.Core
```

### 4. Run the Application

```bash
# Run the backend API
dotnet run --project src/Spinner.Net.Web

# In a separate terminal, run the frontend dev server
cd src/Spinner.Net.Web
npm run dev
```

Access the application at:
- Web UI: http://localhost:5173
- API: https://localhost:7001

## Building for Different Platforms

### Build for Web

```bash
dotnet build src/Spinner.Net.Web
```

### Build for Desktop (MAUI)

```bash
# Windows
dotnet build src/Spinner.Net.Maui -f net9.0-windows

# macOS
dotnet build src/Spinner.Net.Maui -f net9.0-maccatalyst

# Linux
dotnet build src/Spinner.Net.Maui -f net9.0-linux
```

### Build for Mobile (MAUI)

```bash
# Android
dotnet build src/Spinner.Net.Maui -f net9.0-android

# iOS
dotnet build src/Spinner.Net.Maui -f net9.0-ios
```

## Running Tests

```bash
# Run all tests
dotnet test

# Run tests for a specific project
dotnet test test/Spinner.Net.Core.Tests
```

## Project Structure Overview

The Spinner.Net project is organized into several key components:

- **Core**: Base functionality and shared components
- **Assets**: Asset management system
- **Identity**: User identity and permissions
- **Persona**: Digital companion system
- **Web**: Web application and API endpoints
- **MAUI**: Cross-platform desktop and mobile app

For a detailed breakdown, see [Project Structure](PROJECT_STRUCTURE.md).

## Key Concepts

### Asset System

Assets are the primary data model in Spinner.Net. Every piece of content (messages, events, tasks, etc.) is represented as an asset with standardized properties and behaviors.

```csharp
// Example: Working with assets
var assetService = serviceProvider.GetRequiredService<IAssetService>();
var asset = await assetService.GetAssetByIdAsync<MessageAsset>("message-123");
```

### Persona System

Personas are personalized digital companions that adapt to user preferences and behaviors:

```csharp
// Example: Interacting with a persona
var personaEngine = serviceProvider.GetRequiredService<IPersonaEngine>();
var response = await personaEngine.ProcessInteractionAsync(new PersonaInteraction 
{
    PersonaId = "persona-123",
    Content = "What's on my schedule today?",
    Context = new PersonaContext { /* context data */ }
});
```

### Integration Framework

Service connectors enable integration with external services like email, calendar, and messaging:

```csharp
// Example: Using a service connector
var connectorRegistry = serviceProvider.GetRequiredService<IConnectorRegistry>();
var emailConnector = await connectorRegistry.GetConnectorAsync("gmail");
var messages = await emailConnector.FetchDataAsync<MessageAsset>(new ConnectorRequest 
{
    ResourceType = "emails",
    Limit = 10
});
```

## Common Development Tasks

### Creating a New Asset Type

1. Create a new class that inherits from `BaseAsset`:

```csharp
public class MyCustomAsset : BaseAsset
{
    public string CustomProperty { get; set; }
    // Additional properties...
}
```

2. Register the asset type in the service collection:

```csharp
services.AddAssetType<MyCustomAsset>();
```

3. Create a repository for the asset type:

```csharp
public class MyCustomAssetRepository : AssetRepository<MyCustomAsset>
{
    public MyCustomAssetRepository(AssetDbContext context) : base(context) { }
    
    // Additional methods...
}
```

### Implementing a Service Connector

1. Create a class that implements `IServiceConnector`:

```csharp
public class MyServiceConnector : IServiceConnector
{
    public string ServiceId => "my-service";
    public ConnectorCapabilities Capabilities => ConnectorCapabilities.Read | ConnectorCapabilities.Write;
    
    // Implement required methods...
}
```

2. Register the connector in the service collection:

```csharp
services.AddServiceConnector<MyServiceConnector>();
```

### Creating a UI Component

1. Create a new Razor component in the `Spinner.Net.UI` project:

```razor
@namespace Spinner.Net.UI.Components
@inherits AdaptiveComponent<MyComponentParams>

<div class="my-component">
    <h2>@Title</h2>
    <p>@Content</p>
</div>

@code {
    [Parameter]
    public string Title { get; set; }
    
    [Parameter]
    public string Content { get; set; }
    
    protected override void OnInitialized()
    {
        // Component initialization
    }
}
```

2. Add component parameters class:

```csharp
public class MyComponentParams
{
    public string Title { get; set; }
    public string Content { get; set; }
}
```

## Next Steps

After getting familiar with the basics:

1. Check the [Architecture Documentation](ARCHITECTURE.md) for deeper understanding
2. Review the [Roadmap](ROADMAP.md) to see what's being worked on
3. Look at [open issues](https://github.com/spinner-net/spinner-net/issues) tagged with "good first issue"
4. Join the community discussions on our forums

## Getting Help

If you encounter issues:

- Check existing [GitHub issues](https://github.com/spinner-net/spinner-net/issues)
- Ask questions in our community channels
- Review the documentation for specific components

Happy coding!