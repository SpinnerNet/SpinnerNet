# MudBlazor - Stepper Data Flow Pattern

## Problem Statement
MudStepper components need to collect and preserve data across multiple steps while handling navigation, validation, and state management.

## Working Solution

### Complete Stepper Implementation
```razor
@page "/wizard"
@using System.Collections.Generic
@inject ILogger<WizardPage> Logger

<MudStepper @ref="_stepper" 
            ActiveIndex="@_activeStepIndex"
            NonLinear="false"
            ShowResetButton="false"
            CompletedStepColor="Color.Success"
            CurrentStepColor="Color.Primary"
            OnPreviewInteraction="OnStepPreview">
    
    <MudStepperPanel>
        <Title>
            <MudText>Basic Information</MudText>
        </Title>
        <ChildContent>
            <BasicInfoStep @ref="_basicInfoStep"
                          OnValidationChanged="OnStep1ValidationChanged"
                          OnAgeChanged="OnAgeChanged" />
        </ChildContent>
    </MudStepperPanel>

    <MudStepperPanel>
        <Title>
            <MudText>Interview Questions</MudText>
        </Title>
        <ChildContent>
            <InterviewStep @ref="_interviewStep"
                          UserAge="@_userAge" />
        </ChildContent>
    </MudStepperPanel>

    <MudStepperPanel>
        <Title>
            <MudText>Review & Submit</MudText>
        </Title>
        <ChildContent>
            <ReviewStep BasicInfo="@_basicInfo"
                       InterviewAnswers="@_interviewAnswers" />
        </ChildContent>
    </MudStepperPanel>

</MudStepper>

@code {
    private MudStepper _stepper = null!;
    private BasicInfoStep _basicInfoStep = null!;
    private InterviewStep _interviewStep = null!;
    
    private int _activeStepIndex = 0;
    private int _userAge = 18;
    private Dictionary<string, string> _basicInfo = new();
    private Dictionary<string, string> _interviewAnswers = new();

    private async Task OnStepPreview(InteractionType interaction, int targetIndex)
    {
        // Collect data before navigation
        if (interaction == InteractionType.Next || interaction == InteractionType.Previous)
        {
            await CollectCurrentStepData();
        }

        // Validate before moving forward
        if (interaction == InteractionType.Next && !await ValidateCurrentStep())
        {
            return; // Cancel navigation
        }

        _activeStepIndex = targetIndex;
    }

    private async Task<bool> CollectCurrentStepData()
    {
        switch (_activeStepIndex)
        {
            case 0: // Basic Info
                if (_basicInfoStep != null)
                {
                    _basicInfo = _basicInfoStep.GetFormData();
                    Logger.LogInformation("Collected basic info: {Count} fields", _basicInfo.Count);
                }
                break;
                
            case 1: // Interview
                if (_interviewStep != null)
                {
                    _interviewAnswers = _interviewStep.GetAnswers();
                    Logger.LogInformation("Collected interview answers: {Count}", _interviewAnswers.Count);
                }
                break;
        }
        
        return true;
    }

    private async Task<bool> ValidateCurrentStep()
    {
        switch (_activeStepIndex)
        {
            case 0:
                return _basicInfoStep?.IsValid() ?? false;
            case 1:
                return _interviewStep?.IsValid() ?? false;
            default:
                return true;
        }
    }

    private void OnStep1ValidationChanged(bool isValid)
    {
        // Enable/disable navigation based on validation
        StateHasChanged();
    }

    private void OnAgeChanged(int age)
    {
        _userAge = age;
        // Update theme or other age-dependent features
    }
}
```

### Step Component Pattern
```razor
@* BasicInfoStep.razor *@
@inject ILogger<BasicInfoStep> Logger

<MudGrid>
    <MudItem xs="12" md="6">
        <MudTextField @bind-Value="_name"
                     Label="Name"
                     Required="true"
                     Immediate="true"
                     OnBlur="ValidateForm" />
    </MudItem>
    
    <MudItem xs="12" md="6">
        <MudNumericField @bind-Value="_age"
                        Label="Age"
                        Min="5" Max="120"
                        Required="true"
                        OnBlur="OnAgeBlur" />
    </MudItem>
</MudGrid>

@code {
    [Parameter] public EventCallback<bool> OnValidationChanged { get; set; }
    [Parameter] public EventCallback<int> OnAgeChanged { get; set; }

    private string _name = "";
    private int _age = 18;
    private bool _isValid = false;

    public Dictionary<string, string> GetFormData()
    {
        return new Dictionary<string, string>
        {
            ["Name"] = _name,
            ["Age"] = _age.ToString()
        };
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(_name) && _age >= 5 && _age <= 120;
    }

    private async Task ValidateForm()
    {
        var wasValid = _isValid;
        _isValid = IsValid();
        
        if (wasValid != _isValid)
        {
            await OnValidationChanged.InvokeAsync(_isValid);
        }
    }

    private async Task OnAgeBlur()
    {
        await ValidateForm();
        await OnAgeChanged.InvokeAsync(_age);
    }
}
```

