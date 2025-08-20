using Microsoft.JSInterop;
using System.Text.Json;

namespace SpinnerNet.App.Services.WebLLM;

/// <summary>
/// WebLLM service implementation for privacy-first AI processing
/// Implements client-side AI with Hermes-2-Pro-Mistral-7B model
/// Based on Phase 1 Foundation PRP requirements
/// </summary>
public class WebLLMService : IWebLLMService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<WebLLMService> _logger;
    
    private DotNetObjectReference<WebLLMService>? _dotNetObjectRef;
    private bool _isInitialized = false;
    private bool _isInitializing = false;
    private int _initializationProgress = 0;
    private string _currentStatus = "Not initialized";
    private string? _lastError = null;
    private string? _currentSessionId = null;

    // Events
    public event Action<string>? StatusUpdated;
    public event Action<WebLLMProgress>? ProgressUpdated;
    public event Action<string>? ErrorOccurred;
    public event Action<int>? InferenceCompleted;

    // Properties
    public bool IsInitializing => _isInitializing;
    public bool IsInitialized => _isInitialized;
    public int InitializationProgress => _initializationProgress;
    public string CurrentStatus => _currentStatus;
    public string? LastError => _lastError;

    public WebLLMService(IJSRuntime jsRuntime, ILogger<WebLLMService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> InitializeAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
        {
            _logger.LogInformation("WebLLM already initialized for session {SessionId}", sessionId);
            return true;
        }

        if (_isInitializing)
        {
            _logger.LogWarning("WebLLM initialization already in progress for session {SessionId}", sessionId);
            return false;
        }

        try
        {
            _isInitializing = true;
            _currentSessionId = sessionId;
            _lastError = null;
            
            _logger.LogInformation("Starting WebLLM initialization for session {SessionId}", sessionId);
            
            // Create .NET object reference for JavaScript callbacks
            _dotNetObjectRef = DotNetObjectReference.Create(this);
            
            // Call JavaScript initialization
            var result = await _jsRuntime.InvokeAsync<bool>(
                "spinnerNetWebLLM.initialize", 
                cancellationToken,
                _dotNetObjectRef, 
                sessionId);

            if (result)
            {
                _isInitialized = true;
                _logger.LogInformation("WebLLM initialized successfully for session {SessionId}", sessionId);
            }
            else
            {
                _logger.LogError("WebLLM initialization failed for session {SessionId}", sessionId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;
            _logger.LogError(ex, "Exception during WebLLM initialization for session {SessionId}", sessionId);
            return false;
        }
        finally
        {
            _isInitializing = false;
        }
    }

    /// <inheritdoc />
    public async Task<string> GeneratePersonaResponseAsync(
        string prompt, 
        WebLLMGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("WebLLM is not initialized. Call InitializeAsync first.");
        }

        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));
        }

        try
        {
            _logger.LogInformation("Generating persona response for prompt length: {PromptLength}", prompt.Length);

            // Convert options to JavaScript object
            var jsOptions = options != null ? new
            {
                systemPrompt = options.SystemPrompt,
                userAge = options.UserAge,
                temperature = options.Temperature,
                maxTokens = options.MaxTokens,
                topP = options.TopP,
                stream = options.Stream,
                stopSequences = options.StopSequences
            } : null;

            var response = await _jsRuntime.InvokeAsync<string>(
                "spinnerNetWebLLM.generatePersonaResponse",
                cancellationToken,
                prompt,
                jsOptions);

            _logger.LogInformation("Persona response generated successfully. Response length: {ResponseLength}", response?.Length ?? 0);
            
            return response ?? string.Empty;
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;
            _logger.LogError(ex, "Failed to generate persona response");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<WebLLMPerformanceMetrics> GetPerformanceMetricsAsync()
    {
        try
        {
            var metricsJson = await _jsRuntime.InvokeAsync<JsonElement>("spinnerNetWebLLM.getPerformanceMetrics");
            
            return new WebLLMPerformanceMetrics
            {
                IsInitialized = metricsJson.TryGetProperty("isInitialized", out var init) && init.GetBoolean(),
                IsLoading = metricsJson.TryGetProperty("isLoading", out var loading) && loading.GetBoolean(),
                InitializationTime = metricsJson.TryGetProperty("initializationTime", out var initTime) 
                    ? initTime.GetInt32() : null,
                AverageInferenceTime = metricsJson.TryGetProperty("averageInferenceTime", out var avgTime) 
                    ? avgTime.GetInt32() : null,
                TotalInferences = metricsJson.TryGetProperty("totalInferences", out var total) 
                    ? total.GetInt32() : 0,
                LastError = metricsJson.TryGetProperty("lastError", out var error) 
                    ? error.GetString() : null,
                RetryCount = metricsJson.TryGetProperty("retryCount", out var retry) 
                    ? retry.GetInt32() : 0,
                LoadingProgress = metricsJson.TryGetProperty("loadingProgress", out var progress) 
                    ? progress.GetInt32() : 0,
                CurrentStatus = metricsJson.TryGetProperty("currentStatus", out var status) 
                    ? status.GetString() ?? string.Empty : string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get WebLLM performance metrics");
            return new WebLLMPerformanceMetrics
            {
                IsInitialized = _isInitialized,
                IsLoading = _isInitializing,
                CurrentStatus = _currentStatus,
                LastError = ex.Message
            };
        }
    }

    #region JavaScript Callbacks

    /// <summary>
    /// JavaScript callback for status updates
    /// </summary>
    [JSInvokable]
    public void OnWebLLMStatusUpdate(string status)
    {
        _currentStatus = status;
        _logger.LogInformation("WebLLM Status: {Status}", status);
        StatusUpdated?.Invoke(status);
    }

    /// <summary>
    /// JavaScript callback for progress updates
    /// </summary>
    [JSInvokable]
    public void OnWebLLMProgress(JsonElement progressJson)
    {
        try
        {
            var progress = new WebLLMProgress
            {
                Progress = progressJson.TryGetProperty("progress", out var prog) ? prog.GetDouble() : 0.0,
                Text = progressJson.TryGetProperty("text", out var text) ? text.GetString() ?? string.Empty : string.Empty,
                TimeElapsed = progressJson.TryGetProperty("timeElapsed", out var time) ? time.GetInt32() : 0
            };

            _initializationProgress = (int)(progress.Progress * 100);
            
            _logger.LogDebug("WebLLM Progress: {Progress}% - {Text}", 
                _initializationProgress, progress.Text);
            
            ProgressUpdated?.Invoke(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process WebLLM progress update");
        }
    }

    /// <summary>
    /// JavaScript callback for errors
    /// </summary>
    [JSInvokable]
    public void OnWebLLMError(string error)
    {
        _lastError = error;
        _logger.LogError("WebLLM Error: {Error}", error);
        ErrorOccurred?.Invoke(error);
    }

    /// <summary>
    /// JavaScript callback for inference completion
    /// </summary>
    [JSInvokable]
    public void OnWebLLMInferenceComplete(int inferenceTime)
    {
        _logger.LogDebug("WebLLM Inference completed in {InferenceTime}ms", inferenceTime);
        InferenceCompleted?.Invoke(inferenceTime);
    }

    #endregion

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_isInitialized)
            {
                await _jsRuntime.InvokeVoidAsync("spinnerNetWebLLM.dispose");
                _logger.LogInformation("WebLLM disposed for session {SessionId}", _currentSessionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing WebLLM");
        }
        finally
        {
            _dotNetObjectRef?.Dispose();
            _dotNetObjectRef = null;
            _isInitialized = false;
            _isInitializing = false;
            _currentSessionId = null;
        }
    }
}