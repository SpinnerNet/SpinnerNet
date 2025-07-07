# {FEATURE_NAME} - SpinnerNet Product Requirements Document

## 🎯 **FEATURE OVERVIEW**

**Feature Name**: {FEATURE_NAME}
**Implementation Type**: {IMPLEMENTATION_TYPE} (Core Feature / Enhancement / Integration / Infrastructure)
**Target Application**: {TARGET_APP} (SpinnerNet.App / SpinnerNet.Web / Both)

### **Key Objectives**
- {OBJECTIVE_1}
- {OBJECTIVE_2}
- {OBJECTIVE_3}

### **Implementation Duration**: {DURATION}
### **Priority**: {PRIORITY} (High / Medium / Low)

---

## 📋 **TECHNICAL SPECIFICATIONS**

### **Architecture Alignment**
- **Pattern**: Vertical Slice Architecture with MediatR
- **Data**: Cosmos DB with Microsoft NoSQL patterns (PascalCase properties)
- **UI Framework**: Blazor Server with MudBlazor components
- **Localization**: ILocalizationService with .resx resources
- **AI Integration**: WebLLM client-side + Semantic Kernel orchestration
- **Authentication**: Azure AD with role-based access

### **Core Components**

#### **1. Cosmos DB Document Models**
```csharp
// Models/CosmosDb/{FEATURE_NAME}Document.cs
using SpinnerNet.Shared.Models.CosmosDb;

namespace SpinnerNet.Shared.Models.CosmosDb;

public class {FEATURE_NAME}Document : CosmosDocument
{
    public string Id { get; set; } = string.Empty;               // Required by Cosmos
    public string Type { get; set; } = "{feature_type}";         // Document type for partitioning
    public string UserId { get; set; } = string.Empty;           // User association
    public string {FEATURE_NAME}Id { get; set; } = string.Empty; // Unique feature identifier
    
    // Feature-specific properties (PascalCase, no JsonPropertyName attributes)
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Additional feature properties...
}
```

#### **2. MediatR Command/Query Handlers**
```csharp
// Features/{FEATURE_NAME}/Create{FEATURE_NAME}.cs
using MediatR;
using SpinnerNet.Shared.Models.CosmosDb;
using SpinnerNet.Core.Data.CosmosDb;

namespace SpinnerNet.Core.Features.{FEATURE_NAME};

public record Create{FEATURE_NAME}Command(
    string UserId,
    string DisplayName
    // Additional parameters...
) : IRequest<Create{FEATURE_NAME}Result>;

public record Create{FEATURE_NAME}Result(
    bool Success,
    string {FEATURE_NAME}Id,
    string Message
);

public class Create{FEATURE_NAME}Handler : IRequestHandler<Create{FEATURE_NAME}Command, Create{FEATURE_NAME}Result>
{
    private readonly ICosmosRepository<{FEATURE_NAME}Document> _repository;
    private readonly ILogger<Create{FEATURE_NAME}Handler> _logger;

    public Create{FEATURE_NAME}Handler(
        ICosmosRepository<{FEATURE_NAME}Document> repository,
        ILogger<Create{FEATURE_NAME}Handler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Create{FEATURE_NAME}Result> Handle(Create{FEATURE_NAME}Command request, CancellationToken cancellationToken)
    {
        try
        {
            var {feature_name} = new {FEATURE_NAME}Document
            {
                Id = $"{feature_type}_{request.UserId}_{Guid.NewGuid()}",
                UserId = request.UserId,
                {FEATURE_NAME}Id = Guid.NewGuid().ToString(),
                DisplayName = request.DisplayName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repository.CreateAsync({feature_name}, cancellationToken);

            _logger.LogInformation("{FEATURE_NAME} created successfully for user {UserId}", request.UserId);

            return new Create{FEATURE_NAME}Result(true, {feature_name}.{FEATURE_NAME}Id, "Created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating {FEATURE_NAME} for user {UserId}", request.UserId);
            return new Create{FEATURE_NAME}Result(false, string.Empty, $"Error: {ex.Message}");
        }
    }
}
```