## Key Patterns

### 1. OnPreviewInteraction for Data Collection
```csharp
private async Task OnStepPreview(InteractionType interaction, int targetIndex)
{
    // Always collect data before navigation
    await CollectCurrentStepData();
    
    // Validate only when moving forward
    if (interaction == InteractionType.Next && !await ValidateCurrentStep())
    {
        return; // Cancel navigation by not updating index
    }
    
    _activeStepIndex = targetIndex;
}
```

### 2. Step Component Data Access
```csharp
// Parent accesses child data through public methods
public Dictionary<string, string> GetFormData()
{
    return new Dictionary<string, string>
    {
        ["Name"] = _name,
        ["Age"] = _age.ToString()
    };
}
```

### 3. Validation Flow
```csharp
// Child notifies parent of validation state
[Parameter] public EventCallback<bool> OnValidationChanged { get; set; }

private async Task ValidateForm()
{
    _isValid = IsValid();
    await OnValidationChanged.InvokeAsync(_isValid);
}
```

## Common Errors & Solutions

### ❌ Error: Lost Data Between Steps
```csharp
// WRONG: Not collecting data before navigation
<MudStepper ActiveIndex="@_activeStepIndex">
    <!-- Steps without OnPreviewInteraction -->
</MudStepper>
```

### ✅ Solution: Use OnPreviewInteraction
```csharp
<MudStepper OnPreviewInteraction="OnStepPreview">
    <!-- Collect data in OnStepPreview -->
</MudStepper>
```

### ❌ Error: Can't Access Step Data
```csharp
// WRONG: Trying to access private fields
var name = _basicInfoStep._name; // Can't access private field
```

### ✅ Solution: Public Methods
```csharp
// RIGHT: Use public method
var data = _basicInfoStep.GetFormData();
var name = data["Name"];
```

## Advanced Patterns

### Dynamic Step Visibility
```razor
<MudStepper>
    @foreach (var step in _dynamicSteps)
    {
        @if (step.IsVisible)
        {
            <MudStepperPanel>
                <Title>@step.Title</Title>
                <ChildContent>
                    <DynamicComponent Type="@step.ComponentType" 
                                     Parameters="@step.Parameters" />
                </ChildContent>
            </MudStepperPanel>
        }
    }
</MudStepper>
```

### Async Step Loading
```csharp
private async Task LoadStepData(int stepIndex)
{
    _isLoading = true;
    try
    {
        var data = await DataService.LoadStepDataAsync(stepIndex);
        ApplyDataToStep(stepIndex, data);
    }
    finally
    {
        _isLoading = false;
    }
}
```

## Testing Approach

### Safari MCP Testing
```javascript
// Test step navigation
function testStepper() {
    // Find current step
    const activeStep = document.querySelector('.mud-stepper-active');
    console.log('Current step:', activeStep?.textContent);
    
    // Fill form in step 1
    const nameInput = document.querySelector('input[label*="Name"]');
    if (nameInput) {
        nameInput.value = 'Test User';
        nameInput.dispatchEvent(new Event('input', { bubbles: true }));
        nameInput.dispatchEvent(new Event('blur', { bubbles: true }));
    }
    
    // Navigate to next step
    const nextButton = Array.from(document.querySelectorAll('button'))
        .find(btn => btn.textContent.includes('Next'));
    if (nextButton) nextButton.click();
    
    // Verify data persisted
    setTimeout(() => {
        const reviewData = document.querySelector('.review-content');
        console.log('Review shows:', reviewData?.textContent);
    }, 500);
}
```

## Performance Notes
- Step components render on-demand (lazy loading)
- Data collection is synchronous to prevent race conditions
- Validation runs immediately on blur for responsiveness
- OnPreviewInteraction prevents unnecessary navigation

## Production Tips
1. **Always use @ref** for step components you need to access
2. **Implement GetFormData()** in every step component
3. **Use OnPreviewInteraction** for data collection, not OnStepChanged
4. **Validate on blur** for immediate user feedback
5. **Log data collection** for debugging multi-step flows

## Related Patterns
- [01-Attributes-TestIds.md] - Adding test IDs to steps
- [04-Form-Validation.md] - Validation patterns
- [../Services/04-State-Management.md] - Complex state management
- [../Testing/03-Form-Automation.md] - Testing multi-step forms

## Browser Compatibility
- ✅ All modern browsers (MudBlazor 6.0+)
- ✅ Mobile responsive
- ✅ Keyboard navigation support
- ✅ Screen reader compatible with ARIA