# Safari MCP Testing Guide

## Overview

Safari MCP (Model Context Protocol) enables automated browser testing through programmatic control of Safari. This guide provides universal patterns, best practices, and common pitfalls for testing **any web application** - not just specific projects.

## Key MCP Tools Available

### Navigation & Page Control
- `mcp__safari__navigate` - Navigate to URL
- `mcp__safari__get_page_info` - Get current URL, title
- `mcp__safari__refresh_page` - Refresh current page
- `mcp__safari__take_screenshot` - Capture visual state
- `mcp__safari__go_back` / `mcp__safari__go_forward` - Browser history

### Interaction Tools
- `mcp__safari__click_element` - Click elements by CSS selector
- `mcp__safari__type_text` - Enter text into inputs
- `mcp__safari__scroll_to` - Scroll to elements/coordinates
- `mcp__safari__select_option` - Select dropdown options

### Information Gathering
- `mcp__safari__execute_script` - Run JavaScript
- `mcp__safari__get_element_text` - Extract element text
- `mcp__safari__get_console_logs` - View browser console
- `mcp__safari__wait_for_element` - Wait for elements to appear

### Error Monitoring
- `mcp__safari__start_error_monitoring` - Monitor JS errors
- `mcp__safari__stop_error_monitoring` - Stop error monitoring

## Testing Patterns

### 1. State Verification First
**ALWAYS** verify actual state before assuming progress:

```javascript
// ❌ BAD: Assume button click worked
await mcp__safari__click_element({ selector: 'button' });
// Continue assuming it worked...

// ✅ GOOD: Verify state after interaction
await mcp__safari__click_element({ selector: 'button' });
await mcp__safari__take_screenshot({ filename: 'after-click' });
const pageInfo = await mcp__safari__get_page_info();
console.log('Current state:', pageInfo);
```

### 2. Element Discovery Pattern
Find elements systematically before interacting:

```javascript
// Step 1: Discover what's available
const discoveryScript = `
const buttons = Array.from(document.querySelectorAll('button')).map(btn => ({
    text: btn.textContent.trim(),
    id: btn.id,
    className: btn.className,
    disabled: btn.disabled,
    visible: btn.offsetParent !== null
}));
return buttons;
`;

const buttons = await mcp__safari__execute_script({ script: discoveryScript });
console.log('Available buttons:', buttons);

// Step 2: Target specific element
const targetButton = buttons.find(btn => btn.text.includes('Next'));
if (targetButton) {
    await mcp__safari__click_element({ selector: `button:contains("${targetButton.text}")` });
}
```

### 3. Form Interaction Pattern
Handle form inputs systematically:

```javascript
// Discover form fields
const formDiscovery = `
const inputs = Array.from(document.querySelectorAll('input, textarea, select')).map(input => ({
    type: input.type,
    name: input.name,
    id: input.id,
    placeholder: input.placeholder,
    required: input.required,
    value: input.value
}));
return inputs;
`;

const fields = await mcp__safari__execute_script({ script: formDiscovery });

// Fill specific field
const ageField = fields.find(f => f.type === 'number' || f.placeholder?.includes('age'));
if (ageField) {
    const selector = ageField.id ? `#${ageField.id}` : `input[type="${ageField.type}"]`;
    await mcp__safari__type_text({ selector, text: '25' });
}
```

### 4. Console Monitoring Pattern
Monitor for errors and debug information:

```javascript
// Start error monitoring
await mcp__safari__start_error_monitoring({ autoSendToClaude: true });

// Perform actions
await mcp__safari__click_element({ selector: 'button' });

// Check console logs
const logs = await mcp__safari__get_console_logs();
const errors = logs.filter(log => log.type === 'error');
if (errors.length > 0) {
    console.log('Errors detected:', errors);
}

// Stop monitoring
await mcp__safari__stop_error_monitoring();
```

### 5. Wait for Dynamic Content
Handle async loading and dynamic content:

```javascript
// Wait for specific element to appear
await mcp__safari__wait_for_element({ 
    selector: '.ai-response', 
    timeout: 10000,
    visible: true 
});

// Or wait with custom script
const waitScript = `
return new Promise((resolve) => {
    const checkInterval = setInterval(() => {
        const element = document.querySelector('.loading-complete');
        if (element) {
            clearInterval(checkInterval);
            resolve(true);
        }
    }, 100);
    
    setTimeout(() => {
        clearInterval(checkInterval);
        resolve(false);
    }, 10000);
});
`;

const loaded = await mcp__safari__execute_script({ script: waitScript });
```

## Common Pitfalls & Solutions

### 1. Assuming State Changes
**Problem**: Clicking buttons without verifying the action worked.

**Solution**: Always verify state after interactions:
```javascript
// Before
await mcp__safari__click_element({ selector: 'button' });

