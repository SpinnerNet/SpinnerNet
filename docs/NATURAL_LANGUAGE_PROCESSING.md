# Natural Language Processing Guide - Spinner.Net Sprint 1

## Overview

This guide covers natural language processing for Sprint 1 Zeitsparkasse features. You'll implement AI-powered task parsing, persona interviews, and buddy conversations using structured prompts and entity extraction.

**Goal**: Transform natural language like "Remind me to call mom tomorrow at 7pm" into structured tasks with dates, priorities, and categories.

## Core NLP Features for Sprint 1

### 1. Natural Language Task Creation
- Parse user input into structured task data
- Extract dates, times, priorities, and categories
- Handle various input formats and languages

### 2. Persona Interview Processing
- Generate contextual follow-up questions
- Extract personality insights from responses
- Build comprehensive user profiles

### 3. AI Buddy Conversations
- Context-aware responses based on user tasks
- Personality-driven communication styles
- Proactive task management suggestions

## Task Parsing Implementation

### Core Task Parser Service

**Create `Services/NaturalLanguageTaskParser.cs`:**

```csharp
using SpinnerNet.Core.AI;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SpinnerNet.Core.Services;

public interface INaturalLanguageTaskParser
{
    Task<TaskParseResult> ParseTaskAsync(
        string userInput, 
        string userId, 
        UserContext context,
        CancellationToken cancellationToken = default);
}

public class NaturalLanguageTaskParser : INaturalLanguageTaskParser
{
    private readonly IAiService _aiService;
    private readonly ILogger<NaturalLanguageTaskParser> _logger;
    
    public NaturalLanguageTaskParser(IAiService aiService, ILogger<NaturalLanguageTaskParser> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<TaskParseResult> ParseTaskAsync(
        string userInput, 
        string userId, 
        UserContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // First, try rule-based parsing for simple cases
            var quickParse = TryQuickParse(userInput, context);
            if (quickParse.Confidence > 0.8)
            {
                _logger.LogDebug("Quick parse successful for user {UserId}", userId);
                return quickParse;
            }

            // Fall back to AI parsing for complex cases
            return await ParseWithAiAsync(userInput, userId, context, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing task for user {UserId}: {Input}", userId, userInput);
            
            // Return fallback result
            return new TaskParseResult
            {
                Title = userInput.Trim(),
                Description = userInput,
                Priority = TaskPriority.Medium,
                Category = TaskCategory.Personal,
                EstimatedMinutes = 30,
                Confidence = 0.3,
                ParseMethod = "fallback"
            };
        }
    }

    private TaskParseResult TryQuickParse(string input, UserContext context)
    {
        var result = new TaskParseResult
        {
            Description = input,
            ParseMethod = "rule_based"
        };

        // Extract and clean title
        result.Title = ExtractTitle(input);
        
        // Extract due date/time
        result.DueDate = ExtractDateTime(input, context.Timezone);
        
        // Extract priority
        result.Priority = ExtractPriority(input);
        
        // Extract category
        result.Category = ExtractCategory(input);
        
        // Estimate time
        result.EstimatedMinutes = EstimateTimeFromInput(input);
        
        // Extract tags
        result.Tags = ExtractTags(input);
        
        // Calculate confidence based on extracted elements
        result.Confidence = CalculateRuleBasedConfidence(result, input);
        
        return result;
    }

    private async Task<TaskParseResult> ParseWithAiAsync(
        string userInput, 
        string userId, 
        UserContext context,
        CancellationToken cancellationToken)
    {
        var systemPrompt = $"""
            You are an expert task parser for a time management system.
            
            User context:
            - Timezone: {context.Timezone}
            - Current time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
            - Language: {context.Language}
            - User preferences: {JsonSerializer.Serialize(context.Preferences)}
            
            Parse natural language input into structured task data.
            
            Categories: work, personal, health, family, learning, creative, social, finance, shopping, travel
            Priorities: low, medium, high, urgent
            
            Extract these elements:
            - Clean, actionable title (remove "remind me", "I need to", etc.)
            - Due date/time (convert to UTC ISO format or null)
            - Priority based on urgency words and context
            - Category based on content and keywords
            - Estimated time in minutes
            - Relevant tags
            - Confidence (0.0-1.0) based on clarity of input
            """;

        var prompt = $"""
            Parse this task input: "{userInput}"
            
            Respond with JSON only:
            {{
                "title": "Clean, actionable task title",
                "description": "{userInput}",
                "dueDate": "2024-01-15T14:00:00Z or null",
                "priority": "low|medium|high|urgent", 
                "category": "work|personal|health|family|learning|creative|social|finance|shopping|travel",
                "estimatedMinutes": 30,
                "tags": ["tag1", "tag2"],
                "confidence": 0.95,
                "extractedEntities": {{
                    "timeReferences": ["tomorrow", "7pm"],
                    "people": ["mom"],
                    "locations": [],
                    "urgencyIndicators": ["remind me"]
                }}
            }}
            """;

        var aiContext = new AiContext
        {
            UserId = userId,
            SystemPrompt = systemPrompt,
            PreferredProvider = AiProvider.LocalFirst, // Privacy-sensitive personal tasks
            RequiresPrivacy = true,
            MaxTokens = 1024
        };

        var jsonResponse = await _aiService.GenerateResponseAsync(prompt, aiContext, cancellationToken);
        
        try
        {
            var parsed = JsonSerializer.Deserialize<TaskParseResult>(jsonResponse);
            parsed.ParseMethod = "ai_powered";
            
            _logger.LogDebug("AI parse successful for user {UserId}, confidence: {Confidence}", 
                userId, parsed.Confidence);
                
            return parsed;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse AI response as JSON: {Response}", jsonResponse);
            throw new InvalidOperationException("AI returned invalid JSON response", ex);
        }
    }
}
```

