# AI Integration Guide - Spinner.Net Sprint 1

## Overview

This guide covers AI integration patterns for Sprint 1 using **Semantic Kernel** (never direct HTTP calls). You'll implement multi-provider AI (OpenAI + Ollama), natural language processing for tasks, persona interviews, and AI buddy conversations.

**Critical Rule**: Always use `IAiService` interfaces and Semantic Kernel - NEVER direct HTTP connections to AI APIs.

## Core AI Architecture

### Multi-Provider Strategy

```
User Input → AI Router → [Local LLM (Ollama) OR Cloud LLM (OpenAI)] → Response
```

**Routing Logic:**
- **Local First**: Privacy-sensitive content (personal tasks, emotions)
- **Cloud for Complex**: Multi-turn conversations, complex reasoning
- **Cost Optimization**: Prefer free local over paid cloud
- **Fallback**: Cloud if local unavailable

## Setup and Configuration

### 1. Semantic Kernel Service Registration

**Program.cs:**
```csharp
// Semantic Kernel setup - NEVER use direct HTTP
services.AddSemanticKernel(options =>
{
    // Primary cloud provider
    options.AddOpenAITextGeneration(
        modelId: "gpt-4o-mini",
        apiKey: Configuration["OpenAI:ApiKey"]);
        
    options.AddOpenAITextEmbeddingGeneration(
        modelId: "text-embedding-3-small", 
        apiKey: Configuration["OpenAI:ApiKey"]);
});

// Local LLM integration via HTTP connector
services.AddHttpClient<OllamaService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:11434");
});

// Custom AI service interfaces
services.AddScoped<IAiService, MultiProviderAiService>();
services.AddScoped<IPersonaInterviewService, PersonaInterviewService>();
services.AddScoped<ITaskProcessingService, TaskProcessingService>();
services.AddScoped<IBuddyConversationService, BuddyConversationService>();
```

### 2. AI Service Interfaces

**Core interface for all AI operations:**
```csharp
public interface IAiService
{
    Task<string> GenerateResponseAsync(
        string prompt, 
        AiContext context,
        CancellationToken cancellationToken = default);
        
    Task<T> ProcessStructuredAsync<T>(
        string prompt,
        AiContext context,
        CancellationToken cancellationToken = default) where T : class;
        
    IAsyncEnumerable<string> StreamResponseAsync(
        string prompt,
        AiContext context,
        CancellationToken cancellationToken = default);
}

public class AiContext
{
    public string UserId { get; set; } = string.Empty;
    public AiProvider PreferredProvider { get; set; } = AiProvider.Auto;
    public bool RequiresPrivacy { get; set; } = false;
    public int MaxTokens { get; set; } = 1024;
    public string SystemPrompt { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public enum AiProvider
{
    Auto,       // Let service decide
    Local,      // Force local (Ollama)
    Cloud,      // Force cloud (OpenAI)
    LocalFirst  // Try local, fallback to cloud
}
```

### 3. Multi-Provider AI Service Implementation

