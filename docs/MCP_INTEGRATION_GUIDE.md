# MCP Integration Guide - Spinner.Net

## Table of Contents

1. [Overview](#overview)
2. [MCP Architecture Strategy](#mcp-architecture-strategy)
3. [Spinner.Net as MCP Server](#spinner-net-as-mcp-server)
4. [MCP Client Integration](#mcp-client-integration)
5. [AI Buddy System Integration](#ai-buddy-system-integration)
6. [Data Sovereignty & Privacy](#data-sovereignty--privacy)
7. [Vertical Slice Implementation](#vertical-slice-implementation)
8. [Security & Authentication](#security--authentication)
9. [Development Workflow](#development-workflow)
10. [Sprint Planning](#sprint-planning)
11. [Technical Implementation](#technical-implementation)
12. [Testing Strategy](#testing-strategy)
13. [Performance & Monitoring](#performance--monitoring)
14. [Future Roadmap](#future-roadmap)

---

## Overview

The Model Context Protocol (MCP) integration for Spinner.Net creates a revolutionary ecosystem where AI assistants can seamlessly connect to user data and capabilities while respecting data sovereignty principles. This integration serves two primary purposes:

1. **Spinner.Net as MCP Server**: Exposing user data, task management, and AI buddy capabilities to external AI tools
2. **Spinner.Net as MCP Client**: Connecting to external services and tools to enhance user workflows

### Key Benefits

- **Universal AI Integration**: Any MCP-compatible AI assistant can access Spinner.Net capabilities
- **Privacy-First Design**: User controls what data is exposed through granular permissions
- **Workflow Automation**: AI assistants can create tasks, schedule events, and manage user workflows
- **Modular Extensibility**: Each Spinner.Net module (Zeitsparkasse™, LichtFlow) can expose its own MCP tools
- **Local-First Processing**: MCP operations respect user preferences for local vs. cloud AI processing

### Integration Philosophy

MCP integration in Spinner.Net follows our core principles:

```
User Sovereignty → Data Control → AI Enhancement → Workflow Optimization
```

The user always maintains control over:
- Which AI assistants can access their data
- What level of access each assistant has
- Where AI processing occurs (local vs. cloud)
- How their data is used across the ecosystem

---

## MCP Architecture Strategy

### Three-Layer MCP Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    External AI Assistants                  │
│         (Claude Desktop, ChatGPT, Custom Tools)            │
└─────────────────────┬───────────────────────────────────────┘
                      │ MCP Protocol
┌─────────────────────▼───────────────────────────────────────┐
│                 MCP Gateway Layer                           │
│    • Authentication & Authorization                        │
│    • Rate Limiting & Security                              │
│    • Protocol Translation                                  │
│    • Request Routing                                       │
└─────────────────────┬───────────────────────────────────────┘
                      │ Internal API
┌─────────────────────▼───────────────────────────────────────┐
│               Spinner.Net Core Services                     │
│    • User Management • Task Management                     │
│    • AI Buddy System • Data Sovereignty                   │
│    • Module System   • Local LLM Integration              │
└─────────────────────────────────────────────────────────────┘
```

### MCP Server Capabilities

Spinner.Net exposes the following tool categories through MCP:

#### 1. **User & Identity Tools**
- `get_user_profile`: Retrieve user persona and preferences
- `update_user_preferences`: Modify user settings and preferences
- `get_user_context`: Get current user context for AI interactions

#### 2. **Task & Time Management Tools** (Zeitsparkasse™)
- `create_task`: Create tasks from natural language
- `get_tasks`: Retrieve user tasks with filtering
- `update_task`: Modify existing tasks
- `schedule_time_block`: Schedule focused work time
- `get_time_insights`: Retrieve time tracking analytics
- `create_goal`: Create and break down complex goals

#### 3. **AI Buddy Interaction Tools**
- `chat_with_buddy`: Send messages to user's AI buddy
- `get_buddy_context`: Retrieve buddy personality and conversation history
- `create_buddy_memory`: Store important information for the buddy
- `get_buddy_insights`: Get AI-generated insights about user patterns

#### 4. **Asset & Content Tools** (LichtFlow)
- `upload_image`: Upload and process images
- `search_assets`: Find images and documents using AI
- `create_collection`: Organize assets into collections
- `get_asset_metadata`: Retrieve comprehensive asset information

#### 5. **Communication Tools**
- `send_notification`: Send notifications to user
- `schedule_reminder`: Create time-based reminders
- `create_calendar_event`: Add events to user's calendar

#### 6. **Data & Analytics Tools**
- `get_productivity_metrics`: Retrieve user productivity insights
- `export_user_data`: Export user data (respecting sovereignty preferences)
- `get_ai_usage_stats`: Get AI processing usage statistics

### MCP Client Integrations

Spinner.Net connects to external MCP servers to enhance functionality:

#### 1. **Productivity Tools**
- **Calendar Systems**: Google Calendar, Outlook, Apple Calendar
- **Note-Taking**: Obsidian, Notion, Roam Research
- **Project Management**: Asana, Trello, Linear
- **Communication**: Slack, Discord, Teams

#### 2. **Development Tools**
- **Code Repositories**: GitHub, GitLab, Bitbucket
- **CI/CD Systems**: GitHub Actions, Jenkins, CircleCI
- **Documentation**: GitBook, Confluence, Notion

#### 3. **Creative Tools**
- **Design Platforms**: Figma, Adobe Creative Suite
- **Photography**: Lightroom, Capture One, PhotoMechanic
- **Video**: Premiere Pro, Final Cut Pro, DaVinci Resolve

#### 4. **Business Intelligence**
- **Analytics**: Google Analytics, Mixpanel, Amplitude
- **Financial**: QuickBooks, Stripe, PayPal
- **CRM**: Salesforce, HubSpot, Pipedrive

---

## Spinner.Net as MCP Server

### Server Architecture

The MCP server implementation follows Spinner.Net's vertical slice architecture with dedicated MCP endpoints:

```csharp
namespace SpinnerNet.Core.Features.MCP;

public static class McpServerEndpoint
{
    public record ListToolsCommand : IRequest<ListToolsResult>;
    
    public record CallToolCommand : IRequest<CallToolResult>
    {
        public string ToolName { get; init; } = string.Empty;
        public Dictionary<string, object> Arguments { get; init; } = new();
        public string UserId { get; init; } = string.Empty;
        public string SessionId { get; init; } = string.Empty;
    }
    
    public record ListToolsResult
    {
        public bool Success { get; init; }
        public List<McpTool> Tools { get; init; } = new();
        public string? ErrorMessage { get; init; }
    }
    
    public record CallToolResult
    {
        public bool Success { get; init; }
        public object? Content { get; init; }
        public string? ErrorMessage { get; init; }
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
}
```

### Tool Registration System

Each Spinner.Net module can register its own MCP tools:

```csharp
public interface IMcpToolRegistry
{
    void RegisterTool<T>(McpToolDefinition<T> toolDefinition) where T : class;
    Task<List<McpTool>> GetAvailableToolsAsync(string userId);
    Task<object?> ExecuteToolAsync(string toolName, Dictionary<string, object> arguments, string userId);
}

public class McpToolDefinition<T> where T : class
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public JsonSchema InputSchema { get; init; } = new();
    public List<McpPermission> RequiredPermissions { get; init; } = new();
    public Func<T, string, CancellationToken, Task<object?>> Handler { get; init; } = null!;
    public Func<string, Task<bool>> PermissionValidator { get; init; } = null!;
}
```

### Core MCP Tools Implementation

#### Task Management Tools

```csharp
namespace SpinnerNet.Core.Features.MCP.Tools;

public static class CreateTaskTool
{
    public record CreateTaskParameters
    {
        [JsonPropertyName("description")]
        public string Description { get; init; } = string.Empty;
        
        [JsonPropertyName("priority")]
        public string Priority { get; init; } = "medium";
        
        [JsonPropertyName("due_date")]
        public string? DueDate { get; init; }
        
        [JsonPropertyName("category")]
        public string? Category { get; init; }
        
        [JsonPropertyName("estimated_minutes")]
        public int? EstimatedMinutes { get; init; }
    }
    
    public record CreateTaskResult
    {
        [JsonPropertyName("task_id")]
        public string TaskId { get; init; } = string.Empty;
        
        [JsonPropertyName("parsed_title")]
        public string ParsedTitle { get; init; } = string.Empty;
        
        [JsonPropertyName("parsed_due_date")]
        public DateTime? ParsedDueDate { get; init; }
        
        [JsonPropertyName("ai_confidence")]
        public double AiConfidence { get; init; }
        
        [JsonPropertyName("success")]
        public bool Success { get; init; }
    }
    
    public class Handler : IMcpToolHandler<CreateTaskParameters, CreateTaskResult>
    {
        private readonly IMediator _mediator;
        private readonly IAiService _aiService;
        private readonly ILogger<Handler> _logger;
        
        public async Task<CreateTaskResult> ExecuteAsync(
            CreateTaskParameters parameters, 
            string userId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Use existing task creation slice with AI processing
                var parseResult = await ParseNaturalLanguageTask(parameters.Description, userId);
                
                var createCommand = new CreateTask.Command
                {
                    UserId = userId,
                    Title = parseResult.Title,
                    Description = parameters.Description,
                    Priority = parameters.Priority,
                    DueDate = parseResult.DueDate ?? ParseDueDate(parameters.DueDate),
                    Category = parameters.Category ?? parseResult.Category,
                    EstimatedMinutes = parameters.EstimatedMinutes ?? parseResult.EstimatedMinutes
                };
                
                var result = await _mediator.Send(createCommand, cancellationToken);
                
                return new CreateTaskResult
                {
                    TaskId = result.TaskId,
                    ParsedTitle = parseResult.Title,
                    ParsedDueDate = parseResult.DueDate,
                    AiConfidence = parseResult.Confidence,
                    Success = result.Success
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task via MCP for user {UserId}", userId);
                return new CreateTaskResult { Success = false };
            }
        }
    }
}
```

#### AI Buddy Integration Tools

```csharp
public static class ChatWithBuddyTool
{
    public record ChatParameters
    {
        [JsonPropertyName("message")]
        public string Message { get; init; } = string.Empty;
        
        [JsonPropertyName("context")]
        public string? Context { get; init; }
        
        [JsonPropertyName("stream")]
        public bool Stream { get; init; } = false;
    }
    
    public record ChatResult
    {
        [JsonPropertyName("response")]
        public string Response { get; init; } = string.Empty;
        
        [JsonPropertyName("buddy_name")]
        public string BuddyName { get; init; } = string.Empty;
        
        [JsonPropertyName("emotion")]
        public string Emotion { get; init; } = string.Empty;
        
        [JsonPropertyName("suggested_actions")]
        public List<string> SuggestedActions { get; init; } = new();
        
        [JsonPropertyName("success")]
        public bool Success { get; init; }
    }
    
    public class Handler : IMcpToolHandler<ChatParameters, ChatResult>
    {
        private readonly IBuddyConversationService _buddyService;
        private readonly ICosmosRepository<BuddyDocument> _buddyRepository;
        
        public async Task<ChatResult> ExecuteAsync(
            ChatParameters parameters, 
            string userId, 
            CancellationToken cancellationToken = default)
        {
            // Get user's buddy
            var buddy = await _buddyRepository.GetByUserIdAsync(userId, cancellationToken);
            if (buddy == null)
            {
                return new ChatResult { Success = false };
            }
            
            var buddyContext = new BuddyContext
            {
                UserId = userId,
                BuddyName = buddy.Name,
                Personality = buddy.Personality,
                CommunicationStyle = buddy.CommunicationStyle,
                RecentMessages = await GetRecentMessages(userId, 5)
            };
            
            var response = await _buddyService.GenerateResponseAsync(
                parameters.Message, 
                buddyContext, 
                cancellationToken);
                
            return new ChatResult
            {
                Response = response.Message,
                BuddyName = buddy.Name,
                Emotion = response.Emotion,
                SuggestedActions = response.SuggestedActions,
                Success = true
            };
        }
    }
}
```

### MCP Server Configuration

```csharp
// Program.cs - MCP Server Setup
public static void AddMcpServer(this IServiceCollection services, IConfiguration configuration)
{
    // MCP Protocol Handler
    services.AddScoped<IMcpProtocolHandler, McpProtocolHandler>();
    services.AddScoped<IMcpToolRegistry, McpToolRegistry>();
    services.AddScoped<IMcpPermissionService, McpPermissionService>();
    
    // Tool Handlers
    services.AddMcpToolHandler<CreateTaskTool.CreateTaskParameters, CreateTaskTool.CreateTaskResult, CreateTaskTool.Handler>();
    services.AddMcpToolHandler<ChatWithBuddyTool.ChatParameters, ChatWithBuddyTool.ChatResult, ChatWithBuddyTool.Handler>();
    services.AddMcpToolHandler<GetTasksTool.GetTasksParameters, GetTasksTool.GetTasksResult, GetTasksTool.Handler>();
    
    // Authentication for MCP
    services.AddScoped<IMcpAuthenticationService, McpAuthenticationService>();
    
    // Rate limiting
    services.AddMemoryCache();
    services.AddScoped<IMcpRateLimitService, McpRateLimitService>();
}

// Startup.cs - MCP Endpoints
app.MapPost("/mcp/tools/list", async (ListToolsCommand command, IMediator mediator) =>
    await mediator.Send(command));
    
app.MapPost("/mcp/tools/call", async (CallToolCommand command, IMediator mediator) =>
    await mediator.Send(command));
```

### Tool Registration in Modules

Each module (Zeitsparkasse™, LichtFlow) registers its tools:

```csharp
// Zeitsparkasse Module
public class ZeitsparkasseModule : IModule
{
    public void RegisterMcpTools(IMcpToolRegistry registry)
    {
        // Task Management
        registry.RegisterTool(new McpToolDefinition<CreateTaskTool.CreateTaskParameters>
        {
            Name = "zeitsparkasse_create_task",
            Description = "Create a task using natural language processing",
            Category = "task_management",
            InputSchema = JsonSchema.FromType<CreateTaskTool.CreateTaskParameters>(),
            RequiredPermissions = new[] { McpPermission.TaskWrite },
            Handler = async (parameters, userId, ct) => 
                await new CreateTaskTool.Handler().ExecuteAsync(parameters, userId, ct),
            PermissionValidator = async userId => 
                await _permissionService.HasPermissionAsync(userId, McpPermission.TaskWrite)
        });
        
        // Time Tracking
        registry.RegisterTool(new McpToolDefinition<StartTimeBlockTool.StartTimeBlockParameters>
        {
            Name = "zeitsparkasse_start_time_block",
            Description = "Start a focused time tracking session",
            Category = "time_management",
            InputSchema = JsonSchema.FromType<StartTimeBlockTool.StartTimeBlockParameters>(),
            RequiredPermissions = new[] { McpPermission.TimeTracking },
            Handler = async (parameters, userId, ct) => 
                await new StartTimeBlockTool.Handler().ExecuteAsync(parameters, userId, ct)
        });
    }
}

// LichtFlow Module
public class LichtFlowModule : IModule
{
    public void RegisterMcpTools(IMcpToolRegistry registry)
    {
        // Asset Management
        registry.RegisterTool(new McpToolDefinition<UploadImageTool.UploadImageParameters>
        {
            Name = "lichtflow_upload_image",
            Description = "Upload and process an image with AI analysis",
            Category = "asset_management",
            InputSchema = JsonSchema.FromType<UploadImageTool.UploadImageParameters>(),
            RequiredPermissions = new[] { McpPermission.AssetWrite },
            Handler = async (parameters, userId, ct) => 
                await new UploadImageTool.Handler().ExecuteAsync(parameters, userId, ct)
        });
        
        // Search Assets
        registry.RegisterTool(new McpToolDefinition<SearchAssetsTool.SearchAssetsParameters>
        {
            Name = "lichtflow_search_assets",
            Description = "Search for images and assets using AI-powered search",
            Category = "asset_search",
            InputSchema = JsonSchema.FromType<SearchAssetsTool.SearchAssetsParameters>(),
            RequiredPermissions = new[] { McpPermission.AssetRead },
            Handler = async (parameters, userId, ct) => 
                await new SearchAssetsTool.Handler().ExecuteAsync(parameters, userId, ct)
        });
    }
}
```

---

## MCP Client Integration

### Client Architecture

Spinner.Net acts as an MCP client to connect to external services and enhance user workflows:

```csharp
public interface IMcpClientService
{
    Task<List<McpServer>> GetAvailableServersAsync(string userId);
    Task<List<McpTool>> GetServerToolsAsync(string serverId, string userId);
    Task<object?> CallServerToolAsync(string serverId, string toolName, Dictionary<string, object> arguments, string userId);
    Task<bool> ConnectToServerAsync(string serverUrl, McpCredentials credentials, string userId);
}

public class McpClientService : IMcpClientService
{
    private readonly HttpClient _httpClient;
    private readonly IDataSovereigntyService _dataSovereignty;
    private readonly ILogger<McpClientService> _logger;
    private readonly Dictionary<string, IMcpConnection> _connections = new();
    
    public async Task<object?> CallServerToolAsync(
        string serverId, 
        string toolName, 
        Dictionary<string, object> arguments, 
        string userId)
    {
        // Check user permissions for external service access
        var hasPermission = await _dataSovereignty.HasExternalServicePermissionAsync(
            userId, serverId, toolName);
            
        if (!hasPermission)
        {
            throw new UnauthorizedAccessException($"User {userId} does not have permission to use {toolName} on {serverId}");
        }
        
        var connection = _connections[serverId];
        
        // Respect data sovereignty - filter sensitive data
        var filteredArguments = await FilterSensitiveDataAsync(arguments, userId);
        
        return await connection.CallToolAsync(toolName, filteredArguments);
    }
}
```

### External Service Integrations

#### Calendar Integration (Google Calendar MCP)

```csharp
namespace SpinnerNet.Core.Features.MCP.Clients;

public static class GoogleCalendarIntegration
{
    public record CreateCalendarEventCommand : IRequest<CreateCalendarEventResult>
    {
        public string UserId { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
        public string? Description { get; init; }
        public List<string> Attendees { get; init; } = new();
    }
    
    public record CreateCalendarEventResult
    {
        public bool Success { get; init; }
        public string? EventId { get; init; }
        public string? CalendarUrl { get; init; }
        public string? ErrorMessage { get; init; }
    }
    
    public class Handler : IRequestHandler<CreateCalendarEventCommand, CreateCalendarEventResult>
    {
        private readonly IMcpClientService _mcpClient;
        private readonly IDataSovereigntyService _dataSovereignty;
        
        public async Task<CreateCalendarEventResult> Handle(
            CreateCalendarEventCommand request, 
            CancellationToken cancellationToken)
        {
            // Check if user has connected Google Calendar
            var calendarConnection = await _dataSovereignty.GetExternalServiceConnectionAsync(
                request.UserId, "google_calendar");
                
            if (calendarConnection == null)
            {
                return new CreateCalendarEventResult 
                { 
                    Success = false, 
                    ErrorMessage = "Google Calendar not connected" 
                };
            }
            
            var arguments = new Dictionary<string, object>
            {
                ["title"] = request.Title,
                ["start_time"] = request.StartTime.ToString("O"),
                ["end_time"] = request.EndTime.ToString("O"),
                ["description"] = request.Description ?? "",
                ["attendees"] = request.Attendees
            };
            
            try
            {
                var result = await _mcpClient.CallServerToolAsync(
                    "google_calendar", 
                    "create_event", 
                    arguments, 
                    request.UserId);
                    
                return new CreateCalendarEventResult
                {
                    Success = true,
                    EventId = result?.GetProperty("event_id")?.ToString(),
                    CalendarUrl = result?.GetProperty("html_link")?.ToString()
                };
            }
            catch (Exception ex)
            {
                return new CreateCalendarEventResult 
                { 
                    Success = false, 
                    ErrorMessage = ex.Message 
                };
            }
        }
    }
}
```

#### GitHub Integration

```csharp
public static class GitHubIntegration
{
    public record CreateIssueCommand : IRequest<CreateIssueResult>
    {
        public string UserId { get; init; } = string.Empty;
        public string Repository { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;
        public List<string> Labels { get; init; } = new();
    }
    
    public class Handler : IRequestHandler<CreateIssueCommand, CreateIssueResult>
    {
        private readonly IMcpClientService _mcpClient;
        
        public async Task<CreateIssueResult> Handle(
            CreateIssueCommand request, 
            CancellationToken cancellationToken)
        {
            var arguments = new Dictionary<string, object>
            {
                ["repository"] = request.Repository,
                ["title"] = request.Title,
                ["body"] = request.Body,
                ["labels"] = request.Labels
            };
            
            var result = await _mcpClient.CallServerToolAsync(
                "github", 
                "create_issue", 
                arguments, 
                request.UserId);
                
            return new CreateIssueResult
            {
                Success = true,
                IssueNumber = (int)(result?.GetProperty("number") ?? 0),
                IssueUrl = result?.GetProperty("html_url")?.ToString()
            };
        }
    }
}
```

### Workflow Automation

MCP client integration enables powerful workflow automation:

#### Project Economy Workflows

```csharp
public static class ProjectEconomyWorkflows
{
    public record ProjectToExternalFundingWorkflow : IRequest<WorkflowResult>
    {
        public string UserId { get; init; } = string.Empty;
        public string ProjectId { get; init; } = string.Empty;
        public ExternalPlatform TargetPlatform { get; init; }
    }
    
    public class Handler : IRequestHandler<ProjectToExternalFundingWorkflow, WorkflowResult>
    {
        private readonly IMcpClientService _mcpClient;
        private readonly IProjectService _projectService;
        
        public async Task<WorkflowResult> Handle(
            ProjectToExternalFundingWorkflow request, 
            CancellationToken cancellationToken)
        {
            // 1. Get project details
            var project = await _projectService.GetByIdAsync(request.ProjectId);
            
            // 2. Create campaign on external platform via MCP
            var platformArguments = new Dictionary<string, object>
            {
                ["title"] = project.Title,
                ["description"] = project.Description,
                ["funding_goal"] = project.FundingModel.MonetaryGoals["USD"],
                ["project_category"] = project.Pillar.ToString()
            };
            
            var campaignResult = await _mcpClient.CallServerToolAsync(
                request.TargetPlatform.ToString().ToLower(),
                "create_campaign",
                platformArguments,
                request.UserId);
            
            // 3. Update project with external campaign reference
            await _projectService.UpdateExternalCampaignAsync(
                request.ProjectId, 
                campaignResult.GetProperty("campaign_url")?.ToString());
            
            // 4. Notify contributors via MCP integrations
            var contributors = await _projectService.GetContributorsAsync(request.ProjectId);
            foreach (var contributor in contributors)
            {
                await _mcpClient.CallServerToolAsync("email", "send_notification", new
                {
                    to = contributor.Email,
                    subject = $"External funding live for: {project.Title}",
                    body = $"The project is now live on {request.TargetPlatform}!"
                }, request.UserId);
            }
            
            return WorkflowResult.Success("External funding campaign created successfully");
        }
    }
}
```

#### Standard Task Workflows

```csharp
public static class WorkflowAutomation
{
    public record TaskToCalendarWorkflow : IRequest<WorkflowResult>
    {
        public string UserId { get; init; } = string.Empty;
        public string TaskId { get; init; } = string.Empty;
        public DateTime ScheduledTime { get; init; }
    }
    
    public class Handler : IRequestHandler<TaskToCalendarWorkflow, WorkflowResult>
    {
        private readonly IMediator _mediator;
        
        public async Task<WorkflowResult> Handle(
            TaskToCalendarWorkflow request, 
            CancellationToken cancellationToken)
        {
            // 1. Get task details
            var task = await _mediator.Send(new GetTask.Query 
            { 
                TaskId = request.TaskId, 
                UserId = request.UserId 
            });
            
            // 2. Create calendar event
            var calendarResult = await _mediator.Send(new CreateCalendarEventCommand
            {
                UserId = request.UserId,
                Title = $"🎯 {task.Title}",
                StartTime = request.ScheduledTime,
                EndTime = request.ScheduledTime.AddMinutes(task.EstimatedMinutes ?? 60),
                Description = $"Task: {task.Description}\nPriority: {task.Priority}"
            });
            
            // 3. Update task with calendar link
            if (calendarResult.Success)
            {
                await _mediator.Send(new UpdateTask.Command
                {
                    TaskId = request.TaskId,
                    UserId = request.UserId,
                    CalendarEventId = calendarResult.EventId,
                    ScheduledTime = request.ScheduledTime
                });
            }
            
            return new WorkflowResult { Success = calendarResult.Success };
        }
    }
}
```

---

## AI Buddy System Integration

### MCP-Enhanced AI Buddies

The AI buddy system is enhanced with MCP capabilities, allowing buddies to:

1. **Access External Services**: Call MCP tools on behalf of users
2. **Workflow Orchestration**: Chain multiple MCP calls to complete complex tasks
3. **Context Awareness**: Use external data to provide better assistance
4. **Proactive Suggestions**: Recommend actions based on external service data

```csharp
public class McpEnhancedBuddyService : IBuddyConversationService
{
    private readonly IBuddyConversationService _baseBuddyService;
    private readonly IMcpClientService _mcpClient;
    private readonly IMcpToolRegistry _mcpTools;
    private readonly IAiService _aiService;
    
    public async Task<BuddyResponse> GenerateResponseAsync(
        string userMessage,
        BuddyContext buddyContext,
        CancellationToken cancellationToken = default)
    {
        // First, determine if the message requires external service interaction
        var intentAnalysis = await AnalyzeUserIntent(userMessage, buddyContext.UserId);
        
        if (intentAnalysis.RequiresExternalService)
        {
            return await HandleExternalServiceRequest(userMessage, buddyContext, intentAnalysis);
        }
        
        // Otherwise, use standard buddy conversation
        return await _baseBuddyService.GenerateResponseAsync(userMessage, buddyContext, cancellationToken);
    }
    
    private async Task<BuddyResponse> HandleExternalServiceRequest(
        string userMessage,
        BuddyContext buddyContext,
        IntentAnalysis intentAnalysis)
    {
        var responses = new List<string>();
        
        try
        {
            // Execute the required external service calls
            foreach (var action in intentAnalysis.RequiredActions)
            {
                var result = await ExecuteMcpAction(action, buddyContext.UserId);
                responses.Add(FormatActionResult(action, result));
            }
            
            // Generate contextual buddy response
            var context = new AiContext
            {
                UserId = buddyContext.UserId,
                SystemPrompt = BuildContextualPrompt(buddyContext, intentAnalysis, responses),
                PreferredProvider = AiProvider.LocalFirst,
                RequiresPrivacy = true
            };
            
            var buddyResponse = await _aiService.GenerateResponseAsync(
                userMessage, context);
                
            return new BuddyResponse
            {
                Message = buddyResponse,
                SuggestedActions = ExtractSuggestedActions(buddyResponse),
                ExternalServiceResults = responses,
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new BuddyResponse
            {
                Message = $"I encountered an issue while trying to help: {ex.Message}",
                Success = false
            };
        }
    }
}
```

### Intent Analysis for MCP Actions

```csharp
public class McpIntentAnalysisService
{
    private readonly IAiService _aiService;
    
    public async Task<IntentAnalysis> AnalyzeUserIntent(string message, string userId)
    {
        var systemPrompt = """
            Analyze user message to determine if it requires external service interaction.
            
            Available services: calendar, github, slack, notion, email
            Available actions: create_task, schedule_event, send_message, create_note, search_files
            
            Return JSON:
            {
                "requires_external_service": true/false,
                "confidence": 0.0-1.0,
                "identified_services": ["service1", "service2"],
                "required_actions": [
                    {
                        "service": "calendar",
                        "action": "create_event",
                        "parameters": {"title": "Meeting", "time": "2024-01-15T14:00:00Z"}
                    }
                ],
                "reasoning": "explanation of analysis"
            }
            """;
            
        var prompt = $"""
            User message: "{message}"
            
            Analyze the intent and determine required external service actions.
            """;
            
        var context = new AiContext
        {
            UserId = userId,
            SystemPrompt = systemPrompt,
            PreferredProvider = AiProvider.LocalFirst,
            RequiresPrivacy = true,
            MaxTokens = 1024
        };
        
        var response = await _aiService.GenerateResponseAsync(prompt, context);
        return JsonSerializer.Deserialize<IntentAnalysis>(response);
    }
}
```

### Buddy-Initiated Workflows

Buddies can proactively suggest and execute workflows:

```csharp
public static class ProactiveBuddyWorkflows
{
    public record SuggestWorkflowCommand : IRequest<SuggestWorkflowResult>
    {
        public string UserId { get; init; } = string.Empty;
        public string Context { get; init; } = string.Empty;
    }
    
    public class Handler : IRequestHandler<SuggestWorkflowCommand, SuggestWorkflowResult>
    {
        private readonly IMcpClientService _mcpClient;
        private readonly IAiService _aiService;
        
        public async Task<SuggestWorkflowResult> Handle(
            SuggestWorkflowCommand request, 
            CancellationToken cancellationToken)
        {
            // Analyze user's current context
            var userTasks = await GetUserTasks(request.UserId);
            var calendarEvents = await GetUpcomingEvents(request.UserId);
            var recentActivity = await GetRecentActivity(request.UserId);
            
            // Generate AI-powered workflow suggestions
            var systemPrompt = """
                Based on the user's current tasks, calendar, and activity, suggest helpful workflows.
                
                Available workflows:
                - task_to_calendar: Schedule tasks as calendar events
                - deadline_reminder: Set up reminder notifications
                - progress_tracking: Create progress check-ins
                - collaboration_setup: Share tasks with team members
                """;
                
            var prompt = $"""
                User context:
                Tasks: {JsonSerializer.Serialize(userTasks)}
                Calendar: {JsonSerializer.Serialize(calendarEvents)}
                Recent Activity: {JsonSerializer.Serialize(recentActivity)}
                
                Suggest 1-3 helpful workflows with specific parameters.
                """;
                
            var context = new AiContext
            {
                UserId = request.UserId,
                SystemPrompt = systemPrompt,
                PreferredProvider = AiProvider.LocalFirst,
                MaxTokens = 1024
            };
            
            var suggestions = await _aiService.GenerateResponseAsync(prompt, context);
            
            return new SuggestWorkflowResult
            {
                Suggestions = ParseWorkflowSuggestions(suggestions),
                Success = true
            };
        }
    }
}
```

---

## Data Sovereignty & Privacy

### Privacy-First MCP Design

Data sovereignty is paramount in MCP integration. Users must have complete control over:

1. **What data is exposed** to external AI assistants
2. **Which AI assistants** can access their data
3. **How their data is processed** (local vs. cloud)
4. **Data retention and deletion** policies

### Permission System

```csharp
public enum McpPermission
{
    // Task Management
    TaskRead,
    TaskWrite,
    TaskDelete,
    
    // Time Tracking
    TimeTracking,
    TimeAnalytics,
    
    // Asset Management
    AssetRead,
    AssetWrite,
    AssetDelete,
    
    // AI Buddy
    BuddyChat,
    BuddyMemory,
    BuddyInsights,
    
    // Personal Data
    ProfileRead,
    ProfileWrite,
    
    // External Services
    ExternalServiceConnect,
    ExternalServiceCall,
    
    // Administrative
    SystemSettings,
    UserManagement
}

public class McpPermissionService : IMcpPermissionService
{
    private readonly ICosmosRepository<UserMcpPermissionsDocument> _permissionsRepository;
    
    public async Task<bool> HasPermissionAsync(string userId, string clientId, McpPermission permission)
    {
        var permissions = await _permissionsRepository.GetByUserIdAsync(userId);
        if (permissions == null) return false;
        
        var clientPermissions = permissions.ClientPermissions.GetValueOrDefault(clientId);
        if (clientPermissions == null) return false;
        
        return clientPermissions.Permissions.Contains(permission);
    }
    
    public async Task GrantPermissionAsync(string userId, string clientId, McpPermission permission)
    {
        var permissions = await _permissionsRepository.GetByUserIdAsync(userId) 
            ?? new UserMcpPermissionsDocument { UserId = userId };
            
        if (!permissions.ClientPermissions.ContainsKey(clientId))
        {
            permissions.ClientPermissions[clientId] = new ClientPermissions { ClientId = clientId };
        }
        
        permissions.ClientPermissions[clientId].Permissions.Add(permission);
        permissions.ClientPermissions[clientId].GrantedAt = DateTime.UtcNow;
        
        await _permissionsRepository.CreateOrUpdateAsync(permissions, userId);
    }
}

public class UserMcpPermissionsDocument : CosmosDocument
{
    public string UserId { get; set; } = string.Empty;
    public Dictionary<string, ClientPermissions> ClientPermissions { get; set; } = new();
}

public class ClientPermissions
{
    public string ClientId { get; set; } = string.Empty;
    public HashSet<McpPermission> Permissions { get; set; } = new();
    public DateTime GrantedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
}
```

### Data Filtering and Anonymization

```csharp
public class DataSovereigntyMcpFilter
{
    private readonly IDataSovereigntyService _dataSovereignty;
    
    public async Task<object> FilterDataForExternalAccess(object data, string userId, string clientId)
    {
        var userPreferences = await _dataSovereignty.GetDataSharingPreferencesAsync(userId);
        
        return data switch
        {
            TaskDocument task => FilterTaskData(task, userPreferences, clientId),
            BuddyDocument buddy => FilterBuddyData(buddy, userPreferences, clientId),
            PersonaDocument persona => FilterPersonaData(persona, userPreferences, clientId),
            _ => data
        };
    }
    
    private object FilterTaskData(TaskDocument task, DataSharingPreferences preferences, string clientId)
    {
        var filtered = new
        {
            Id = task.Id,
            Title = preferences.ShareTaskTitles ? task.Title : "[Task]",
            Priority = task.Priority,
            Category = task.Category,
            Status = task.Status,
            DueDate = preferences.ShareDueDates ? task.DueDate : null,
            // Never share description content unless explicitly allowed
            Description = preferences.ShareTaskDetails ? task.Description : null,
            CreatedAt = task.CreatedAt
        };
        
        return filtered;
    }
    
    private object FilterBuddyData(BuddyDocument buddy, DataSharingPreferences preferences, string clientId)
    {
        return new
        {
            Name = buddy.Name,
            Personality = preferences.SharePersonalityTraits ? buddy.Personality : "friendly",
            CommunicationStyle = buddy.CommunicationStyle,
            // Never share conversation history
            IsActive = buddy.IsActive
        };
    }
}
```

### Local Processing Enforcement

For privacy-sensitive operations, enforce local processing:

```csharp
public class PrivacyEnforcedMcpHandler
{
    private readonly IAiService _aiService;
    private readonly IDataSovereigntyService _dataSovereignty;
    
    public async Task<object?> ExecuteWithPrivacyEnforcement(
        string toolName, 
        Dictionary<string, object> arguments, 
        string userId)
    {
        var aiPreference = await _dataSovereignty.GetAiProcessingPreferenceAsync(userId);
        var isPrivacySensitive = IsPrivacySensitiveOperation(toolName, arguments);
        
        // Force local processing for privacy-sensitive operations
        if (isPrivacySensitive && aiPreference != AiProcessingPreference.CloudAllowed)
        {
            var context = new AiContext
            {
                UserId = userId,
                PreferredProvider = AiProvider.Local, // Force local
                RequiresPrivacy = true
            };
            
            return await ExecuteWithLocalProcessing(toolName, arguments, context);
        }
        
        return await ExecuteStandard(toolName, arguments, userId);
    }
    
    private bool IsPrivacySensitiveOperation(string toolName, Dictionary<string, object> arguments)
    {
        var privacySensitiveTools = new[]
        {
            "chat_with_buddy",
            "create_personal_task",
            "analyze_mood",
            "get_personal_insights"
        };
        
        if (privacySensitiveTools.Contains(toolName)) return true;
        
        // Check for sensitive content in arguments
        var sensitiveKeywords = new[] { "personal", "private", "emotion", "feeling", "health" };
        var argumentsText = JsonSerializer.Serialize(arguments).ToLowerInvariant();
        
        return sensitiveKeywords.Any(keyword => argumentsText.Contains(keyword));
    }
}
```

---

## Vertical Slice Implementation

### MCP-Specific Vertical Slices

MCP functionality follows Spinner.Net's vertical slice architecture with dedicated slices for each MCP operation:

```csharp
namespace SpinnerNet.Core.Features.MCP;

public static class ListMcpTools
{
    public record Query : IRequest<Result>
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; init; } = string.Empty;
        
        [JsonPropertyName("client_id")]
        public string ClientId { get; init; } = string.Empty;
        
        [JsonPropertyName("category")]
        public string? Category { get; init; }
    }
    
    public record Result
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }
        
        [JsonPropertyName("tools")]
        public List<McpToolInfo> Tools { get; init; } = new();
        
        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; init; }
        
        public static Result SuccessResult(List<McpToolInfo> tools) => new() 
        { 
            Success = true, 
            Tools = tools 
        };
        
        public static Result Failure(string error) => new() 
        { 
            Success = false, 
            ErrorMessage = error 
        };
    }
    
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.UserId).NotEmpty().MaximumLength(100);
            RuleFor(x => x.ClientId).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Category).MaximumLength(50);
        }
    }
    
    public class Handler : IRequestHandler<Query, Result>
    {
        private readonly IMcpToolRegistry _toolRegistry;
        private readonly IMcpPermissionService _permissionService;
        private readonly ILogger<Handler> _logger;
        
        public Handler(
            IMcpToolRegistry toolRegistry,
            IMcpPermissionService permissionService,
            ILogger<Handler> logger)
        {
            _toolRegistry = toolRegistry;
            _permissionService = permissionService;
            _logger = logger;
        }
        
        public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Listing MCP tools for user {UserId}, client {ClientId}", 
                    request.UserId, request.ClientId);
                
                // Get all available tools
                var allTools = await _toolRegistry.GetAvailableToolsAsync(request.UserId);
                
                // Filter by category if specified
                if (!string.IsNullOrEmpty(request.Category))
                {
                    allTools = allTools.Where(t => t.Category == request.Category).ToList();
                }
                
                // Filter by permissions
                var authorizedTools = new List<McpToolInfo>();
                foreach (var tool in allTools)
                {
                    var hasPermissions = true;
                    foreach (var permission in tool.RequiredPermissions)
                    {
                        if (!await _permissionService.HasPermissionAsync(request.UserId, request.ClientId, permission))
                        {
                            hasPermissions = false;
                            break;
                        }
                    }
                    
                    if (hasPermissions)
                    {
                        authorizedTools.Add(tool);
                    }
                }
                
                _logger.LogInformation("Returning {Count} authorized tools for client {ClientId}", 
                    authorizedTools.Count, request.ClientId);
                
                return Result.SuccessResult(authorizedTools);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing MCP tools for user {UserId}", request.UserId);
                return Result.Failure("Failed to retrieve available tools");
            }
        }
    }
    
    [ApiController]
    [Route("api/mcp")]
    public class Endpoint : ControllerBase
    {
        private readonly IMediator _mediator;
        
        public Endpoint(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpGet("tools")]
        public async Task<ActionResult<Result>> ListTools(
            [FromQuery] string userId,
            [FromQuery] string clientId,
            [FromQuery] string? category = null)
        {
            var query = new Query
            {
                UserId = userId,
                ClientId = clientId,
                Category = category
            };
            
            var result = await _mediator.Send(query);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
```

### MCP Tool Execution Slice

```csharp
public static class ExecuteMcpTool
{
    public record Command : IRequest<Result>
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; init; } = string.Empty;
        
        [JsonPropertyName("client_id")]
        public string ClientId { get; init; } = string.Empty;
        
        [JsonPropertyName("tool_name")]
        public string ToolName { get; init; } = string.Empty;
        
        [JsonPropertyName("arguments")]
        public Dictionary<string, object> Arguments { get; init; } = new();
    }
    
    public record Result
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }
        
        [JsonPropertyName("result")]
        public object? Result { get; init; }
        
        [JsonPropertyName("execution_time_ms")]
        public long ExecutionTimeMs { get; init; }
        
        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; init; }
        
        public static Result SuccessResult(object? result, long executionTime) => new() 
        { 
            Success = true, 
            Result = result,
            ExecutionTimeMs = executionTime
        };
        
        public static Result Failure(string error) => new() 
        { 
            Success = false, 
            ErrorMessage = error 
        };
    }
    
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.UserId).NotEmpty().MaximumLength(100);
            RuleFor(x => x.ClientId).NotEmpty().MaximumLength(100);
            RuleFor(x => x.ToolName).NotEmpty().MaximumLength(100);
        }
    }
    
    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly IMcpToolRegistry _toolRegistry;
        private readonly IMcpPermissionService _permissionService;
        private readonly IMcpRateLimitService _rateLimitService;
        private readonly ILogger<Handler> _logger;
        
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Rate limiting
                var rateLimitResult = await _rateLimitService.CheckRateLimitAsync(
                    request.UserId, request.ClientId, request.ToolName);
                    
                if (!rateLimitResult.IsAllowed)
                {
                    return Result.Failure($"Rate limit exceeded. Try again in {rateLimitResult.RetryAfterSeconds} seconds");
                }
                
                // Permission validation
                var tool = await _toolRegistry.GetToolAsync(request.ToolName);
                if (tool == null)
                {
                    return Result.Failure($"Tool '{request.ToolName}' not found");
                }
                
                foreach (var permission in tool.RequiredPermissions)
                {
                    if (!await _permissionService.HasPermissionAsync(request.UserId, request.ClientId, permission))
                    {
                        return Result.Failure($"Missing permission: {permission}");
                    }
                }
                
                // Execute tool
                var result = await _toolRegistry.ExecuteToolAsync(
                    request.ToolName, 
                    request.Arguments, 
                    request.UserId);
                
                stopwatch.Stop();
                
                _logger.LogInformation("MCP tool {ToolName} executed for user {UserId} in {ElapsedMs}ms", 
                    request.ToolName, request.UserId, stopwatch.ElapsedMilliseconds);
                
                return Result.SuccessResult(result, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error executing MCP tool {ToolName} for user {UserId}", 
                    request.ToolName, request.UserId);
                
                return Result.Failure("Tool execution failed");
            }
        }
    }
    
    [HttpPost("tools/execute")]
    public async Task<ActionResult<Result>> ExecuteTool([FromBody] Command command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
```

### MCP Permission Management Slice

```csharp
public static class GrantMcpPermission
{
    public record Command : IRequest<Result>
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; init; } = string.Empty;
        
        [JsonPropertyName("client_id")]
        public string ClientId { get; init; } = string.Empty;
        
        [JsonPropertyName("permissions")]
        public List<McpPermission> Permissions { get; init; } = new();
        
        [JsonPropertyName("expires_at")]
        public DateTime? ExpiresAt { get; init; }
    }
    
    public record Result
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }
        
        [JsonPropertyName("granted_permissions")]
        public List<McpPermission> GrantedPermissions { get; init; } = new();
        
        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; init; }
    }
    
    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly IMcpPermissionService _permissionService;
        private readonly ICosmosRepository<UserMcpPermissionsDocument> _permissionsRepository;
        
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var grantedPermissions = new List<McpPermission>();
                
                foreach (var permission in request.Permissions)
                {
                    await _permissionService.GrantPermissionAsync(
                        request.UserId, 
                        request.ClientId, 
                        permission,
                        request.ExpiresAt);
                        
                    grantedPermissions.Add(permission);
                }
                
                return new Result
                {
                    Success = true,
                    GrantedPermissions = grantedPermissions
                };
            }
            catch (Exception ex)
            {
                return Result.Failure("Failed to grant permissions");
            }
        }
    }
}
```

---

## Security & Authentication

### MCP Authentication Framework

```csharp
public interface IMcpAuthenticationService
{
    Task<McpAuthenticationResult> AuthenticateClientAsync(string clientId, string clientSecret);
    Task<McpSession> CreateSessionAsync(string userId, string clientId, TimeSpan? duration = null);
    Task<bool> ValidateSessionAsync(string sessionToken);
    Task RevokeSessionAsync(string sessionToken);
    Task<List<McpSession>> GetActiveSessionsAsync(string userId);
}