### Rule-Based Parsing Helpers

**Time and Date Extraction:**

```csharp
private DateTime? ExtractDateTime(string input, string timezone)
{
    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
    var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
    
    // Common time patterns
    var patterns = new Dictionary<string, Func<DateTime>>
    {
        // Relative dates
        { @"\btomorrow\b", () => now.AddDays(1) },
        { @"\btoday\b", () => now },
        { @"\bnext week\b", () => now.AddDays(7) },
        { @"\bnext month\b", () => now.AddMonths(1) },
        
        // Day names
        { @"\bmonday\b", () => GetNextWeekday(now, DayOfWeek.Monday) },
        { @"\btuesday\b", () => GetNextWeekday(now, DayOfWeek.Tuesday) },
        { @"\bwednesday\b", () => GetNextWeekday(now, DayOfWeek.Wednesday) },
        { @"\bthursday\b", () => GetNextWeekday(now, DayOfWeek.Thursday) },
        { @"\bfriday\b", () => GetNextWeekday(now, DayOfWeek.Friday) },
        { @"\bsaturday\b", () => GetNextWeekday(now, DayOfWeek.Saturday) },
        { @"\bsunday\b", () => GetNextWeekday(now, DayOfWeek.Sunday) }
    };
    
    DateTime? baseDate = null;
    
    // Find date
    foreach (var pattern in patterns)
    {
        if (Regex.IsMatch(input, pattern.Key, RegexOptions.IgnoreCase))
        {
            baseDate = pattern.Value();
            break;
        }
    }
    
    // Extract time
    var timePattern = @"\b(\d{1,2}):?(\d{2})?\s*(am|pm|AM|PM)?\b";
    var timeMatch = Regex.Match(input, timePattern);
    
    if (timeMatch.Success && baseDate.HasValue)
    {
        var hour = int.Parse(timeMatch.Groups[1].Value);
        var minute = timeMatch.Groups[2].Success ? int.Parse(timeMatch.Groups[2].Value) : 0;
        var ampm = timeMatch.Groups[3].Value.ToLowerInvariant();
        
        if (ampm == "pm" && hour != 12)
            hour += 12;
        else if (ampm == "am" && hour == 12)
            hour = 0;
            
        var dateTime = baseDate.Value.Date.AddHours(hour).AddMinutes(minute);
        return TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZone);
    }
    
    return baseDate.HasValue ? TimeZoneInfo.ConvertTimeToUtc(baseDate.Value, timeZone) : null;
}

private DateTime GetNextWeekday(DateTime from, DayOfWeek dayOfWeek)
{
    var daysUntil = ((int)dayOfWeek - (int)from.DayOfWeek + 7) % 7;
    if (daysUntil == 0) daysUntil = 7; // Next week, not today
    return from.AddDays(daysUntil);
}
```

