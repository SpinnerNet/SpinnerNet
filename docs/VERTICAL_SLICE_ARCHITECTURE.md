# Vertical Slice Architecture for Spinner.Net

## Overview

Spinner.Net follows the Vertical Slice Architecture pattern, where each feature is implemented as a complete, self-contained vertical slice from API endpoint to data storage. This approach was successfully proven in the LichtFlow project and ensures maintainable, testable, and independently deployable features.

## Core Principles

### 1. Feature-Complete Slices
Each slice contains everything needed for a single feature:
- **Command/Query** (Input model)
- **Result** (Output model) 
- **Validator** (Input validation using FluentValidation)
- **Handler** (Business logic + data access)
- **Endpoint** (HTTP API controller)

### 2. Single File Implementation
Each feature is implemented in a single `.cs` file, making it easy to:
- Understand the complete feature flow
- Test in isolation
- Develop features in parallel
- Maintain and refactor independently

### 3. MediatR CQRS Pattern
All slices use MediatR for:
- Command/Query separation
- Dependency injection
- Request/Response handling
- Validation pipeline

## Slice Structure

```csharp
namespace SpinnerNet.Core.Features.{Area};

public static class {FeatureName}
{
    // 1. Command/Query (Input)
    public record Command : IRequest<Result>
    {
        [JsonPropertyName("userId")] 
        public string UserId { get; init; } = string.Empty;
        // ... other properties
    }
    
    // 2. Result (Output) 
    public record Result
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }
        // ... result properties
        
        public static Result SuccessResult(...) => new() { Success = true, ... };
        public static Result Failure(string error) => new() { Success = false, ErrorMessage = error };
    }
    
    // 3. Validator (Input Validation)
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.UserId).NotEmpty().MaximumLength(100);
            // ... validation rules
        }
    }
    
    // 4. Handler (Business Logic + Data Access)
    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ICosmosRepository<Document> _repository;
        private readonly ILogger<Handler> _logger;
        
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                // Business logic
                // Direct Cosmos DB operations
                // Return result
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Feature}", nameof(FeatureName));
                return Result.Failure("Error message");
            }
        }
    }
    
    // 5. Endpoint (HTTP API)
    [ApiController]
    [Route("api/{area}")]
    public class Endpoint : ControllerBase
    {
        private readonly IMediator _mediator;
        
        [HttpPost("{action}")]
        public async Task<ActionResult<Result>> Action([FromBody] Command command)
        {
            var result = await _mediator.Send(command);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
```

## Key Benefits

### 1. **Independent Development**
- Teams can work on different slices simultaneously
- No complex merge conflicts
- Clear feature boundaries

### 2. **Easy Testing**
- Each slice can be unit tested in isolation
- Integration tests focus on single features
- Mocking is simplified

### 3. **Performance Optimization**
- Direct Cosmos DB queries (no repository abstraction overhead)
- Feature-specific optimizations
- Minimal dependencies per slice

### 4. **Maintainability**
- Easy to understand complete feature flow
- Changes are localized to single files
- Refactoring is safer and easier

## Data Access Pattern

### Direct Cosmos Repository
```csharp
// No separate repository layer - direct Cosmos operations
var document = await _cosmosRepository.CreateOrUpdateAsync(
    entity, 
    userId, // partition key
    cancellationToken);

var results = await _cosmosRepository.QueryAsync(
    x => x.UserId == userId && x.Status == "active",
    maxResults: 10,
    cancellationToken: cancellationToken);
```

### User-First Data Sovereignty
Every handler respects user data preferences:
```csharp
var dataLocation = await _dataSovereignty.GetDataLocationAsync(userId, "tasks");
var repository = _dataSovereignty.GetRepositoryForLocation(dataLocation);
```

## Multi-Language Support

All slices support German, Spanish, and English:
```csharp
private string GetLocalizedText(string text, string language)
{
    return language switch
    {
        "de" => text switch { "Hello" => "Hallo", _ => text },
        "es" => text switch { "Hello" => "Hola", _ => text },
        _ => text
    };
}
```