// After  
const beforeState = await mcp__safari__get_page_info();
await mcp__safari__click_element({ selector: 'button' });
const afterState = await mcp__safari__get_page_info();

if (beforeState.url === afterState.url) {
    // Check if page content changed instead
    await mcp__safari__take_screenshot({ filename: 'state-check' });
}
```

### 2. JavaScript Syntax Errors with Safari MCP
**Problem**: AppleScript syntax errors when executing JavaScript with special characters.

**Common Causes**:
- Template literals with backticks
- Nested quotes (single quotes inside double quotes)
- Special characters in strings
- Complex string concatenation

**Solution**: Use simple JavaScript patterns:
```javascript
// ❌ BAD - Template literals cause AppleScript errors
const result = `Title: ${title}, Has keys: ${hasKeys}`;

// ✅ GOOD - Simple concatenation
const result = 'Title: ' + title + ', Has keys: ' + hasKeys;

// ❌ BAD - Nested quotes and special characters
const text = document.querySelector('.class[data-attr="value"]')?.textContent || 'default';

// ✅ GOOD - Avoid complex selectors and optional chaining
const element = document.querySelector('.class');
const text = element ? element.textContent : 'default';

// ❌ BAD - Array methods with arrow functions
const items = Array.from(elements).map(el => el.textContent);

// ✅ GOOD - Traditional loops
const items = [];
const elements = document.querySelectorAll('.item');
for (let i = 0; i < elements.length; i++) {
    items.push(elements[i].textContent);
}
```

**Safari MCP JavaScript Rules**:
1. Avoid template literals (backticks)
2. Use simple string concatenation
3. Avoid optional chaining (?.)
4. Use traditional loops instead of array methods
5. Keep selectors simple
6. Test incrementally with small scripts

### 3. Selector Issues
**Problem**: Using selectors that don't exist or are too generic.

**Solution**: Discover actual selectors first:
```javascript
// Find exact selectors
const selectorDiscovery = `
const element = document.querySelector('button');
return {
    tagName: element.tagName,
    id: element.id,
    className: element.className,
    text: element.textContent.trim(),
    outerHTML: element.outerHTML
};
`;
```

### 3. Localization Key Display
**Problem**: Page showing `[PersonaCreation_Title]` instead of actual text.

**Solution**: Check localization service status:
```javascript
const localizationCheck = `
// Check if localization service is working
const hasLocalizationService = typeof window.localizationService !== 'undefined';
const sampleText = document.querySelector('h1, h2, h3')?.textContent;
return {
    hasService: hasLocalizationService,
    sampleText: sampleText,
    showingKeys: sampleText?.includes('[') && sampleText?.includes(']')
};
`;
```

### 4. JavaScript Errors
**Problem**: Silent JavaScript failures.

**Solution**: Always monitor console:
```javascript
// Check for errors before proceeding
const logs = await mcp__safari__get_console_logs();
const jsErrors = logs.filter(log => log.type === 'error');
if (jsErrors.length > 0) {
    console.log('JavaScript errors detected:', jsErrors);
    // Handle errors before continuing
}
```

## Testing Blazor/Razor Applications

### Special Considerations
1. **Blazor Components**: Use SignalR connection status checks
2. **Razor Syntax**: Watch for compilation errors in console
3. **MudBlazor**: Components may have specific class patterns
4. **WebLLM Integration**: Monitor model loading status

### Blazor-Specific Patterns
```javascript
// Check Blazor connection status
const blazorStatus = `
return {
    blazorStarted: typeof window.Blazor !== 'undefined',
    signalRConnected: window.blazorSignalR?.state === 'Connected',
    webLLMReady: window.spinnerNetWebLLM?.getPerformanceMetrics?.()
};
`;

// Wait for Blazor to fully load
const waitForBlazor = `
return new Promise((resolve) => {
    if (typeof window.Blazor !== 'undefined') {
        resolve(true);
    } else {
        const checkInterval = setInterval(() => {
            if (typeof window.Blazor !== 'undefined') {
                clearInterval(checkInterval);
                resolve(true);
            }
        }, 100);
        setTimeout(() => {
            clearInterval(checkInterval);
            resolve(false);
        }, 5000);
    }
});
`;
```

## Debugging Workflow

1. **Take Screenshot**: Visual state verification
2. **Get Page Info**: URL, title verification
3. **Check Console**: Error detection
4. **Discover Elements**: Available interactions
5. **Execute Action**: Perform interaction
6. **Verify Result**: State confirmation
7. **Document Findings**: Update test documentation

## Example Test Flow

```javascript
// 1. Navigate and verify
await mcp__safari__navigate({ url: 'https://app.example.com' });
await mcp__safari__take_screenshot({ filename: 'initial-load' });

