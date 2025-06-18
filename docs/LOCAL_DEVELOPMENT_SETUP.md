# Local Development Setup - Spinner.Net Sprint 1

## Overview

This guide provides step-by-step instructions to set up a complete development environment for Sprint 1 implementation. You'll have Cosmos DB, AI integration (OpenAI + Ollama), and all tools needed to implement the 15 vertical slices.

**Goal**: Get from zero to running the first vertical slice (`RegisterUser.cs`) locally in under 30 minutes.

## Prerequisites

### 1. Required Software

**Install in this order:**

```bash
# 1. .NET 9.0 SDK
https://dotnet.microsoft.com/download/dotnet/9.0

# 2. Visual Studio Code
https://code.visualstudio.com/

# 3. Node.js (for web components)
https://nodejs.org/ (LTS version)

# 4. Git
https://git-scm.com/
```

**Verify installations:**
```bash
dotnet --version   # Should show 9.0.x
code --version     # Should show VS Code version
node --version     # Should show 18.x or higher
git --version      # Should show Git version
```

### 2. Required VS Code Extensions

Install these extensions in VS Code:

```json
{
  "recommendations": [
    "ms-dotnettools.csharp",
    "ms-dotnettools.vscode-dotnet-runtime",
    "ms-vscode.vscode-json",
    "bradlc.vscode-tailwindcss",
    "humao.rest-client",
    "ms-vscode.azure-account",
    "ms-azuretools.vscode-cosmosdb"
  ]
}
```

**Install via VS Code Extensions panel or command:**
```bash
code --install-extension ms-dotnettools.csharp
code --install-extension ms-dotnettools.vscode-dotnet-runtime
code --install-extension ms-vscode.vscode-json
code --install-extension humao.rest-client
code --install-extension ms-azuretools.vscode-cosmosdb
```

## Step 1: Cosmos DB Emulator Setup

### Windows (Recommended for Development)

**Download and Install:**
1. Download: https://aka.ms/cosmosdb-emulator
2. Run installer with default settings
3. Start Cosmos DB Emulator from Start Menu

**Verify Installation:**
1. Open browser to: https://localhost:8081/_explorer/index.html
2. Accept SSL certificate warnings
3. You should see the Cosmos DB Explorer interface

**Default Connection String:**
```csharp
"AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="
```

### macOS/Linux Alternative

**Use Docker:**
```bash
# Pull and run Cosmos DB Linux Emulator
docker pull mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest

docker run -p 8081:8081 -p 10251:10251 -p 10252:10252 -p 10253:10253 -p 10254:10254 \
  -e AZURE_COSMOS_EMULATOR_PARTITION_COUNT=10 \
  -e AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE=true \
  mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest
```

**Create SSL certificate (macOS/Linux):**
```bash
# Download certificate
curl -k https://localhost:8081/_explorer/emulator.pem > cosmos_emulator.pem

# Trust certificate (macOS)
sudo security add-trusted-cert -d -r trustRoot -k /Library/Keychains/System.keychain cosmos_emulator.pem

# Trust certificate (Linux)
sudo cp cosmos_emulator.pem /usr/local/share/ca-certificates/cosmos_emulator.crt
sudo update-ca-certificates
```

## Step 2: OpenAI API Setup

### Get OpenAI API Key

1. Go to: https://platform.openai.com/api-keys
2. Sign in or create account
3. Click "Create new secret key"
4. Copy the key (starts with `sk-`)
5. **Important**: Store securely, you won't see it again

### Set Environment Variable

**Windows (PowerShell):**
```powershell
$env:OPENAI_API_KEY = "sk-your-key-here"
# Make permanent:
[Environment]::SetEnvironmentVariable("OPENAI_API_KEY", "sk-your-key-here", "User")
```

**macOS/Linux:**
```bash
export OPENAI_API_KEY="sk-your-key-here"
# Make permanent:
echo 'export OPENAI_API_KEY="sk-your-key-here"' >> ~/.zshrc  # or ~/.bashrc
source ~/.zshrc
```

**Verify:**
```bash
echo $OPENAI_API_KEY  # Should show your key
```

## Step 3: Ollama Local LLM Setup

### Install Ollama

**Windows:**
```bash
# Download from: https://ollama.com/download/windows
# Run installer
```