#### **3. Blazor Component with MudBlazor**
```razor
@* Components/Pages/{FEATURE_NAME}.razor *@
@page "/{feature_path}"
@using MediatR
@using SpinnerNet.Shared.Localization
@using SpinnerNet.Core.Features.{FEATURE_NAME}
@inject IMediator Mediator
@inject ILocalizationService LocalizationService
@inject IJSRuntime JSRuntime
@inject ILogger<{FEATURE_NAME}> Logger

<PageTitle>@LocalizationService.GetString("{FEATURE_NAME}_Title") - Spinner.Net</PageTitle>

<MudContainer MaxWidth="MaxWidth.Large" Class="mt-4">
    <MudPaper Class="pa-6" Elevation="2">
        <MudStack Spacing="4">
            
            <MudText Typo="Typo.h4" GutterBottom="true">
                @LocalizationService.GetString("{FEATURE_NAME}_Heading")
            </MudText>
            
            <MudText Typo="Typo.body1" Class="mb-4">
                @LocalizationService.GetString("{FEATURE_NAME}_Description")
            </MudText>

            @if (isLoading)
            {
                <MudProgressCircular Indeterminate="true" />
                <MudText Typo="Typo.body2">
                    @LocalizationService.GetString("Common_Loading")
                </MudText>
            }
            else
            {
                <MudForm @ref="form" Model="model" Validation="validator">
                    <MudStack Spacing="3">
                        
                        <MudTextField @bind-Value="model.DisplayName"
                                      Label="@LocalizationService.GetString("{FEATURE_NAME}_DisplayName")"
                                      Required="true"
                                      For="@(() => model.DisplayName)" />
                        
                        <MudButton Variant="Variant.Filled"
                                   Color="Color.Primary"
                                   StartIcon="@Icons.Material.Filled.Add"
                                   OnClick="HandleSubmit"
                                   Disabled="isProcessing">
                            @if (isProcessing)
                            {
                                <MudProgressCircular Size="Size.Small" Indeterminate="true" />
                                @LocalizationService.GetString("Common_Processing")
                            }
                            else
                            {
                                @LocalizationService.GetString("{FEATURE_NAME}_Create")
                            }
                        </MudButton>
                        
                    </MudStack>
                </MudForm>
            }

            @if (!string.IsNullOrEmpty(resultMessage))
            {
                <MudAlert Severity="@(isSuccess ? Severity.Success : Severity.Error)">
                    @resultMessage
                </MudAlert>
            }

        </MudStack>
    </MudPaper>
</MudContainer>

@code {
    private MudForm? form;
    private {FEATURE_NAME}Model model = new();
    private {FEATURE_NAME}ModelValidator validator = new();
    
    private bool isLoading = false;
    private bool isProcessing = false;
    private bool isSuccess = false;
    private string resultMessage = string.Empty;

    [Parameter] public string? UserId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        try
        {
            // Initialize component
            await LoadData();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error initializing {FEATURE_NAME} component");
            resultMessage = LocalizationService.GetString("Common_ErrorLoading");
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task LoadData()
    {
        // Load any required data
        await Task.CompletedTask;
    }

    private async Task HandleSubmit()
    {
        if (form == null || !await form.IsValid()) 
            return;

        isProcessing = true;
        resultMessage = string.Empty;

        try
        {
            var command = new Create{FEATURE_NAME}Command(
                UserId ?? string.Empty,
                model.DisplayName
            );

            var result = await Mediator.Send(command);

            isSuccess = result.Success;
            resultMessage = result.Success 
                ? LocalizationService.GetString("{FEATURE_NAME}_CreatedSuccessfully")
                : result.Message;

            if (result.Success)
            {
                model = new {FEATURE_NAME}Model(); // Reset form
                await form.ResetAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating {FEATURE_NAME}");
            isSuccess = false;
            resultMessage = LocalizationService.GetString("Common_ErrorProcessing");
        }
        finally
        {
            isProcessing = false;
        }
    }

    public class {FEATURE_NAME}Model
    {
        public string DisplayName { get; set; } = string.Empty;
        // Additional model properties...
    }

    public class {FEATURE_NAME}ModelValidator : AbstractValidator<{FEATURE_NAME}Model>
    {
        public {FEATURE_NAME}ModelValidator()
        {
            RuleFor(x => x.DisplayName)
                .NotEmpty()
                .WithMessage("Display name is required")
                .MaximumLength(100)
                .WithMessage("Display name must be 100 characters or less");
        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(ValidationContext<{FEATURE_NAME}Model>.CreateWithOptions(({FEATURE_NAME}Model)model, x => x.IncludeProperties(propertyName)));
            return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
        };
    }
}
```