**MultiProviderAiService.cs:**
```csharp
public class MultiProviderAiService : IAiService
{
    private readonly Kernel _kernel;
    private readonly OllamaService _ollamaService;
    private readonly ILogger<MultiProviderAiService> _logger;
    private readonly IConfiguration _configuration;
    
    public MultiProviderAiService(
        Kernel kernel,
        OllamaService ollamaService,
        ILogger<MultiProviderAiService> logger,
        IConfiguration configuration)
    {
        _kernel = kernel;
        _ollamaService = ollamaService;
        _logger = logger;
        _configuration = configuration;
    }
    
    public async Task<string> GenerateResponseAsync(
        string prompt, 
        AiContext context,
        CancellationToken cancellationToken = default)
    {
        var provider = DetermineProvider(context);
        
        try
        {
            return provider switch
            {
                AiProvider.Local => await GenerateLocalAsync(prompt, context, cancellationToken),
                AiProvider.Cloud => await GenerateCloudAsync(prompt, context, cancellationToken),
                AiProvider.LocalFirst => await GenerateLocalFirstAsync(prompt, context, cancellationToken),
                _ => await GenerateAutoAsync(prompt, context, cancellationToken)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI generation failed for provider {Provider}", provider);
            
            // Fallback strategy
            if (provider == AiProvider.Local && IsCloudAvailable())
            {
                _logger.LogInformation("Falling back to cloud provider");
                return await GenerateCloudAsync(prompt, context, cancellationToken);
            }
            
            throw;
        }
    }
    
    private async Task<string> GenerateLocalAsync(string prompt, AiContext context, CancellationToken cancellationToken)
    {
        var ollamaRequest = new OllamaRequest
        {
            Model = _configuration["LocalLLM:ModelName"] ?? "llama3.1:8b",
            Prompt = CombinePrompts(context.SystemPrompt, prompt),
            Stream = false,
            Options = new { temperature = 0.7, max_tokens = context.MaxTokens }
        };
        
        var response = await _ollamaService.GenerateAsync(ollamaRequest, cancellationToken);
        
        _logger.LogDebug("Local AI response generated for user {UserId}", context.UserId);
        return response.Response;
    }
    
    private async Task<string> GenerateCloudAsync(string prompt, AiContext context, CancellationToken cancellationToken)
    {
        // Use Semantic Kernel - NEVER direct HTTP calls
        var function = _kernel.CreateFunctionFromPrompt(
            CombinePrompts(context.SystemPrompt, prompt),
            new OpenAIPromptExecutionSettings 
            { 
                MaxTokens = context.MaxTokens,
                Temperature = 0.7
            });
            
        var result = await _kernel.InvokeAsync(function, cancellationToken: cancellationToken);
        
        _logger.LogDebug("Cloud AI response generated for user {UserId}", context.UserId);
        return result.ToString();
    }
    
    private AiProvider DetermineProvider(AiContext context)
    {
        // Privacy-sensitive content goes local
        if (context.RequiresPrivacy)
            return AiProvider.LocalFirst;
            
        // User preference
        if (context.PreferredProvider != AiProvider.Auto)
            return context.PreferredProvider;
            
        // Configuration-based routing
        var preferLocal = _configuration.GetValue<bool>("AI:PreferLocal", true);
        var useCloudForComplex = _configuration.GetValue<bool>("AI:UseCloudForComplexTasks", true);
        
        // Complex tasks (>2048 tokens) prefer cloud
        if (useCloudForComplex && context.MaxTokens > 2048)
            return AiProvider.LocalFirst;
            
        return preferLocal ? AiProvider.LocalFirst : AiProvider.Cloud;
    }
}
```

## Sprint 1 AI Features

### 1. Persona Interview AI

**PersonaInterviewService.cs:**
```csharp
public class PersonaInterviewService : IPersonaInterviewService
{
    private readonly IAiService _aiService;
    
    public async Task<InterviewQuestion> GenerateNextQuestionAsync(
        string userId,
        List<InterviewResponse> previousResponses,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt = """
            You are conducting a friendly persona discovery interview.
            Goal: Understand the user's interests, goals, communication style, and preferences.
            
            Guidelines:
            - Ask one question at a time
            - Be conversational and warm
            - Build on previous responses
            - Discover: interests, goals, work style, communication preferences
            - Total interview: 6-8 questions maximum
            """;
            
        var conversationHistory = BuildConversationHistory(previousResponses);
        var prompt = $"""
            Conversation so far:
            {conversationHistory}
            
            Generate the next interview question. Make it personal, engaging, and build on what they've shared.
            Response format: Just the question, nothing else.
            """;
            
        var context = new AiContext
        {
            UserId = userId,
            SystemPrompt = systemPrompt,
            PreferredProvider = AiProvider.LocalFirst, // Personal interview is privacy-sensitive
            MaxTokens = 512
        };
        
        var question = await _aiService.GenerateResponseAsync(prompt, context, cancellationToken);
        
        return new InterviewQuestion
        {
            Text = question.Trim(),
            QuestionNumber = previousResponses.Count + 1,
            IsComplete = previousResponses.Count >= 7 // Stop after 8 questions
        };
    }
    
    public async Task<PersonaInsights> AnalyzeResponsesAsync(
        string userId,
        List<InterviewResponse> responses,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt = """
            Analyze interview responses to create a user persona.
            Extract: interests, goals, communication style, preferred working methods, personality traits.
            """;
            
        var allResponses = string.Join("\n\n", responses.Select(r => $"Q: {r.Question}\nA: {r.Answer}"));
        
        var prompt = $"""
            Interview responses:
            {allResponses}
            
            Create a JSON persona analysis:
            {{
                "interests": ["array", "of", "interests"],
                "goals": ["array", "of", "goals"],
                "communicationStyle": "formal|casual|friendly",
                "workStyle": "focused|collaborative|flexible",
                "personalityTraits": ["trait1", "trait2"],
                "preferredLanguage": "detected language code",
                "uiComplexity": "simple|standard|advanced"
            }}
            """;
            
        var context = new AiContext
        {
            UserId = userId,
            SystemPrompt = systemPrompt,
            PreferredProvider = AiProvider.LocalFirst,
            RequiresPrivacy = true, // Personal analysis
            MaxTokens = 1024
        };
        
        var jsonResponse = await _aiService.GenerateResponseAsync(prompt, context, cancellationToken);
        
        // Parse JSON response to PersonaInsights object
        return JsonSerializer.Deserialize<PersonaInsights>(jsonResponse);
    }
}
```

