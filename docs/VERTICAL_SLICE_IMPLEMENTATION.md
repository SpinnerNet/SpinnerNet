# Vertical Slice Implementation Guide - Spinner.Net Sprint 1

## Overview

This guide provides step-by-step instructions for implementing the 15 Sprint 1 vertical slices using the **Command → Validation → Handler → Endpoint** pattern with MediatR and Cosmos DB.

**Goal**: Create self-contained, testable features that implement complete user scenarios from API to database.

## Vertical Slice Architecture Pattern

### Core Pattern Structure

Each vertical slice is a **complete feature in a single file**:

```csharp
namespace SpinnerNet.Core.Features.{Area};

public static class {FeatureName}
{
    // 1. Command/Query (Input)
    public record Command : IRequest<Result> { ... }
    
    // 2. Result (Output)
    public record Result { ... }
    
    // 3. Validator (Input Validation)
    public class Validator : AbstractValidator<Command> { ... }
    
    // 4. Handler (Business Logic + Data Access)
    public class Handler : IRequestHandler<Command, Result> { ... }
    
    // 5. Endpoint (HTTP API)
    [ApiController]
    public class Endpoint : ControllerBase { ... }
}
```

### Key Principles

- **Self-contained**: Everything for one feature in one file
- **No shared repositories**: Direct Cosmos DB access in handlers
- **Strong typing**: Explicit command/result types
- **Validation first**: FluentValidation for all inputs
- **Error handling**: Consistent success/failure patterns

## Step-by-Step Implementation

### Step 1: Create the Feature Directory

```bash
# Navigate to project root
cd src/SpinnerNet.Core

# Create feature directory structure
mkdir -p Features/Users
mkdir -p Features/PersonaInterview
mkdir -p Features/Tasks
mkdir -p Features/Goals
mkdir -p Features/Buddies
mkdir -p Features/AI
```

### Step 2: Implement Your First Slice - RegisterUser

**Create `Features/Users/RegisterUser.cs`:**