#### **4. Service Integration (if applicable)**
```csharp
// Services/{FEATURE_NAME}Service.cs
using SpinnerNet.Shared.Models.CosmosDb;
using SpinnerNet.Core.Data.CosmosDb;

namespace SpinnerNet.App.Services;

public interface I{FEATURE_NAME}Service
{
    Task<List<{FEATURE_NAME}Document>> GetUserFeaturesAsync(string userId);
    Task<{FEATURE_NAME}Document?> GetFeatureAsync(string featureId);
    Task<bool> DeleteFeatureAsync(string featureId);
}

public class {FEATURE_NAME}Service : I{FEATURE_NAME}Service
{
    private readonly ICosmosRepository<{FEATURE_NAME}Document> _repository;
    private readonly ILogger<{FEATURE_NAME}Service> _logger;

    public {FEATURE_NAME}Service(
        ICosmosRepository<{FEATURE_NAME}Document> repository,
        ILogger<{FEATURE_NAME}Service> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<{FEATURE_NAME}Document>> GetUserFeaturesAsync(string userId)
    {
        try
        {
            var query = $"SELECT * FROM c WHERE c.Type = '{feature_type}' AND c.UserId = '{userId}' AND c.IsActive = true ORDER BY c.CreatedAt DESC";
            return await _repository.QueryAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving features for user {UserId}", userId);
            return new List<{FEATURE_NAME}Document>();
        }
    }

    public async Task<{FEATURE_NAME}Document?> GetFeatureAsync(string featureId)
    {
        try
        {
            var query = $"SELECT * FROM c WHERE c.Type = '{feature_type}' AND c.{FEATURE_NAME}Id = '{featureId}'";
            var results = await _repository.QueryAsync(query);
            return results.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feature {FeatureId}", featureId);
            return null;
        }
    }

    public async Task<bool> DeleteFeatureAsync(string featureId)
    {
        try
        {
            var feature = await GetFeatureAsync(featureId);
            if (feature == null) return false;

            feature.IsActive = false;
            feature.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(feature.Id, feature);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting feature {FeatureId}", featureId);
            return false;
        }
    }
}
```

#### **5. WebLLM Integration (if AI features)**
```typescript
// wwwroot/js/{feature-name}-ai.js
/**
 * {FEATURE_NAME} AI Integration for SpinnerNet
 * Extends WebLLM functionality for feature-specific AI operations
 */

class {FEATURE_NAME}AIIntegration {
    constructor() {
        this.webLLM = window.webLLM;
        this.isInitialized = false;
    }

    async initialize() {
        if (this.isInitialized) return;
        
        try {
            await this.webLLM.initialize();
            this.isInitialized = true;
            console.log(`✅ {FEATURE_NAME} AI integration initialized`);
        } catch (error) {
            console.error(`❌ Failed to initialize {FEATURE_NAME} AI:`, error);
        }
    }

    async processFeatureInput(userInput, context) {
        if (!this.isInitialized) {
            await this.initialize();
        }

        try {
            const prompt = this.buildFeaturePrompt(userInput, context);
            const response = await this.webLLM.generateInterviewResponse(prompt, 0, 1);
            return response;
        } catch (error) {
            console.error(`❌ Error processing {FEATURE_NAME} input:`, error);
            return this.getFallbackResponse();
        }
    }

    buildFeaturePrompt(userInput, context) {
        return `Process this {FEATURE_NAME} input: "${userInput}". Context: ${JSON.stringify(context)}. Provide helpful response.`;
    }

    getFallbackResponse() {
        return "I understand you're working with {FEATURE_NAME}. Could you provide more details?";
    }
}

// Global instance
const {feature_name}AI = new {FEATURE_NAME}AIIntegration();

// Blazor interop
window.{feature_name}AI = {
    initialize: () => {feature_name}AI.initialize(),
    processInput: (input, context) => {feature_name}AI.processFeatureInput(input, context)
};
```

#### **6. Localization Resources**
```xml
<!-- Resources/Strings.resx -->
<data name="{FEATURE_NAME}_Title" xml:space="preserve">
    <value>{FEATURE_TITLE}</value>
</data>
<data name="{FEATURE_NAME}_Heading" xml:space="preserve">
    <value>{FEATURE_HEADING}</value>
</data>
<data name="{FEATURE_NAME}_Description" xml:space="preserve">
    <value>{FEATURE_DESCRIPTION}</value>
</data>
<data name="{FEATURE_NAME}_DisplayName" xml:space="preserve">
    <value>Display Name</value>
</data>
<data name="{FEATURE_NAME}_Create" xml:space="preserve">
    <value>Create {FEATURE_NAME}</value>
</data>
<data name="{FEATURE_NAME}_CreatedSuccessfully" xml:space="preserve">
    <value>{FEATURE_NAME} created successfully!</value>
</data>
```

---

## 📚 **RESEARCH CONTEXT**

### **Documentation References**
{DOCUMENTATION_URLS}

### **Existing Patterns to Follow**
{EXISTING_PATTERNS}

### **Implementation Gotchas**
{GOTCHAS_AND_CONSIDERATIONS}

