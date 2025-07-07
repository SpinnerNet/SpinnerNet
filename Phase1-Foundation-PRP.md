# Phase 1: Foundation - Product Requirements Document

## 🎯 **PHASE OVERVIEW**

**Phase 1** establishes the foundational infrastructure for the AI-powered persona creation system. This phase focuses on building the core WebLLM integration and age-adaptive UI system that all subsequent features will depend upon.

### **Key Objectives**
- Implement client-side AI processing with WebLLM
- Create age-adaptive UI foundation with MudBlazor theming
- Establish basic persona creation flow
- Build scalable architecture for future enhancements

### **Phase Duration**: 4-6 weeks
### **Team Size**: 2-3 developers (1 Frontend, 1 Backend, 1 AI/Integration)

---

## 📦 **PACKAGE 1: CORE WEBLLM INTEGRATION**

### **Overview**
Implement client-side AI processing using WebLLM for privacy-first persona creation. This package establishes the foundation for all AI-powered features.

### **Core Features**
- **WebLLM Engine Setup**: Browser-based AI model execution
- **Privacy-First Architecture**: All AI processing happens client-side
- **Basic Chat Interface**: Simple conversational UI for persona creation
- **AI Model Management**: Loading, caching, and optimization
- **Error Handling**: Graceful fallbacks and user feedback

### **Technical Specifications**

#### **WebLLM Integration Architecture**
```javascript
// webllm-integration.js
import { CreateMLCEngine } from "@mlc-ai/web-llm";

class SpinnerNetWebLLM {
    constructor() {
        this.engine = null;
        this.isLoading = false;
        this.modelId = "Hermes-2-Pro-Mistral-7B-q4f32_1-MLC";
        this.dotNetHelper = null;
    }

    async initialize(dotNetHelper) {
        this.dotNetHelper = dotNetHelper;
        
        try {
            this.isLoading = true;
            await this.notifyStatus("Loading AI model...");
            
            this.engine = await CreateMLCEngine(this.modelId, {
                initProgressCallback: (progress) => {
                    this.notifyProgress(progress);
                }
            });
            
            this.isLoading = false;
            await this.notifyStatus("AI model ready!");
            return true;
            
        } catch (error) {
            this.isLoading = false;
            await this.notifyError(`Failed to load AI model: ${error.message}`);
            return false;
        }
    }

    async generateResponse(prompt, options = {}) {
        if (!this.engine) {
            throw new Error("WebLLM engine not initialized");
        }

        const completion = await this.engine.chat.completions.create({
            messages: [
                { role: "system", content: options.systemPrompt || "You are a helpful AI assistant." },
                { role: "user", content: prompt }
            ],
            temperature: options.temperature || 0.7,
            max_tokens: options.maxTokens || 500,
            stream: options.stream || false
        });

        return completion.choices[0].message.content;
    }

    async notifyStatus(message) {
        if (this.dotNetHelper) {
            await this.dotNetHelper.invokeMethodAsync('OnWebLLMStatusUpdate', message);
        }
    }

    async notifyProgress(progress) {
        if (this.dotNetHelper) {
            await this.dotNetHelper.invokeMethodAsync('OnWebLLMProgress', progress);
        }
    }

    async notifyError(error) {
        if (this.dotNetHelper) {
            await this.dotNetHelper.invokeMethodAsync('OnWebLLMError', error);
        }
    }
}

window.SpinnerNetWebLLM = new SpinnerNetWebLLM();
```

