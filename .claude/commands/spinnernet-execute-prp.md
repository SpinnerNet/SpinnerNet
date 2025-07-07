# Execute SpinnerNet PRP

Implement a SpinnerNet feature using the SpinnerNet-specific PRP file.

## PRP File: $ARGUMENTS

## SpinnerNet Execution Process

1. **Load SpinnerNet PRP & Foundation Documents**
   - Read the specified SpinnerNet PRP file
   - Understand all SpinnerNet context and requirements
   - Follow all instructions in the PRP and extend the research if needed
   - **MANDATORY: Review Foundation Documents:**
     - CLAUDE.md for current project state and critical rules
     - docs/ARCHITECTURE.md for system design patterns
     - docs/VERTICAL_SLICE_ARCHITECTURE.md for implementation patterns
     - docs/AI_INTEGRATION_GUIDE.md for AI integration patterns
     - docs/Age-Adaptive-UI-Implementation.md for age adaptation requirements
     - docs/DATA_MODEL_DESIGN.md for Cosmos DB patterns
     - docs/PROJECT_STRUCTURE.md for file organization standards
     - docs/DEVELOPMENT.md for development workflow
     - docs/AZURE_DEPLOYMENT_GUIDE.md for deployment patterns
   - Ensure you have all needed context to implement the PRP fully within SpinnerNet architecture
   - Do more Context7 documentation lookups and codebase exploration as needed

2. **ULTRATHINK - SpinnerNet Planning**
   - Think hard before you execute the plan. Create a comprehensive plan addressing all SpinnerNet requirements.
   - Break down complex tasks into smaller, manageable steps using your TodoWrite tool.
   - Use the TodoWrite tool to create and track your SpinnerNet implementation plan.
   - Identify SpinnerNet implementation patterns from existing codebase to follow:
     - Vertical slice architecture patterns
     - Cosmos DB Microsoft NoSQL patterns
     - MediatR command/query handler patterns
     - Blazor Server component patterns with MudBlazor
     - Localization service integration patterns
     - WebLLM integration patterns (if AI features)
     - Age-adaptive UI patterns

3. **Execute the SpinnerNet Implementation Plan**
   - Execute the PRP following SpinnerNet architecture
   - Implement all the code using SpinnerNet patterns and conventions
   - Follow these critical SpinnerNet rules:
     - NEVER hardcode strings - use ILocalizationService
     - NEVER use JsonPropertyName attributes with Cosmos DB
     - ALWAYS follow vertical slice architecture
     - ALWAYS implement proper logging and error handling
     - ALWAYS validate user input thoroughly
     - ALWAYS consider age-adaptive UI requirements
     - Use Microsoft NoSQL patterns (PascalCase properties)
     - Follow MudBlazor design system patterns
     - Integrate WebLLM for client-side AI (if applicable)
     - Use Azure KeyVault for secrets (never hardcode)

4. **SpinnerNet Validation**
   - Run each SpinnerNet validation command:
   ```bash
   # .NET Build Validation
   cd src && dotnet build SpinnerNet.sln --configuration Release
   
   # Code Format Validation
   cd src && dotnet format --verify-no-changes
   
   # Unit Tests
   cd src && dotnet test SpinnerNet.Tests --configuration Release --logger "console;verbosity=detailed"
   
   # Publish Check
   cd src && dotnet publish SpinnerNet.App/*.csproj -c Release -o publish
   ```
   - Fix any failures following SpinnerNet patterns
   - Re-run until all validation passes

5. **SpinnerNet Compliance Check**
   - Verify Cosmos DB document follows Microsoft NoSQL patterns
   - Confirm MediatR handlers follow vertical slice architecture
   - Check Blazor component uses MudBlazor design system
   - Validate all strings use ILocalizationService
   - Test WebLLM integration (if applicable)
   - Verify age-adaptive UI considerations
   - Check Azure KeyVault usage for secrets
   - Confirm proper error handling and logging

