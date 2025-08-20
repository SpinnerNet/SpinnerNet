using MediatR;
using Microsoft.Extensions.Logging;
using SpinnerNet.Core.Data.CosmosDb;
using SpinnerNet.Shared.Models.CosmosDb;
using FluentValidation;

namespace SpinnerNet.Core.Features.PersonaCreation;

// Command following SpinnerNet's vertical slice patterns
public record InitializePersonaCreationCommand(
    string UserId,
    int UserAge,
    string PreferredLanguage = "en"
) : IRequest<InitializePersonaCreationResult>;

// Result following SpinnerNet's result patterns
public record InitializePersonaCreationResult(
    bool Success,
    string SessionId,
    string Message,
    List<string> InitialQuestions
);

// Validator following SpinnerNet's FluentValidation patterns
public class InitializePersonaCreationValidator : AbstractValidator<InitializePersonaCreationCommand>
{
    public InitializePersonaCreationValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.UserAge)
            .GreaterThan(0)
            .LessThan(150)
            .WithMessage("Valid age is required");

        RuleFor(x => x.PreferredLanguage)
            .NotEmpty()
            .Length(2, 5)
            .WithMessage("Valid language code is required");
    }
}

// Handler following SpinnerNet's MediatR patterns
public class InitializePersonaCreationHandler : IRequestHandler<InitializePersonaCreationCommand, InitializePersonaCreationResult>
{
    private readonly ICosmosRepository<InterviewSessionDocument> _sessionRepository;
    private readonly ILogger<InitializePersonaCreationHandler> _logger;

    public InitializePersonaCreationHandler(
        ICosmosRepository<InterviewSessionDocument> sessionRepository,
        ILogger<InitializePersonaCreationHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    public async Task<InitializePersonaCreationResult> Handle(
        InitializePersonaCreationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Create interview session using SpinnerNet's Cosmos DB patterns
            var sessionId = Guid.NewGuid().ToString();
            var session = new InterviewSessionDocument
            {
                id = $"interview_session_{sessionId}",
                type = "interview_session",
                sessionId = sessionId,
                UserId = request.UserId,
                language = request.PreferredLanguage,
                status = InterviewStatus.InProgress,
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow,
                metadata = new Dictionary<string, object>
                {
                    ["userAge"] = request.UserAge,
                    ["initializedVersion"] = "Phase1-Foundation",
                    ["sessionType"] = "persona_creation"
                },
                progress = new InterviewProgress
                {
                    currentQuestionIndex = 0,
                    totalQuestions = GetTotalQuestionsForAge(request.UserAge),
                    completionPercentage = 0.0,
                    currentSection = "basics",
                    estimatedTimeRemainingMinutes = GetEstimatedTimeForAge(request.UserAge)
                }
            };

            await _sessionRepository.CreateOrUpdateAsync(session, session.UserId, cancellationToken);

            // Generate age-appropriate initial questions
            var initialQuestions = GenerateInitialQuestions(request.UserAge, request.PreferredLanguage);

            _logger.LogInformation("Initialized persona creation session {SessionId} for user {UserId} (age: {UserAge})", 
                sessionId, request.UserId, request.UserAge);

            return new InitializePersonaCreationResult(
                Success: true,
                SessionId: sessionId,
                Message: "Persona creation session initialized successfully",
                InitialQuestions: initialQuestions
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing persona creation for user {UserId}", request.UserId);
            return new InitializePersonaCreationResult(
                Success: false,
                SessionId: string.Empty,
                Message: "Failed to initialize persona creation session",
                InitialQuestions: new List<string>()
            );
        }
    }

    private List<string> GenerateInitialQuestions(int userAge, string language)
    {
        // Age-adaptive question generation following SpinnerNet patterns
        return userAge switch
        {
            < 13 => new List<string>
            {
                "What's your favorite thing to do when you're not at school?",
                "If you could have any superpower, what would it be?",
                "What makes you really happy?",
                "What's your favorite subject to learn about?",
                "If you could meet any character from a book or movie, who would it be?"
            },
            >= 13 and < 18 => new List<string>
            {
                "What are your main interests or hobbies?",
                "What kind of future do you imagine for yourself?",
                "What challenges do you face in your daily life?",
                "What motivates you to keep going when things get tough?",
                "How do you like to spend your free time?"
            },
            >= 18 and < 65 => new List<string>
            {
                "What are your primary goals right now?",
                "How do you prefer to organize your tasks and time?",
                "What motivates you most in your work or personal life?",
                "What skills would you like to develop further?",
                "How do you handle stress and challenges?"
            },
            >= 65 => new List<string>
            {
                "What experiences have shaped who you are today?",
                "What wisdom would you like to share with others?",
                "How do you like to spend your time now?",
                "What brings you the most satisfaction in life?",
                "What would you like your legacy to be?"
            }
        };
    }

    private int GetTotalQuestionsForAge(int userAge)
    {
        // Adjust question count based on age and attention span
        return userAge switch
        {
            < 13 => 8,      // Shorter for children
            >= 13 and < 18 => 10,  // Moderate for teens
            >= 18 and < 65 => 12,  // Full questionnaire for adults
            _ => 10         // Comfortable pace for seniors (65+)
        };
    }

    private int GetEstimatedTimeForAge(int userAge)
    {
        // Estimated time in minutes based on age
        return userAge switch
        {
            < 13 => 15,     // Quick and engaging for children
            >= 13 and < 18 => 20,  // Moderate time for teens
            >= 18 and < 65 => 25,  // Comprehensive for adults
            _ => 30         // Comfortable pace for seniors (65+)
        };
    }
}