// 2. Discover available actions
const pageAnalysis = await mcp__safari__execute_script({
    script: `
        return {
            title: document.title,
            buttons: Array.from(document.querySelectorAll('button')).map(b => b.textContent.trim()),
            inputs: Array.from(document.querySelectorAll('input')).map(i => i.type),
            errors: Array.from(document.querySelectorAll('.error, .alert-danger')).map(e => e.textContent)
        };
    `
});

// 3. Perform actions with verification
await mcp__safari__type_text({ selector: 'input[type="number"]', text: '25' });
await mcp__safari__click_element({ selector: 'button:contains("Next")' });

// 4. Verify state change
await mcp__safari__take_screenshot({ filename: 'after-interaction' });
const logs = await mcp__safari__get_console_logs();

// 5. Document results
console.log('Test completed:', {
    pageAnalysis,
    consoleLogs: logs,
    finalState: await mcp__safari__get_page_info()
});
```

## Best Practices

1. **Always verify**: Take screenshots after significant actions
2. **Discover first**: Use scripts to understand page structure before acting
3. **Monitor console**: Check for JavaScript errors continuously
4. **Wait appropriately**: Use proper waiting mechanisms for dynamic content
5. **Document learnings**: Update examples with new patterns discovered
6. **Handle failures gracefully**: Include fallback strategies for common issues

## Safari MCP JavaScript Quick Reference

### ✅ Safe Patterns
```javascript
// String operations
const text = 'Hello ' + name;
const hasKey = text.indexOf('[') > -1;

// Element selection
const element = document.querySelector('.class');
if (element) {
    const content = element.textContent;
}

// Loops
for (let i = 0; i < array.length; i++) {
    // process array[i]
}

// Object creation
const data = {
    title: title,
    hasKeys: hasKeys
};

// Simple returns
return 'Result: ' + value;
```

### ❌ Patterns to Avoid
```javascript
// Template literals
`Hello ${name}`

// Optional chaining
element?.textContent

// Nullish coalescing
value ?? 'default'

// Arrow functions in Safari MCP context
array.map(x => x.value)

// Complex one-liners
return array.filter(x => x.active).map(x => x.name).join(', ');

// Regex with special characters (escape carefully)
text.match(/\[PersonaCreation_[^\]]+\]/)
```

## Common Web Application Patterns

### Numeric Inputs
```javascript
const numericInput = document.querySelector('input[type="number"]');
```

### Framework-Specific Buttons (Material UI, MudBlazor, etc.)
```javascript
// Material UI
const materialButton = document.querySelector('.MuiButton-root');

// MudBlazor  
const mudButton = document.querySelector('.mud-button-filled');

// Bootstrap
const bootstrapButton = document.querySelector('.btn.btn-primary');
```

### Multi-Step Forms/Wizards
```javascript
// Generic stepper/wizard detection
const activeStep = document.querySelector('.active, [aria-current="step"], .current-step');

// Framework-specific
const mudStep = document.querySelector('.mud-stepper-step-active');
const materialStep = document.querySelector('.MuiStep-active');
```

### Custom JavaScript Objects
```javascript
// Check for custom application objects
const customAPI = window.myApp?.api?.someFunction?.();
const reactState = window.React && window.React.version;
const vueState = window.Vue && window.Vue.version;
```

This guide should be referenced and updated with each testing session to build institutional knowledge.

## Critical Safari MCP Testing Discoveries

### Navigation State Verification Issues

**Problem**: Button clicks may not actually change application state even when they appear successful.

**Example**: Clicking "Next" button reports success but stepper remains on same step.

**Solution**: Always verify state after navigation:
```javascript
// ❌ BAD - Assume click worked
nextButton.click();
// Continue assuming we're on next step...

// ✅ GOOD - Verify state change
nextButton.click();

