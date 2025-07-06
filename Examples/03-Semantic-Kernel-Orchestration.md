# Semantic Kernel Orchestration Example

This example demonstrates how to implement server-side AI orchestration using Microsoft Semantic Kernel with OpenAI integration for enterprise-grade workflow management in the hybrid architecture.

## Overview

Semantic Kernel provides powerful orchestration capabilities that complement client-side WebLLM:
- **Workflow orchestration** - Complex multi-step AI processes
- **Memory management** - Persistent conversation context
- **Enterprise integration** - Azure services and APIs
- **Prompt engineering** - Optimized AI interactions
- **Planning and reasoning** - Advanced cognitive capabilities

## Key Components

### 1. AI Orchestration Service

**File:** `Core/Services/AIOrchestrationService.cs`

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpinnerNet.Shared.Models;

namespace SpinnerNet.Core.Services
{
    /// <summary>
    /// AI Orchestration Service using Semantic Kernel for workflow management
    /// Coordinates with client-side WebLLM for hybrid intelligence
    /// </summary>
    public class AIOrchestrationService
    {
        private readonly IKernel _kernel;
        private readonly IChatCompletionService _chatService;
        private readonly ILogger<AIOrchestrationService> _logger;
        private readonly Dictionary<string, InterviewSession> _sessions;

        public AIOrchestrationService(
            IKernel kernel,
            IChatCompletionService chatService,
            ILogger<AIOrchestrationService> logger)
        {
            _kernel = kernel;
            _chatService = chatService;
            _logger = logger;
            _sessions = new Dictionary<string, InterviewSession>();
        }