**macOS:**
```bash
# Download from: https://ollama.com/download/mac
# Or via Homebrew:
brew install ollama
```

**Linux:**
```bash
curl -fsSL https://ollama.com/install.sh | sh
```

### Pull Required Models

```bash
# Start Ollama service
ollama serve

# In new terminal, pull models for Sprint 1
ollama pull llama3.1:8b      # General conversation and persona interviews
ollama pull codellama:7b     # Code analysis and TypeLeap integration
ollama pull mistral:7b       # Alternative model for testing

# Verify models
ollama list
```

**Test Local LLM:**
```bash
ollama run llama3.1:8b "Hello, can you help me manage my tasks?"
```

## Step 4: Clone and Configure Project

### Clone Repository

```bash
git clone https://github.com/your-org/Spinner.Net-Public.git
cd Spinner.Net-Public/src
```

### Configure Development Settings

**Create `appsettings.Development.json`:**
```json
{
  "ConnectionStrings": {
    "CosmosDb": "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
    "DatabaseName": "SpinnerNetDev"
  },
  "OpenAI": {
    "ApiKey": "",
    "ModelName": "gpt-4o-mini",
    "EmbeddingModel": "text-embedding-3-small"
  },
  "LocalLLM": {
    "BaseUrl": "http://localhost:11434",
    "ModelName": "llama3.1:8b",
    "CodeModel": "codellama:7b"
  },
  "AI": {
    "PreferLocal": true,
    "UseCloudForComplexTasks": true,
    "MaxTokensLocal": 2048,
    "MaxTokensCloud": 4096
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "SpinnerNet": "Debug",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**Create User Secrets (Recommended):**
```bash
cd SpinnerNet.Web
dotnet user-secrets init
dotnet user-secrets set "OpenAI:ApiKey" "sk-your-key-here"
```

## Step 5: Initialize Development Database

### Create Database Setup Script

**Create `scripts/setup-dev-db.sql`:**
```sql
-- This will be executed via C# code, not SQL
-- Cosmos DB containers will be created programmatically
```

**Create `Data/DevDatabaseSeeder.cs`:**
```csharp
public class DevDatabaseSeeder
{
    public static async Task SeedAsync(CosmosClient cosmosClient)
    {
        // Create database
        var database = await cosmosClient.CreateDatabaseIfNotExistsAsync("SpinnerNetDev");
        
        // Create containers with proper partitioning
        await database.Database.CreateContainerIfNotExistsAsync("Users", "/userId");
        await database.Database.CreateContainerIfNotExistsAsync("Personas", "/userId");
        await database.Database.CreateContainerIfNotExistsAsync("Tasks", "/userId");
        await database.Database.CreateContainerIfNotExistsAsync("Conversations", "/userId");
        await database.Database.CreateContainerIfNotExistsAsync("Communications", "/userId");
        
        // Seed test data
        await SeedTestUsers();
        await SeedTestPersonas();
    }
}
```

## Step 6: First Build and Run

### Restore Packages

```bash
# From src directory
dotnet restore SpinnerNet.sln
```

### Build Solution

```bash
dotnet build SpinnerNet.sln
```

**Expected output:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Run Development Server

```bash
cd SpinnerNet.Web
dotnet run
```

**Expected output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]  
      Now listening on: http://localhost:5000
```

### Verify Setup

**Open browser to:**
- https://localhost:5001 (main application)
- https://localhost:8081/_explorer/index.html (Cosmos DB explorer)

**Check database creation:**
1. In Cosmos DB Explorer, you should see:
   - Database: `SpinnerNetDev`
   - Containers: `Users`, `Personas`, `Tasks`, `Conversations`, `Communications`

## Step 7: Development Workflow

### Hot Reload Setup

**Enable hot reload in VS Code:**
1. Open project in VS Code: `code .`
2. Install recommended extensions when prompted
3. Use `Ctrl+Shift+P` → "Tasks: Run Task" → "watch"

**Or command line:**
```bash
dotnet watch run --project SpinnerNet.Web
```

### Database Management

**Reset development database:**
```bash
# Stop application
# Delete database in Cosmos DB Explorer
# Restart application (will recreate)
```

**View data:**
```bash
# Use Cosmos DB Explorer at https://localhost:8081/_explorer/index.html
# Navigate to SpinnerNetDev → [Container] → Items
```

### Testing AI Integration