// Wait and verify
setTimeout(function() {
    const newStep = getCurrentStepNumber();
    if (newStep === expectedStep) {
        console.log('Navigation successful');
    } else {
        console.log('Navigation failed - still on step', newStep);
    }
}, 1000);
```

### Stepper State Detection

**Problem**: MudBlazor steppers use complex CSS classes that are hard to detect.

**Working Pattern**:
```javascript
// Find active step by looking for step numbers with active parent
function getCurrentStepNumber() {
    const numberElements = document.querySelectorAll('*');
    for (let i = 0; i < numberElements.length; i++) {
        const text = numberElements[i].textContent;
        if (text && text.trim().match(/^[1-4]$/)) {
            const parent = numberElements[i].parentElement;
            if (parent && parent.className.indexOf('active') > -1) {
                return text.trim();
            }
        }
    }
    return 'unknown';
}
```

### Button State Validation

**Problem**: Buttons may appear clickable but be functionally disabled.

**Solution**: Check both disabled attribute and visual state:
```javascript
function isButtonReallyClickable(button) {
    if (!button) return false;
    if (button.disabled) return false;
    
    // Check if button has content
    const hasText = button.textContent.trim().length > 0;
    
    // Check if button is visible
    const isVisible = button.offsetParent !== null;
    
    // Check classes for disabled state
    const hasDisabledClass = button.className.indexOf('disabled') > -1;
    
    return hasText && isVisible && !hasDisabledClass;
}
```

### Page Content Validation

**Problem**: Elements may exist but not contain expected content.

**Working Patterns**:
```javascript
// Validate page loaded correctly
function validatePageContent() {
    const pageTitle = document.title;
    const hasLocalizationKeys = document.body.innerText.indexOf('[') > -1;
    const hasExpectedElements = document.querySelectorAll('button').length > 0;
    
    return {
        titleCorrect: pageTitle.indexOf('Create Your AI Persona') > -1,
        localized: !hasLocalizationKeys,
        hasButtons: hasExpectedElements
    };
}

// Check for specific step content
function validateStepContent(expectedStep) {
    switch (expectedStep) {
        case '1': // Basic Info
            return !!document.querySelector('input[type="number"]');
        case '2': // Interview
            return !!document.querySelector('textarea');
        case '3': // AI Generation
            return Array.from(document.querySelectorAll('button')).some(btn => 
                btn.textContent.toLowerCase().indexOf('generate') > -1);
        case '4': // Review
            return document.body.innerText.toLowerCase().indexOf('review') > -1;
        default:
            return false;
    }
}
```

### Form Input Validation

**Problem**: Input events may not trigger component updates properly.

**Comprehensive Input Pattern**:
```javascript
function setInputValue(selector, value) {
    const input = document.querySelector(selector);
    if (!input) return false;
    
    // Focus first
    input.focus();
    
    // Clear existing value
    input.value = '';
    
    // Set new value
    input.value = value;
    
    // Trigger all possible events
    const events = ['input', 'change', 'blur', 'keyup'];
    events.forEach(eventType => {
        const event = new Event(eventType, { bubbles: true });
        input.dispatchEvent(event);
    });
    
    // Verify value was set
    return input.value === value;
}
```

### Error Detection Patterns

**Problem**: Blazor errors may not appear in console immediately.

**Comprehensive Error Checking**:
```javascript
function checkForErrors() {
    // Check console logs
    const logs = console.getEntries ? console.getEntries() : [];
    
    // Check for error indicators in UI
    const errorElements = document.querySelectorAll('.error, .mud-alert-error, [class*="error"]');
    
    // Check for missing content (empty divs, loading states)
    const emptyContent = document.querySelectorAll('div:empty').length;
    
    // Check for broken images or resources
    const brokenImages = Array.from(document.querySelectorAll('img')).filter(img => !img.complete);
    
    return {
        consoleErrors: logs.filter(log => log.level === 'error'),
        uiErrors: errorElements.length,
        emptyElements: emptyContent,
        brokenResources: brokenImages.length
    };
}
```

### WebLLM Integration Testing

**Problem**: WebLLM may report ready but not actually function.

**Comprehensive WebLLM Test**:
```javascript
function testWebLLMIntegration() {
    if (!window.spinnerNetWebLLM) {
        return { status: 'not_available', error: 'spinnerNetWebLLM not found' };
    }
    
    try {
        const metrics = window.spinnerNetWebLLM.getPerformanceMetrics();
        
        // Test actual functionality
        const canGenerate = typeof window.spinnerNetWebLLM.generatePersonaResponse === 'function';
        
        return {
            status: metrics.isInitialized ? 'ready' : 'initializing',
            metrics: metrics,
            functional: canGenerate,
            currentStatus: metrics.currentStatus,
            lastError: metrics.lastError
        };
    } catch (error) {
        return { status: 'error', error: error.message };
    }
}
```

### Phase 1 Requirements Validation

**Automated Phase 1 Test Pattern**:
```javascript
function validatePhase1Requirements() {
    const results = {
        localization: validatePageContent().localized,
        webllm_ready: testWebLLMIntegration().status === 'ready',
        age_adaptive_ui: !!document.querySelector('input[type="number"]'),
        stepper_navigation: getCurrentStepNumber() !== 'unknown',
        interview_functionality: !!document.querySelector('textarea'),
        error_free: checkForErrors().consoleErrors.length === 0
    };
    
    const passedCount = Object.values(results).filter(Boolean).length;
    const totalCount = Object.keys(results).length;
    
    return {
        results: results,
        score: passedCount + '/' + totalCount,
        passed: passedCount === totalCount
    };
}
```

### Debugging Workflow for Stuck Navigation

When navigation appears to work but doesn't:

1. **Verify current state**: Check step numbers, active classes
2. **Check button state**: Ensure button is actually clickable
3. **Validate form data**: Ensure all required fields are filled
4. **Check for validation errors**: Look for hidden error messages
5. **Test manual click**: Try clicking with mouse to compare behavior
6. **Check network requests**: Look for failed API calls
7. **Verify component state**: Check if Blazor component is in expected state

### Performance Testing Patterns

**WebLLM Model Loading**:
```javascript
function testWebLLMPerformance() {
    const startTime = Date.now();
    
    return new Promise((resolve) => {
        const checkInterval = setInterval(() => {
            const status = testWebLLMIntegration();
            if (status.status === 'ready') {
                clearInterval(checkInterval);
                const loadTime = Date.now() - startTime;
                resolve({ loadTime: loadTime, status: 'success' });
            }
        }, 1000);
        
        // Timeout after 60 seconds
        setTimeout(() => {
            clearInterval(checkInterval);
            resolve({ loadTime: 60000, status: 'timeout' });
        }, 60000);
    });
}
```

### Browser Compatibility Detection

```javascript
function checkBrowserCompatibility() {
    return {
        webgpu: !!navigator.gpu,
        webgl: !!document.createElement('canvas').getContext('webgl'),
        es6_modules: typeof Symbol !== 'undefined',
        fetch_api: typeof fetch !== 'undefined',
        local_storage: typeof localStorage !== 'undefined',
        session_storage: typeof sessionStorage !== 'undefined',
        user_agent: navigator.userAgent
    };
}
```

These patterns have been discovered through extensive Safari MCP testing and should be used to ensure reliable automated testing of the SpinnerNet persona creation workflow.

## Safari MCP Testing Best Practices

### 0. USE TEST IDs - THE GOLDEN RULE  
**The #1 most important practice: Add test IDs to ALL interactive elements**

**For MudBlazor Components - Complete Form Coverage:**
```razor
<!-- ✅ BUTTONS -->
<MudButton 
    Variant="Variant.Filled" 
    Color="Color.Primary"
    OnClick="NextStep"
    @attributes="@(new Dictionary<string, object> { { "id", "next-button" } })">
    Next