### 2. Natural Language Task Processing

**TaskProcessingService.cs:**
```csharp
public class TaskProcessingService : ITaskProcessingService
{
    private readonly IAiService _aiService;
    
    public async Task<TaskParseResult> ParseNaturalLanguageTaskAsync(
        string userInput,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt = """
            Parse natural language input into structured task data.
            Extract: title, due date, priority, category, estimated time.
            
            Categories: work, personal, health, family, learning, creative, social
            Priority: low, medium, high, urgent
            """;
            
        var prompt = $"""
            User input: "{userInput}"
            Current time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
            
            Parse into JSON:
            {{
                "title": "clean task title",
                "description": "original input",
                "dueDate": "2024-01-15T14:00:00Z or null",
                "priority": "low|medium|high|urgent",
                "category": "work|personal|health|family|learning|creative|social",
                "estimatedMinutes": 30,
                "tags": ["tag1", "tag2"],
                "confidence": 0.95
            }}
            """;
            
        var context = new AiContext
        {
            UserId = userId,
            SystemPrompt = systemPrompt,
            PreferredProvider = AiProvider.LocalFirst, // Personal tasks are privacy-sensitive
            RequiresPrivacy = true,
            MaxTokens = 512
        };
        
        var jsonResponse = await _aiService.GenerateResponseAsync(prompt, context, cancellationToken);
        
        return JsonSerializer.Deserialize<TaskParseResult>(jsonResponse);
    }
}
```

### 3. AI Buddy Conversations

**BuddyConversationService.cs:**
```csharp
public class BuddyConversationService : IBuddyConversationService
{
    private readonly IAiService _aiService;
    private readonly ITaskRepository _taskRepository;
    
    public async Task<BuddyResponse> GenerateResponseAsync(
        string userMessage,
        BuddyContext buddyContext,
        CancellationToken cancellationToken = default)
    {
        // Get user's current tasks for context
        var recentTasks = await _taskRepository.GetRecentTasksAsync(buddyContext.UserId, 5);
        var taskContext = BuildTaskContext(recentTasks);
        
        var systemPrompt = $"""
            You are {buddyContext.BuddyName}, a helpful AI companion for {buddyContext.UserDisplayName}.
            
            Personality: {buddyContext.Personality}
            Communication style: {buddyContext.CommunicationStyle}
            
            User's current tasks:
            {taskContext}
            
            Guidelines:
            - Be helpful and supportive
            - Reference their tasks when relevant
            - Suggest task management improvements
            - Keep responses conversational and brief
            - Use their communication style
            """;
            
        var conversationHistory = BuildConversationHistory(buddyContext.RecentMessages);
        
        var prompt = $"""
            Conversation:
            {conversationHistory}
            
            User: {userMessage}
            
            Respond as {buddyContext.BuddyName}:
            """;
            
        var context = new AiContext
        {
            UserId = buddyContext.UserId,
            SystemPrompt = systemPrompt,
            PreferredProvider = AiProvider.LocalFirst, // Personal conversations are private
            RequiresPrivacy = true,
            MaxTokens = 512
        };
        
        var response = await _aiService.GenerateResponseAsync(prompt, context, cancellationToken);
        
        return new BuddyResponse
        {
            Message = response.Trim(),
            Emotion = "supportive",
            SuggestedActions = ExtractSuggestedActions(response),
            Confidence = 0.9
        };
    }
    
    public async IAsyncEnumerable<string> StreamResponseAsync(
        string userMessage,
        BuddyContext buddyContext,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // For real-time chat streaming
        var context = new AiContext
        {
            UserId = buddyContext.UserId,
            PreferredProvider = AiProvider.Cloud, // Streaming works better with cloud
            MaxTokens = 512
        };
        
        await foreach (var chunk in _aiService.StreamResponseAsync(userMessage, context, cancellationToken))
        {
            yield return chunk;
        }
    }
}
```