**Test OpenAI connection:**
```bash
# Create test endpoint in your application
curl -X POST https://localhost:5001/api/test/openai \
  -H "Content-Type: application/json" \
  -d '{"message": "Hello from Spinner.Net"}'
```

**Test Ollama connection:**
```bash
# Direct Ollama test
curl http://localhost:11434/api/generate \
  -d '{"model": "llama3.1:8b", "prompt": "Hello", "stream": false}'
```

## Step 8: Create Your First Vertical Slice

### Generate RegisterUser.cs

**Create directory structure:**
```bash
mkdir -p SpinnerNet.Core/Features/Users
```

**Create `RegisterUser.cs`:**
```csharp
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace SpinnerNet.Core.Features.Users;

public static class RegisterUser
{
    public record Command : IRequest<Result>
    {
        public string Email { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
    
    public record Result
    {
        public bool Success { get; init; }
        public string? UserId { get; init; }
        public string? ErrorMessage { get; init; }
    }
    
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
                
            RuleFor(x => x.DisplayName)
                .NotEmpty()
                .MinimumLength(2);
                
            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8);
        }
    }
    
    public class Handler : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            // TODO: Implement user registration logic
            var userId = Guid.NewGuid().ToString();
            
            return new Result 
            { 
                Success = true, 
                UserId = userId 
            };
        }
    }
    
    [ApiController]
    [Route("api/users")]
    public class Endpoint : ControllerBase
    {
        private readonly IMediator _mediator;
        
        public Endpoint(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpPost("register")]
        public async Task<ActionResult<Result>> Register([FromBody] Command command)
        {
            var result = await _mediator.Send(command);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
```

### Test Your First Slice

**Run the application:**
```bash
dotnet run --project SpinnerNet.Web
```

**Test with curl:**
```bash
curl -X POST https://localhost:5001/api/users/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "displayName": "Test User",
    "password": "password123"
  }'
```

**Expected response:**
```json
{
  "success": true,
  "userId": "some-guid-here",
  "errorMessage": null
}
```

## Troubleshooting

### Common Issues

**1. Cosmos DB Emulator not starting:**
```bash
# Windows: Restart as administrator
# macOS/Linux: Check Docker container status
docker ps | grep cosmos
```

**2. OpenAI API errors:**
```bash
# Check API key
echo $OPENAI_API_KEY
# Check API quota: https://platform.openai.com/usage
```

**3. Ollama not responding:**
```bash
# Check service status
ollama list
# Restart service
ollama serve
```

**4. SSL certificate issues:**
```bash
# Trust development certificates
dotnet dev-certs https --trust
```

**5. Port conflicts:**
```bash
# Check what's using ports
netstat -ano | grep :5001
netstat -ano | grep :8081
# Kill conflicting processes or change ports in launchSettings.json
```

### Logs and Debugging

**Application logs:**
```bash
# Check console output when running
dotnet run --project SpinnerNet.Web --verbosity diagnostic
```

**Cosmos DB logs:**
- Check Windows Event Viewer → Applications and Services Logs → Microsoft-Azure-Cosmos-Emulator
- Or Docker logs: `docker logs [container-id]`

**AI Integration logs:**
- OpenAI responses logged at Debug level
- Ollama requests logged to console

## Next Steps

Once your development environment is working:

1. **Read**: `AI_INTEGRATION_GUIDE.md` for AI patterns
2. **Read**: `VERTICAL_SLICE_IMPLEMENTATION.md` for slice patterns
3. **Implement**: The remaining 14 Sprint 1 vertical slices
4. **Deploy**: Follow `AZURE_DEPLOYMENT_GUIDE.md` for production

**You're ready to start Sprint 1 implementation! 🚀**

---

## Quick Reference

**Start development:**
```bash
# Terminal 1: Start Cosmos DB (if not auto-started)
# Terminal 2: Start Ollama
ollama serve
# Terminal 3: Start application
dotnet watch run --project SpinnerNet.Web
```

**URLs:**
- Application: https://localhost:5001
- Cosmos DB Explorer: https://localhost:8081/_explorer/index.html
- Ollama API: http://localhost:11434

**Key files:**
- `appsettings.Development.json` - Local configuration
- `SpinnerNet.Core/Features/` - Vertical slices
- `SpinnerNet.Web/` - Blazor UI and API endpoints