</MudButton>

<!-- ✅ TEXT INPUTS -->
<MudTextField @bind-Value="_displayName"
             Label="Display Name"
             Required="true"
             @attributes="@(new Dictionary<string, object> { { "id", "display-name-input" } })" />

<!-- ✅ NUMERIC INPUTS -->
<MudNumericField @bind-Value="_age"
                Label="Age"
                Required="true"
                @attributes="@(new Dictionary<string, object> { { "id", "age-input" } })" />

<!-- ✅ DROPDOWNS/SELECTS -->
<MudSelect @bind-Value="_primaryUse"
          Label="Primary Use"
          Required="true"
          @attributes="@(new Dictionary<string, object> { { "id", "primary-use-select" } })">
    <MudSelectItem Value="companion">Companion</MudSelectItem>
    <MudSelectItem Value="assistant">Assistant</MudSelectItem>
</MudSelect>

<!-- ✅ TEXTAREAS -->
<MudTextField @bind-Value="_description"
             Label="Description"
             Lines="3"
             @attributes="@(new Dictionary<string, object> { { "id", "description-input" } })" />

<!-- ✅ MULTIPLE ATTRIBUTES -->
<MudButton 
    @attributes="@(new Dictionary<string, object> { 
        { "id", "next-button" }, 
        { "data-test", "persona-next" },
        { "data-step", "1" }
    })">
    Next
</MudButton>

<!-- ❌ BAD - These don't work with MudBlazor -->
<MudButton id="next-button" data-testid="persona-next-btn">Next</MudButton>
<MudTextField id="name-input" data-testid="name-field" />
```

**For Standard HTML Elements:**
```html
<!-- ✅ EXCELLENT - Standard HTML elements -->
<button id="next-button" data-testid="persona-next-btn">Next</button>
<input id="age-input" data-testid="user-age" type="number" />
<textarea id="interview-answer" data-testid="interview-response"></textarea>
```

**JavaScript Testing - Comprehensive Form Automation:**
```javascript
// ✅ EXCELLENT - Reliable ID-based selection for all form elements
function testFormCompletely() {
    // Text inputs
    const nameInput = document.getElementById('display-name-input');
    const descriptionInput = document.getElementById('description-input');
    
    // Numeric inputs  
    const ageInput = document.getElementById('age-input');
    
    // Dropdowns/Selects
    const primaryUseSelect = document.getElementById('primary-use-select');
    const commStyleSelect = document.getElementById('communication-style-select');
    
    // Buttons
    const nextButton = document.getElementById('next-button');
    const previousButton = document.getElementById('previous-button');
    const saveDraftButton = document.getElementById('save-draft-button');
    
    return {
        formElements: {
            nameInput: !!nameInput,
            ageInput: !!ageInput,
            descriptionInput: !!descriptionInput,
            primaryUseSelect: !!primaryUseSelect,
            commStyleSelect: !!commStyleSelect
        },
        navigationElements: {
            nextButton: !!nextButton,
            previousButton: !!previousButton,
            saveDraftButton: !!saveDraftButton
        },
        allElementsFound: [nameInput, ageInput, primaryUseSelect, commStyleSelect, nextButton].every(el => !!el)
    };
}