**Priority and Category Extraction:**

```csharp
private TaskPriority ExtractPriority(string input)
{
    var lowPriorityWords = new[] { "maybe", "sometime", "eventually", "when i have time", "low priority" };
    var highPriorityWords = new[] { "urgent", "asap", "immediately", "critical", "important", "high priority" };
    var urgentWords = new[] { "now", "right now", "emergency", "urgent" };
    
    var lowerInput = input.ToLowerInvariant();
    
    if (urgentWords.Any(word => lowerInput.Contains(word)))
        return TaskPriority.Urgent;
        
    if (highPriorityWords.Any(word => lowerInput.Contains(word)))
        return TaskPriority.High;
        
    if (lowPriorityWords.Any(word => lowerInput.Contains(word)))
        return TaskPriority.Low;
        
    return TaskPriority.Medium;
}

private TaskCategory ExtractCategory(string input)
{
    var categoryKeywords = new Dictionary<TaskCategory, string[]>
    {
        { TaskCategory.Work, new[] { "work", "job", "office", "meeting", "project", "deadline", "boss", "colleague" } },
        { TaskCategory.Health, new[] { "doctor", "dentist", "exercise", "gym", "medication", "health", "appointment" } },
        { TaskCategory.Family, new[] { "mom", "dad", "parent", "child", "family", "wedding", "birthday", "anniversary" } },
        { TaskCategory.Personal, new[] { "personal", "self", "hobby", "relax", "read", "journal" } },
        { TaskCategory.Shopping, new[] { "buy", "shop", "store", "grocery", "purchase", "order" } },
        { TaskCategory.Finance, new[] { "bank", "pay", "bill", "money", "budget", "tax", "investment" } },
        { TaskCategory.Learning, new[] { "learn", "study", "course", "book", "research", "tutorial" } },
        { TaskCategory.Creative, new[] { "create", "write", "draw", "design", "music", "art", "paint" } },
        { TaskCategory.Social, new[] { "friend", "party", "social", "dinner", "lunch", "hangout", "date" } }
    };
    
    var lowerInput = input.ToLowerInvariant();
    
    foreach (var category in categoryKeywords)
    {
        if (category.Value.Any(keyword => lowerInput.Contains(keyword)))
        {
            return category.Key;
        }
    }
    
    return TaskCategory.Personal; // Default
}
```

## Persona Interview NLP

### Interview Question Generation

**PersonaInterviewProcessor.cs:**

```csharp
public class PersonaInterviewProcessor
{
    private readonly IAiService _aiService;
    
    public async Task<InterviewQuestion> GenerateNextQuestionAsync(
        List<InterviewResponse> previousResponses,
        UserContext context,
        CancellationToken cancellationToken = default)
    {
        var conversationAnalysis = AnalyzeConversationFlow(previousResponses);
        
        var systemPrompt = $"""
            You are conducting a friendly persona discovery interview.
            Goal: Build a comprehensive understanding of the user for personalized AI assistance.
            
            Interview progress: {previousResponses.Count}/8 questions
            Language: {context.Language}
            
            Areas to explore:
            - Interests and hobbies
            - Work style and environment  
            - Goals and aspirations
            - Communication preferences
            - Time management challenges
            - Learning style
            - Social preferences
            - Stress management
            
            Guidelines:
            - One question at a time
            - Build on previous responses
            - Be warm and conversational
            - Avoid repetitive topics
            - Keep questions open-ended
            - Show genuine interest
            """;
            
        var conversationHistory = BuildConversationSummary(previousResponses);
        var areasToExplore = GetUnexploredAreas(conversationAnalysis);
        
        var prompt = $"""
            Conversation so far:
            {conversationHistory}
            
            Areas still to explore: {string.Join(", ", areasToExplore)}
            
            Generate the next interview question. Make it:
            - Personal and engaging
            - Building on what they've shared
            - Exploring new aspects of their personality
            - Natural and conversational
            
            Just return the question, nothing else.
            """;
            
        var aiContext = new AiContext
        {
            UserId = context.UserId,
            SystemPrompt = systemPrompt,
            PreferredProvider = AiProvider.LocalFirst,
            RequiresPrivacy = true,
            MaxTokens = 256
        };
        
        var question = await _aiService.GenerateResponseAsync(prompt, aiContext, cancellationToken);
        
        return new InterviewQuestion
        {
            Text = question.Trim(),
            QuestionNumber = previousResponses.Count + 1,
            FocusArea = DetermineFocusArea(question, areasToExplore),
            IsComplete = previousResponses.Count >= 7
        };
    }
}
```