### **Best Practices**
{BEST_PRACTICES}

---

## 🔧 **IMPLEMENTATION PHASES**

### **Phase 1: Skeleton Code**
Implementation blueprint with detailed comments explaining each step:

#### **Tasks to Complete (in order)**
1. [ ] Create Cosmos DB document model with proper partitioning
2. [ ] Implement MediatR command/query handlers
3. [ ] Set up Cosmos repository registration in Program.cs
4. [ ] Create Blazor component with MudBlazor UI
5. [ ] Add localization strings to .resx files
6. [ ] Set up service layer (if needed)
7. [ ] Implement WebLLM integration (if AI features)
8. [ ] Add navigation routing
9. [ ] Register services in dependency injection

#### **Key Implementation Notes**
- Use Microsoft NoSQL patterns (PascalCase properties, no JsonPropertyName)
- Follow vertical slice architecture - one feature per file/folder
- Implement proper error handling and logging
- Use ILocalizationService for all user-facing text
- Apply age-adaptive UI patterns where applicable

### **Phase 2: Production Ready Code**
Complete implementation with full error handling, validation, and optimization:

#### **Production Enhancements**
- [ ] Add comprehensive input validation
- [ ] Implement proper error boundaries
- [ ] Add telemetry and monitoring
- [ ] Optimize performance for large datasets
- [ ] Add accessibility features (ARIA labels, keyboard navigation)
- [ ] Implement caching strategies
- [ ] Add audit logging for data changes
- [ ] Security hardening (input sanitization, authorization)

### **Phase 3: Unit Tests**
Comprehensive test suite covering all functionality:

#### **Test Coverage Requirements**
- [ ] Unit tests for MediatR handlers (95%+ coverage)
- [ ] Integration tests for Cosmos DB operations
- [ ] Blazor component tests using bUnit
- [ ] Service layer tests with mocking
- [ ] WebLLM integration tests (if applicable)
- [ ] Localization tests for all cultures
- [ ] Performance tests for critical paths
- [ ] End-to-end user workflow tests

---

## ✅ **VALIDATION GATES**

### **Build Validation**
```bash
# .NET Build and Syntax Check
cd src && dotnet build SpinnerNet.sln --configuration Release

# Style and Code Analysis
cd src && dotnet format --verify-no-changes
```

### **Test Validation**
```bash
# Unit Tests
cd src && dotnet test SpinnerNet.Tests --configuration Release --logger "console;verbosity=detailed"

# Integration Tests (if applicable)
cd src && dotnet test SpinnerNet.IntegrationTests --configuration Release
```

### **Deployment Validation**
```bash
# Publish Check
cd src && dotnet publish SpinnerNet.App/*.csproj -c Release -o publish

# Azure Deployment (if production ready)
cd src && ./DeployToAzure.sh
```

---

## 📊 **QUALITY CHECKLIST**

- [ ] All necessary SpinnerNet context included
- [ ] Follows vertical slice architecture patterns
- [ ] Uses Microsoft NoSQL Cosmos DB patterns
- [ ] Implements proper localization with ILocalizationService
- [ ] MudBlazor components follow design system
- [ ] WebLLM integration (if AI features)
- [ ] Age-adaptive UI considerations
- [ ] Comprehensive error handling
- [ ] Security best practices applied
- [ ] Validation gates are executable
- [ ] References existing SpinnerNet patterns
- [ ] Clear implementation path defined
- [ ] Performance considerations addressed

---

## 🏆 **SUCCESS CRITERIA**

**Implementation Confidence Score**: _/10

**Deployment Readiness**: 
- [ ] Development environment
- [ ] Staging environment  
- [ ] Production environment

**Feature Acceptance Criteria**:
- [ ] {ACCEPTANCE_CRITERIA_1}
- [ ] {ACCEPTANCE_CRITERIA_2}
- [ ] {ACCEPTANCE_CRITERIA_3}

---

## 📝 **IMPLEMENTATION NOTES**

### **Critical Reminders**
- NEVER hardcode strings - use localization service
- NEVER use JsonPropertyName attributes with Cosmos DB
- ALWAYS follow vertical slice architecture
- ALWAYS implement proper logging and error handling
- ALWAYS validate user input thoroughly
- ALWAYS consider age-adaptive UI requirements

### **SpinnerNet-Specific Considerations**
- Two-app architecture: SpinnerNet.Web (public) vs SpinnerNet.App (authenticated)
- Azure KeyVault for secrets management
- WebLLM for client-side AI processing
- Semantic Kernel for server-side AI orchestration
- Ultra-low latency TypeLeap interface requirements
- Comprehensive localization for global audience