// ✅ GOOD - Form filling with focus management (critical for Blazor validation)
function fillFormWithValidation() {
    const fields = [
        { id: 'display-name-input', value: 'Test User', type: 'text' },
        { id: 'age-input', value: '28', type: 'number' },
        { id: 'description-input', value: 'Test description', type: 'text' }
    ];
    
    fields.forEach((field, index) => {
        const element = document.getElementById(field.id);
        if (element) {
            element.focus(); // CRITICAL for Blazor validation
            element.value = field.value;
            element.dispatchEvent(new Event('input', { bubbles: true }));
            element.dispatchEvent(new Event('change', { bubbles: true }));
            
            // Move focus to trigger validation
            if (index < fields.length - 1) {
                setTimeout(() => fields[index + 1] && document.getElementById(fields[index + 1].id)?.focus(), 100);
            } else {
                element.blur();
                document.body.focus(); // Complete validation cycle
            }
        }
    });
}

// ✅ GOOD - Dropdown interaction for MudSelect
function selectDropdownOption(selectId, optionValue) {
    const select = document.getElementById(selectId);
    if (select) {
        // Click to open dropdown
        select.click();
        
        // Wait for dropdown to appear, then select option
        setTimeout(() => {
            const dropdown = document.querySelector('.mud-popover-content, .mud-list, [role="listbox"]');
            if (dropdown) {
                const options = Array.from(dropdown.querySelectorAll('*')).filter(el => 
                    el.textContent && el.textContent.trim().length > 0
                );
                
                const targetOption = options.find(opt => 
                    opt.textContent.toLowerCase().includes(optionValue.toLowerCase())
                ) || options[0];
                
                if (targetOption) {
                    targetOption.click();
                    document.body.focus(); // Complete validation
                    return true;
                }
            }
            return false;
        }, 300);
    }
    return false;
}

// ❌ BAD - Complex selectors that break easily
const nextButton = Array.from(document.querySelectorAll('button')).find(btn => 
    btn.textContent.toLowerCase().includes('next') && !btn.disabled
);
```

**Benefits of Test IDs:**
- **Reliable**: Never changes even if styling/classes change
- **Fast**: Direct selection with `getElementById()`
- **Maintainable**: Clear intent for testing
- **Framework agnostic**: Works with any UI framework
- **Safari MCP friendly**: No syntax complexity

### 1. Always Verify State Changes
**Never assume an action worked - always verify:**
```javascript
// ❌ BAD
button.click();
// Continue with next step...