#### **Blazor WebLLM Service**
```csharp
// Services/WebLLMService.cs
using Microsoft.JSInterop;
using SpinnerNet.Shared.Models;

namespace SpinnerNet.App.Services;

public class WebLLMService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _webLLMModule;
    private DotNetObjectReference<WebLLMService>? _dotNetRef;
    private bool _isInitialized = false;

    public event Action<string>? OnStatusUpdate;
    public event Action<double>? OnProgress;
    public event Action<string>? OnError;

    public WebLLMService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> InitializeAsync()
    {
        try
        {
            _webLLMModule = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./js/webllm-integration.js");
            
            _dotNetRef = DotNetObjectReference.Create(this);
            
            _isInitialized = await _webLLMModule.InvokeAsync<bool>(
                "SpinnerNetWebLLM.initialize", _dotNetRef);
            
            return _isInitialized;
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Failed to initialize WebLLM: {ex.Message}");
            return false;
        }
    }

    public async Task<string> GenerateResponseAsync(string prompt, WebLLMOptions? options = null)
    {
        if (!_isInitialized || _webLLMModule == null)
        {
            throw new InvalidOperationException("WebLLM not initialized");
        }

        options ??= new WebLLMOptions();
        
        return await _webLLMModule.InvokeAsync<string>(
            "SpinnerNetWebLLM.generateResponse", prompt, options);
    }

    [JSInvokable]
    public void OnWebLLMStatusUpdate(string message)
    {
        OnStatusUpdate?.Invoke(message);
    }

    [JSInvokable]
    public void OnWebLLMProgress(double progress)
    {
        OnProgress?.Invoke(progress);
    }

    [JSInvokable]
    public void OnWebLLMError(string error)
    {
        OnError?.Invoke(error);
    }

    public async ValueTask DisposeAsync()
    {
        if (_webLLMModule != null)
        {
            await _webLLMModule.DisposeAsync();
        }
        
        _dotNetRef?.Dispose();
    }
}

public class WebLLMOptions
{
    public string? SystemPrompt { get; set; }
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 500;
    public bool Stream { get; set; } = false;
}
```

