# Spinner.Net Quick Start Guide

Get Spinner.Net running in Azure in under 30 minutes with user persona initialization!

## 🎯 What You're Building

A complete **Digital Persona Platform** featuring:
- ✅ User registration and authentication
- ✅ 4-step persona onboarding wizard
- ✅ Age-based TypeLeap UI adaptation
- ✅ Basic dashboard with persona insights
- ✅ Azure-ready infrastructure
- 🔜 AI companion integration (future)
- 🔜 Commercial room framework (LichtFlow integration)

## 🚀 Deployment Steps

### Step 1: Prerequisites

```bash
# Install required tools
- Azure CLI
- .NET 9.0 SDK
- PowerShell (Windows) or Bash (Linux/macOS)
- Git
```

### Step 2: Clone and Prepare

```bash
# Clone the repository
git clone <your-repo-url>
cd LichtFlow/Spinner.Net-Public/src

# Restore dependencies
dotnet restore SpinnerNet.sln
```

### Step 3: Deploy Azure Infrastructure

#### Option A: PowerShell (Windows)
```powershell
cd Azure
.\deploy.ps1 -Environment "dev" -ResourceGroupName "spinnernet-dev-rg" -Location "westeurope" -SqlAdminPassword "SpinnerNet123!"
```

#### Option B: Bash (Linux/macOS)
```bash
cd Azure
chmod +x deploy.sh
./deploy.sh -e dev -g spinnernet-dev-rg -l westeurope -p "SpinnerNet123!"
```

### Step 4: Deploy Application Code

```bash
# Build and publish
dotnet publish SpinnerNet.Web -c Release -o ./publish

# Create deployment package
cd publish
zip -r ../spinnernet-app.zip .
cd ..

# Deploy to Azure App Service
az webapp deployment source config-zip \
  --resource-group spinnernet-dev-rg \
  --name spinnernet-dev-web \
  --src spinnernet-app.zip
```

### Step 5: Initialize Database

```bash
# The application will automatically create the database schema on first run
# Entity Framework will run migrations automatically
```

## 🎉 Test Your Deployment

1. **Access Your App**: Visit the Web App URL from deployment output
2. **Register a New User**: Click "Sign Up" 
3. **Complete Onboarding**: Go through the 4-step persona wizard
4. **Experience TypeLeap**: Notice how UI adapts to your age and preferences

## 🧪 Test Scenarios

### Scenario 1: Child User (Age 10)
- **UI Adaptation**: Simple interface, large icons, bright colors
- **Features**: Parental controls enabled, guidance tooltips
- **Navigation**: Simplified menu structure

### Scenario 2: Teen User (Age 16)
- **UI Adaptation**: Standard interface with social features
- **Features**: Enhanced social features, gamification
- **Navigation**: Full navigation with teen-appropriate content

### Scenario 3: Senior User (Age 70)
- **UI Adaptation**: Large fonts, high contrast, simplified navigation
- **Features**: Extra tooltips, minimal animations
- **Navigation**: Simple menu with clear labels

### Scenario 4: Professional User (Age 35, Photography Interest)
- **UI Adaptation**: Advanced interface with professional tools
- **Features**: Hints about LichtFlow photography room
- **Navigation**: Full feature access

## 📊 What You'll See

### After Registration
1. **Onboarding Wizard** with 4 steps:
   - Basic Information (name, age, cultural background)
   - Language & Location (timezone, interests)
   - Preferences (learning style, UI complexity)
   - Privacy & AI (data sovereignty, companion preferences)

### After Onboarding
1. **Personalized Dashboard** showing:
   - Your digital persona details
   - UI adaptation settings
   - Placeholder for future AI companions
   - Placeholder for commercial rooms

### TypeLeap Adaptations
- **Font sizes** adjust based on age
- **Color schemes** adapt to preferences and age
- **UI complexity** scales with user capability
- **Navigation** simplifies for children and seniors

## 🔧 Architecture Overview

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Spinner.Net   │    │  Azure App      │    │   Azure SQL     │
│   Web App       │◄──►│   Service       │◄──►│   Database      │
│   (Blazor)      │    │                 │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  MudBlazor UI   │    │ Application     │    │  User Personas  │
│  + TypeLeap     │    │ Insights        │    │  + Metadata     │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 🛠️ Key Components

### 1. **SpinnerNet.Shared**
- User and UserPersona models
- DTOs for onboarding workflow
- Shared contracts and interfaces

### 2. **SpinnerNet.Personas**
- PersonaService for persona management
- Age-based TypeLeap adaptation algorithms
- Persona creation and configuration logic

### 3. **SpinnerNet.Core**
- Entity Framework DbContext
- ASP.NET Core Identity integration
- Database migrations and seeding

### 4. **SpinnerNet.Web**
- Blazor Server application
- MudBlazor UI components
- Onboarding wizard implementation
- TypeLeap adaptation engine

## 🔮 Next Steps

### Immediate (Ready to Implement)
1. **CI/CD Pipeline**: Automated deployments
2. **Basic Data Sovereignty**: User data controls
3. **Enhanced TypeLeap**: Cultural adaptations

### Phase 2 (Architecture Ready)
1. **AI Companion Integration**: Daily buddy system
2. **Commercial Room Framework**: LichtFlow integration
3. **Vector Database**: Semantic storage
4. **Advanced Persona Features**: Context switching

### Phase 3 (Platform Expansion)
1. **Multiple AI Companions**: Specialized expertise
2. **Room Provider SDK**: Third-party integrations
3. **Advanced Data Sovereignty**: Sandboxing
4. **Cross-Platform Apps**: Mobile and desktop

## 🆘 Troubleshooting

### Common Issues

1. **Deployment Fails**
   ```bash
   # Check Azure permissions
   az account show
   az role assignment list --assignee $(az account show --query user.name -o tsv)
   ```

2. **Database Connection Issues**
   ```bash
   # Verify SQL Server firewall rules
   az sql server firewall-rule list --resource-group spinnernet-dev-rg --server spinnernet-dev-sql
   ```

3. **Application Won't Start**
   ```bash
   # Check application logs
   az webapp log tail --resource-group spinnernet-dev-rg --name spinnernet-dev-web
   ```

### Getting Help

- **Azure Issues**: Check Azure Portal → Activity Log
- **Application Issues**: Application Insights → Failures
- **Database Issues**: SQL Server → Query Performance Insight

## 🎊 Success Metrics

You'll know it's working when:
- ✅ Users can register and login
- ✅ Onboarding wizard completes successfully  
- ✅ UI adapts based on user age and preferences
- ✅ Dashboard shows personalized persona information
- ✅ Application runs smoothly in Azure

**Congratulations! You now have a running Spinner.Net digital persona platform!** 🎉

Ready for AI companions, commercial rooms, and the full three-layer ecosystem.