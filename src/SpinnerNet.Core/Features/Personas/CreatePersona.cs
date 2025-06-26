using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SpinnerNet.Shared.Models.CosmosDb;

namespace SpinnerNet.Core.Features.Personas;

public static class CreatePersona
{
    public record Command : IRequest<Result>
    {
        public string UserId { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public int Age { get; init; }
        public string CulturalBackground { get; init; } = string.Empty;
        public string Occupation { get; init; } = string.Empty;
        public List<string> Interests { get; init; } = new();
        public string MotherTongue { get; init; } = "en";
        public string PreferredLanguage { get; init; } = "en";
        public List<string> SpokenLanguages { get; init; } = new();
        public string UIComplexityLevel { get; init; } = "Standard";
        public string FontSizePreferences { get; init; } = "Medium";
        public string ColorPreferences { get; init; } = "Default";
        public bool EnableAnimations { get; init; } = true;
        public string NavigationStyle { get; init; } = "Standard";
        public string PreferredLearningStyle { get; init; } = "Visual";
        public string PacePreference { get; init; } = "SelfPaced";
        public string DifficultyLevel { get; init; } = "Intermediate";
        public string DataSharing { get; init; } = "Selective";
        public string AIInteraction { get; init; } = "Standard";
        public string EmailAccess { get; init; } = "None";
        public bool IsDefault { get; init; } = false;
    }

    public record Result
    {
        public bool Success { get; init; }
        public string? Error { get; init; }
        public string? PersonaId { get; init; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.DisplayName)
                .NotEmpty()
                .Length(1, 100)
                .WithMessage("Display name must be between 1 and 100 characters");

            RuleFor(x => x.Age)
                .InclusiveBetween(13, 120)
                .WithMessage("Age must be between 13 and 120");

            RuleFor(x => x.CulturalBackground)
                .MaximumLength(200)
                .WithMessage("Cultural background must not exceed 200 characters");

            RuleFor(x => x.Occupation)
                .MaximumLength(100)
                .WithMessage("Occupation must not exceed 100 characters");

            RuleFor(x => x.Interests)
                .Must(interests => interests.Count <= 20)
                .WithMessage("Cannot have more than 20 interests");

            RuleFor(x => x.MotherTongue)
                .NotEmpty()
                .Length(2, 5)
                .WithMessage("Mother tongue must be a valid language code");

            RuleFor(x => x.PreferredLanguage)
                .NotEmpty()
                .Length(2, 5)
                .WithMessage("Preferred language must be a valid language code");

            RuleFor(x => x.UIComplexityLevel)
                .Must(x => new[] { "Simple", "Standard", "Advanced" }.Contains(x))
                .WithMessage("UI complexity level must be Simple, Standard, or Advanced");

            RuleFor(x => x.FontSizePreferences)
                .Must(x => new[] { "Small", "Medium", "Large", "ExtraLarge" }.Contains(x))
                .WithMessage("Font size must be Small, Medium, Large, or ExtraLarge");

            RuleFor(x => x.ColorPreferences)
                .Must(x => new[] { "Default", "HighContrast", "Dark", "Bright" }.Contains(x))
                .WithMessage("Color preferences must be Default, HighContrast, Dark, or Bright");

            RuleFor(x => x.NavigationStyle)
                .Must(x => new[] { "Simple", "Standard", "Full" }.Contains(x))
                .WithMessage("Navigation style must be Simple, Standard, or Full");

            RuleFor(x => x.PreferredLearningStyle)
                .Must(x => new[] { "Visual", "Auditory", "Kinesthetic", "ReadingWriting" }.Contains(x))
                .WithMessage("Learning style must be Visual, Auditory, Kinesthetic, or ReadingWriting");

            RuleFor(x => x.PacePreference)
                .Must(x => new[] { "SelfPaced", "Guided", "Structured" }.Contains(x))
                .WithMessage("Pace preference must be SelfPaced, Guided, or Structured");