#### **Chat Interface Component**
```razor
@* Components/WebLLMChat.razor *@
@using SpinnerNet.App.Services
@inject WebLLMService WebLLMService
@inject ILocalizationService LocalizationService
@implements IAsyncDisposable

<MudCard Class="webllm-chat-card">
    <MudCardHeader>
        <MudText Typo="Typo.h6">@LocalizationService.GetString("WebLLM_ChatTitle")</MudText>
        <MudSpacer />
        <MudChip Color="@(_isReady ? Color.Success : Color.Warning)" 
                 Icon="@(_isReady ? Icons.Material.Filled.Check : Icons.Material.Filled.Hourglass)">
            @_statusMessage
        </MudChip>
    </MudCardHeader>
    
    <MudCardContent>
        @if (_isLoading)
        {
            <MudProgressLinear Value="@_loadProgress" Color="Color.Primary" />
            <MudText Typo="Typo.body2" Class="mt-2">@_statusMessage</MudText>
        }
        else if (_isReady)
        {
            <MudStack Spacing="4">
                <div class="chat-messages" style="height: 400px; overflow-y: auto;">
                    @foreach (var message in _messages)
                    {
                        <div class="message @(message.IsUser ? "user-message" : "ai-message")">
                            <MudPaper Class="message-bubble pa-3" Elevation="1">
                                <MudText Typo="Typo.body1">@message.Content</MudText>
                                <MudText Typo="Typo.caption" Class="mt-1">@message.Timestamp.ToString("HH:mm")</MudText>
                            </MudPaper>
                        </div>
                    }
                </div>
                
                <MudStack Row="true" Spacing="2">
                    <MudTextField @bind-Value="_inputMessage" 
                                  Label="@LocalizationService.GetString("WebLLM_InputPlaceholder")"
                                  Variant="Variant.Outlined"
                                  @onkeypress="@(async (e) => { if (e.Key == "Enter") await SendMessageAsync(); })"
                                  Disabled="@_isProcessing" />
                    <MudButton Variant="Variant.Filled" 
                               Color="Color.Primary" 
                               OnClick="SendMessageAsync"
                               Disabled="@(_isProcessing || string.IsNullOrWhiteSpace(_inputMessage))">
                        @if (_isProcessing)
                        {
                            <MudProgressCircular Size="Size.Small" />
                        }
                        else
                        {
                            @LocalizationService.GetString("WebLLM_SendButton")
                        }
                    </MudButton>
                </MudStack>
            </MudStack>
        }
        else
        {
            <MudAlert Severity="Severity.Error">@_errorMessage</MudAlert>
        }
    </MudCardContent>
</MudCard>

@code {
    private bool _isReady = false;
    private bool _isLoading = false;
    private bool _isProcessing = false;
    private string _statusMessage = "Initializing...";
    private string _errorMessage = "";
    private double _loadProgress = 0;
    private string _inputMessage = "";
    private List<ChatMessage> _messages = new();

    protected override async Task OnInitializedAsync()
    {
        WebLLMService.OnStatusUpdate += OnStatusUpdate;
        WebLLMService.OnProgress += OnProgress;
        WebLLMService.OnError += OnError;
        
        await InitializeWebLLM();
    }

    private async Task InitializeWebLLM()
    {
        _isLoading = true;
        _statusMessage = LocalizationService.GetString("WebLLM_LoadingModel");
        StateHasChanged();
        
        _isReady = await WebLLMService.InitializeAsync();
        _isLoading = false;
        
        if (_isReady)
        {
            _statusMessage = LocalizationService.GetString("WebLLM_Ready");
            await AddWelcomeMessage();
        }
        else
        {
            _errorMessage = LocalizationService.GetString("WebLLM_InitializationFailed");
        }
        
        StateHasChanged();
    }

    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(_inputMessage) || _isProcessing)
            return;

        var userMessage = _inputMessage.Trim();
        _inputMessage = "";
        
        _messages.Add(new ChatMessage { Content = userMessage, IsUser = true, Timestamp = DateTime.Now });
        _isProcessing = true;
        StateHasChanged();

        try
        {
            var response = await WebLLMService.GenerateResponseAsync(userMessage, new WebLLMOptions
            {
                SystemPrompt = "You are a helpful AI assistant helping users create their ideal AI companion. Be friendly, engaging, and ask thoughtful questions about their preferences.",
                Temperature = 0.8,
                MaxTokens = 300
            });

            _messages.Add(new ChatMessage { Content = response, IsUser = false, Timestamp = DateTime.Now });
        }
        catch (Exception ex)
        {
            _messages.Add(new ChatMessage 
            { 
                Content = $"Error: {ex.Message}", 
                IsUser = false, 
                Timestamp = DateTime.Now,
                IsError = true 
            });
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }

    private async Task AddWelcomeMessage()
    {
        _messages.Add(new ChatMessage 
        { 
            Content = LocalizationService.GetString("WebLLM_WelcomeMessage"), 
            IsUser = false, 
            Timestamp = DateTime.Now 
        });
    }

    private void OnStatusUpdate(string message)
    {
        _statusMessage = message;
        InvokeAsync(StateHasChanged);
    }

    private void OnProgress(double progress)
    {
        _loadProgress = progress * 100;
        InvokeAsync(StateHasChanged);
    }

    private void OnError(string error)
    {
        _errorMessage = error;
        _isLoading = false;
        InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        WebLLMService.OnStatusUpdate -= OnStatusUpdate;
        WebLLMService.OnProgress -= OnProgress;
        WebLLMService.OnError -= OnError;
        
        await WebLLMService.DisposeAsync();
    }

    public class ChatMessage
    {
        public string Content { get; set; } = "";
        public bool IsUser { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsError { get; set; }
    }
}
```

### **Implementation Requirements**
1. **WebLLM Model Selection**: Use Hermes-2-Pro-Mistral-7B for superior humanized conversation quality
   - **Primary Model**: `Hermes-2-Pro-Mistral-7B-q4f32_1-MLC`
   - **Why Hermes-2-Pro-Mistral-7B over Llama-3.2-3B**: 
     - Specifically trained on 1M+ high-quality conversational pairs from GPT-4
     - Excels at natural, human-like dialogue with 90% function calling accuracy
     - Superior multi-turn conversation capabilities and personality consistency
     - Better contextual understanding and response quality for chat applications
     - Optimized for conversational AI with improved response naturalness
     - Scores 1,074 on Chatbot Arena Leaderboard vs 989 for comparable models
   - **Alternative Models** (fallback options in order of preference):
     - `Phi-3.5-mini-instruct-q4f32_1-MLC` - Best multilingual support (22 languages)
     - `Llama-3.1-8B-Instruct-q4f32_1-MLC` - Larger context window (128K tokens)
     - `Qwen2.5-7B-Instruct-q4f32_1-MLC` - Strong reasoning and multi-turn dialogue