### Response Analysis and Insight Extraction

**Persona Insights Generator:**

```csharp
public async Task<PersonaInsights> ExtractInsightsAsync(
    List<InterviewResponse> responses,
    UserContext context,
    CancellationToken cancellationToken = default)
{
    var systemPrompt = """
        Analyze interview responses to create a comprehensive user persona.
        
        Extract insights about:
        - Core interests and passions
        - Professional goals and work style
        - Communication preferences
        - Learning style and preferences
        - Time management challenges
        - Social interaction style
        - Stress management approach
        - Technology comfort level
        
        Provide actionable insights for AI personalization.
        """;
        
    var allResponses = string.Join("\n\n", responses.Select(r => 
        $"Q: {r.Question}\nA: {r.Answer}"));
        
    var prompt = $"""
        Interview responses:
        {allResponses}
        
        Create a comprehensive persona analysis in JSON:
        {{
            "corePersonality": {{
                "traits": ["trait1", "trait2", "trait3"],
                "workStyle": "focused|collaborative|flexible|detail-oriented",
                "communicationStyle": "direct|diplomatic|casual|formal",
                "learningStyle": "visual|auditory|kinesthetic|reading",
                "socialStyle": "introverted|extroverted|ambivert"
            }},
            "interests": {{
                "primary": ["interest1", "interest2"],
                "secondary": ["interest3", "interest4"],
                "hobbies": ["hobby1", "hobby2"]
            }},
            "goals": {{
                "short_term": ["goal1", "goal2"],
                "long_term": ["goal3", "goal4"],
                "areas_of_focus": ["area1", "area2"]
            }},
            "preferences": {{
                "communication_tone": "friendly|professional|casual|encouraging",
                "notification_frequency": "minimal|moderate|frequent",
                "ui_complexity": "simple|standard|advanced",
                "feedback_style": "gentle|direct|detailed"
            }},
            "challenges": {{
                "time_management": ["challenge1", "challenge2"],
                "work_life_balance": ["challenge3"],
                "stress_triggers": ["trigger1", "trigger2"]
            }},
            "ai_assistant_preferences": {{
                "proactiveness": 0.7,
                "formality": 0.3,
                "verbosity": 0.5,
                "encouragement_level": 0.8
            }},
            "confidence_score": 0.92
        }}
        """;
        
    var aiContext = new AiContext
    {
        UserId = context.UserId,
        SystemPrompt = systemPrompt,
        PreferredProvider = AiProvider.LocalFirst,
        RequiresPrivacy = true,
        MaxTokens = 2048
    };
    
    var jsonResponse = await _aiService.GenerateResponseAsync(prompt, aiContext, cancellationToken);
    
    return JsonSerializer.Deserialize<PersonaInsights>(jsonResponse);
}
```

## AI Buddy Conversation Processing

### Context-Aware Response Generation

**BuddyConversationProcessor.cs:**