public class McpAuthenticationService : IMcpAuthenticationService
{
    private readonly ICosmosRepository<McpClientDocument> _clientRepository;
    private readonly ICosmosRepository<McpSessionDocument> _sessionRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<McpAuthenticationService> _logger;
    
    public async Task<McpAuthenticationResult> AuthenticateClientAsync(string clientId, string clientSecret)
    {
        try
        {
            var client = await _clientRepository.GetByIdAsync(clientId, "system");
            if (client == null)
            {
                return McpAuthenticationResult.Failure("Invalid client credentials");
            }
            
            // Verify client secret (hashed)
            var isValidSecret = BCrypt.Net.BCrypt.Verify(clientSecret, client.HashedSecret);
            if (!isValidSecret)
            {
                return McpAuthenticationResult.Failure("Invalid client credentials");
            }
            
            // Check if client is active
            if (!client.IsActive)
            {
                return McpAuthenticationResult.Failure("Client is disabled");
            }
            
            return McpAuthenticationResult.Success(client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating MCP client {ClientId}", clientId);
            return McpAuthenticationResult.Failure("Authentication failed");
        }
    }
    
    public async Task<McpSession> CreateSessionAsync(string userId, string clientId, TimeSpan? duration = null)
    {
        var sessionDuration = duration ?? TimeSpan.FromHours(24);
        var expiresAt = DateTime.UtcNow.Add(sessionDuration);
        
        var session = new McpSessionDocument
        {
            Id = $"session_{Guid.NewGuid()}",
            UserId = userId,
            ClientId = clientId,
            Token = _tokenService.GenerateSecureToken(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsActive = true
        };
        
        await _sessionRepository.CreateOrUpdateAsync(session, "system");
        
        return new McpSession
        {
            Token = session.Token,
            ExpiresAt = session.ExpiresAt,
            ClientId = session.ClientId
        };
    }
}

public class McpClientDocument : CosmosDocument
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string HashedSecret { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<string> AllowedOrigins { get; set; } = new();
    public List<McpPermission> DefaultPermissions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class McpSessionDocument : CosmosDocument
{
    public string UserId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
```

### Rate Limiting and Security

```csharp
public interface IMcpRateLimitService
{
    Task<RateLimitResult> CheckRateLimitAsync(string userId, string clientId, string operation);
    Task RecordRequestAsync(string userId, string clientId, string operation);
}

public class McpRateLimitService : IMcpRateLimitService
{
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    
    public async Task<RateLimitResult> CheckRateLimitAsync(string userId, string clientId, string operation)
    {
        var key = $"rate_limit:{clientId}:{userId}:{operation}";
        var requests = _cache.Get<List<DateTime>>(key) ?? new List<DateTime>();
        
        // Get rate limit configuration for this operation
        var limits = GetRateLimits(operation);
        var now = DateTime.UtcNow;
        
        // Remove old requests outside the time window
        requests.RemoveAll(r => now - r > limits.TimeWindow);
        
        if (requests.Count >= limits.MaxRequests)
        {
            var oldestRequest = requests.Min();
            var retryAfter = (int)(limits.TimeWindow - (now - oldestRequest)).TotalSeconds;
            
            return new RateLimitResult
            {
                IsAllowed = false,
                RetryAfterSeconds = retryAfter,
                RequestsRemaining = 0
            };
        }
        
        return new RateLimitResult
        {
            IsAllowed = true,
            RequestsRemaining = limits.MaxRequests - requests.Count
        };
    }
    
    private RateLimitConfig GetRateLimits(string operation)
    {
        return operation switch
        {
            "create_task" => new RateLimitConfig { MaxRequests = 100, TimeWindow = TimeSpan.FromHours(1) },
            "chat_with_buddy" => new RateLimitConfig { MaxRequests = 500, TimeWindow = TimeSpan.FromHours(1) },
            "upload_image" => new RateLimitConfig { MaxRequests = 50, TimeWindow = TimeSpan.FromHours(1) },
            _ => new RateLimitConfig { MaxRequests = 200, TimeWindow = TimeSpan.FromHours(1) }
        };
    }
}
```

### CORS and Origin Validation

```csharp
public class McpSecurityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMcpAuthenticationService _authService;
    private readonly ILogger<McpSecurityMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api/mcp"))
        {
            // Validate origin for MCP requests
            if (!await ValidateOriginAsync(context))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Origin not allowed");
                return;
            }
            
            // Validate session token
            if (!await ValidateSessionAsync(context))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid or expired session");
                return;
            }
        }
        
        await _next(context);
    }
    