2. **Browser Compatibility**: Support Chrome 113+, Firefox 115+, Safari 16.4+
3. **Performance Optimization**: Implement model caching and lazy loading
4. **Error Handling**: Graceful fallbacks for unsupported browsers
5. **Progress Feedback**: Clear loading indicators and progress bars

---

## 📦 **PACKAGE 2: AGE-ADAPTIVE UI FOUNDATION**

### **Overview**
Implement age-adaptive UI system that dynamically adjusts interface elements based on user demographics and preferences.

### **Core Features**
- **Age Detection**: Browser-based and direct input methods
- **Dynamic Theming**: MudBlazor theme adaptation
- **Adaptive Components**: Size, color, and interaction adjustments
- **Accessibility Integration**: WCAG compliance and assistive technology support
- **Cultural Adaptation**: Basic localization and regional preferences

### **Technical Specifications**

#### **Age Detection Service**
```csharp
// Services/AgeDetectionService.cs
using Microsoft.JSInterop;
using SpinnerNet.Shared.Models;

namespace SpinnerNet.App.Services;

public class AgeDetectionService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILocalStorageService _localStorage;

    public AgeDetectionService(IJSRuntime jsRuntime, ILocalStorageService localStorage)
    {
        _jsRuntime = jsRuntime;
        _localStorage = localStorage;
    }

    public async Task<AgeProfile> DetectAgeProfileAsync()
    {
        var profile = new AgeProfile();
        
        // Check for stored age preference
        var storedAge = await _localStorage.GetItemAsync<int?>("user_age");
        if (storedAge.HasValue)
        {
            profile.Age = storedAge.Value;
            profile.Source = AgeDetectionSource.Stored;
            profile.Confidence = 1.0;
            return profile;
        }
        
        // Detect from browser patterns
        var browserProfile = await DetectFromBrowserAsync();
        var interactionProfile = await DetectFromInteractionAsync();
        
        // Combine detection methods
        profile = CombineDetectionResults(browserProfile, interactionProfile);
        
        return profile;
    }

    private async Task<AgeProfile> DetectFromBrowserAsync()
    {
        var profile = new AgeProfile();
        
        try
        {
            // Get browser characteristics
            var browserInfo = await _jsRuntime.InvokeAsync<BrowserInfo>("getBrowserInfo");
            
            // Analyze accessibility features
            if (browserInfo.HasHighContrast || browserInfo.HasLargeFont)
            {
                profile.EstimatedAgeRange = AgeRange.Senior;
                profile.Confidence = 0.6;
                profile.Source = AgeDetectionSource.Browser;
            }
            
            // Analyze device patterns
            if (browserInfo.IsMobile && browserInfo.HasTouchCapability)
            {
                profile.EstimatedAgeRange = AgeRange.YoungAdult;
                profile.Confidence = 0.4;
                profile.Source = AgeDetectionSource.Device;
            }
            
            // Analyze time zone and language
            if (browserInfo.TimeZone != null)
            {
                profile.CulturalContext = DetermineCulturalContext(browserInfo.TimeZone, browserInfo.Language);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail
            profile.DetectionErrors.Add(ex.Message);
        }
        
        return profile;
    }

    private async Task<AgeProfile> DetectFromInteractionAsync()
    {
        var profile = new AgeProfile();
        
        try
        {
            // Get interaction metrics
            var metrics = await _jsRuntime.InvokeAsync<InteractionMetrics>("getInteractionMetrics");
            
            // Analyze click patterns
            if (metrics.AverageClickDuration > 300) // ms
            {
                profile.EstimatedAgeRange = AgeRange.Senior;
                profile.Confidence = 0.5;
            }
            else if (metrics.AverageClickDuration < 150)
            {
                profile.EstimatedAgeRange = AgeRange.YoungAdult;
                profile.Confidence = 0.4;
            }
            
            // Analyze typing patterns
            if (metrics.TypingSpeed < 20) // WPM
            {
                profile.EstimatedAgeRange = AgeRange.Senior;
                profile.Confidence = 0.3;
            }
            else if (metrics.TypingSpeed > 60)
            {
                profile.EstimatedAgeRange = AgeRange.YoungAdult;
                profile.Confidence = 0.4;
            }
            
            profile.Source = AgeDetectionSource.Interaction;
        }
        catch (Exception ex)
        {
            profile.DetectionErrors.Add(ex.Message);
        }
        
        return profile;
    }

    public async Task<bool> SetUserAgeAsync(int age, bool persist = true)
    {
        if (age < 6 || age > 120)
            return false;
        
        if (persist)
        {
            await _localStorage.SetItemAsync("user_age", age);
        }
        
        // Update current session
        await _jsRuntime.InvokeVoidAsync("updateAgeAdaptation", age);
        
        return true;
    }
}

public class AgeProfile
{
    public int? Age { get; set; }
    public AgeRange EstimatedAgeRange { get; set; } = AgeRange.Unknown;
    public double Confidence { get; set; } = 0.0;
    public AgeDetectionSource Source { get; set; } = AgeDetectionSource.Unknown;
    public string CulturalContext { get; set; } = "western";
    public List<string> DetectionErrors { get; set; } = new();
}

public enum AgeRange
{
    Unknown,
    Child,      // 6-12
    Teen,       // 13-17
    YoungAdult, // 18-25
    EarlyAdult, // 26-39
    MiddleAdult,// 40-54
    YoungSenior,// 55-69
    Senior,     // 70-84
    ElderSenior // 85+
}

public enum AgeDetectionSource
{
    Unknown,
    Browser,
    Device,
    Interaction,
    Direct,
    Stored
}
```