```csharp
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using SpinnerNet.Shared.Models.CosmosDb;
using System.Text.Json.Serialization;

namespace SpinnerNet.Core.Features.Users;

/// <summary>
/// Vertical slice for user registration with age verification and data sovereignty
/// </summary>
public static class RegisterUser
{
    // 1. Command (Input) - What the user sends
    public record Command : IRequest<Result>
    {
        [JsonPropertyName("email")]
        public string Email { get; init; } = string.Empty;

        [JsonPropertyName("displayName")]
        public string DisplayName { get; init; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; init; } = string.Empty;

        [JsonPropertyName("birthDate")]
        public DateTime BirthDate { get; init; }

        [JsonPropertyName("language")]
        public string Language { get; init; } = "en";

        [JsonPropertyName("timezone")]
        public string Timezone { get; init; } = "UTC";

        [JsonPropertyName("acceptTerms")]
        public bool AcceptTerms { get; init; }

        [JsonPropertyName("acceptPrivacyPolicy")]
        public bool AcceptPrivacyPolicy { get; init; }

        // Parental consent for minors
        [JsonPropertyName("parentalEmail")]
        public string? ParentalEmail { get; init; }

        [JsonPropertyName("parentalConsentToken")]
        public string? ParentalConsentToken { get; init; }

        [JsonPropertyName("isParentalConsentVerified")]
        public bool IsParentalConsentVerified { get; init; }
    }

    // 2. Result (Output) - What we return to the user
    public record Result
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }

        [JsonPropertyName("userId")]
        public string? UserId { get; init; }

        [JsonPropertyName("user")]
        public UserSummary? User { get; init; }

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; init; }

        [JsonPropertyName("validationErrors")]
        public Dictionary<string, string[]>? ValidationErrors { get; init; }

        public static Result SuccessResult(UserDocument user) =>
            new() 
            { 
                Success = true, 
                UserId = user.UserId, 
                User = UserSummary.FromDocument(user) 
            };

        public static Result Failure(string errorMessage) =>
            new() { Success = false, ErrorMessage = errorMessage };

        public static Result ValidationFailure(Dictionary<string, string[]> validationErrors) =>
            new() { Success = false, ValidationErrors = validationErrors };
    }

    public record UserSummary
    {
        [JsonPropertyName("userId")]
        public string UserId { get; init; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; init; } = string.Empty;

        [JsonPropertyName("displayName")]
        public string DisplayName { get; init; } = string.Empty;

        [JsonPropertyName("age")]
        public int Age { get; init; }

        [JsonPropertyName("isMinor")]
        public bool IsMinor { get; init; }

        [JsonPropertyName("language")]
        public string Language { get; init; } = string.Empty;

        public static UserSummary FromDocument(UserDocument doc) =>
            new()
            {
                UserId = doc.UserId,
                Email = doc.Email,
                DisplayName = doc.DisplayName,
                Age = doc.Age,
                IsMinor = doc.IsMinor,
                Language = doc.Preferences.Language
            };
    }

    // 3. Validator (Input Validation) - Validate before processing
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address")
                .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("Display name is required")
                .MinimumLength(2).WithMessage("Display name must be at least 2 characters")
                .MaximumLength(100).WithMessage("Display name must not exceed 100 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .MaximumLength(128).WithMessage("Password must not exceed 128 characters")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character");

            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("Birth date is required")
                .LessThan(DateTime.UtcNow).WithMessage("Birth date must be in the past")
                .GreaterThan(DateTime.UtcNow.AddYears(-120)).WithMessage("Birth date must be within reasonable range")
                .Must(BeAtLeastMinimumAge).WithMessage("You must be at least 13 years old to register");

            RuleFor(x => x.Language)
                .NotEmpty().WithMessage("Language is required")
                .Length(2, 5).WithMessage("Language must be a valid language code");

            RuleFor(x => x.AcceptTerms)
                .Equal(true).WithMessage("You must accept the terms of service");

            RuleFor(x => x.AcceptPrivacyPolicy)
                .Equal(true).WithMessage("You must accept the privacy policy");

            // Parental consent validation for minors
            RuleFor(x => x.ParentalEmail)
                .NotEmpty().WithMessage("Parental email is required for users under 18")
                .EmailAddress().WithMessage("Parental email must be a valid email address")
                .When(x => CalculateAge(x.BirthDate) < 18);

            RuleFor(x => x.ParentalConsentToken)
                .NotEmpty().WithMessage("Parental consent verification is required for users under 18")
                .When(x => CalculateAge(x.BirthDate) < 18);

            RuleFor(x => x.IsParentalConsentVerified)
                .Equal(true).WithMessage("Parental consent must be verified for users under 18")
                .When(x => CalculateAge(x.BirthDate) < 18);
        }

        private static bool BeAtLeastMinimumAge(DateTime birthDate)
        {
            return CalculateAge(birthDate) >= 13;
        }

        private static int CalculateAge(DateTime birthDate)
        {
            var age = DateTime.UtcNow.Year - birthDate.Year;
            if (DateTime.UtcNow.DayOfYear < birthDate.DayOfYear) age--;
            return age;
        }
    }

    // 4. Handler (Business Logic + Data Access) - Core implementation
    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly CosmosClient _cosmosClient;
        private readonly ILogger<Handler> _logger;

        public Handler(CosmosClient cosmosClient, ILogger<Handler> logger)
        {
            _cosmosClient = cosmosClient;
            _logger = logger;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting user registration for email: {Email}", request.Email);

                // Get Cosmos container
                var container = _cosmosClient.GetContainer("SpinnerNetDev", "Users");

                // 1. Calculate user age
                var age = CalculateAge(request.BirthDate);
                var isMinor = age < 18;

                _logger.LogInformation("User registration: Age {Age}, IsMinor: {IsMinor}", age, isMinor);

                // 2. Verify parental consent for minors
                if (isMinor)
                {
                    if (!request.IsParentalConsentVerified || 
                        string.IsNullOrEmpty(request.ParentalEmail) || 
                        string.IsNullOrEmpty(request.ParentalConsentToken))
                    {
                        _logger.LogWarning("Registration attempt for minor without proper parental consent");
                        return Result.Failure("Parental consent verification is required for users under 18");
                    }
                }

                // 3. Check if user already exists
                var existingUserQuery = new QueryDefinition(
                    "SELECT * FROM c WHERE c.email = @email")
                    .WithParameter("@email", request.Email.ToLowerInvariant());

                var existingUsers = container.GetItemQueryIterator<UserDocument>(existingUserQuery);
                
                if (await existingUsers.ReadNextAsync(cancellationToken) is { Count: > 0 })
                {
                    _logger.LogWarning("Registration attempt for existing email: {Email}", request.Email);
                    return Result.Failure("A user with this email address already exists");
                }

                // 4. Generate user ID
                var userId = $"user_{Guid.NewGuid():N}";

                // 5. Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // 6. Create user document
                var userDocument = new UserDocument
                {
                    Id = $"user_{userId}",
                    UserId = userId,
                    Email = request.Email.ToLowerInvariant(),
                    DisplayName = request.DisplayName,
                    Age = age,
                    BirthDate = request.BirthDate,
                    IsMinor = isMinor,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    AuthProviders = new List<AuthProvider>
                    {
                        new()
                        {
                            Provider = "local",
                            ProviderId = userId,
                            Email = request.Email.ToLowerInvariant(),
                            PasswordHash = passwordHash
                        }
                    },
                    Preferences = new UserPreferences
                    {
                        Language = request.Language,
                        Timezone = request.Timezone
                    },
                    // Apply age-appropriate settings (from content safety framework)
                    SafetySettings = CreateAgeAppropriateSafetySettings(age),
                    ParentalControls = isMinor ? new ParentalControlSettings
                    {
                        ParentEmail = request.ParentalEmail,
                        ConsentVerifiedAt = DateTime.UtcNow,
                        NotificationLevel = age < 16 ? "all" : "important",
                        RequiresOversight = age < 16
                    } : null
                };

                // 7. Store user in Cosmos DB
                var response = await container.CreateItemAsync(
                    userDocument, 
                    new PartitionKey(userId), 
                    cancellationToken: cancellationToken);

                _logger.LogInformation("User registered successfully: {UserId}, Age: {Age}", userId, age);

                return Result.SuccessResult(response.Resource);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                _logger.LogWarning(ex, "Conflict during user registration for email: {Email}", request.Email);
                return Result.Failure("A user with this email address already exists");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user with email: {Email}", request.Email);
                return Result.Failure("An error occurred during registration. Please try again.");
            }
        }

        private static int CalculateAge(DateTime birthDate)
        {
            var age = DateTime.UtcNow.Year - birthDate.Year;
            if (DateTime.UtcNow.DayOfYear < birthDate.DayOfYear) age--;
            return age;
        }

        private static UserSafetySettings CreateAgeAppropriateSafetySettings(int age)
        {
            return new UserSafetySettings
            {
                MaxContentLevel = age switch
                {
                    < 13 => "safe",
                    < 16 => "mild",
                    < 18 => "moderate",
                    _ => "adult"
                },
                ContentFilteringEnabled = true,
                SafeModeEnabled = age < 18,
                ParentalNotificationsEnabled = age < 18,
                RestrictedTopics = age < 18 ? new List<string> 
                { 
                    "adult_content", 
                    "explicit_relationships" 
                } : new List<string>(),
                AllowedInteractionTypes = age < 13 ? new List<string> 
                { 
                    "educational", 
                    "family_safe", 
                    "creative" 
                } : new List<string>()
            };
        }
    }

    // 5. Endpoint (HTTP API) - Web API controller
    [ApiController]
    [Route("api/users")]
    public class Endpoint : ControllerBase
    {
        private readonly IMediator _mediator;

        public Endpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Register a new user with age verification and content safety
        /// </summary>
        /// <param name="command">User registration data</param>
        /// <returns>Registration result with user information</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(Result), 201)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 409)]
        public async Task<ActionResult<Result>> Register([FromBody] Command command)
        {
            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                if (result.ErrorMessage?.Contains("already exists") == true)
                {
                    return Conflict(result);
                }

                if (result.ValidationErrors != null)
                {
                    return BadRequest(result);
                }

                return BadRequest(result);
            }

            return CreatedAtAction(
                nameof(GetUser.Endpoint.GetById), // Reference to another slice
                new { id = result.UserId },
                result);
        }
    }
}
```