## Ollama Local LLM Integration

### OllamaService Implementation

**OllamaService.cs:**
```csharp
public class OllamaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaService> _logger;
    
    public OllamaService(HttpClient httpClient, ILogger<OllamaService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<OllamaResponse> GenerateAsync(
        OllamaRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/generate", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<OllamaResponse>(responseJson);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ollama service unavailable");
            throw new AiServiceUnavailableException("Local LLM service is not available", ex);
        }
    }
    
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/tags", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

public class OllamaRequest
{
    public string Model { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public bool Stream { get; set; } = false;
    public object? Options { get; set; }
}

public class OllamaResponse
{
    public string Response { get; set; } = string.Empty;
    public bool Done { get; set; }
}
```

## AI Context and Privacy

### Privacy-First AI Processing

**Data sovereignty integration:**
```csharp
public class PrivacyAwareAiService : IAiService
{
    private readonly IAiService _baseService;
    private readonly IDataSovereigntyService _dataSovereignty;
    
    public async Task<string> GenerateResponseAsync(
        string prompt, 
        AiContext context,
        CancellationToken cancellationToken = default)
    {
        // Check user's AI processing preferences
        var aiPreference = await _dataSovereignty.GetAiProcessingPreferenceAsync(context.UserId);
        
        // Override context based on user preferences
        context.PreferredProvider = aiPreference switch
        {
            AiProcessingPreference.LocalOnly => AiProvider.Local,
            AiProcessingPreference.LocalPreferred => AiProvider.LocalFirst,
            AiProcessingPreference.CloudAllowed => AiProvider.Auto,
            _ => context.PreferredProvider
        };
        
        // Mark as requiring privacy for certain content types
        if (IsPersonalContent(prompt))
        {
            context.RequiresPrivacy = true;
        }
        
        return await _baseService.GenerateResponseAsync(prompt, context, cancellationToken);
    }
    
    private bool IsPersonalContent(string prompt)
    {
        var personalIndicators = new[] 
        { 
            "personal", "private", "emotion", "feeling", "family", 
            "relationship", "health", "finance", "goal", "dream" 
        };
        
        return personalIndicators.Any(indicator => 
            prompt.Contains(indicator, StringComparison.OrdinalIgnoreCase));
    }
}
```

## MCP (Model Context Protocol) Integration

### MCP as External AI Integration

**MCP Client Service for External AI Tools:**
```csharp
public interface IMcpClientService
{
    Task<McpResponse> ExecuteToolAsync(
        string toolName, 
        Dictionary<string, object> parameters,
        string userId,
        CancellationToken cancellationToken = default);
        
    Task<List<McpTool>> GetAvailableToolsAsync(
        string userId,
        CancellationToken cancellationToken = default);
}

public class McpClientService : IMcpClientService
{
    private readonly IAiService _aiService;
    private readonly IDataSovereigntyService _dataSovereignty;
    private readonly ILogger<McpClientService> _logger;
    
    public async Task<McpResponse> ExecuteToolAsync(
        string toolName, 
        Dictionary<string, object> parameters,
        string userId,
        CancellationToken cancellationToken = default)
    {
        // Check user permissions for MCP tool access
        var hasPermission = await _dataSovereignty.CheckMcpToolPermissionAsync(userId, toolName);
        if (!hasPermission)
        {
            throw new UnauthorizedAccessException($"User {userId} not authorized for tool {toolName}");
        }
        
        // Apply data filtering based on sovereignty settings
        var filteredParams = await FilterParametersForPrivacy(parameters, userId);
        
        // Execute via AI with MCP context
        var context = new AiContext
        {
            UserId = userId,
            PreferredProvider = AiProvider.LocalFirst,
            RequiresPrivacy = true,
            Metadata = new Dictionary<string, object>
            {
                ["mcpTool"] = toolName,
                ["mcpParams"] = filteredParams
            }
        };
        
        var prompt = BuildMcpPrompt(toolName, filteredParams);
        var response = await _aiService.GenerateResponseAsync(prompt, context, cancellationToken);
        
        return new McpResponse
        {
            ToolName = toolName,
            Result = response,
            Success = true,
            ProcessedLocally = context.PreferredProvider == AiProvider.Local
        };
    }
}
```