#### **Adaptive Theme Service**
```csharp
// Services/AdaptiveThemeService.cs
using MudBlazor;
using SpinnerNet.Shared.Models;

namespace SpinnerNet.App.Services;

public class AdaptiveThemeService
{
    private readonly ILocalStorageService _localStorage;
    private MudTheme _currentTheme;

    public event Action<MudTheme>? ThemeChanged;

    public AdaptiveThemeService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
        _currentTheme = GetDefaultTheme();
    }

    public async Task<MudTheme> AdaptThemeForAgeAsync(AgeProfile ageProfile)
    {
        var theme = GetBaseThemeForAge(ageProfile.EstimatedAgeRange);
        
        // Apply accessibility adaptations
        if (ageProfile.EstimatedAgeRange == AgeRange.Senior || 
            ageProfile.EstimatedAgeRange == AgeRange.ElderSenior)
        {
            theme = ApplyAccessibilityEnhancements(theme);
        }
        
        // Apply cultural adaptations
        theme = ApplyCulturalAdaptations(theme, ageProfile.CulturalContext);
        
        _currentTheme = theme;
        ThemeChanged?.Invoke(theme);
        
        // Persist theme preferences
        await _localStorage.SetItemAsync("adaptive_theme", theme);
        
        return theme;
    }

    private MudTheme GetBaseThemeForAge(AgeRange ageRange)
    {
        return ageRange switch
        {
            AgeRange.Child => CreateChildTheme(),
            AgeRange.Teen => CreateTeenTheme(),
            AgeRange.YoungAdult => CreateYoungAdultTheme(),
            AgeRange.EarlyAdult => CreateEarlyAdultTheme(),
            AgeRange.MiddleAdult => CreateMiddleAdultTheme(),
            AgeRange.YoungSenior => CreateYoungSeniorTheme(),
            AgeRange.Senior => CreateSeniorTheme(),
            AgeRange.ElderSenior => CreateElderSeniorTheme(),
            _ => GetDefaultTheme()
        };
    }

    private MudTheme CreateChildTheme()
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#FF6B6B",      // Vibrant coral
                Secondary = "#4ECDC4",    // Bright teal
                Tertiary = "#FFE66D",     // Sunny yellow
                Success = "#95E1D3",      // Soft mint
                Info = "#87CEEB",         // Sky blue
                Warning = "#FFB347",      // Peach
                Error = "#FF8A80",        // Soft red
                Background = "#FFF9E6",   // Cream
                Surface = "#FFFFFF",
                TextPrimary = "#2C3E50",  // Dark blue-gray
                TextSecondary = "#7F8C8D"
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "12px" // Rounded corners
            },
            Typography = new Typography
            {
                Default = new Default
                {
                    FontFamily = new[] { "Comic Sans MS", "Arial", "sans-serif" },
                    FontSize = "16px",
                    LineHeight = 1.6
                },
                H1 = new H1 { FontSize = "2.5rem", FontWeight = 700 },
                H2 = new H2 { FontSize = "2rem", FontWeight = 600 },
                Button = new Button { FontSize = "1.1rem", FontWeight = 600 }
            }
        };
    }

    private MudTheme CreateTeenTheme()
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#667eea",      // Gradient purple
                Secondary = "#f093fb",    // Gradient pink
                Tertiary = "#4facfe",     // Gradient blue
                Success = "#43e97b",      // Bright green
                Info = "#38f9d7",         // Cyan
                Warning = "#feca57",      // Vibrant orange
                Error = "#ff5983",        // Hot pink
                Background = "#f8f9fa",   // Light gray
                Surface = "#ffffff",
                TextPrimary = "#2d3748",
                TextSecondary = "#718096"
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "8px"
            },
            Typography = new Typography
            {
                Default = new Default
                {
                    FontFamily = new[] { "Inter", "Arial", "sans-serif" },
                    FontSize = "15px",
                    LineHeight = 1.5
                },
                H1 = new H1 { FontSize = "2.25rem", FontWeight = 700 },
                H2 = new H2 { FontSize = "1.875rem", FontWeight = 600 },
                Button = new Button { FontSize = "1rem", FontWeight = 600 }
            }
        };
    }

    private MudTheme CreateSeniorTheme()
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#2563eb",      // Clear blue
                Secondary = "#7c3aed",    // Purple
                Tertiary = "#059669",     // Green
                Success = "#10b981",      // Success green
                Info = "#3b82f6",         // Info blue
                Warning = "#f59e0b",      // Warning amber
                Error = "#ef4444",        // Error red
                Background = "#ffffff",   // Pure white
                Surface = "#f9fafb",      // Light gray
                TextPrimary = "#111827",  // Very dark gray
                TextSecondary = "#4b5563" // Medium gray
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "4px" // Less rounded
            },
            Typography = new Typography
            {
                Default = new Default
                {
                    FontFamily = new[] { "Arial", "sans-serif" },
                    FontSize = "18px",        // Larger base size
                    LineHeight = 1.7          // More spacing
                },
                H1 = new H1 { FontSize = "2.5rem", FontWeight = 600 },
                H2 = new H2 { FontSize = "2rem", FontWeight = 500 },
                Button = new Button { FontSize = "1.2rem", FontWeight = 500 }
            }
        };
    }

    private MudTheme ApplyAccessibilityEnhancements(MudTheme theme)
    {
        // Increase contrast ratios
        theme.PaletteLight.TextPrimary = "#000000";
        theme.PaletteLight.TextSecondary = "#333333";
        
        // Enhance focus indicators
        theme.PaletteLight.Primary = "#0066cc"; // High contrast blue
        
        // Increase touch targets
        theme.LayoutProperties.DefaultBorderRadius = "6px";
        
        return theme;
    }

    private MudTheme GetDefaultTheme()
    {
        return new MudTheme(); // Standard MudBlazor theme
    }
}
```

