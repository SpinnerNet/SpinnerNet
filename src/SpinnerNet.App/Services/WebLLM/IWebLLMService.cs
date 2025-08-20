using Microsoft.JSInterop;

namespace SpinnerNet.App.Services.WebLLM;

/// <summary>
/// Interface for WebLLM service that handles client-side AI processing
/// Implements privacy-first AI with Hermes-2-Pro-Mistral-7B model
/// </summary>
public interface IWebLLMService
{
    /// <summary>
    /// Event fired when WebLLM initialization status changes
    /// </summary>
    event Action<string>? StatusUpdated;
    
    /// <summary>
    /// Event fired when WebLLM loading progress changes
    /// </summary>
    event Action<WebLLMProgress>? ProgressUpdated;
    
    /// <summary>
    /// Event fired when WebLLM encounters an error
    /// </summary>
    event Action<string>? ErrorOccurred;
    
    /// <summary>
    /// Event fired when AI inference is completed
    /// </summary>
    event Action<int>? InferenceCompleted;

    /// <summary>
    /// Gets whether WebLLM is currently initializing
    /// </summary>
    bool IsInitializing { get; }
    
    /// <summary>
    /// Gets whether WebLLM is initialized and ready for use
    /// </summary>
    bool IsInitialized { get; }
    
    /// <summary>
    /// Gets the current initialization progress (0-100)
    /// </summary>
    int InitializationProgress { get; }
    
    /// <summary>
    /// Gets the current status message
    /// </summary>
    string CurrentStatus { get; }
    
    /// <summary>
    /// Gets the last error message if any
    /// </summary>
    string? LastError { get; }

    /// <summary>
    /// Initialize WebLLM engine with the specified model
    /// </summary>
    /// <param name="sessionId">Unique session identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if initialization was successful</returns>
    Task<bool> InitializeAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate AI response for persona creation
    /// </summary>
    /// <param name="prompt">User input or structured prompt</param>
    /// <param name="options">Generation options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI generated response</returns>
    Task<string> GeneratePersonaResponseAsync(
        string prompt, 
        WebLLMGenerationOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current performance metrics
    /// </summary>
    /// <returns>Performance metrics object</returns>
    Task<WebLLMPerformanceMetrics> GetPerformanceMetricsAsync();

    /// <summary>
    /// Dispose resources and clean up WebLLM engine
    /// </summary>
    /// <returns>ValueTask representing the disposal operation</returns>
    ValueTask DisposeAsync();
}

/// <summary>
/// WebLLM progress information
/// </summary>
public record WebLLMProgress
{
    /// <summary>
    /// Progress percentage (0.0 to 1.0)
    /// </summary>
    public double Progress { get; init; }
    
    /// <summary>
    /// Progress description text
    /// </summary>
    public string Text { get; init; } = string.Empty;
    
    /// <summary>
    /// Time elapsed in milliseconds
    /// </summary>
    public int TimeElapsed { get; init; }
}

/// <summary>
/// WebLLM generation options
/// </summary>
public record WebLLMGenerationOptions
{
    /// <summary>
    /// System prompt for AI context
    /// </summary>
    public string? SystemPrompt { get; init; }
    
    /// <summary>
    /// User's age for age-adaptive prompting
    /// </summary>
    public int? UserAge { get; init; }
    
    /// <summary>
    /// Temperature for response randomness (0.0 to 1.0)
    /// </summary>
    public double Temperature { get; init; } = 0.7;
    
    /// <summary>
    /// Maximum number of tokens to generate
    /// </summary>
    public int MaxTokens { get; init; } = 800;
    
    /// <summary>
    /// Top-p sampling parameter (0.0 to 1.0)
    /// </summary>
    public double TopP { get; init; } = 0.9;
    
    /// <summary>
    /// Whether to stream the response
    /// </summary>
    public bool Stream { get; init; } = false;
    
    /// <summary>
    /// Stop sequences to end generation
    /// </summary>
    public string[]? StopSequences { get; init; }
}

/// <summary>
/// WebLLM performance metrics
/// </summary>
public record WebLLMPerformanceMetrics
{
    /// <summary>
    /// Whether WebLLM is initialized
    /// </summary>
    public bool IsInitialized { get; init; }
    
    /// <summary>
    /// Whether WebLLM is currently loading
    /// </summary>
    public bool IsLoading { get; init; }
    
    /// <summary>
    /// Initialization time in milliseconds
    /// </summary>
    public int? InitializationTime { get; init; }
    
    /// <summary>
    /// Average inference time in milliseconds
    /// </summary>
    public int? AverageInferenceTime { get; init; }
    
    /// <summary>
    /// Total number of inferences performed
    /// </summary>
    public int TotalInferences { get; init; }
    
    /// <summary>
    /// Last error message if any
    /// </summary>
    public string? LastError { get; init; }
    
    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryCount { get; init; }
    
    /// <summary>
    /// Current loading progress (0-100)
    /// </summary>
    public int LoadingProgress { get; init; }
    
    /// <summary>
    /// Current status message
    /// </summary>
    public string CurrentStatus { get; init; } = string.Empty;
}