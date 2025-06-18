# Development Guide

This guide will help you set up your development environment for contributing to Spinner.Net.

## Prerequisites

Before getting started, make sure you have the following installed:

- **.NET SDK**: Version 9.0 or later
- **Node.js**: Version 20.0.0 or later
- **Visual Studio Code** or **Visual Studio 2022** (recommended)
- **Git**: Latest version

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/spinner-net/spinner-net.git
cd spinner-net
```

### 2. Set Up Development Environment

#### Install .NET Tools

```bash
dotnet tool restore
```

#### Restore NuGet Packages

```bash
dotnet restore
```

#### Install Frontend Dependencies

```bash
cd src/Spinner.Net.Web
npm install
```

### 3. Database Setup

Spinner.Net uses SQLite for development by default:

```bash
# Create development database
dotnet ef database update --project src/Spinner.Net.Core
```

For vector database setup:

```bash
# Option 1: Use Docker for Qdrant (recommended for local development)
docker run -p 6333:6333 -p 6334:6334 qdrant/qdrant

# Option 2: Use in-memory vector database (for quick testing)
# No setup required, configured in appsettings.Development.json
```

### 4. Running the Application

#### Start the Backend

```bash
dotnet run --project src/Spinner.Net.Web
```

#### Start the Frontend Development Server

```bash
cd src/Spinner.Net.Web
npm run dev
```

The application will be available at:
- Backend API: https://localhost:7001
- Frontend: https://localhost:5173

## Project Structure

The Spinner.Net codebase is organized as follows:

```
spinner-net/
├── src/
│   ├── Spinner.Net.Core/             # Core application logic and domain models
│   ├── Spinner.Net.Core.Assets/      # Asset domain models and services
│   ├── Spinner.Net.Core.Identity/    # Identity and permission system
│   ├── Spinner.Net.Persona/          # Persona system implementation
│   ├── Spinner.Net.Integration/      # Integration framework and connectors
│   ├── Spinner.Net.Knowledge/        # Vector database and knowledge services
│   ├── Spinner.Net.Community/        # Community framework implementation
│   ├── Spinner.Net.UI/               # Shared UI components
│   ├── Spinner.Net.Web/              # Web application and API
│   └── Spinner.Net.Maui/             # MAUI cross-platform application
├── test/
│   ├── Spinner.Net.Core.Tests/       # Unit tests for core components
│   ├── Spinner.Net.Integration.Tests/ # Integration tests
│   └── Spinner.Net.E2E.Tests/        # End-to-end tests
├── docs/                             # Documentation
└── examples/                         # Example implementations and demos
```

## Development Workflow

### 1. Create a Branch

All development should be done in feature branches:

```bash
git checkout -b feature/your-feature-name
```

### 2. Make Changes

Write your code following the [Coding Standards](../CONTRIBUTING.md#coding-standards).

### 3. Run Tests

Before submitting a PR, make sure all tests pass:

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test test/Spinner.Net.Core.Tests

# Run with code coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov
```

### 4. Commit Your Changes

Follow the [conventional commits](https://www.conventionalcommits.org/) format:

```bash
git commit -m "feat: add new feature"
git commit -m "fix: resolve issue with feature"
git commit -m "docs: update documentation"
```

### 5. Submit a Pull Request

Push your branch to your fork and submit a pull request:

```bash
git push -u origin feature/your-feature-name
```

Then create a pull request on GitHub.

## Development Scripts

The repository includes several useful scripts:

```bash
# Build the solution
./build.sh

# Clean and rebuild
./build.sh --clean

# Run tests
./build.sh --test

# Build release configuration
./build.sh --release

# Run linting
./scripts/lint.sh

# Format code
./scripts/format.sh
```

## Working with Vector Databases

Spinner.Net uses vector databases for knowledge storage. For development, you can:

1. **Use Docker** (recommended):
   ```bash
   docker run -p 6333:6333 qdrant/qdrant
   ```

2. **Use in-memory implementation**:
   Configure in `appsettings.Development.json`:
   ```json
   "VectorDatabase": {
     "Provider": "InMemory",
     "ConnectionString": ""
   }
   ```

3. **Use existing instance**:
   ```json
   "VectorDatabase": {
     "Provider": "Qdrant",
     "ConnectionString": "http://localhost:6333"
   }
   ```

## Environment Configuration

Create a `appsettings.Development.json` file for local configurations:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Warning"
    }
  },
  "Database": {
    "Provider": "Sqlite",
    "ConnectionString": "Data Source=spinner-net-dev.db"
  },
  "VectorDatabase": {
    "Provider": "InMemory",
    "ConnectionString": ""
  },
  "Authentication": {
    "DevelopmentMode": true,
    "TestUser": {
      "Id": "dev-user",
      "Name": "Developer",
      "Email": "dev@example.com"
    }
  }
}
```

## Working with the Persona System

To develop with the Persona System:

1. **Create a test persona**:
```bash
dotnet run --project src/Spinner.Net.Tools create-persona --name "TestPersona" --preferences-file examples/personas/default.json
```

2. **Test persona interactions**:
```bash
dotnet run --project src/Spinner.Net.Tools test-interaction --persona-id "persona-id" --interaction-file examples/interactions/sample.json
```

## Working with Service Connectors

For development with service connectors:

1. **Use mock connectors** (preferred for most development):
   Configure in `appsettings.Development.json`:
   ```json
   "Integration": {
     "UseMockConnectors": true
   }
   ```

2. **Test with real services**:
   Create a `secrets.json` file (not committed to repo):
   ```json
   {
     "ServiceConnectors": {
       "Gmail": {
         "ClientId": "your-client-id",
         "ClientSecret": "your-client-secret"
       }
     }
   }
   ```

## Common Issues

### Unable to connect to vector database

Ensure Qdrant is running and accessible:
```bash
curl http://localhost:6333/collections
```

### Tests failing with permission errors

On Linux/macOS, ensure proper file permissions:
```bash
chmod +x ./build.sh
chmod +x ./scripts/*.sh
```

### Frontend build errors

Clear node modules and reinstall:
```bash
cd src/Spinner.Net.Web
rm -rf node_modules
npm install
```

## Additional Resources

- [Architecture Documentation](ARCHITECTURE.md)
- [API Documentation](API.md) (generated)
- [Frontend Development Guide](FRONTEND.md)
- [Testing Guide](TESTING.md)

## Getting Help

If you encounter issues not covered here:

- Check [GitHub Issues](https://github.com/spinner-net/spinner-net/issues)
- Ask in the [Developer Forum](https://community.spinner.net/c/development)
- Join the weekly developer office hours (see Community Calendar)

Happy coding!