#### **Adaptive Components**
```razor
@* Components/AdaptiveButton.razor *@
@inherits MudButton

<MudButton Class="@GetAdaptiveClasses()" 
           Style="@GetAdaptiveStyles()"
           Size="@GetAdaptiveSize()"
           @attributes="UserAttributes"
           OnClick="@OnClick">
    @ChildContent
</MudButton>

@code {
    [Parameter] public AgeRange UserAge { get; set; } = AgeRange.Unknown;
    [Parameter] public string AdaptiveClass { get; set; } = "";
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> UserAttributes { get; set; } = new();

    private string GetAdaptiveClasses()
    {
        var classes = new List<string> { "adaptive-button", AdaptiveClass };
        
        classes.Add(UserAge switch
        {
            AgeRange.Child => "child-button",
            AgeRange.Teen => "teen-button", 
            AgeRange.YoungAdult => "young-adult-button",
            AgeRange.EarlyAdult => "early-adult-button",
            AgeRange.MiddleAdult => "middle-adult-button",
            AgeRange.YoungSenior => "young-senior-button",
            AgeRange.Senior => "senior-button",
            AgeRange.ElderSenior => "elder-senior-button",
            _ => "default-button"
        });
        
        return string.Join(" ", classes);
    }

    private string GetAdaptiveStyles()
    {
        return UserAge switch
        {
            AgeRange.Child => "min-height: 48px; font-size: 1.1rem; padding: 12px 24px;",
            AgeRange.Teen => "min-height: 44px; font-size: 1rem; padding: 10px 20px;",
            AgeRange.YoungAdult => "min-height: 42px; font-size: 0.95rem; padding: 8px 16px;",
            AgeRange.EarlyAdult => "min-height: 40px; font-size: 0.9rem; padding: 8px 16px;",
            AgeRange.MiddleAdult => "min-height: 42px; font-size: 0.95rem; padding: 10px 18px;",
            AgeRange.YoungSenior => "min-height: 48px; font-size: 1.05rem; padding: 12px 20px;",
            AgeRange.Senior => "min-height: 56px; font-size: 1.2rem; padding: 14px 24px;",
            AgeRange.ElderSenior => "min-height: 64px; font-size: 1.3rem; padding: 16px 28px;",
            _ => "min-height: 40px; font-size: 0.9rem; padding: 8px 16px;"
        };
    }

    private Size GetAdaptiveSize()
    {
        return UserAge switch
        {
            AgeRange.Child => Size.Large,
            AgeRange.Senior => Size.Large,
            AgeRange.ElderSenior => Size.Large,
            _ => Size.Medium
        };
    }
}
```