// ✅ GOOD  
const beforeState = getCurrentState();
button.click();
setTimeout(() => {
    const afterState = getCurrentState();
    if (afterState !== beforeState) {
        console.log('Action successful');
        proceedToNextStep();
    } else {
        console.log('Action failed - investigating...');
        debugFailedAction();
    }
}, 1000);
```

### 2. Progressive Element Discovery
**Build complex operations from simple, verified steps:**
```javascript
// ✅ Step-by-step verification
function findAndClickNext() {
    // Step 1: Find all buttons
    const buttons = document.querySelectorAll('button');
    console.log('Found', buttons.length, 'buttons');
    
    // Step 2: Filter for Next buttons
    const nextButtons = Array.from(buttons).filter(btn => 
        btn.textContent.toLowerCase().includes('next')
    );
    console.log('Found', nextButtons.length, 'Next buttons');
    
    // Step 3: Check clickability
    const clickableNext = nextButtons.filter(btn => !btn.disabled);
    console.log('Found', clickableNext.length, 'clickable Next buttons');
    
    // Step 4: Attempt click with verification
    if (clickableNext.length > 0) {
        const beforeStep = getCurrentStepNumber();
        clickableNext[0].click();
        
        setTimeout(() => {
            const afterStep = getCurrentStepNumber();
            console.log('Step change:', beforeStep, '->', afterStep);
            return afterStep !== beforeStep;
        }, 1000);
    }
    
    return false;
}
```

### 3. Comprehensive Form Validation
**Ensure forms are actually valid before proceeding:**
```javascript
function validateFormBeforeSubmit() {
    // Check required fields
    const requiredInputs = document.querySelectorAll('input[required], textarea[required]');
    const missingFields = [];
    
    for (let i = 0; i < requiredInputs.length; i++) {
        const input = requiredInputs[i];
        if (!input.value || input.value.trim() === '') {
            missingFields.push(input.name || input.id || 'unnamed field');
        }
    }
    
    // Check for validation errors
    const errorElements = document.querySelectorAll('.field-validation-error, .mud-input-error');
    const hasErrors = errorElements.length > 0;
    
    // Check for disabled submit buttons
    const submitButtons = Array.from(document.querySelectorAll('button')).filter(btn =>
        btn.type === 'submit' || btn.textContent.toLowerCase().includes('next')
    );
    const hasEnabledSubmit = submitButtons.some(btn => !btn.disabled);
    
    return {
        valid: missingFields.length === 0 && !hasErrors && hasEnabledSubmit,
        missingFields: missingFields,
        errors: Array.from(errorElements).map(el => el.textContent),
        submitButtonsEnabled: hasEnabledSubmit
    };
}
```

### 4. Error Recovery Patterns
**Plan for failures and provide recovery paths:**
```javascript
function robustNavigation(targetStep) {
    const maxAttempts = 3;
    let attempts = 0;
    
    function attemptNavigation() {
        attempts++;
        const currentStep = getCurrentStepNumber();
        
        if (currentStep === targetStep) {
            console.log('Already at target step', targetStep);
            return true;
        }
        
        if (attempts > maxAttempts) {
            console.log('Max attempts reached, navigation failed');
            return false;
        }
        
        // Try different navigation strategies
        const strategies = [
            () => clickNextButton(),
            () => clickStepDirectly(targetStep),
            () => fillMissingFieldsAndTryAgain(),
            () => refreshPageAndRetry()
        ];
        
        const strategy = strategies[attempts - 1];
        if (strategy) {
            console.log('Attempting strategy', attempts);
            strategy();
            
            setTimeout(() => {
                const newStep = getCurrentStepNumber();
                if (newStep === targetStep) {
                    console.log('Navigation successful on attempt', attempts);
                    return true;
                } else {
                    console.log('Attempt', attempts, 'failed, trying next strategy');
                    return attemptNavigation();
                }
            }, 2000);
        }
        
        return false;
    }
    
    return attemptNavigation();
}
```

### 5. Timing and Synchronization
**Handle async operations properly:**
```javascript
// ✅ GOOD - Wait for operations to complete
async function waitForCondition(conditionFn, timeout = 10000) {
    const startTime = Date.now();
    
    return new Promise((resolve) => {
        const checkInterval = setInterval(() => {
            if (conditionFn()) {
                clearInterval(checkInterval);
                resolve(true);
            } else if (Date.now() - startTime > timeout) {
                clearInterval(checkInterval);
                resolve(false);
            }
        }, 100);
    });
}