## Integration Patterns

### 1. **Cross-Slice Communication**
```csharp
// Use MediatR to call other slices
var buddyResult = await _mediator.Send(new CreateBuddy.Command 
{ 
    UserId = userId, 
    PersonaId = personaId 
});
```

### 2. **Event Publishing**
```csharp
// Publish domain events for loosely coupled integration
await _eventPublisher.PublishAsync(new TaskCreatedEvent 
{ 
    TaskId = taskId, 
    UserId = userId 
});
```

### 3. **Shared Models**
Common models are in `SpinnerNet.Shared.Models.CosmosDb`:
- `UserDocument`
- `PersonaDocument`
- `TaskDocument`
- `BuddyDocument`

## Sprint 1 Implementation

### Completed Slices (8/15)
1. ✅ `RegisterUser` - User registration with email verification
2. ✅ `StartPersonaInterview` - AI-guided persona discovery
3. ✅ `ProcessInterviewResponse` - Conversational interview flow
4. ✅ `CompletePersonaInterview` - Finalize persona and generate buddy recommendations
5. ✅ `CreateTask` - Natural language task creation ("Remind me to call mom tomorrow")
6. ✅ `CreateBuddy` - AI companion creation based on persona
7. ✅ `ChatWithBuddy` - Real-time conversation with personality-aware AI
8. ✅ `SetDataResidencyPreference` - User data sovereignty controls

### Remaining Slices (7/15)
9. 🔄 `UpdateTask` - Task completion and editing
10. 🔄 `GetUserTasks` - Task retrieval and display
11. 🔄 `CreateGoal` - Basic goal creation
12. 🔄 `LinkTaskToGoal` - Connect tasks to goals
13. 🔄 `GetBuddyContext` - Task-aware buddy state
14. 🔄 `RouteToAiProvider` - Choose local vs cloud AI
15. 🔄 `ProcessNaturalLanguage` - Parse user input

## Testing Strategy

### Unit Tests
```csharp
[Fact]
public async Task CreateTask_WithValidInput_ShouldSucceed()
{
    // Arrange
    var handler = new CreateTask.Handler(_repository, _userRepo, _personaRepo, _logger);
    var command = new CreateTask.Command { UserId = "user123", Input = "Call mom tomorrow" };
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.Success.Should().BeTrue();
    result.Task.Should().NotBeNull();
    result.Task.Title.Should().Be("Call mom");
}
```

### Integration Tests
```csharp
[Fact]
public async Task CreateTaskEndpoint_WithValidRequest_ShouldReturn201()
{
    // Test complete slice including HTTP endpoint
    var response = await _client.PostAsJsonAsync("/api/tasks/create", command);
    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

## Migration from Traditional Architecture

### Before (Repository Pattern)
```csharp
// Multiple layers, interfaces, complex dependencies
public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepo;
    private readonly IUserRepository _userRepo;
    private readonly IMapper _mapper;
    // ... complex setup
}
```

### After (Vertical Slice)
```csharp
// Single file, direct data access, focused feature
public static class CreateTask
{
    // Everything needed for task creation in one place
}
```

## Best Practices

1. **Keep slices focused** - One responsibility per slice
2. **Use direct Cosmos queries** - No unnecessary abstractions
3. **Handle errors gracefully** - Always return Result types
4. **Support multi-language** - Localize user-facing text
5. **Respect data sovereignty** - Check user preferences
6. **Log appropriately** - Structured logging for monitoring
7. **Validate inputs** - Use FluentValidation consistently
8. **Return consistent results** - Use Result pattern everywhere

## Future Enhancements

- **Local LLM Integration** - AI processing without cloud dependencies
- **Real-time Updates** - SignalR integration for live features
- **Caching Strategy** - Redis integration for performance
- **Event Sourcing** - Domain events for audit trails
- **Module System** - Plugin architecture for extensibility

This architecture provides the foundation for Spinner.Net's modular, scalable, and maintainable codebase while supporting the unique requirements of data sovereignty and multi-language AI companions.