### **Implementation Requirements**
1. **Age Detection Accuracy**: 70%+ accuracy for age range detection
2. **Theme Switching**: < 200ms theme application time
3. **Accessibility Compliance**: WCAG 2.1 AA standards
4. **Performance**: No perceivable UI lag during adaptations
5. **Persistence**: User preferences saved across sessions

---

## 🏗️ **TECHNICAL ARCHITECTURE**

### **System Architecture Diagram**
```
┌─────────────────────────────────────┐
│           Blazor Client             │
├─────────────────────────────────────┤
│  ┌───────────────────────────────┐  │
│  │      WebLLM Engine            │  │
│  │   - Model Management          │  │
│  │   - Privacy-First Processing  │  │
│  │   - Error Handling           │  │
│  └───────────────────────────────┘  │
│  ┌───────────────────────────────┐  │
│  │   Age-Adaptive UI System      │  │
│  │   - Age Detection            │  │
│  │   - Theme Management         │  │
│  │   - Component Adaptation     │  │
│  └───────────────────────────────┘  │
│  ┌───────────────────────────────┐  │
│  │     Chat Interface            │  │
│  │   - Message Handling         │  │
│  │   - Real-time Updates        │  │
│  │   - User Interaction         │  │
│  └───────────────────────────────┘  │
└─────────────────────────────────────┘
```

### **Component Dependencies**
```
WebLLMService ← WebLLMChat Component
     ↓
AgeDetectionService ← AdaptiveThemeService
     ↓
AdaptiveComponents ← Chat Interface
```

### **Data Flow**
1. **Initialization**: WebLLM model loads → Age detection runs → Theme adapts
2. **User Interaction**: Input received → Age analysis → UI adaptation → AI processing
3. **Response**: AI generates response → Age-appropriate formatting → Display

---

## 🧪 **TESTING REQUIREMENTS**

### **Unit Tests**
- **WebLLMService**: Model loading, response generation, error handling
- **AgeDetectionService**: Detection algorithms, confidence scoring
- **AdaptiveThemeService**: Theme generation, accessibility compliance
- **AdaptiveComponents**: Size calculations, style applications

### **Integration Tests**
- **WebLLM + UI**: Model responses trigger appropriate UI updates
- **Age Detection + Theming**: Age changes properly update themes
- **Component Adaptation**: All components respond to age changes

### **Browser Compatibility Tests**
- **Chrome**: 113+ with WebGPU support
- **Firefox**: 115+ with WebAssembly support
- **Safari**: 16.4+ with WebAssembly support
- **Edge**: 113+ with WebGPU support

### **Performance Tests**
- **Model Loading**: < 30 seconds on 25Mbps connection
- **Response Generation**: < 3 seconds for 300 tokens
- **Theme Switching**: < 200ms visual update
- **Age Detection**: < 500ms analysis time