### Step 3: Configure MediatR and Validation

**Update `Program.cs`:**

```csharp
// MediatR registration
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(RegisterUser).Assembly);
});

// FluentValidation registration
services.AddValidatorsFromAssembly(typeof(RegisterUser).Assembly);

// Validation pipeline
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

**Create `Common/Behaviors/ValidationBehavior.cs`:**

```csharp
using FluentValidation;
using MediatR;

namespace SpinnerNet.Core.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
```

### Step 4: Test Your Slice

**Create `Tests/Features/Users/RegisterUserTests.cs`:**

```csharp
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using SpinnerNet.Core.Features.Users;
using Xunit;

namespace SpinnerNet.Tests.Features.Users;

public class RegisterUserTests
{
    private readonly Mock<CosmosClient> _mockCosmosClient;
    private readonly Mock<ILogger<RegisterUser.Handler>> _mockLogger;
    private readonly RegisterUser.Handler _handler;

    public RegisterUserTests()
    {
        _mockCosmosClient = new Mock<CosmosClient>();
        _mockLogger = new Mock<ILogger<RegisterUser.Handler>>();
        _handler = new RegisterUser.Handler(_mockCosmosClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidAdultUser_ReturnsSuccess()
    {
        // Arrange
        var command = new RegisterUser.Command
        {
            Email = "test@example.com",
            DisplayName = "Test User",
            Password = "Password123!",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Language = "en",
            Timezone = "UTC",
            AcceptTerms = true,
            AcceptPrivacyPolicy = true
        };

        // Setup Cosmos DB mock
        var mockContainer = new Mock<Container>();
        _mockCosmosClient.Setup(x => x.GetContainer("SpinnerNetDev", "Users"))
            .Returns(mockContainer.Object);

        // Mock existing user check (return empty)
        var mockIterator = new Mock<FeedIterator<UserDocument>>();
        mockIterator.Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<FeedResponse<UserDocument>>(r => r.Count == 0));
        
        mockContainer.Setup(x => x.GetItemQueryIterator<UserDocument>(
            It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
            .Returns(mockIterator.Object);

        // Mock user creation
        var createdUser = new UserDocument { UserId = "user_123", Email = "test@example.com" };
        mockContainer.Setup(x => x.CreateItemAsync(
            It.IsAny<UserDocument>(), 
            It.IsAny<PartitionKey>(), 
            It.IsAny<ItemRequestOptions>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ItemResponse<UserDocument>>(r => r.Resource == createdUser));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.UserId.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Handle_MinorWithoutParentalConsent_ReturnsFailure()
    {
        // Arrange
        var command = new RegisterUser.Command
        {
            Email = "minor@example.com",
            DisplayName = "Minor User",
            Password = "Password123!",
            BirthDate = DateTime.UtcNow.AddYears(-15), // 15 years old
            AcceptTerms = true,
            AcceptPrivacyPolicy = true
            // Missing parental consent
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Parental consent verification is required");
    }
}
```

### Step 5: Implement Additional Slices

**Follow the same pattern for all 15 Sprint 1 slices:**

1. **Users/CreatePersona.cs** - Create persona from AI interview results
2. **PersonaInterview/StartPersonaInterview.cs** - Begin AI questionnaire
3. **PersonaInterview/ProcessInterviewResponse.cs** - Handle user responses
4. **PersonaInterview/CompletePersonaInterview.cs** - Finalize persona
5. **Tasks/CreateTask.cs** - Natural language task creation
6. **Tasks/UpdateTask.cs** - Task completion and editing
7. **Tasks/GetUserTasks.cs** - Task retrieval and display
8. **Goals/CreateGoal.cs** - Basic goal creation
9. **Goals/LinkTaskToGoal.cs** - Connect tasks to goals
10. **Buddies/CreateBuddy.cs** - AI companion creation
11. **Buddies/ChatWithBuddy.cs** - Conversation with AI
12. **Buddies/GetBuddyContext.cs** - Task-aware buddy state
13. **AI/RouteToAiProvider.cs** - Choose local vs cloud AI
14. **AI/ProcessNaturalLanguage.cs** - Parse user input

## Quick Slice Template

**Template for new slices:**

```csharp
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.Text.Json.Serialization;

namespace SpinnerNet.Core.Features.{Area};

public static class {FeatureName}
{
    public record Command : IRequest<Result>
    {
        // Input properties with JsonPropertyName attributes
    }
    
    public record Result
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }
        
        // Output properties
        
        public static Result SuccessResult(/* params */) => new() { Success = true };
        public static Result Failure(string error) => new() { Success = false };
    }
    
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            // Validation rules
        }
    }
    
    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly CosmosClient _cosmosClient;
        private readonly ILogger<Handler> _logger;
        
        public Handler(CosmosClient cosmosClient, ILogger<Handler> logger)
        {
            _cosmosClient = cosmosClient;
            _logger = logger;
        }
        
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Get Cosmos container
                var container = _cosmosClient.GetContainer("SpinnerNetDev", "{ContainerName}");
                
                // 2. Business logic
                
                // 3. Database operations
                
                // 4. Return success result
                return Result.SuccessResult(/* data */);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {FeatureName}");
                return Result.Failure("An error occurred");
            }
        }
    }
    
    [ApiController]
    [Route("api/{route}")]
    public class Endpoint : ControllerBase
    {
        private readonly IMediator _mediator;
        
        public Endpoint(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [Http{Verb}("{action}")]
        public async Task<ActionResult<Result>> {ActionName}([FromBody] Command command)
        {
            var result = await _mediator.Send(command);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
```

## Best Practices

### Error Handling

```csharp
// Consistent error handling pattern
try
{
    // Business logic
    return Result.SuccessResult(data);
}
catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
{
    return Result.Failure("Resource not found");
}
catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
{
    return Result.Failure("Resource already exists");
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error in {Feature}", nameof(FeatureName));
    return Result.Failure("An unexpected error occurred");
}
```

### Logging

```csharp
// Structured logging
_logger.LogInformation("Starting {Operation} for user {UserId}", 
    nameof(FeatureName), request.UserId);

_logger.LogWarning("Validation failed for {Feature}: {Errors}", 
    nameof(FeatureName), string.Join(", ", validationErrors));

_logger.LogError(ex, "Error in {Feature} for user {UserId}", 
    nameof(FeatureName), request.UserId);
```

### Testing

```csharp
// Arrange-Act-Assert pattern
[Fact]
public async Task Handle_ValidInput_ReturnsExpectedResult()
{
    // Arrange
    var command = new FeatureName.Command { /* test data */ };
    var expectedResult = /* expected outcome */;
    
    // Setup mocks
    SetupMocks();
    
    // Act
    var result = await _handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.Success.Should().BeTrue();
    result.Data.Should().BeEquivalentTo(expectedResult);
}
```

## Development Workflow

### 1. Create New Slice
```bash
# Copy template to new file
cp SliceTemplate.cs Features/NewArea/NewFeature.cs

# Update namespace, class name, and implement logic
```

### 2. Test Slice
```bash
# Run specific tests
dotnet test --filter "ClassName=NewFeatureTests"

# Test via HTTP
curl -X POST https://localhost:5001/api/endpoint \
  -H "Content-Type: application/json" \
  -d '{"property": "value"}'
```

### 3. Iterate
```bash
# Watch mode for rapid development
dotnet watch run --project SpinnerNet.Web
```

---

## Quick Reference

**Slice Checklist:**
- [ ] Command with validation attributes
- [ ] Result with success/failure patterns  
- [ ] Validator with comprehensive rules
- [ ] Handler with error handling and logging
- [ ] Endpoint with proper HTTP status codes
- [ ] Unit tests covering success and failure cases

**Common Patterns:**
- User ID as partition key: `new PartitionKey(userId)`
- Query by user: `SELECT * FROM c WHERE c.userId = @userId`
- Create item: `container.CreateItemAsync(item, partitionKey)`
- Update item: `container.ReplaceItemAsync(item, id, partitionKey)`
- Error results: `Result.Failure("message")`

Ready to implement all 15 Sprint 1 vertical slices! 🚀