        /// <summary>
        /// Creates a new interview flow using Semantic Kernel planning
        /// </summary>
        public async Task<InterviewFlow> CreateInterviewFlowAsync(string userId, string goal)
        {
            try
            {
                _logger.LogInformation("Creating interview flow for user {UserId} with goal: {Goal}", userId, goal);

                // Use Semantic Kernel for high-level planning
                var planningFunction = _kernel.CreateFunctionFromPrompt(
                    """
                    You are an expert interview designer. Create a structured interview flow for personality assessment.
                    
                    Goal: {{$goal}}
                    
                    Create a 5-question interview that progresses logically:
                    1. Opening question about motivations
                    2. Values and decision-making exploration  
                    3. Challenge and problem-solving approach
                    4. Goals and aspirations
                    5. Summary and closing insights
                    
                    Return a JSON structure with:
                    - stepNumber: Question number (1-5)
                    - focusArea: What this question explores
                    - promptTemplate: Template for generating the actual question
                    - expectedInsights: What traits this reveals
                    
                    Make it conversational and engaging.
                    """,
                    new OpenAIPromptExecutionSettings
                    {
                        Temperature = 0.7,
                        MaxTokens = 800,
                        ResponseFormat = "json_object"
                    }
                );

                var planResult = await planningFunction.InvokeAsync(_kernel, new()
                {
                    ["goal"] = goal
                });

                var planJson = planResult.ToString();
                
                // Store the interview plan
                var interviewFlow = new InterviewFlow
                {
                    UserId = userId,
                    Goal = goal,
                    CreatedAt = DateTime.UtcNow,
                    PlanJson = planJson
                };

                _logger.LogInformation("Interview flow created for user {UserId}", userId);
                return interviewFlow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating interview flow for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Generates the next interview prompt based on conversation history
        /// </summary>
        public async Task<string> GenerateNextPromptAsync(string sessionId, string previousResponse)
        {
            try
            {
                // Get or create session
                if (!_sessions.TryGetValue(sessionId, out var session))
                {
                    session = new InterviewSession
                    {
                        SessionId = sessionId,
                        StartedAt = DateTime.UtcNow,
                        Responses = new List<string>()
                    };
                    _sessions[sessionId] = session;
                }

                // Store the previous response
                if (!string.IsNullOrEmpty(previousResponse))
                {
                    session.Responses.Add(previousResponse);
                }

                var currentStep = session.Responses.Count + 1;
                var conversationHistory = string.Join("\n", session.Responses.Select((r, i) => $"Response {i + 1}: {r}"));

                // Generate contextual prompt using Semantic Kernel
                var promptFunction = _kernel.CreateFunctionFromPrompt(
                    """
                    You are generating an interview question for a personality assessment. This is step {{$step}} of 5.
                    
                    Previous responses from the interviewee:
                    {{$history}}
                    
                    Based on their previous responses and the interview stage, generate a specific, engaging question that:
                    
                    Step 1: Explores core motivations and what drives them
                    Step 2: Investigates values and decision-making style  
                    Step 3: Examines how they handle challenges and problems
                    Step 4: Discovers goals, aspirations, and future vision
                    Step 5: Summarizes and seeks final insights about their essence
                    
                    Guidelines:
                    - Be conversational and warm
                    - Ask ONE focused question
                    - Build on their previous responses naturally
                    - Keep it under 25 words
                    - Make it thought-provoking but not overwhelming
                    
                    Return ONLY the question, no additional text.
                    """,
                    new OpenAIPromptExecutionSettings
                    {
                        Temperature = 0.8,
                        MaxTokens = 100
                    }
                );

                var promptResult = await promptFunction.InvokeAsync(_kernel, new()
                {
                    ["step"] = currentStep.ToString(),
                    ["history"] = conversationHistory
                });

                var nextPrompt = promptResult.ToString().Trim();
                
                _logger.LogInformation("Generated prompt for session {SessionId}, step {Step}", sessionId, currentStep);
                return nextPrompt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating prompt for session {SessionId}", sessionId);
                return "That's interesting! Could you tell me more about what motivates you?";
            }
        }

        /// <summary>
        /// Stores user response and updates conversation memory
        /// </summary>
        public async Task StoreUserResponseAsync(string sessionId, string response)
        {
            try
            {
                if (_sessions.TryGetValue(sessionId, out var session))
                {
                    session.Responses.Add(response);
                    session.LastUpdated = DateTime.UtcNow;
                    
                    // Use Semantic Kernel to extract key insights for memory
                    var insightFunction = _kernel.CreateFunctionFromPrompt(
                        """
                        Extract 2-3 key personality insights from this response:
                        
                        "{{$response}}"
                        
                        Focus on:
                        - Communication style
                        - Values and motivations  
                        - Problem-solving approach
                        - Emotional intelligence indicators
                        
                        Return as brief bullet points (max 15 words each).
                        """,
                        new OpenAIPromptExecutionSettings
                        {
                            Temperature = 0.3,
                            MaxTokens = 150
                        }
                    );

                    var insights = await insightFunction.InvokeAsync(_kernel, new()
                    {
                        ["response"] = response
                    });

                    session.Insights.Add(insights.ToString());
                    
                    _logger.LogInformation("Stored response and insights for session {SessionId}", sessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing response for session {SessionId}", sessionId);
            }
        }

        /// <summary>
        /// Extracts comprehensive persona traits using Semantic Kernel analysis
        /// </summary>
        public async Task<PersonaTraits> ExtractPersonaTraitsAsync(string sessionId, string finalInsights)
        {
            try
            {
                if (!_sessions.TryGetValue(sessionId, out var session))
                {
                    throw new InvalidOperationException($"Session {sessionId} not found");
                }

                var allResponses = string.Join("\n\n", session.Responses);
                var allInsights = string.Join("\n", session.Insights);

                // Use Semantic Kernel for comprehensive personality analysis
                var analysisFunction = _kernel.CreateFunctionFromPrompt(
                    """
                    Analyze this complete interview conversation and extract a comprehensive personality profile.
                    
                    INTERVIEW RESPONSES:
                    {{$responses}}
                    
                    EXTRACTED INSIGHTS:
                    {{$insights}}
                    
                    FINAL AI ANALYSIS:
                    {{$finalInsights}}
                    
                    Based on this complete picture, determine:
                    
                    1. PRIMARY PERSONALITY TYPE (choose one):
                       - Analytical Thinker
                       - Creative Innovator  
                       - Collaborative Builder
                       - Strategic Leader
                       - Empathetic Supporter
                    
                    2. COMMUNICATION STYLE (choose one):
                       - Direct and Concise
                       - Detailed and Thorough
                       - Warm and Personal
                       - Strategic and Visionary
                       - Supportive and Encouraging
                    
                    3. PROBLEM-SOLVING APPROACH (choose one):
                       - Data-Driven Analysis
                       - Creative Brainstorming
                       - Collaborative Discussion
                       - Strategic Planning
                       - Intuitive Assessment
                    
                    4. PRIMARY MOTIVATION (choose one):
                       - Achievement and Excellence
                       - Innovation and Creativity
                       - Connection and Relationships
                       - Impact and Leadership
                       - Growth and Learning
                    
                    5. WORK STYLE PREFERENCE (choose one):
                       - Independent Deep Work
                       - Creative Collaboration
                       - Team Leadership
                       - Supportive Partnership
                       - Strategic Overview
                    
                    Return as JSON with these exact fields:
                    {
                      "personalityType": "",
                      "communicationStyle": "",
                      "problemSolvingApproach": "",
                      "primaryMotivation": "",
                      "workStylePreference": "",
                      "keyStrengths": ["", "", ""],
                      "summary": "2-sentence personality summary"
                    }
                    """,
                    new OpenAIPromptExecutionSettings
                    {
                        Temperature = 0.3,
                        MaxTokens = 500,
                        ResponseFormat = "json_object"
                    }
                );

                var analysisResult = await analysisFunction.InvokeAsync(_kernel, new()
                {
                    ["responses"] = allResponses,
                    ["insights"] = allInsights,
                    ["finalInsights"] = finalInsights
                });

                var traitsJson = analysisResult.ToString();
                var traits = System.Text.Json.JsonSerializer.Deserialize<PersonaTraits>(traitsJson);
                
                // Store the completed analysis
                session.PersonaTraits = traits;
                session.CompletedAt = DateTime.UtcNow;
                
                _logger.LogInformation("Extracted persona traits for session {SessionId}: {PersonalityType}", 
                    sessionId, traits?.PersonalityType);
                
                return traits ?? new PersonaTraits();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting persona traits for session {SessionId}", sessionId);
                throw;
            }
        }

        /// <summary>
        /// Gets interview session analytics and insights
        /// </summary>
        public async Task<InterviewAnalytics> GetSessionAnalyticsAsync(string sessionId)
        {
            try
            {
                if (!_sessions.TryGetValue(sessionId, out var session))
                {
                    throw new InvalidOperationException($"Session {sessionId} not found");
                }

                var analytics = new InterviewAnalytics
                {
                    SessionId = sessionId,
                    StartedAt = session.StartedAt,
                    CompletedAt = session.CompletedAt,
                    Duration = session.CompletedAt?.Subtract(session.StartedAt),
                    ResponseCount = session.Responses.Count,
                    AverageResponseLength = session.Responses.Count > 0 ? 
                        session.Responses.Average(r => r.Length) : 0,
                    PersonaTraits = session.PersonaTraits,
                    KeyInsights = session.Insights
                };

                _logger.LogInformation("Generated analytics for session {SessionId}", sessionId);
                return analytics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating analytics for session {SessionId}", sessionId);
                throw;
            }
        }

        /// <summary>
        /// Cleanup expired sessions
        /// </summary>
        public Task CleanupExpiredSessionsAsync()
        {
            try
            {
                var expiredSessions = _sessions
                    .Where(kvp => kvp.Value.LastUpdated < DateTime.UtcNow.AddHours(-24))
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var sessionId in expiredSessions)
                {
                    _sessions.Remove(sessionId);
                }

                _logger.LogInformation("Cleaned up {Count} expired sessions", expiredSessions.Count);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during session cleanup");
                return Task.CompletedTask;
            }
        }
    }

    // Supporting models
    public class InterviewSession
    {
        public string SessionId { get; set; } = "";
        public DateTime StartedAt { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<string> Responses { get; set; } = new();
        public List<string> Insights { get; set; } = new();
        public PersonaTraits? PersonaTraits { get; set; }
    }

    public class InterviewFlow
    {
        public string UserId { get; set; } = "";
        public string Goal { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string PlanJson { get; set; } = "";
    }

    public class PersonaTraits
    {
        public string PersonalityType { get; set; } = "";
        public string CommunicationStyle { get; set; } = "";
        public string ProblemSolvingApproach { get; set; } = "";
        public string PrimaryMotivation { get; set; } = "";
        public string WorkStylePreference { get; set; } = "";
        public List<string> KeyStrengths { get; set; } = new();
        public string Summary { get; set; } = "";
    }

    public class InterviewAnalytics
    {
        public string SessionId { get; set; } = "";
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TimeSpan? Duration { get; set; }
        public int ResponseCount { get; set; }
        public double AverageResponseLength { get; set; }
        public PersonaTraits? PersonaTraits { get; set; }
        public List<string> KeyInsights { get; set; } = new();
    }
}
```

### 2. Program.cs Configuration

**File:** `Program.cs` (Semantic Kernel setup)

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using SpinnerNet.Core.Services;

// Azure KeyVault client for secure API key retrieval
var keyVaultClient = new SecretClient(
    new Uri($"https://{builder.Configuration["Azure:KeyVault:Name"]}.vault.azure.net/"),
    new DefaultAzureCredential()
);

try
{
    // Retrieve OpenAI API key from Azure KeyVault
    var openAiKeySecret = await keyVaultClient.GetSecretAsync("OpenAI-API-Key");
    var openAiApiKey = openAiKeySecret.Value.Value;
    
    if (!string.IsNullOrEmpty(openAiApiKey))
    {
        // Configure Semantic Kernel with OpenAI
        var kernelBuilder = Kernel.CreateBuilder();
        
        // Add OpenAI Chat Completion service
        kernelBuilder.AddOpenAIChatCompletion(
            modelId: "gpt-4o-mini",
            apiKey: openAiApiKey
        );

        // Build the kernel
        var kernel = kernelBuilder.Build();
        
        // Register Semantic Kernel services
        builder.Services.AddSingleton(kernel);
        builder.Services.AddSingleton<IChatCompletionService>(provider => 
            provider.GetRequiredService<IKernel>()
                   .GetRequiredService<IChatCompletionService>());
        
        // Register AI Orchestration Service
        builder.Services.AddScoped<AIOrchestrationService>();
        
        Console.WriteLine("✅ Semantic Kernel configured with OpenAI");
    }
    else
    {
        throw new InvalidOperationException("OpenAI API key not found in KeyVault");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Failed to configure AI services: {ex.Message}");
    
    // Require proper AI configuration - no mock fallback
    Console.WriteLine("❌ No AI service configured! OpenAI API key is required. Check Azure KeyVault or environment variables.");
    throw new InvalidOperationException("AI service configuration required. Please ensure OpenAI API key is available in Azure KeyVault or environment variables.");
}
```

### 3. SignalR Hub Integration

**File:** `Hubs/AIHub.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SpinnerNet.Core.Services;

namespace SpinnerNet.App.Hubs
{
    /// <summary>
    /// SignalR Hub that bridges client-side WebLLM with server-side Semantic Kernel
    /// Provides real-time coordination between hybrid AI components
    /// </summary>
    [Authorize]
    public class AIHub : Hub
    {
        private readonly AIOrchestrationService _orchestrationService;
        private readonly ILogger<AIHub> _logger;

        public AIHub(
            AIOrchestrationService orchestrationService,
            ILogger<AIHub> logger)
        {
            _orchestrationService = orchestrationService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the next interview prompt based on conversation history
        /// Uses Semantic Kernel for contextual prompt generation
        /// </summary>
        public async Task<string> GetNextPrompt(string sessionId, string? previousResponse)
        {
            try
            {
                _logger.LogInformation("Getting next prompt for session {SessionId}", sessionId);

                // Store previous response if provided
                if (!string.IsNullOrEmpty(previousResponse))
                {
                    await _orchestrationService.StoreUserResponseAsync(sessionId, previousResponse);
                }

                // Generate next prompt using Semantic Kernel
                var nextPrompt = await _orchestrationService.GenerateNextPromptAsync(sessionId, previousResponse ?? string.Empty);
                
                _logger.LogInformation("Generated prompt for session {SessionId}: {PromptPreview}", 
                    sessionId, nextPrompt.Length > 50 ? nextPrompt[..50] + "..." : nextPrompt);
                
                return nextPrompt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating next prompt for session {SessionId}", sessionId);
                return "I'm having trouble generating the next question. Could you tell me more about what you're looking for?";
            }
        }

        /// <summary>
        /// Processes insights from the client-side WebLLM and extracts persona traits
        /// Uses Semantic Kernel for advanced personality analysis
        /// </summary>
        public async Task SaveInsights(string sessionId, string insights)
        {
            try
            {
                _logger.LogInformation("Processing insights for session {SessionId}", sessionId);
                
                // Extract persona traits using Semantic Kernel
                var traits = await _orchestrationService.ExtractPersonaTraitsAsync(sessionId, insights);
                
                // Send extracted traits back to the client
                await Clients.Caller.SendAsync("PersonaTraitsExtracted", traits);
                
                _logger.LogInformation("Persona traits extracted for session {SessionId}: {PersonalityType}", 
                    sessionId, traits.PersonalityType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing insights for session {SessionId}", sessionId);
                await Clients.Caller.SendAsync("Error", "Failed to process insights");
            }
        }

        /// <summary>
        /// Creates a new interview flow using Semantic Kernel planning
        /// </summary>
        public async Task<string> CreateInterviewFlow(string userId, string goal)
        {
            try
            {
                _logger.LogInformation("Creating interview flow for user {UserId} with goal: {Goal}", userId, goal);
                
                var flow = await _orchestrationService.CreateInterviewFlowAsync(userId, goal);
                
                _logger.LogInformation("Interview flow created for user {UserId}", userId);
                return flow.PlanJson;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating interview flow for user {UserId}", userId);
                await Clients.Caller.SendAsync("Error", "Failed to create interview flow");
                return "{}";
            }
        }

        /// <summary>
        /// Gets comprehensive analytics for a completed interview session
        /// </summary>
        public async Task<object> GetSessionAnalytics(string sessionId)
        {
            try
            {
                _logger.LogInformation("Getting analytics for session {SessionId}", sessionId);
                
                var analytics = await _orchestrationService.GetSessionAnalyticsAsync(sessionId);
                
                return new
                {
                    analytics.SessionId,
                    analytics.Duration,
                    analytics.ResponseCount,
                    analytics.AverageResponseLength,
                    PersonalityType = analytics.PersonaTraits?.PersonalityType,
                    Summary = analytics.PersonaTraits?.Summary,
                    KeyInsights = analytics.KeyInsights
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics for session {SessionId}", sessionId);
                await Clients.Caller.SendAsync("Error", "Failed to retrieve session analytics");
                return new { };
            }
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("✅ Client connected to AI Hub: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception != null)
            {
                _logger.LogWarning("❌ Client disconnected with error: {ConnectionId}, {Error}", 
                    Context.ConnectionId, exception.Message);
            }
            else
            {
                _logger.LogInformation("✅ Client disconnected cleanly: {ConnectionId}", Context.ConnectionId);
            }
            
            await base.OnDisconnectedAsync(exception);
        }
    }
}
```

## Implementation Steps

### Step 1: Install Semantic Kernel Packages

```xml
<!-- In SpinnerNet.Core.csproj -->
<PackageReference Include="Microsoft.SemanticKernel" Version="1.4.0" />
<PackageReference Include="Microsoft.SemanticKernel.Connectors.OpenAI" Version="1.4.0" />
```

### Step 2: Configure Dependency Injection

```csharp
// Add Semantic Kernel services
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-4o-mini", apiKey)
    .Build();

builder.Services.AddSingleton(kernel);
builder.Services.AddScoped<AIOrchestrationService>();
```

### Step 3: Create AI Functions

```csharp
// Define semantic functions
var promptFunction = kernel.CreateFunctionFromPrompt(
    "Generate an interview question about {{$topic}}",
    new OpenAIPromptExecutionSettings
    {
        Temperature = 0.7,
        MaxTokens = 100
    }
);

// Execute the function
var result = await promptFunction.InvokeAsync(kernel, new() 
{ 
    ["topic"] = "communication style" 
});
```

### Step 4: Implement Memory Management

```csharp
// Store conversation context
await kernel.Memory.SaveInformationAsync(
    "interview-sessions",
    userResponse,
    $"{sessionId}-{DateTime.UtcNow.Ticks}"
);

// Retrieve relevant context
var memories = await kernel.Memory.SearchAsync(
    "interview-sessions", 
    sessionId, 
    limit: 5
);
```

## Advanced Features

### Planning with Semantic Kernel

```csharp
public async Task<InterviewPlan> CreateInterviewPlanAsync(string goal)
{
    // Use Semantic Kernel Planner for complex workflows
    var planner = new SequentialPlanner(kernel);
    
    var plan = await planner.CreatePlanAsync($"""
        Create a 5-step personality interview plan for: {goal}
        
        Steps should include:
        1. Opening rapport building
        2. Core values exploration  
        3. Decision-making analysis
        4. Future goals assessment
        5. Summary and insights
        """);
    
    return new InterviewPlan
    {
        Steps = plan.Steps.Select(s => new InterviewStep
        {
            Description = s.Description,
            SkillName = s.SkillName,
            Parameters = s.Parameters
        }).ToList()
    };
}
```

### Function Calling

```csharp
// Define native functions for Semantic Kernel
[KernelFunction]
[Description("Analyzes personality traits from interview responses")]
public async Task<PersonaTraits> AnalyzePersonality(
    [Description("Interview responses")] string responses,
    [Description("Analysis depth")] string depth = "detailed")
{
    // Advanced personality analysis logic
    return new PersonaTraits();
}

// Register with kernel
kernel.Plugins.AddFromType<PersonalityAnalyzer>();
```

### Prompt Templates

```csharp
// Create reusable prompt templates
var interviewTemplate = """
    You are conducting a {{$interviewType}} interview.
    Current step: {{$step}} of {{$totalSteps}}
    
    Previous responses:
    {{$history}}
    
    Generate the next question focusing on {{$focusArea}}.
    
    Guidelines:
    - Be conversational and warm
    - Build on previous responses
    - Keep under {{$maxWords}} words
    - Focus on {{$personality_aspect}}
    
    Question:
    """;

var function = kernel.CreateFunctionFromPrompt(interviewTemplate);
```

## Performance Optimization

### Caching Strategies

```csharp
// Cache frequently used prompts
private readonly MemoryCache _promptCache = new MemoryCache(new MemoryCacheOptions
{
    SizeLimit = 100
});

public async Task<string> GetCachedPrompt(string cacheKey, Func<Task<string>> generator)
{
    if (_promptCache.TryGetValue(cacheKey, out string cachedPrompt))
    {
        return cachedPrompt;
    }
    
    var prompt = await generator();
    _promptCache.Set(cacheKey, prompt, TimeSpan.FromMinutes(10));
    return prompt;
}
```

### Batch Processing

```csharp
// Process multiple insights in batch
public async Task<List<PersonaTraits>> ExtractMultipleTraitsAsync(
    Dictionary<string, string> sessionInsights)
{
    var tasks = sessionInsights.Select(async kvp =>
    {
        return new
        {
            SessionId = kvp.Key,
            Traits = await ExtractPersonaTraitsAsync(kvp.Key, kvp.Value)
        };
    });
    
    var results = await Task.WhenAll(tasks);
    return results.Select(r => r.Traits).ToList();
}
```

## Testing and Monitoring

### Logging Integration

```csharp
// Add detailed logging for SK operations
public async Task<string> GeneratePromptWithLogging(string sessionId, string input)
{
    using var activity = _logger.BeginScope("GeneratePrompt-{SessionId}", sessionId);
    
    var stopwatch = Stopwatch.StartNew();
    
    try
    {
        var result = await _kernel.InvokeAsync(promptFunction, new() { ["input"] = input });
        
        stopwatch.Stop();
        _logger.LogInformation("✅ Prompt generated in {ElapsedMs}ms for session {SessionId}", 
            stopwatch.ElapsedMilliseconds, sessionId);
        
        return result.ToString();
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        _logger.LogError(ex, "❌ Prompt generation failed after {ElapsedMs}ms for session {SessionId}", 
            stopwatch.ElapsedMilliseconds, sessionId);
        throw;
    }
}
```

### Unit Testing

```csharp
[Test]
public async Task GenerateNextPrompt_ShouldReturnContextualQuestion()
{
    // Arrange
    var mockKernel = new Mock<IKernel>();
    var service = new AIOrchestrationService(mockKernel.Object, null, null);
    
    // Act
    var prompt = await service.GenerateNextPromptAsync("test-session", "I love solving problems");
    
    // Assert
    Assert.That(prompt, Is.Not.Empty);
    Assert.That(prompt.Length, Is.LessThan(200));
}
```

## Production Considerations

### Error Handling

```csharp
public async Task<string> GeneratePromptWithFallback(string sessionId, string input)
{
    try
    {
        return await GenerateNextPromptAsync(sessionId, input);
    }
    catch (OpenAIException ex) when (ex.Message.Contains("rate limit"))
    {
        // Handle rate limiting
        await Task.Delay(1000);
        return await GeneratePromptWithFallback(sessionId, input);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Falling back to static prompt for session {SessionId}", sessionId);
        return GetStaticFallbackPrompt(sessionId);
    }
}
```

### Resource Management

```csharp
// Implement proper disposal
public class AIOrchestrationService : IDisposable
{
    public void Dispose()
    {
        _kernel?.Dispose();
        _sessions.Clear();
    }
}
```

## Results

✅ **Enterprise orchestration** - Complex AI workflow management  
✅ **Memory management** - Persistent conversation context  
✅ **Advanced planning** - Multi-step reasoning capabilities  
✅ **OpenAI integration** - GPT-4o-mini for high-quality responses  
✅ **Real-time coordination** - Seamless client-server AI collaboration  

**Live Demo:** https://spinnernet-app-3lauxg.azurewebsites.net/ai-interview-hybrid

The Semantic Kernel orchestration provides enterprise-grade AI workflow management that perfectly complements the ultra-fast client-side WebLLM, creating a powerful hybrid intelligence system.