```csharp
public class BuddyConversationProcessor
{
    private readonly IAiService _aiService;
    private readonly ITaskRepository _taskRepository;
    
    public async Task<BuddyResponse> GenerateResponseAsync(
        string userMessage,
        BuddyContext buddyContext,
        CancellationToken cancellationToken = default)
    {
        // Get current task context
        var taskContext = await BuildTaskContextAsync(buddyContext.UserId);
        
        var systemPrompt = BuildBuddySystemPrompt(buddyContext, taskContext);
        
        var conversationHistory = BuildConversationHistory(buddyContext.RecentMessages);
        
        var prompt = $"""
            Recent conversation:
            {conversationHistory}
            
            Current user tasks context:
            {taskContext}
            
            User message: "{userMessage}"
            
            Respond as {buddyContext.BuddyName} with personality: {buddyContext.Personality}
            
            Guidelines:
            - Reference their tasks when relevant
            - Offer helpful suggestions
            - Match their communication style
            - Be supportive and encouraging
            - Keep responses conversational and brief
            - Suggest actionable next steps when appropriate
            """;
            
        var aiContext = new AiContext
        {
            UserId = buddyContext.UserId,
            SystemPrompt = systemPrompt,
            PreferredProvider = AiProvider.LocalFirst,
            RequiresPrivacy = true,
            MaxTokens = 512
        };
        
        var response = await _aiService.GenerateResponseAsync(prompt, aiContext, cancellationToken);
        
        return new BuddyResponse
        {
            Message = response.Trim(),
            Emotion = DetermineResponseEmotion(response),
            SuggestedActions = ExtractSuggestedActions(response, taskContext),
            Confidence = 0.9,
            ContextUsed = taskContext.HasActiveTasks
        };
    }
    
    private async Task<TaskContext> BuildTaskContextAsync(string userId)
    {
        var recentTasks = await _taskRepository.GetRecentTasksAsync(userId, 10);
        var overdueTasks = recentTasks.Where(t => t.DueDate < DateTime.UtcNow && !t.IsCompleted).ToList();
        var todayTasks = recentTasks.Where(t => t.DueDate?.Date == DateTime.Today).ToList();
        
        return new TaskContext
        {
            TotalActiveTasks = recentTasks.Count(t => !t.IsCompleted),
            OverdueCount = overdueTasks.Count,
            DueTodayCount = todayTasks.Count,
            RecentActivity = BuildRecentActivitySummary(recentTasks),
            HasActiveTasks = recentTasks.Any(t => !t.IsCompleted)
        };
    }
}
```

## Prompt Engineering Best Practices

### Effective Prompt Structure

**1. Clear Role Definition:**
```csharp
var systemPrompt = """
    You are an expert task parser for a personal productivity system.
    Your role is to extract structured data from natural language input.
    """;
```

**2. Context Provision:**
```csharp
var contextPrompt = $"""
    User context:
    - Current time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
    - Timezone: {userTimezone}
    - Language: {userLanguage}
    - Previous tasks: {recentTasksSummary}
    """;
```

**3. Clear Output Format:**
```csharp
var outputPrompt = """
    Respond with valid JSON only:
    {
        "property": "value",
        "confidence": 0.95
    }
    """;
```

### Multi-Language Support

**Language-Aware Prompts:**

```csharp
private string GetLocalizedSystemPrompt(string language)
{
    return language switch
    {
        "de" => """
            Sie sind ein Experte für die Verarbeitung natürlicher Sprache.
            Analysieren Sie die deutsche Eingabe und extrahieren Sie Aufgabeninformationen.
            """,
        "es" => """
            Eres un experto en procesamiento de lenguaje natural.
            Analiza la entrada en español y extrae información de tareas.
            """,
        _ => """
            You are an expert natural language processor.
            Analyze the English input and extract task information.
            """
    };
}
```

## Error Handling and Fallbacks

### Graceful Degradation

```csharp
public async Task<TaskParseResult> ParseWithFallbackAsync(
    string input, 
    UserContext context)
{
    try
    {
        // Try AI parsing first
        return await ParseWithAiAsync(input, context);
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
        _logger.LogWarning(ex, "AI parsing failed, falling back to rule-based");
        
        try
        {
            // Fall back to rule-based parsing
            return TryQuickParse(input, context);
        }
        catch (Exception fallbackEx)
        {
            _logger.LogError(fallbackEx, "All parsing methods failed");
            
            // Ultimate fallback: create basic task
            return new TaskParseResult
            {
                Title = input.Trim(),
                Description = input,
                Category = TaskCategory.Personal,
                Priority = TaskPriority.Medium,
                EstimatedMinutes = 30,
                Confidence = 0.1,
                ParseMethod = "fallback_minimal"
            };
        }
    }
}
```

### Confidence Scoring