    private async Task<bool> ValidateOriginAsync(HttpContext context)
    {
        var origin = context.Request.Headers["Origin"].FirstOrDefault();
        if (string.IsNullOrEmpty(origin))
        {
            return false; // Origin required for MCP requests
        }
        
        // Get client ID from Authorization header or query string
        var clientId = ExtractClientId(context);
        if (string.IsNullOrEmpty(clientId))
        {
            return false;
        }
        
        // Check if origin is allowed for this client
        var client = await GetClientAsync(clientId);
        if (client == null || !client.AllowedOrigins.Contains(origin))
        {
            _logger.LogWarning("Origin {Origin} not allowed for client {ClientId}", origin, clientId);
            return false;
        }
        
        return true;
    }
}
```

---

## Development Workflow

### MCP Development Environment Setup

#### 1. Local MCP Server Development

```bash
# Start Spinner.Net with MCP server enabled
cd Spinner.Net-Public/src
dotnet run --project SpinnerNet.Web --environment Development

# The MCP server will be available at:
# http://localhost:5000/api/mcp
```

#### 2. MCP Client Testing

```bash
# Install MCP CLI tools for testing
npm install -g @modelcontextprotocol/cli

# Test MCP server connection
mcp-test http://localhost:5000/api/mcp \
  --client-id "test-client" \
  --client-secret "test-secret"