### **Accessibility Tests**
- **Screen Reader**: NVDA, JAWS, VoiceOver compatibility
- **Keyboard Navigation**: Full keyboard accessibility
- **Color Contrast**: 4.5:1 minimum ratio
- **Touch Targets**: 44px minimum size

---

## 📋 **IMPLEMENTATION TIMELINE**

### **Week 1-2: WebLLM Foundation**
- [ ] Set up WebLLM integration
- [ ] Implement basic model loading
- [ ] Create WebLLMService with error handling
- [ ] Basic chat interface
- [ ] Unit tests for WebLLM functionality

### **Week 3-4: Age Detection System**
- [ ] Implement AgeDetectionService
- [ ] Browser-based detection algorithms
- [ ] Interaction pattern analysis
- [ ] Age profile persistence
- [ ] Detection accuracy testing

### **Week 5-6: Adaptive Theme System**
- [ ] Create AdaptiveThemeService
- [ ] Implement age-specific themes
- [ ] Accessibility enhancements
- [ ] Theme switching optimization
- [ ] Accessibility compliance testing

### **Week 7-8: Integration & Testing**
- [ ] Integrate all components
- [ ] End-to-end testing
- [ ] Performance optimization
- [ ] Browser compatibility testing
- [ ] Documentation and deployment

---

## 🎯 **SUCCESS CRITERIA**

### **Functional Requirements**
- [ ] WebLLM model loads successfully in supported browsers
- [ ] Age detection achieves 70%+ accuracy for age ranges
- [ ] Theme adaptation works for all age groups
- [ ] Chat interface responds appropriately to user age
- [ ] All components are accessible (WCAG 2.1 AA)

### **Performance Requirements**
- [ ] Model loading completes within 30 seconds
- [ ] Response generation under 3 seconds
- [ ] Theme switching under 200ms
- [ ] Age detection under 500ms
- [ ] No memory leaks during extended use

### **Quality Requirements**
- [ ] 90%+ unit test coverage
- [ ] Cross-browser compatibility verified
- [ ] Accessibility standards met
- [ ] Performance benchmarks achieved
- [ ] Documentation complete and accurate

---

## 📚 **DOCUMENTATION REQUIREMENTS**

### **Technical Documentation**
- [ ] API documentation for all services
- [ ] Component usage guides
- [ ] Integration patterns
- [ ] Error handling procedures
- [ ] Performance optimization guide

### **User Documentation**
- [ ] Age detection explanation
- [ ] Theme customization guide
- [ ] Accessibility features
- [ ] Browser compatibility
- [ ] Troubleshooting guide

### **Developer Documentation**
- [ ] Setup and configuration
- [ ] Testing procedures
- [ ] Deployment guide
- [ ] Contributing guidelines
- [ ] Architecture decisions

---

## 🔗 **DEPENDENCIES & PREREQUISITES**

### **External Dependencies**
- **WebLLM**: @mlc-ai/web-llm ^0.2.46
- **MudBlazor**: MudBlazor ^6.11.0
- **Blazor WebAssembly**: .NET 8.0
- **Browser Support**: Chrome 113+, Firefox 115+, Safari 16.4+

### **Internal Dependencies**
- **SpinnerNet.Shared**: Models and interfaces
- **SpinnerNet.Core**: Business logic foundation
- **Localization Service**: String resources
- **Local Storage**: User preferences

### **Prerequisites**
- [ ] Project structure established
- [ ] Blazor WebAssembly configured
- [ ] MudBlazor installed and configured
- [ ] Localization service implemented
- [ ] Development environment set up

---

## 🚀 **DEPLOYMENT CONSIDERATIONS**

### **Build Configuration**
- WebAssembly optimization enabled
- WebLLM model files included in build
- Static asset compression
- Service worker for offline capability

### **Performance Optimization**
- Lazy loading for non-essential components
- WebLLM model caching
- Theme preloading
- Component virtualization

### **Monitoring & Analytics**
- Age detection accuracy metrics
- Model loading performance
- Theme switching frequency
- User interaction patterns

---

*This Phase 1 PRP establishes the foundation for all subsequent features. Success here is critical for the overall project timeline and user experience quality.*