            RuleFor(x => x.DifficultyLevel)
                .Must(x => new[] { "Beginner", "Intermediate", "Advanced" }.Contains(x))
                .WithMessage("Difficulty level must be Beginner, Intermediate, or Advanced");

            RuleFor(x => x.DataSharing)
                .Must(x => new[] { "None", "Selective", "Open" }.Contains(x))
                .WithMessage("Data sharing must be None, Selective, or Open");

            RuleFor(x => x.AIInteraction)
                .Must(x => new[] { "Basic", "Standard", "Enhanced" }.Contains(x))
                .WithMessage("AI interaction must be Basic, Standard, or Enhanced");

            RuleFor(x => x.EmailAccess)
                .Must(x => new[] { "None", "ReadOnly", "Authorized" }.Contains(x))
                .WithMessage("Email access must be None, ReadOnly, or Authorized");
        }
    }

    public class Handler : IRequestHandler<Command, Result>
    {
        // TODO: Inject ICosmosDbService when available
        
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var personaid = Guid.NewGuid().ToString();
                var documentid = $"persona_{request.UserId}_{personaid}";

                var personaDocument = new PersonaDocument
                {
                    id = documentid,
                    type = "persona",
                    UserId = request.UserId,
                    personaId = personaid,
                    isDefault = request.IsDefault,
                    basicInfo = new PersonaBasicInfo
                    {
                        displayName = request.DisplayName,
                        age = request.Age,
                        culturalBackground = request.CulturalBackground,
                        occupation = request.Occupation,
                        interests = request.Interests,
                        languages = new LanguageInfo
                        {
                            motherTongue = request.MotherTongue,
                            preferred = request.PreferredLanguage,
                            spoken = request.SpokenLanguages.Any() ? request.SpokenLanguages : new List<string> { request.MotherTongue },
                            proficiency = request.SpokenLanguages.ToDictionary(lang => lang, _ => "Native")
                        }
                    },
                    typeLeapConfig = new TypeLeapConfiguration
                    {
                        uiComplexityLevel = request.UIComplexityLevel,
                        fontSizePreferences = request.FontSizePreferences,
                        colorPreferences = request.ColorPreferences,
                        enableAnimations = request.EnableAnimations,
                        navigationStyle = request.NavigationStyle,
                        ageAdaptations = CreateAgeAdaptations(request.Age)
                    },
                    learningPreferences = new LearningPreferences
                    {
                        preferredLearningStyle = request.PreferredLearningStyle,
                        pacePreference = request.PacePreference,
                        difficultyLevel = request.DifficultyLevel
                    },
                    privacySettings = new PrivacySettings
                    {
                        dataSharing = request.DataSharing,
                        aiInteraction = request.AIInteraction,
                        emailAccess = request.EmailAccess,
                        consentTimestamp = DateTime.UtcNow
                    },
                    buddyRelationships = new List<BuddyRelationship>(),
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow
                };

                // TODO: Save to Cosmos DB when service is available
                // await _cosmosDbService.CreateDocumentAsync("Personas", personaDocument, request.UserId);

                // For now, simulate success
                await Task.Delay(100, cancellationToken);

                return new Result
                {
                    Success = true,
                    PersonaId = personaid
                };
            }
            catch (Exception ex)
            {
                return new Result
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private Dictionary<string, string> CreateAgeAdaptations(int age)
        {
            var adaptations = new Dictionary<string, string>();

            if (age < 18)
            {
                adaptations["parentalConsent"] = "true";
                adaptations["contentFiltering"] = "strict";
                adaptations["timeRestrictions"] = "true";
            }
            else if (age >= 65)
            {
                adaptations["largerClickTargets"] = "true";
                adaptations["simplifiedInterface"] = "true";
                adaptations["enhancedContrast"] = "true";
            }

            return adaptations;
        }
    }

    [ApiController]
    [Route("api/personas")]
    public class Endpoint : ControllerBase
    {
        private readonly IMediator _mediator;

        public Endpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> CreatePersona([FromBody] Command command)
        {
            var result = await _mediator.Send(command);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
    }
}