# List available tools
mcp-test list-tools \
  --server http://localhost:5000/api/mcp \
  --user-id "test-user"

# Execute a tool
mcp-test call-tool \
  --server http://localhost:5000/api/mcp \
  --tool-name "zeitsparkasse_create_task" \
  --arguments '{"description": "Test task from MCP"}'
```

#### 3. AI Assistant Integration

**Claude Desktop Configuration** (`claude_desktop_config.json`):

```json
{
  "mcpServers": {
    "spinner-net": {
      "command": "http",
      "args": [
        "http://localhost:5000/api/mcp"
      ],
      "env": {
        "SPINNER_NET_CLIENT_ID": "claude-desktop",
        "SPINNER_NET_CLIENT_SECRET": "your-client-secret",
        "SPINNER_NET_USER_ID": "your-user-id"
      }
    }
  }
}
```

**Custom AI Assistant Integration**:

```typescript
import { MCPClient } from '@modelcontextprotocol/client';

class SpinnerNetMCPClient {
  private client: MCPClient;
  
  constructor(serverUrl: string, credentials: MCPCredentials) {
    this.client = new MCPClient(serverUrl, credentials);
  }
  
  async createTask(description: string): Promise<CreateTaskResult> {
    return await this.client.callTool('zeitsparkasse_create_task', {
      description,
      priority: 'medium'
    });
  }
  