```csharp
private double CalculateParsingConfidence(TaskParseResult result, string originalInput)
{
    var score = 0.0;
    
    // Title quality (0-0.3)
    if (!string.IsNullOrEmpty(result.Title) && result.Title != originalInput)
        score += 0.3;
    else if (!string.IsNullOrEmpty(result.Title))
        score += 0.1;
    
    // Date extraction (0-0.3)
    if (result.DueDate.HasValue)
        score += 0.3;
    
    // Category classification (0-0.2)
    if (result.Category != TaskCategory.Personal) // Non-default category
        score += 0.2;
    
    // Priority detection (0-0.1)
    if (result.Priority != TaskPriority.Medium) // Non-default priority
        score += 0.1;
    
    // Time estimation (0-0.1)
    if (result.EstimatedMinutes != 30) // Non-default estimate
        score += 0.1;
    
    return Math.Min(score, 1.0);
}
```

## Testing NLP Components

### Unit Tests for Task Parsing

```csharp
[Theory]
[InlineData("Call mom tomorrow at 7pm", "Call mom", TaskPriority.Medium)]
[InlineData("URGENT: Fix the server issue", "Fix the server issue", TaskPriority.Urgent)]
[InlineData("Maybe read a book sometime", "Read a book", TaskPriority.Low)]
public async Task ParseTask_ExtractsPriorityCorrectly(
    string input, 
    string expectedTitle, 
    TaskPriority expectedPriority)
{
    // Arrange
    var context = new UserContext 
    { 
        Timezone = "UTC", 
        Language = "en" 
    };
    
    // Act
    var result = await _parser.ParseTaskAsync(input, "user123", context);
    
    // Assert
    result.Title.Should().Be(expectedTitle);
    result.Priority.Should().Be(expectedPriority);
    result.Confidence.Should().BeGreaterThan(0.5);
}
```

### Integration Tests with AI

```csharp
[Fact]
public async Task ParseComplexTask_WithRealAI_ReturnsStructuredData()
{
    // Arrange
    var complexInput = "I need to schedule a dentist appointment for next Tuesday afternoon and also remind me to buy groceries on the way home";
    
    // Act
    var result = await _parser.ParseTaskAsync(complexInput, "user123", _context);
    
    // Assert
    result.Should().NotBeNull();
    result.Title.Should().NotBe(complexInput); // Should be cleaned up
    result.DueDate.Should().BeCloseTo(GetNextTuesday(), TimeSpan.FromHours(6));
    result.Category.Should().BeOneOf(TaskCategory.Health, TaskCategory.Personal);
    result.Confidence.Should().BeGreaterThan(0.7);
}
```

## Performance Optimization

### Caching Strategies

```csharp
public class CachedNaturalLanguageParser : INaturalLanguageTaskParser
{
    private readonly INaturalLanguageTaskParser _baseParser;
    private readonly IMemoryCache _cache;
    
    public async Task<TaskParseResult> ParseTaskAsync(
        string userInput, 
        string userId, 
        UserContext context,
        CancellationToken cancellationToken = default)
    {
        // Create cache key from input and context
        var cacheKey = $"task_parse_{userInput.GetHashCode()}_{context.Language}_{context.Timezone}";
        
        if (_cache.TryGetValue(cacheKey, out TaskParseResult cachedResult))
        {
            return cachedResult;
        }
        
        var result = await _baseParser.ParseTaskAsync(userInput, userId, context, cancellationToken);
        
        // Cache successful parses with high confidence
        if (result.Confidence > 0.8)
        {
            _cache.Set(cacheKey, result, TimeSpan.FromHours(1));
        }
        
        return result;
    }
}
```

---

## Quick Reference

**Key NLP Components:**
- `INaturalLanguageTaskParser` - Main task parsing interface
- `PersonaInterviewProcessor` - Interview question generation
- `BuddyConversationProcessor` - Context-aware buddy responses
- `MultiLanguagePromptBuilder` - Localized prompt generation

**Best Practices:**
- Always provide fallbacks for AI failures
- Use confidence scoring for result quality
- Cache common parsing results
- Test with various input formats
- Support multiple languages from day one

**Common Patterns:**
```csharp
// Task parsing
var result = await _parser.ParseTaskAsync(input, userId, context);

// Persona interview  
var question = await _interviewer.GenerateNextQuestionAsync(responses, context);

// Buddy response
var response = await _buddy.GenerateResponseAsync(message, buddyContext);
```

Ready to implement intelligent natural language processing for Sprint 1! 🧠