## Error Handling and Resilience

### AI Service Resilience

**Retry and fallback patterns:**
```csharp
public class ResilientAiService : IAiService
{
    private readonly IAiService _baseService;
    private readonly ILogger<ResilientAiService> _logger;
    
    public async Task<string> GenerateResponseAsync(
        string prompt, 
        AiContext context,
        CancellationToken cancellationToken = default)
    {
        var retryPolicy = Policy
            .Handle<AiServiceUnavailableException>()
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("AI service retry {RetryCount} after {Delay}ms", 
                        retryCount, timespan.TotalMilliseconds);
                });
                
        return await retryPolicy.ExecuteAsync(async () =>
        {
            return await _baseService.GenerateResponseAsync(prompt, context, cancellationToken);
        });
    }
}
```

## Testing AI Integration

### Unit Testing

**Mock AI service for testing:**
```csharp
public class MockAiService : IAiService
{
    public Task<string> GenerateResponseAsync(string prompt, AiContext context, CancellationToken cancellationToken = default)
    {
        // Return predictable responses for testing
        if (prompt.Contains("task"))
            return Task.FromResult("I can help you with that task!");
            
        if (prompt.Contains("interview"))
            return Task.FromResult("What interests you most in your daily life?");
            
        return Task.FromResult("I'm here to help!");
    }
    
    // ... other methods
}
```

**Integration testing:**
```csharp
[Test]
public async Task PersonaInterview_GeneratesAppropriateQuestions()
{
    // Arrange
    var service = new PersonaInterviewService(_aiService);
    var responses = new List<InterviewResponse>
    {
        new() { Question = "What do you do for work?", Answer = "I'm a software developer" }
    };
    
    // Act
    var question = await service.GenerateNextQuestionAsync("user123", responses);
    
    // Assert
    Assert.That(question.Text, Does.Not.Contain("work")); // Should ask about something else
    Assert.That(question.QuestionNumber, Is.EqualTo(2));
}
```

## Performance and Monitoring

### AI Usage Analytics

**Track AI usage:**
```csharp
public class AnalyticsAiService : IAiService
{
    private readonly IAiService _baseService;
    private readonly IAnalyticsService _analytics;
    
    public async Task<string> GenerateResponseAsync(
        string prompt, 
        AiContext context,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = await _baseService.GenerateResponseAsync(prompt, context, cancellationToken);
            
            await _analytics.TrackAiUsageAsync(new AiUsageEvent
            {
                UserId = context.UserId,
                Provider = DetermineActualProvider(context),
                TokensUsed = EstimateTokens(prompt + result),
                Duration = stopwatch.Elapsed,
                Success = true
            });
            
            return result;
        }
        catch (Exception ex)
        {
            await _analytics.TrackAiUsageAsync(new AiUsageEvent
            {
                UserId = context.UserId,
                Duration = stopwatch.Elapsed,
                Success = false,
                Error = ex.Message
            });
            
            throw;
        }
    }
}
```

---

## Quick Reference

**Key Principles:**
- ✅ Always use Semantic Kernel (`IAiService`)
- ❌ Never direct HTTP calls to OpenAI
- 🔒 Privacy-sensitive → Local LLM
- ☁️ Complex tasks → Cloud LLM
- 🔄 Always have fallback strategies
- 🔌 MCP integration → User-controlled external tool access

**Sprint 1 Usage:**
- **Persona Interviews**: `PersonaInterviewService`
- **Task Processing**: `TaskProcessingService` 
- **Buddy Chat**: `BuddyConversationService`
- **All routing**: `MultiProviderAiService`

**MCP Integration (Sprint 6+):**
- **External AI Tools**: `IMcpClientService`
- **Data Sovereignty**: Privacy-first MCP access
- **Tool Discovery**: User-controlled external integrations
- **Workflow Automation**: AI-powered cross-service tasks

**Key Services:**
```csharp
// Core AI routing
services.AddScoped<IAiService, MultiProviderAiService>();

// MCP external tool integration  
services.AddScoped<IMcpClientService, McpClientService>();

// Privacy-aware processing
services.AddScoped<IDataSovereigntyService, DataSovereigntyService>();
```

Ready to implement AI-powered Sprint 1 features with future MCP integration! 🤖🔌