  async chatWithBuddy(message: string): Promise<ChatResult> {
    return await this.client.callTool('chat_with_buddy', {
      message,
      stream: false
    });
  }
  
  async uploadImage(imageData: Buffer, metadata: ImageMetadata): Promise<UploadResult> {
    return await this.client.callTool('lichtflow_upload_image', {
      image_data: imageData.toString('base64'),
      metadata
    });
  }
}
```

### Testing MCP Integration

#### Unit Testing MCP Tools

```csharp
[TestFixture]
public class CreateTaskMcpToolTests
{
  private Mock<IMediator> _mediatorMock;
  private Mock<IAiService> _aiServiceMock;
  private CreateTaskTool.Handler _handler;
  
  [SetUp]
  public void Setup()
  {
    _mediatorMock = new Mock<IMediator>();
    _aiServiceMock = new Mock<IAiService>();
    _handler = new CreateTaskTool.Handler(_mediatorMock.Object, _aiServiceMock.Object, Mock.Of<ILogger<CreateTaskTool.Handler>>());
  }
  
  [Test]
  public async Task ExecuteAsync_ValidTask_ReturnsSuccess()
  {
    // Arrange
    var parameters = new CreateTaskTool.CreateTaskParameters
    {
      Description = "Call mom tomorrow at 3pm",
      Priority = "high"
    };
    
    _aiServiceMock.Setup(x => x.GenerateResponseAsync(It.IsAny<string>(), It.IsAny<AiContext>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync("""{"title": "Call mom", "due_date": "2024-01-15T15:00:00Z", "confidence": 0.95}""");
    
    _mediatorMock.Setup(x => x.Send(It.IsAny<CreateTask.Command>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new CreateTask.Result { Success = true, TaskId = "task123" });
    
    // Act
    var result = await _handler.ExecuteAsync(parameters, "user123");
    
    // Assert
    Assert.That(result.Success, Is.True);
    Assert.That(result.TaskId, Is.EqualTo("task123"));
    Assert.That(result.ParsedTitle, Is.EqualTo("Call mom"));
  }
  
  [Test]
  public async Task ExecuteAsync_InvalidInput_ReturnsFailure()
  {
    // Arrange
    var parameters = new CreateTaskTool.CreateTaskParameters
    {
      Description = "", // Invalid empty description
      Priority = "invalid"
    };
    
    // Act
    var result = await _handler.ExecuteAsync(parameters, "user123");
    
    // Assert
    Assert.That(result.Success, Is.False);
  }
}
```

#### Integration Testing

```csharp
[TestFixture]
public class McpServerIntegrationTests
{
  private WebApplicationFactory<Program> _factory;
  private HttpClient _client;
  
  [SetUp]
  public void Setup()
  {
    _factory = new WebApplicationFactory<Program>()
      .WithWebHostBuilder(builder =>
      {
        builder.ConfigureServices(services =>
        {
          // Use in-memory database for testing
          services.AddSingleton<ICosmosRepository<TaskDocument>>(Mock.Of<ICosmosRepository<TaskDocument>>());
        });
      });
    
    _client = _factory.CreateClient();
  }
  
  [Test]
  public async Task ListTools_ValidRequest_ReturnsAvailableTools()
  {
    // Arrange
    var request = new
    {
      user_id = "test-user",
      client_id = "test-client"
    };
    
    // Act
    var response = await _client.GetAsync($"/api/mcp/tools?userId={request.user_id}&clientId={request.client_id}");
    
    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    
    var content = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<ListMcpTools.Result>(content);
    
    Assert.That(result.Success, Is.True);
    Assert.That(result.Tools, Is.Not.Empty);
  }
  
  [Test]
  public async Task ExecuteTool_CreateTask_CreatesTaskSuccessfully()
  {
    // Arrange
    var request = new
    {
      user_id = "test-user",
      client_id = "test-client",
      tool_name = "zeitsparkasse_create_task",
      arguments = new Dictionary<string, object>
      {
        ["description"] = "Test task from integration test"
      }
    };
    
    // Act
    var response = await _client.PostAsJsonAsync("/api/mcp/tools/execute", request);
    
    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    
    var content = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<ExecuteMcpTool.Result>(content);
    
    Assert.That(result.Success, Is.True);
    Assert.That(result.Result, Is.Not.Null);
  }
}
```

### Performance Testing

```csharp
[TestFixture]
public class McpPerformanceTests
{
  [Test]
  public async Task ConcurrentToolExecution_HandlesLoad()
  {
    var tasks = new List<Task>();
    var concurrentRequests = 50;
    
    for (int i = 0; i < concurrentRequests; i++)
    {
      tasks.Add(ExecuteCreateTaskTool($"Task {i}"));
    }
    
    var results = await Task.WhenAll(tasks);
    
    Assert.That(results.All(r => r.Success), Is.True);
  }
  
  private async Task<CreateTaskTool.CreateTaskResult> ExecuteCreateTaskTool(string description)
  {
    // Implementation for performance testing
    var handler = new CreateTaskTool.Handler(/* dependencies */);
    var parameters = new CreateTaskTool.CreateTaskParameters { Description = description };
    
    return await handler.ExecuteAsync(parameters, "test-user");
  }
}
```

---

## Sprint Planning

### MCP Integration Sprint Plan

#### **Sprint 6: MCP Foundation (Q2 2025)**

**Duration**: 4 weeks  
**Goal**: Establish MCP server foundation and core tools

**Week 1: MCP Server Architecture**
- ✅ MCP protocol handler implementation
- ✅ Authentication and session management
- ✅ Permission system framework
- ✅ Rate limiting and security middleware

**Week 2: Core MCP Tools**
- ✅ Task management tools (create, read, update)
- ✅ AI buddy chat integration
- ✅ User profile and context tools
- ✅ Basic asset management tools

**Week 3: Data Sovereignty Integration**
- ✅ Privacy-aware data filtering
- ✅ Local processing enforcement
- ✅ Permission granularity controls
- ✅ Data export/import tools

**Week 4: Testing and Documentation**
- ✅ Comprehensive unit testing
- ✅ Integration testing with AI assistants
- ✅ Performance testing and optimization
- ✅ Developer documentation and examples

#### **Sprint 7: MCP Client & External Integrations (Q3 2025)**

**Duration**: 4 weeks  
**Goal**: Enable Spinner.Net to connect to external MCP servers

**Week 1: MCP Client Framework**
- ✅ MCP client service implementation
- ✅ External server discovery and connection
- ✅ Credential management and storage
- ✅ Connection monitoring and health checks

**Week 2: Core External Integrations**
- ✅ Calendar systems (Google, Outlook, Apple)
- ✅ Communication platforms (Slack, Teams)
- ✅ Note-taking tools (Notion, Obsidian)
- ✅ Project management (Asana, Linear)

**Week 3: Workflow Automation**
- ✅ Cross-service workflow engine
- ✅ AI-powered workflow suggestions
- ✅ Task-to-calendar automation
- ✅ Smart notification routing

**Week 4: Advanced Features**
- ✅ Multi-service data aggregation
- ✅ Conflict resolution and synchronization
- ✅ Offline capability and queuing
- ✅ Error handling and retry logic

#### **Sprint 8: AI-Enhanced MCP Ecosystem (Q4 2025)**

**Duration**: 4 weeks  
**Goal**: Advanced AI-powered MCP features and ecosystem

**Week 1: Enhanced AI Buddy Integration**
- ✅ Proactive workflow suggestions
- ✅ Cross-service context awareness
- ✅ Intelligent action chaining
- ✅ Learning from user patterns

**Week 2: Advanced Data Processing**
- ✅ Multi-modal content processing
- ✅ Intelligent data transformation
- ✅ Semantic search across services
- ✅ Automated categorization and tagging

**Week 3: Business Intelligence Integration**
- ✅ Analytics and reporting tools
- ✅ Productivity insights across platforms
- ✅ Performance optimization suggestions
- ✅ Custom dashboard creation

**Week 4: Ecosystem Expansion**
- ✅ Developer SDK for custom tools
- ✅ Plugin marketplace framework
- ✅ Community tool sharing
- ✅ Enterprise deployment options

### Development Priorities

#### **High Priority (Sprint 6)**
1. **Security Foundation**: Authentication, authorization, and rate limiting
2. **Core Task Tools**: Enable basic task management through MCP
3. **AI Buddy Integration**: Allow external assistants to chat with user's buddy
4. **Data Privacy**: Ensure user control over data exposure

#### **Medium Priority (Sprint 7)**
1. **External Service Integration**: Connect to popular productivity tools
2. **Workflow Automation**: Enable cross-service task automation
3. **Performance Optimization**: Handle concurrent requests efficiently
4. **Error Recovery**: Robust error handling and retry mechanisms

#### **Low Priority (Sprint 8)**
1. **Advanced AI Features**: Proactive suggestions and learning
2. **Custom Tool Development**: SDK for third-party developers
3. **Enterprise Features**: Advanced security and administration
4. **Analytics and Insights**: Usage patterns and optimization

### Resource Allocation

#### **Team Structure**
- **Backend Developer**: MCP protocol implementation, security
- **AI Integration Specialist**: AI buddy enhancement, workflow automation
- **Frontend Developer**: Permission management UI, tool configuration
- **DevOps Engineer**: Deployment, monitoring, and scaling

#### **Technology Dependencies**
- **Cosmos DB**: Enhanced indexing for MCP data
- **SignalR**: Real-time communication for MCP events
- **Azure Key Vault**: Secure credential storage for external services
- **Application Insights**: Monitoring and performance tracking

#### **Success Metrics**
- **Tool Execution Latency**: < 500ms for simple tools, < 2s for complex tools
- **Permission Granularity**: 100% of user data access controllable
- **External Service Coverage**: 15+ popular productivity tools integrated
- **User Adoption**: 80% of users connect at least one external service

---

## Technical Implementation

### MCP Protocol Implementation

#### Core Protocol Handler

```csharp
public interface IMcpProtocolHandler
{
    Task<McpResponse> HandleRequestAsync(McpRequest request);
    Task<McpToolsResponse> ListToolsAsync(McpListToolsRequest request);
    Task<McpCallToolResponse> CallToolAsync(McpCallToolRequest request);
    Task<McpResourcesResponse> ListResourcesAsync(McpListResourcesRequest request);
    Task<McpPromptResponse> GetPromptAsync(McpGetPromptRequest request);
}

public class McpProtocolHandler : IMcpProtocolHandler
{
    private readonly IMcpToolRegistry _toolRegistry;
    private readonly IMcpPermissionService _permissionService;
    private readonly IMcpAuthenticationService _authService;
    private readonly ILogger<McpProtocolHandler> _logger;
    
    public async Task<McpToolsResponse> ListToolsAsync(McpListToolsRequest request)
    {
        try
        {
            // Validate session
            var session = await _authService.ValidateSessionAsync(request.SessionToken);
            if (session == null)
            {
                return McpToolsResponse.Unauthorized();
            }
            
            // Get available tools for user
            var tools = await _toolRegistry.GetAvailableToolsAsync(session.UserId);
            
            // Filter by permissions
            var authorizedTools = new List<McpTool>();
            foreach (var tool in tools)
            {
                var hasAllPermissions = true;
                foreach (var permission in tool.RequiredPermissions)
                {
                    if (!await _permissionService.HasPermissionAsync(session.UserId, session.ClientId, permission))
                    {
                        hasAllPermissions = false;
                        break;
                    }
                }
                
                if (hasAllPermissions)
                {
                    authorizedTools.Add(new McpTool
                    {
                        Name = tool.Name,
                        Description = tool.Description,
                        InputSchema = tool.InputSchema
                    });
                }
            }
            
            return McpToolsResponse.Success(authorizedTools);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing MCP tools");
            return McpToolsResponse.Error("Failed to list tools");
        }
    }
    
    public async Task<McpCallToolResponse> CallToolAsync(McpCallToolRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Validate session
            var session = await _authService.ValidateSessionAsync(request.SessionToken);
            if (session == null)
            {
                return McpCallToolResponse.Unauthorized();
            }
            
            // Rate limiting
            var rateLimitResult = await _rateLimitService.CheckRateLimitAsync(
                session.UserId, session.ClientId, request.Name);
                
            if (!rateLimitResult.IsAllowed)
            {
                return McpCallToolResponse.RateLimited(rateLimitResult.RetryAfterSeconds);
            }
            
            // Execute tool
            var result = await _toolRegistry.ExecuteToolAsync(
                request.Name, 
                request.Arguments, 
                session.UserId);
                
            stopwatch.Stop();
            
            return McpCallToolResponse.Success(result, stopwatch.ElapsedMilliseconds);
        }
        catch (McpToolNotFoundException ex)
        {
            return McpCallToolResponse.ToolNotFound(ex.ToolName);
        }
        catch (McpPermissionDeniedException ex)
        {
            return McpCallToolResponse.PermissionDenied(ex.Permission.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling MCP tool {ToolName}", request.Name);
            return McpCallToolResponse.Error("Tool execution failed");
        }
    }
}
```

#### Tool Registry Implementation

```csharp
public class McpToolRegistry : IMcpToolRegistry
{
    private readonly Dictionary<string, IMcpToolDefinition> _tools = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<McpToolRegistry> _logger;
    
    public void RegisterTool<TInput>(McpToolDefinition<TInput> toolDefinition) where TInput : class
    {
        _tools[toolDefinition.Name] = toolDefinition;
        _logger.LogInformation("Registered MCP tool: {ToolName}", toolDefinition.Name);
    }
    
    public async Task<object?> ExecuteToolAsync(string toolName, Dictionary<string, object> arguments, string userId)
    {
        if (!_tools.TryGetValue(toolName, out var toolDefinition))
        {
            throw new McpToolNotFoundException(toolName);
        }
        
        try
        {
            // Validate permissions
            foreach (var permission in toolDefinition.RequiredPermissions)
            {
                var hasPermission = await toolDefinition.PermissionValidator(userId);
                if (!hasPermission)
                {
                    throw new McpPermissionDeniedException(permission);
                }
            }
            
            // Deserialize arguments to input type
            var json = JsonSerializer.Serialize(arguments);
            var inputType = toolDefinition.GetInputType();
            var input = JsonSerializer.Deserialize(json, inputType);
            
            if (input == null)
            {
                throw new ArgumentException("Invalid tool input arguments");
            }
            
            // Execute tool handler
            var result = await toolDefinition.ExecuteAsync(input, userId);
            
            _logger.LogInformation("Executed MCP tool {ToolName} for user {UserId}", toolName, userId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing MCP tool {ToolName} for user {UserId}", toolName, userId);
            throw;
        }
    }
    
    public async Task<List<McpToolInfo>> GetAvailableToolsAsync(string userId)
    {
        var tools = new List<McpToolInfo>();
        
        foreach (var kvp in _tools)
        {
            var toolDefinition = kvp.Value;
            
            tools.Add(new McpToolInfo
            {
                Name = toolDefinition.Name,
                Description = toolDefinition.Description,
                Category = toolDefinition.Category,
                InputSchema = toolDefinition.InputSchema,
                RequiredPermissions = toolDefinition.RequiredPermissions
            });
        }
        
        return tools;
    }
}
```

#### Advanced Tool Implementation Example

```csharp
namespace SpinnerNet.Core.Features.MCP.Tools.Advanced;

public static class ScheduleOptimizedWorkflow
{
    public record Parameters
    {
        [JsonPropertyName("task_ids")]
        public List<string> TaskIds { get; init; } = new();
        
        [JsonPropertyName("available_time_slots")]
        public List<TimeSlot> AvailableTimeSlots { get; init; } = new();
        
        [JsonPropertyName("preferences")]
        public SchedulingPreferences Preferences { get; init; } = new();
    }
    
    public record TimeSlot
    {
        [JsonPropertyName("start_time")]
        public DateTime StartTime { get; init; }
        
        [JsonPropertyName("end_time")]
        public DateTime EndTime { get; init; }
        
        [JsonPropertyName("priority")]
        public int Priority { get; init; } = 1;
    }
    
    public record SchedulingPreferences
    {
        [JsonPropertyName("focus_time_preference")]
        public string FocusTimePreference { get; init; } = "morning"; // morning, afternoon, evening
        
        [JsonPropertyName("break_duration_minutes")]
        public int BreakDurationMinutes { get; init; } = 15;
        
        [JsonPropertyName("max_consecutive_hours")]
        public int MaxConsecutiveHours { get; init; } = 3;
        
        [JsonPropertyName("consider_energy_levels")]
        public bool ConsiderEnergyLevels { get; init; } = true;
    }
    
    public record Result
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }
        
        [JsonPropertyName("scheduled_tasks")]
        public List<ScheduledTask> ScheduledTasks { get; init; } = new();
        
        [JsonPropertyName("optimization_score")]
        public double OptimizationScore { get; init; }
        
        [JsonPropertyName("recommendations")]
        public List<string> Recommendations { get; init; } = new();
        
        [JsonPropertyName("calendar_events_created")]
        public List<string> CalendarEventIds { get; init; } = new();
    }
    
    public record ScheduledTask
    {
        [JsonPropertyName("task_id")]
        public string TaskId { get; init; } = string.Empty;
        
        [JsonPropertyName("scheduled_start")]
        public DateTime ScheduledStart { get; init; }
        
        [JsonPropertyName("scheduled_end")]
        public DateTime ScheduledEnd { get; init; }
        
        [JsonPropertyName("confidence")]
        public double Confidence { get; init; }
        
        [JsonPropertyName("reason")]
        public string Reason { get; init; } = string.Empty;
    }
    
    public class Handler : IMcpToolHandler<Parameters, Result>
    {
        private readonly IMediator _mediator;
        private readonly IAiService _aiService;
        private readonly IMcpClientService _mcpClient;
        private readonly ILogger<Handler> _logger;
        
        public async Task<Result> ExecuteAsync(
            Parameters parameters, 
            string userId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                // 1. Get task details
                var tasks = await GetTaskDetails(parameters.TaskIds, userId);
                
                // 2. Use AI to optimize scheduling
                var optimizedSchedule = await GenerateOptimizedSchedule(
                    tasks, 
                    parameters.AvailableTimeSlots, 
                    parameters.Preferences, 
                    userId);
                
                // 3. Create calendar events for each scheduled task
                var calendarEventIds = new List<string>();
                foreach (var scheduledTask in optimizedSchedule.ScheduledTasks)
                {
                    var eventResult = await _mcpClient.CallServerToolAsync(
                        "google_calendar", 
                        "create_event",
                        new Dictionary<string, object>
                        {
                            ["title"] = $"🎯 {scheduledTask.TaskTitle}",
                            ["start_time"] = scheduledTask.ScheduledStart.ToString("O"),
                            ["end_time"] = scheduledTask.ScheduledEnd.ToString("O"),
                            ["description"] = $"Optimized scheduling: {scheduledTask.Reason}"
                        },
                        userId);
                        
                    if (eventResult != null)
                    {
                        var eventId = eventResult.GetProperty("event_id")?.ToString();
                        if (!string.IsNullOrEmpty(eventId))
                        {
                            calendarEventIds.Add(eventId);
                        }
                    }
                }
                
                // 4. Update tasks with scheduling information
                await UpdateTasksWithSchedule(optimizedSchedule.ScheduledTasks, userId);
                
                return new Result
                {
                    Success = true,
                    ScheduledTasks = optimizedSchedule.ScheduledTasks,
                    OptimizationScore = optimizedSchedule.OptimizationScore,
                    Recommendations = optimizedSchedule.Recommendations,
                    CalendarEventIds = calendarEventIds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in optimized workflow scheduling for user {UserId}", userId);
                return new Result { Success = false };
            }
        }
        
        private async Task<OptimizedScheduleResult> GenerateOptimizedSchedule(
            List<TaskDetails> tasks, 
            List<TimeSlot> availableSlots, 
            SchedulingPreferences preferences, 
            string userId)
        {
            var systemPrompt = """
                You are an expert time management AI. Optimize task scheduling based on:
                1. Task priority, duration, and complexity
                2. User's energy level preferences
                3. Available time slots
                4. Scheduling preferences (breaks, max consecutive hours)
                
                Consider:
                - High-priority tasks during peak energy times
                - Group similar tasks together
                - Respect break requirements
                - Balance workload across time slots
                
                Return optimized schedule with reasoning.
                """;
                
            var prompt = $"""
                Tasks to schedule:
                {JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true })}
                
                Available time slots:
                {JsonSerializer.Serialize(availableSlots, new JsonSerializerOptions { WriteIndented = true })}
                
                User preferences:
                {JsonSerializer.Serialize(preferences, new JsonSerializerOptions { WriteIndented = true })}
                
                Generate an optimized schedule in JSON format:
                {{
                    "scheduled_tasks": [
                        {{
                            "task_id": "task_id",
                            "scheduled_start": "2024-01-15T09:00:00Z",
                            "scheduled_end": "2024-01-15T10:30:00Z",
                            "confidence": 0.95,
                            "reason": "High priority task scheduled during peak energy time"
                        }}
                    ],
                    "optimization_score": 0.87,
                    "recommendations": [
                        "Consider blocking 15-minute buffers between complex tasks",
                        "Schedule creative tasks during your peak energy time"
                    ]
                }}
                """;
                
            var context = new AiContext
            {
                UserId = userId,
                SystemPrompt = systemPrompt,
                PreferredProvider = AiProvider.LocalFirst, // Personal scheduling data
                RequiresPrivacy = true,
                MaxTokens = 2048
            };
            
            var response = await _aiService.GenerateResponseAsync(prompt, context);
            return JsonSerializer.Deserialize<OptimizedScheduleResult>(response);
        }
    }
}
```

### Cross-Service Data Synchronization

```csharp
public class McpDataSynchronizationService
{
    private readonly IMcpClientService _mcpClient;
    private readonly ICosmosRepository<SyncStateDocument> _syncRepository;
    private readonly ILogger<McpDataSynchronizationService> _logger;
    
    public async Task<SyncResult> SynchronizeUserDataAsync(string userId, List<string> serviceIds)
    {
        var syncResults = new List<ServiceSyncResult>();
        
        foreach (var serviceId in serviceIds)
        {
            try
            {
                var lastSync = await GetLastSyncTime(userId, serviceId);
                var serviceResult = await SynchronizeService(userId, serviceId, lastSync);
                syncResults.Add(serviceResult);
                
                await UpdateSyncState(userId, serviceId, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync service {ServiceId} for user {UserId}", serviceId, userId);
                syncResults.Add(new ServiceSyncResult
                {
                    ServiceId = serviceId,
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }
        
        return new SyncResult
        {
            Success = syncResults.All(r => r.Success),
            ServiceResults = syncResults,
            SyncedAt = DateTime.UtcNow
        };
    }
    
    private async Task<ServiceSyncResult> SynchronizeService(string userId, string serviceId, DateTime? lastSync)
    {
        return serviceId switch
        {
            "google_calendar" => await SyncGoogleCalendar(userId, lastSync),
            "notion" => await SyncNotion(userId, lastSync),
            "github" => await SyncGitHub(userId, lastSync),
            "slack" => await SyncSlack(userId, lastSync),
            _ => new ServiceSyncResult { ServiceId = serviceId, Success = false, ErrorMessage = "Unknown service" }
        };
    }
    
    private async Task<ServiceSyncResult> SyncGoogleCalendar(string userId, DateTime? lastSync)
    {
        // Get calendar events since last sync
        var events = await _mcpClient.CallServerToolAsync(
            "google_calendar",
            "list_events",
            new Dictionary<string, object>
            {
                ["updated_since"] = lastSync?.ToString("O") ?? DateTime.UtcNow.AddDays(-30).ToString("O"),
                ["max_results"] = 100
            },
            userId);
            
        if (events == null) return new ServiceSyncResult { ServiceId = "google_calendar", Success = false };
        
        // Process and store events
        var calendarEvents = JsonSerializer.Deserialize<List<CalendarEvent>>(events.ToString());
        var processedCount = 0;
        
        foreach (var calendarEvent in calendarEvents)
        {
            // Create or update corresponding task in Spinner.Net
            if (ShouldCreateTaskFromEvent(calendarEvent))
            {
                await CreateTaskFromCalendarEvent(calendarEvent, userId);
                processedCount++;
            }
        }
        
        return new ServiceSyncResult
        {
            ServiceId = "google_calendar",
            Success = true,
            ItemsProcessed = processedCount,
            Details = $"Processed {processedCount} calendar events"
        };
    }
}
```

---

## Performance & Monitoring

### Performance Optimization

#### Caching Strategy

```csharp
public class CachedMcpToolRegistry : IMcpToolRegistry
{
    private readonly IMcpToolRegistry _baseRegistry;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    
    public async Task<List<McpToolInfo>> GetAvailableToolsAsync(string userId)
    {
        var cacheKey = $"mcp_tools:{userId}";
        var cacheDuration = _configuration.GetValue<TimeSpan>("MCP:ToolsCacheDuration", TimeSpan.FromMinutes(15));
        
        if (_cache.TryGetValue(cacheKey, out List<McpToolInfo> cachedTools))
        {
            return cachedTools;
        }
        
        var tools = await _baseRegistry.GetAvailableToolsAsync(userId);
        _cache.Set(cacheKey, tools, cacheDuration);
        
        return tools;
    }
    
    public async Task<object?> ExecuteToolAsync(string toolName, Dictionary<string, object> arguments, string userId)
    {
        // Some tools can be cached based on their arguments
        if (IsCacheableToolOperation(toolName, arguments))
        {
            var cacheKey = GenerateToolCacheKey(toolName, arguments, userId);
            var cacheDuration = GetToolCacheDuration(toolName);
            
            if (_cache.TryGetValue(cacheKey, out object cachedResult))
            {
                return cachedResult;
            }
            
            var result = await _baseRegistry.ExecuteToolAsync(toolName, arguments, userId);
            _cache.Set(cacheKey, result, cacheDuration);
            
            return result;
        }
        
        return await _baseRegistry.ExecuteToolAsync(toolName, arguments, userId);
    }
    
    private bool IsCacheableToolOperation(string toolName, Dictionary<string, object> arguments)
    {
        // Cache read-only operations
        var cacheableTools = new[]
        {
            "get_user_profile",
            "get_tasks", // when not filtering by status
            "get_buddy_context",
            "search_assets"
        };
        
        return cacheableTools.Contains(toolName);
    }
}
```

#### Connection Pooling

```csharp
public class PooledMcpClientService : IMcpClientService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ConcurrentDictionary<string, McpConnectionPool> _connectionPools = new();
    private readonly ILogger<PooledMcpClientService> _logger;
    
    public async Task<object?> CallServerToolAsync(
        string serverId, 
        string toolName, 
        Dictionary<string, object> arguments, 
        string userId)
    {
        var pool = _connectionPools.GetOrAdd(serverId, _ => new McpConnectionPool(serverId, _httpClientFactory));
        
        using var connection = await pool.GetConnectionAsync();
        return await connection.CallToolAsync(toolName, arguments);
    }
}

public class McpConnectionPool : IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private readonly ConcurrentQueue<IMcpConnection> _connections = new();
    private readonly string _serverId;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly int _maxConnections;
    
    public McpConnectionPool(string serverId, IHttpClientFactory httpClientFactory, int maxConnections = 10)
    {
        _serverId = serverId;
        _httpClientFactory = httpClientFactory;
        _maxConnections = maxConnections;
        _semaphore = new SemaphoreSlim(maxConnections, maxConnections);
    }
    
    public async Task<PooledMcpConnection> GetConnectionAsync()
    {
        await _semaphore.WaitAsync();
        
        if (_connections.TryDequeue(out var connection) && connection.IsHealthy)
        {
            return new PooledMcpConnection(connection, this);
        }
        
        // Create new connection
        var newConnection = await CreateConnectionAsync();
        return new PooledMcpConnection(newConnection, this);
    }
    
    public void ReturnConnection(IMcpConnection connection)
    {
        if (connection.IsHealthy && _connections.Count < _maxConnections)
        {
            _connections.Enqueue(connection);
        }
        else
        {
            connection.Dispose();
        }
        
        _semaphore.Release();
    }
}
```

### Monitoring and Analytics

#### MCP Usage Analytics

```csharp
public class McpAnalyticsService : IMcpAnalyticsService
{
    private readonly ICosmosRepository<McpUsageEventDocument> _usageRepository;
    private readonly ILogger<McpAnalyticsService> _logger;
    
    public async Task TrackToolUsageAsync(McpToolUsageEvent usageEvent)
    {
        var document = new McpUsageEventDocument
        {
            Id = $"usage_{Guid.NewGuid()}",
            UserId = usageEvent.UserId,
            ClientId = usageEvent.ClientId,
            ToolName = usageEvent.ToolName,
            ExecutionTimeMs = usageEvent.ExecutionTimeMs,
            Success = usageEvent.Success,
            ErrorMessage = usageEvent.ErrorMessage,
            Timestamp = DateTime.UtcNow,
            ArgumentsHash = ComputeArgumentsHash(usageEvent.Arguments)
        };
        
        await _usageRepository.CreateOrUpdateAsync(document, "system");
        
        // Real-time metrics
        if (usageEvent.Success)
        {
            _logger.LogInformation("MCP tool {ToolName} executed successfully in {ExecutionTime}ms", 
                usageEvent.ToolName, usageEvent.ExecutionTimeMs);
        }
        else
        {
            _logger.LogWarning("MCP tool {ToolName} failed: {Error}", 
                usageEvent.ToolName, usageEvent.ErrorMessage);
        }
    }
    
    public async Task<McpUsageReport> GenerateUsageReportAsync(string userId, TimeSpan period)
    {
        var since = DateTime.UtcNow.Subtract(period);
        
        var usageEvents = await _usageRepository.QueryAsync(
            x => x.UserId == userId && x.Timestamp >= since,
            maxResults: 10000);
        
        var report = new McpUsageReport
        {
            UserId = userId,
            PeriodStart = since,
            PeriodEnd = DateTime.UtcNow,
            TotalRequests = usageEvents.Count,
            SuccessfulRequests = usageEvents.Count(e => e.Success),
            FailedRequests = usageEvents.Count(e => !e.Success),
            AverageExecutionTimeMs = usageEvents.Where(e => e.Success).Average(e => e.ExecutionTimeMs),
            MostUsedTools = usageEvents
                .GroupBy(e => e.ToolName)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .ToDictionary(g => g.Key, g => g.Count()),
            ClientUsage = usageEvents
                .GroupBy(e => e.ClientId)
                .ToDictionary(g => g.Key, g => g.Count())
        };
        
        return report;
    }
}

public class McpUsageEventDocument : CosmosDocument
{
    public string UserId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public long ExecutionTimeMs { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; }
    public string ArgumentsHash { get; set; } = string.Empty;
}
```

#### Health Monitoring

```csharp
public class McpHealthCheckService : IHealthCheck
{
    private readonly IMcpToolRegistry _toolRegistry;
    private readonly IMcpClientService _mcpClient;
    private readonly ICosmosRepository<McpHealthCheckDocument> _healthRepository;
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var healthData = new Dictionary<string, object>();
        var isHealthy = true;
        var errors = new List<string>();
        
        try
        {
            // Check tool registry
            var tools = await _toolRegistry.GetAvailableToolsAsync("health-check-user");
            healthData["available_tools_count"] = tools.Count;
            
            if (tools.Count == 0)
            {
                isHealthy = false;
                errors.Add("No MCP tools available");
            }
            
            // Check external service connections
            var externalServiceHealth = await CheckExternalServicesAsync();
            healthData["external_services"] = externalServiceHealth;
            
            if (externalServiceHealth.Values.Any(h => !h.IsHealthy))
            {
                errors.Add("Some external services are unhealthy");
            }
            
            // Check database connectivity
            var dbHealth = await CheckDatabaseHealthAsync();
            healthData["database"] = dbHealth;
            
            if (!dbHealth.IsHealthy)
            {
                isHealthy = false;
                errors.Add("Database connectivity issues");
            }
            
            await RecordHealthCheckResult(isHealthy, healthData, errors);
            
            return isHealthy 
                ? HealthCheckResult.Healthy("MCP services are operational", healthData)
                : HealthCheckResult.Unhealthy($"MCP services have issues: {string.Join(", ", errors)}", null, healthData);
        }
        catch (Exception ex)
        {
            await RecordHealthCheckResult(false, healthData, new[] { ex.Message });
            return HealthCheckResult.Unhealthy("MCP health check failed", ex, healthData);
        }
    }
    
    private async Task<Dictionary<string, ServiceHealth>> CheckExternalServicesAsync()
    {
        var services = new[] { "google_calendar", "notion", "github", "slack" };
        var results = new Dictionary<string, ServiceHealth>();
        
        var tasks = services.Select(async serviceId =>
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var healthResult = await _mcpClient.CallServerToolAsync(serviceId, "health_check", new(), "health-check-user");
                stopwatch.Stop();
                
                return new KeyValuePair<string, ServiceHealth>(serviceId, new ServiceHealth
                {
                    IsHealthy = healthResult != null,
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                    LastChecked = DateTime.UtcNow
                });
            }
            catch
            {
                return new KeyValuePair<string, ServiceHealth>(serviceId, new ServiceHealth
                {
                    IsHealthy = false,
                    LastChecked = DateTime.UtcNow
                });
            }
        });
        
        var serviceResults = await Task.WhenAll(tasks);
        return serviceResults.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}

public class ServiceHealth
{
    public bool IsHealthy { get; set; }
    public long? ResponseTimeMs { get; set; }
    public DateTime LastChecked { get; set; }
    public string? ErrorMessage { get; set; }
}
```

### Performance Benchmarking

```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class McpPerformanceBenchmarks
{
    private IMcpToolRegistry _toolRegistry;
    private Dictionary<string, object> _testArguments;
    
    [GlobalSetup]
    public void Setup()
    {
        // Initialize test dependencies
        _toolRegistry = CreateTestToolRegistry();
        _testArguments = new Dictionary<string, object>
        {
            ["description"] = "Test task for benchmarking"
        };
    }
    
    [Benchmark]
    public async Task<object?> ExecuteCreateTaskTool()
    {
        return await _toolRegistry.ExecuteToolAsync("zeitsparkasse_create_task", _testArguments, "benchmark-user");
    }
    
    [Benchmark]
    public async Task<object?> ExecuteChatWithBuddyTool()
    {
        var arguments = new Dictionary<string, object>
        {
            ["message"] = "Hello, how can you help me today?"
        };
        
        return await _toolRegistry.ExecuteToolAsync("chat_with_buddy", arguments, "benchmark-user");
    }
    
    [Benchmark]
    [Arguments(1)]
    [Arguments(10)]
    [Arguments(50)]
    public async Task ConcurrentToolExecution(int concurrency)
    {
        var tasks = Enumerable.Range(0, concurrency)
            .Select(_ => _toolRegistry.ExecuteToolAsync("zeitsparkasse_create_task", _testArguments, "benchmark-user"))
            .ToArray();
            
        await Task.WhenAll(tasks);
    }
}
```

---

## Future Roadmap

### Phase 1: Foundation (Q2 2025)
**Status: ✅ Planned for Sprint 6**

- ✅ MCP server implementation with core tools
- ✅ Authentication and permission system
- ✅ Basic AI buddy integration
- ✅ Task management tools
- ✅ Data sovereignty controls

### Phase 2: External Integration (Q3 2025)
**Status: 📋 Planned for Sprint 7**

- 📋 MCP client implementation
- 📋 Calendar system integrations (Google, Outlook, Apple)
- 📋 Communication platform integrations (Slack, Teams, Discord)
- 📋 Note-taking tool integrations (Notion, Obsidian, Roam)
- 📋 Project management integrations (Asana, Linear, Trello)

### Phase 3: AI Enhancement (Q4 2025)
**Status: 📋 Planned for Sprint 8**

- 📋 Proactive workflow suggestions
- 📋 Cross-service data aggregation
- 📋 Intelligent workflow automation
- 📋 Advanced AI buddy capabilities
- 📋 Learning and adaptation systems

### Phase 4: Ecosystem Expansion (2026)

#### Developer Platform
- 🔮 MCP tool development SDK
- 🔮 Custom tool marketplace
- 🔮 Third-party tool certification
- 🔮 Community tool sharing

#### Enterprise Features
- 🔮 Advanced security and compliance
- 🔮 Multi-tenant tool management
- 🔮 Enterprise SSO integration
- 🔮 Advanced analytics and reporting

#### Advanced AI Capabilities
- 🔮 Multi-modal content processing
- 🔮 Semantic understanding across services
- 🔮 Predictive workflow optimization
- 🔮 Natural language service orchestration

### Phase 5: ZeitCoin Integration (Q3 2026)
**Status: 🔮 Future Vision**

#### Cryptocurrency-Powered MCP Economy
- 🔮 ZeitCoin rewards for tool usage optimization
- 🔮 Service credits earned through time management
- 🔮 Marketplace for premium MCP tools and services
- 🔮 Decentralized tool development incentives

#### Economic Workflows
- 🔮 Time-to-value calculation across all MCP services
- 🔮 Automated time investment recommendations
- 🔮 Cross-service productivity ROI tracking
- 🔮 ZeitCoin-based service subscriptions

### Long-term Vision (2027+)

#### Global MCP Ecosystem
- 🔮 Standard protocol for productivity tool interoperability
- 🔮 Open-source MCP tool library
- 🔮 AI assistant marketplace integration
- 🔮 Cross-platform workflow portability

#### Advanced Intelligence
- 🔮 Federated learning across user workflows
- 🔮 Global productivity pattern analysis
- 🔮 AI-powered time management consulting
- 🔮 Predictive life optimization

#### Technology Evolution
- 🔮 Edge computing for real-time MCP processing
- 🔮 IoT device integration for ambient computing
- 🔮 AR/VR interfaces for immersive workflow management
- 🔮 Brain-computer interface exploration

---

## Conclusion

The MCP integration for Spinner.Net represents a fundamental shift toward an interconnected, AI-powered productivity ecosystem while maintaining user sovereignty and privacy as core principles. This implementation enables:

### Immediate Benefits (2025)
- **Universal AI Assistant Integration**: Any MCP-compatible AI can access Spinner.Net capabilities
- **Workflow Automation**: Seamless task and calendar synchronization across services
- **Privacy-First Design**: Complete user control over data exposure and AI processing
- **Modular Extensibility**: Each Spinner.Net module contributes its own MCP tools

### Medium-term Impact (2026)
- **Productivity Revolution**: AI assistants become true workflow orchestrators
- **Data Liberation**: Users own and control their productivity data across all platforms
- **Service Interoperability**: Breaking down silos between productivity tools
- **Intelligent Automation**: AI-powered optimization of daily workflows

### Long-term Vision (2027+)
- **ZeitCoin Economy**: Cryptocurrency rewards for optimal time management
- **Global Standards**: MCP becomes the standard for productivity tool integration
- **Advanced Intelligence**: Federated learning for global productivity optimization
- **Life Optimization**: AI assistants that understand and optimize entire life patterns

The MCP integration positions Spinner.Net as the central hub for AI-powered life management, enabling the vision of the world's first Time-to-Value Digital Economy while maintaining the highest standards of user privacy and data sovereignty.

This comprehensive implementation guide provides the foundation for building a revolutionary MCP ecosystem that respects user autonomy while unleashing the full potential of AI-powered productivity tools.