// Usage
const navigationComplete = await waitForCondition(
    () => getCurrentStepNumber() === '3',
    5000
);
```

### 6. Comprehensive Debugging
**When things go wrong, gather all available information:**
```javascript
function debugApplicationState() {
    const state = {
        url: window.location.href,
        title: document.title,
        currentStep: getCurrentStepNumber(),
        buttons: Array.from(document.querySelectorAll('button')).map(btn => ({
            text: btn.textContent.trim(),
            disabled: btn.disabled,
            visible: btn.offsetParent !== null,
            classes: btn.className
        })),
        inputs: Array.from(document.querySelectorAll('input, textarea')).map(input => ({
            type: input.type,
            value: input.value,
            required: input.required,
            valid: input.checkValidity ? input.checkValidity() : 'unknown'
        })),
        errors: Array.from(document.querySelectorAll('.error, [class*="error"]')).map(el => el.textContent),
        webllm: window.spinnerNetWebLLM ? window.spinnerNetWebLLM.getPerformanceMetrics() : 'not available'
    };
    
    console.log('APPLICATION STATE DEBUG:', JSON.stringify(state, null, 2));
    return state;
}
```

### 7. Test Data Management
**Use realistic, context-appropriate test data:**
```javascript
const testData = {
    // User registration forms
    registration: {
        email: 'test.user@example.com',
        password: 'SecureP@ssw0rd123',
        firstName: 'Test',
        lastName: 'User',
        phone: '+1-555-123-4567'
    },
    
    // E-commerce testing
    ecommerce: {
        creditCard: '4111111111111111', // Test card number
        expiry: '12/25',
        cvv: '123',
        address: '123 Test Street, Test City, TC 12345'
    },
    
    // Content testing
    content: {
        shortText: 'Quick test content',
        mediumText: 'This is a medium length test content for form fields that need more text.',
        longText: 'This is a very long test content that can be used for testing text areas, blog posts, comments, and other fields that accept extended text input. It should be long enough to test scroll behavior and text limits.',
    },
    
    // Dates and numbers
    dates: {
        past: '2020-01-15',
        present: new Date().toISOString().split('T')[0],
        future: '2025-12-31'
    },
    
    numbers: {
        small: 5,
        medium: 150,
        large: 999999,
        decimal: 123.45
    }
};
```

### 8. Environment Verification
**Always check browser capabilities before testing:**
```javascript
function verifyTestEnvironment() {
    const requirements = {
        safari: navigator.userAgent.indexOf('Safari') > -1,
        webgpu: !!navigator.gpu,
        localStorage: typeof localStorage !== 'undefined',
        fetch: typeof fetch !== 'undefined',
        es6: typeof Symbol !== 'undefined',
        viewport: {
            width: window.innerWidth,
            height: window.innerHeight
        }
    };
    
    const issues = [];
    if (!requirements.safari) issues.push('Not running in Safari');
    if (!requirements.webgpu) issues.push('WebGPU not available - WebLLM may be slow');
    if (!requirements.localStorage) issues.push('localStorage not available');
    
    console.log('ENVIRONMENT CHECK:', requirements);
    if (issues.length > 0) {
        console.warn('ENVIRONMENT ISSUES:', issues);
    }
    
    return { requirements, issues };
}
```

### 9. Documentation During Testing
**Log everything for debugging and improvement:**
```javascript
function logTestAction(action, beforeState, afterState, success) {
    const logEntry = {
        timestamp: new Date().toISOString(),
        action: action,
        beforeState: beforeState,
        afterState: afterState,
        success: success,
        userAgent: navigator.userAgent.substring(0, 100)
    };
    
    console.log('TEST ACTION:', JSON.stringify(logEntry, null, 2));
    
    // Store for later analysis
    if (!window.testLog) window.testLog = [];
    window.testLog.push(logEntry);
}

// Usage
const before = getCurrentStepNumber();
const success = clickNextButton();
const after = getCurrentStepNumber();
logTestAction('click_next_button', before, after, success);
```

### 10. Project Requirements Validation
**Systematic validation against any project requirements:**
```javascript
function validateProjectRequirements(requirements) {
    // Example requirements structure:
    // const requirements = [
    //     {
    //         name: 'Page Loads',
    //         test: () => document.readyState === 'complete',
    //         critical: true
    //     },
    //     {
    //         name: 'No Console Errors',
    //         test: () => checkForErrors().consoleErrors.length === 0,
    //         critical: true
    //     },
    //     {
    //         name: 'Forms Work',
    //         test: () => testFormFunctionality(),
    //         critical: true
    //     },
    //     {
    //         name: 'Custom API Available',
    //         test: () => typeof window.myAPI !== 'undefined',
    //         critical: false
    //     }
    // ];
    
    const results = requirements.map(req => {
        let passed = false;
        let error = null;
        
        try {
            passed = req.test();
        } catch (e) {
            error = e.message;
        }
        
        return {
            name: req.name,
            passed: passed,
            critical: req.critical,
            error: error
        };
    });
    
    const criticalPassed = results.filter(r => r.critical && r.passed).length;
    const criticalTotal = results.filter(r => r.critical).length;
    const allPassed = results.filter(r => r.passed).length;
    const hasErrors = results.some(r => r.error);
    
    return {
        results: results,
        criticalScore: criticalPassed + '/' + criticalTotal,
        totalScore: allPassed + '/' + results.length,
        allCriticalPassed: criticalPassed === criticalTotal,
        hasErrors: hasErrors,
        ready: criticalPassed === criticalTotal && !hasErrors
    };
}

// Universal web app tests
function createBasicWebAppTests() {
    return [
        {
            name: 'Page Loaded',
            test: () => document.readyState === 'complete',
            critical: true
        },
        {
            name: 'No JavaScript Errors',
            test: () => checkForErrors().consoleErrors.length === 0,
            critical: true
        },
        {
            name: 'Has Interactive Elements',
            test: () => document.querySelectorAll('button, input, select, textarea').length > 0,
            critical: true
        },
        {
            name: 'Navigation Works',
            test: () => testBasicNavigation(),
            critical: true
        },
        {
            name: 'Responsive Design',
            test: () => window.innerWidth > 0 && window.innerHeight > 0,
            critical: false
        }
    ];
}
```

**Golden Rule**: Never trust that an action worked - always verify the result.