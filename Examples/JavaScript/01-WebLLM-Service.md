# JavaScript Interop - WebLLM Service Pattern

## Problem Statement
Integrating browser-based AI (WebLLM) with Blazor requires robust JavaScript interop with proper initialization, error handling, and bidirectional communication.

## Working Solution

### C# Service Interface
```csharp
public interface IWebLLMService
{
    bool IsInitialized { get; }
    bool IsInitializing { get; }
    Task<bool> InitializeAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<string> GeneratePersonaAsync(Dictionary<string, string> inputs, CancellationToken cancellationToken = default);
    event Action<string>? StatusUpdated;
    event Action<WebLLMProgress>? ProgressUpdated;
}
```

### C# Service Implementation
```csharp
public class WebLLMService : IWebLLMService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<WebLLMService> _logger;
    private DotNetObjectReference<WebLLMService>? _dotNetObjectRef;
    private bool _isInitialized = false;
    private bool _isInitializing = false;

    public event Action<string>? StatusUpdated;
    public event Action<WebLLMProgress>? ProgressUpdated;

    public WebLLMService(IJSRuntime jsRuntime, ILogger<WebLLMService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task<bool> InitializeAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (_isInitialized) return true;
        if (_isInitializing) return false;

        try
        {
            _isInitializing = true;
            _dotNetObjectRef = DotNetObjectReference.Create(this);
            
            var result = await _jsRuntime.InvokeAsync<bool>(
                "spinnerNetWebLLM.initialize",
                cancellationToken,
                _dotNetObjectRef,
                sessionId
            );

            _isInitialized = result;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize WebLLM");
            return false;
        }
        finally
        {
            _isInitializing = false;
        }
    }

    [JSInvokable]
    public void OnWebLLMStatusUpdate(string status)
    {
        _logger.LogInformation("WebLLM Status: {Status}", status);
        StatusUpdated?.Invoke(status);
    }

    [JSInvokable]
    public void OnWebLLMProgress(WebLLMProgress progress)
    {
        _logger.LogDebug("WebLLM Progress: {Progress}%", progress.Progress * 100);
        ProgressUpdated?.Invoke(progress);
    }

    public async ValueTask DisposeAsync()
    {
        if (_dotNetObjectRef != null)
        {
            await _jsRuntime.InvokeVoidAsync("spinnerNetWebLLM.dispose");
            _dotNetObjectRef.Dispose();
        }
    }
}
```

### JavaScript Implementation
```javascript
class SpinnerNetWebLLM {
    constructor() {
        this.engine = null;
        this.isInitialized = false;
        this.isLoading = false;
        this.modelId = "Llama-3.2-1B-Instruct-q4f32_1-MLC";
        this.dotNetHelper = null;
        this.initializationPromise = null;
    }

    async initialize(dotNetHelper) {
        // Prevent multiple initializations
        if (this.initializationPromise) {
            return this.initializationPromise;
        }

        this.initializationPromise = this._performInitialization(dotNetHelper);
        return this.initializationPromise;
    }

    async _performInitialization(dotNetHelper) {
        try {
            this.dotNetHelper = dotNetHelper;
            this.isLoading = true;
            
            // Notify Blazor of status
            await this._notifyStatus("Loading WebLLM library...");
            
            // Import WebLLM
            const { CreateMLCEngine } = await import("https://esm.run/@mlc-ai/web-llm");
            
            // Create engine with progress callback
            this.engine = await CreateMLCEngine(
                this.modelId,
                { 
                    initProgressCallback: (progress) => {
                        this._notifyProgress(Math.round(progress.progress * 100));
                    }
                }
            );
            
            this.isInitialized = true;
            await this._notifyStatus("WebLLM engine ready!");
            
            return true;
        } catch (error) {
            await this._notifyError(error.message);
            return false;
        } finally {
            this.isLoading = false;
        }
    }

    async _notifyStatus(message) {
        if (this.dotNetHelper) {
            await this.dotNetHelper.invokeMethodAsync('OnWebLLMStatusUpdate', message);
        }
    }

    async _notifyProgress(progress) {
        if (this.dotNetHelper) {
            await this.dotNetHelper.invokeMethodAsync('OnWebLLMProgress', {
                progress: progress / 100.0,
                text: `Loading: ${progress}%`
            });
        }
    }

    async generateResponse(prompt, options = {}) {
        if (!this.isInitialized) {
            throw new Error("WebLLM not initialized");
        }

        const completion = await this.engine.chat.completions.create({
            messages: [
                { role: "system", content: this._getSystemPrompt(options.userAge) },
                { role: "user", content: prompt }
            ],
            temperature: options.temperature || 0.7,
            max_tokens: options.maxTokens || 200
        });

        return completion.choices[0].message.content;
    }

    dispose() {
        this.engine = null;
        this.isInitialized = false;
        this.dotNetHelper = null;
    }
}

// Global instance and namespace
let webllmInstance = null;

window.spinnerNetWebLLM = {
    initialize: async function(dotNetHelper, sessionId) {
        if (!webllmInstance) {
            webllmInstance = new SpinnerNetWebLLM();
        }
        return await webllmInstance.initialize(dotNetHelper);
    },

    generatePersonaResponse: async function(prompt, options) {
        if (!webllmInstance) {
            throw new Error("WebLLM not initialized");
        }
        return await webllmInstance.generateResponse(prompt, options);
    },

    dispose: function() {
        if (webllmInstance) {
            webllmInstance.dispose();
            webllmInstance = null;
        }
    }
};
```

