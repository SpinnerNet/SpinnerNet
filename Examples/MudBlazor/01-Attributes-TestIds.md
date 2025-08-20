# MudBlazor HTML Attributes & Test IDs Pattern

## Problem Statement
MudBlazor components don't directly support lowercase HTML attributes like `id` or `data-testid`. Direct attribute assignment causes compilation errors.

## Working Solution

### ✅ Correct Pattern: @attributes Dictionary
```razor
@* For simple ID *@
<MudTextField @bind-Value="displayName"
             Label="Display Name"
             @attributes="@(new Dictionary<string, object> { { "id", "display-name-input" } })" />

@* For data-testid *@
<MudButton Variant="Variant.Filled"
          Color="Color.Primary"
          @attributes="@(new Dictionary<string, object> { { "data-testid", "submit-button" } })">
    Submit
</MudButton>

@* Multiple attributes *@
<MudSelect @bind-Value="selectedOption"
          @attributes="@(new Dictionary<string, object> { 
              { "id", "option-select" },
              { "data-testid", "option-selector" },
              { "aria-label", "Choose an option" }
          })">
    <MudSelectItem Value="1">Option 1</MudSelectItem>
</MudSelect>
```

### ❌ Common Errors
```razor
@* WRONG: Direct attribute assignment *@
<MudTextField id="name-input" />  @* Compilation error *@

@* WRONG: Uppercase ID *@
<MudTextField Id="name-input" />  @* MudBlazor doesn't have Id property *@

@* WRONG: String interpolation in dictionary *@
@attributes="@(new Dictionary<string, object> { { "id", $"input-{index}" } })"
@* Use proper string concatenation instead *@
```

## Real-World Implementation

### Form with Comprehensive Test IDs
```razor
@page "/form-example"
@inject ILocalizationService LocalizationService

<MudGrid>
    <MudItem xs="12" md="6">
        <MudTextField @bind-Value="_name"
                     Label="@LocalizationService.GetString("Form_Name")"
                     Required="true"
                     @attributes="@(new Dictionary<string, object> { { "id", "name-input" } })" />
    </MudItem>

    <MudItem xs="12" md="6">
        <MudNumericField @bind-Value="_age"
                        Label="@LocalizationService.GetString("Form_Age")"
                        Min="1" Max="120"
                        @attributes="@(new Dictionary<string, object> { { "id", "age-input" } })" />
    </MudItem>

    <MudItem xs="12">
        <MudSelect @bind-Value="_category"
                  Label="@LocalizationService.GetString("Form_Category")"
                  @attributes="@(new Dictionary<string, object> { { "id", "category-select" } })">
            <MudSelectItem Value="@("personal")">Personal</MudSelectItem>
            <MudSelectItem Value="@("business")">Business</MudSelectItem>
        </MudSelect>
    </MudItem>

    <MudItem xs="12">
        <MudButton Variant="Variant.Filled"
                  Color="Color.Primary"
                  OnClick="Submit"
                  @attributes="@(new Dictionary<string, object> { { "id", "submit-button" } })">
            @LocalizationService.GetString("Form_Submit")
        </MudButton>
    </MudItem>
</MudGrid>

@code {
    private string _name = "";
    private int _age = 18;
    private string _category = "personal";

    private async Task Submit()
    {
        // Form submission logic
    }
}
```

## Testing Approach

### Safari MCP Automation
```javascript
// Find and interact with MudBlazor components
const nameInput = document.getElementById('name-input');
const ageInput = document.getElementById('age-input');
const categorySelect = document.getElementById('category-select');
const submitButton = document.getElementById('submit-button');

// Fill form
if (nameInput) {
    nameInput.value = 'Test User';
    nameInput.dispatchEvent(new Event('input', { bubbles: true }));
}

if (ageInput) {
    ageInput.value = '25';
    ageInput.dispatchEvent(new Event('input', { bubbles: true }));
}

// Verify form state
const formState = {
    name: nameInput?.value,
    age: ageInput?.value,
    category: categorySelect?.value,
    submitEnabled: !submitButton?.disabled
};

console.log('Form state:', JSON.stringify(formState, null, 2));
```

## Key Insights

1. **MudBlazor uses Material Design naming**: Properties follow PascalCase Material conventions
2. **@attributes is pass-through**: Dictionary entries become raw HTML attributes
3. **Browser sees lowercase**: Even though Blazor uses PascalCase, browser DOM gets lowercase
4. **Works with all MudBlazor components**: TextField, Button, Select, DatePicker, etc.

## Related Patterns
- [41-Component-Localization.md] - Using ILocalizationService
- [50-Testing-SafariMCP-DOM.md] - DOM manipulation patterns
- [11-MudBlazor-Stepper-DataFlow.md] - Complex component patterns

## Production Example
From `PersonaBasicInfoStep.razor`:
```razor
<MudTextField @bind-Value="_displayName"
             Label="@LocalizationService.GetString("PersonaCreation_DisplayName")"
             Required="true"
             MaxLength="50"
             Immediate="true"
             OnBlur="ValidateAndNotify"
             @attributes="@(new Dictionary<string, object> { { "id", "display-name-input" } })" />
```

## Browser Compatibility
- ✅ Chrome 90+
- ✅ Safari 14+
- ✅ Firefox 88+
- ✅ Edge 90+

## Performance Notes
- Dictionary allocation is minimal (< 1ms)
- No impact on rendering performance
- Attributes applied during initial render