6. **Azure Integration Testing (Safari MCP)**
   - **Navigate to Azure deployments for live testing:**
   ```bash
   # Test SpinnerNet.Web deployment
   mcp__safari__navigate "https://spinnernet-3lauxg.azurewebsites.net"
   mcp__safari__take_screenshot
   mcp__safari__get_console_logs
   
   # Test SpinnerNet.App deployment
   mcp__safari__navigate "https://spinnernet-app-3lauxg.azurewebsites.net"
   mcp__safari__take_screenshot
   mcp__safari__get_console_logs
   ```
   
   - **WebLLM Browser Compatibility Testing:**
   ```bash
   # Test WebLLM initialization in deployed environment
   mcp__safari__execute_script "window.webLLM.initialize().then(() => console.log('WebLLM ready')).catch(e => console.error('WebLLM failed:', e))"
   mcp__safari__get_console_logs
   
   # Test WebGPU availability
   mcp__safari__execute_script "navigator.gpu ? console.log('WebGPU available') : console.log('WebGPU not available')"
   ```
   
   - **Age-Adaptive UI Validation:**
   ```bash
   # Test different age profiles in live environment
   mcp__safari__navigate "https://spinnernet-app-3lauxg.azurewebsites.net/interview"
   mcp__safari__click_element "[data-testid='age-6-profile']"
   mcp__safari__take_screenshot
   mcp__safari__click_element "[data-testid='age-65-profile']"
   mcp__safari__take_screenshot
   ```
   
   - **Feature-Specific Integration Testing:**
   ```bash
   # Test new feature in deployed environment
   mcp__safari__navigate "https://spinnernet-app-3lauxg.azurewebsites.net/{feature-path}"
   mcp__safari__wait_for_element "[data-testid='{feature-element}']"
   mcp__safari__type_text "[data-testid='input-field']" "Test input"
   mcp__safari__click_element "[data-testid='submit-button']"
   mcp__safari__wait_for_element "[data-testid='result']"
   mcp__safari__get_element_text "[data-testid='result']"
   mcp__safari__take_screenshot
   ```
   
   - **Performance and Error Monitoring:**
   ```bash
   # Monitor for JavaScript errors during testing
   mcp__safari__start_error_monitoring true
   # Perform feature testing...
   mcp__safari__get_console_logs
   mcp__safari__stop_error_monitoring
   ```

7. **Complete SpinnerNet Implementation**
   - Ensure all SpinnerNet checklist items done
   - Run final SpinnerNet validation suite
   - Report completion status with SpinnerNet-specific metrics
   - Read the PRP again to ensure you have implemented everything following SpinnerNet patterns

7. **SpinnerNet Deployment Readiness** (if applicable)
   - Verify Azure deployment configuration
   - Check both SpinnerNet.Web and SpinnerNet.App integration
   - Validate localization for all supported cultures
   - Test WebLLM browser compatibility
   - Confirm age-adaptive UI across age groups

8. **Reference the SpinnerNet PRP**
   - You can always reference the SpinnerNet PRP again if needed
   - Check against existing SpinnerNet Phase documents for consistency

## SpinnerNet-Specific Validation Commands

### Development Validation
```bash
# Check project structure
cd src && find . -name "*.cs" -path "*/Features/*" | head -10
cd src && find . -name "*Document.cs" -path "*/CosmosDb/*" | head -5

# Verify localization files
cd src && find . -name "*.resx" -path "*/Resources/*"

# Check MudBlazor component usage
cd src && grep -r "MudBlazor" SpinnerNet.App/Components/ | head -5
```

### Integration Validation
```bash
# WebLLM integration check
cd src && find . -name "*.js" -path "*/wwwroot/js/*" | xargs grep -l "webLLM"

# Cosmos DB pattern validation
cd src && grep -r "JsonPropertyName" SpinnerNet.Shared/Models/CosmosDb/ || echo "✅ No JsonPropertyName found (correct)"

# Localization service usage
cd src && grep -r "ILocalizationService" SpinnerNet.App/Components/ | wc -l
```

### Production Readiness
```bash
# Azure deployment check
cd src && test -f "Azure/deploy.bicep" && echo "✅ Azure deployment ready"

# Security validation (no hardcoded secrets)
cd src && grep -r "ClientSecret\|ApiKey\|Password" appsettings.json | grep -v '""' || echo "✅ No hardcoded secrets"
```

## SpinnerNet Error Recovery

If validation fails, use these SpinnerNet-specific error patterns to fix and retry:

### Common SpinnerNet Issues
1. **Cosmos DB JsonPropertyName**: Remove all JsonPropertyName attributes, use PascalCase properties
2. **Hardcoded Strings**: Replace with `LocalizationService.GetString("Key_Name")`
3. **Non-Vertical Slice**: Move handler to `Features/{FeatureName}/` folder
4. **Missing MudBlazor**: Use MudBlazor components, not plain HTML
5. **WebLLM Not Integrated**: Check webllm-integration.js patterns
6. **Age-Adaptive Missing**: Add age profile considerations

### SpinnerNet Pattern Recovery
- Check existing Phase1, Phase2, Phase3 PRP documents for patterns
- Review similar features in SpinnerNet.Core/Features/ for MediatR patterns  
- Examine SpinnerNet.App/Components/ for Blazor component patterns
- Study SpinnerNet.Shared/Models/CosmosDb/ for document patterns

Note: If validation fails, use SpinnerNet-specific error patterns in PRP to fix and retry following the two-app architecture (Web vs App) requirements.