## Key Patterns

### 1. Singleton Initialization
```javascript
// Prevent race conditions with initialization promise
if (this.initializationPromise) {
    return this.initializationPromise;
}
this.initializationPromise = this._performInitialization(dotNetHelper);
```

### 2. DotNetObjectReference for Callbacks
```csharp
// C# side
_dotNetObjectRef = DotNetObjectReference.Create(this);
await _jsRuntime.InvokeAsync<bool>("spinnerNetWebLLM.initialize", _dotNetObjectRef);

// JS side
await this.dotNetHelper.invokeMethodAsync('OnWebLLMStatusUpdate', message);
```

### 3. Progress Tracking
```javascript
initProgressCallback: (progress) => {
    this._notifyProgress(Math.round(progress.progress * 100));
}
```

## Common Errors & Solutions

### ❌ Error: Multiple Initialization Attempts
```csharp
// WRONG: Can cause race conditions
await WebLLMService.InitializeAsync("session1");
await WebLLMService.InitializeAsync("session2"); // May fail
```

### ✅ Solution: Check State First
```csharp
if (!WebLLMService.IsInitialized && !WebLLMService.IsInitializing)
{
    await WebLLMService.InitializeAsync(sessionId);
}
```

### ❌ Error: Memory Leaks
```csharp
// WRONG: Not disposing DotNetObjectReference
public void Dispose()
{
    // Missing cleanup
}
```

### ✅ Solution: Proper Disposal
```csharp
public async ValueTask DisposeAsync()
{
    if (_dotNetObjectRef != null)
    {
        await _jsRuntime.InvokeVoidAsync("spinnerNetWebLLM.dispose");
        _dotNetObjectRef.Dispose();
    }
}
```

## Component Usage Pattern

```razor
@page "/ai-demo"
@inject IWebLLMService WebLLMService
@implements IAsyncDisposable

<h3>WebLLM Demo</h3>

@if (!WebLLMService.IsInitialized)
{
    <MudProgressLinear Value="@_progress" Max="100" Color="Color.Primary" />
    <MudText>@_status</MudText>
}
else
{
    <MudButton OnClick="GenerateText">Generate</MudButton>
}

@code {
    private int _progress;
    private string _status = "Initializing...";

    protected override async Task OnInitializedAsync()
    {
        WebLLMService.StatusUpdated += OnStatusUpdated;
        WebLLMService.ProgressUpdated += OnProgressUpdated;
        
        await WebLLMService.InitializeAsync(Guid.NewGuid().ToString());
    }

    private void OnStatusUpdated(string status)
    {
        _status = status;
        InvokeAsync(StateHasChanged);
    }

    private void OnProgressUpdated(WebLLMProgress progress)
    {
        _progress = (int)(progress.Progress * 100);
        InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        WebLLMService.StatusUpdated -= OnStatusUpdated;
        WebLLMService.ProgressUpdated -= OnProgressUpdated;
    }
}
```

## Performance Considerations

1. **Model Loading**: ~30-60 seconds for 1B model on first load
2. **Memory Usage**: ~1-2GB for model weights
3. **WebGPU Acceleration**: 10x faster than CPU mode
4. **Caching**: Model cached in browser after first load

## Browser Compatibility
- ✅ Chrome 113+ (WebGPU required)
- ✅ Edge 113+ (WebGPU required)
- ⚠️ Safari (WebGPU experimental)
- ❌ Firefox (WebGPU in development)

## Related Patterns
- [21-JSInterop-DotNetObjectRef.md] - Callback patterns
- [70-AI-WebLLM-ClientSide.md] - AI implementation details
- [31-Service-AsyncLifecycle.md] - Service lifecycle management