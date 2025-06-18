using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpinnerNet.Core.Data.CosmosDb;
using SpinnerNet.Shared.Models.CosmosDb;
using System.Text.Json.Serialization;

namespace SpinnerNet.Core.Features.Buddies;

/// <summary>
/// Vertical slice for creating AI buddy companions
/// Creates personalized AI assistants based on user persona and preferences
/// </summary>
public static class CreateBuddy
{
    // 1. Command (Input)
    public record Command : IRequest<Result>
    {
        [JsonPropertyName("userId")]
        public string UserId { get; init; } = string.Empty;

        [JsonPropertyName("personaId")]
        public string PersonaId { get; init; } = string.Empty;

        [JsonPropertyName("buddyType")]
        public string BuddyType { get; init; } = "DailyCompanion";

        [JsonPropertyName("customization")]
        public BuddyCustomization? Customization { get; init; }

        [JsonPropertyName("requestedCapabilities")]
        public List<string> RequestedCapabilities { get; init; } = new();

        [JsonPropertyName("language")]
        public string Language { get; init; } = "en";
    }

    // 2. Result (Output)
    public record Result
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }

        [JsonPropertyName("buddyId")]
        public string? BuddyId { get; init; }

        [JsonPropertyName("buddy")]
        public BuddyDocument? Buddy { get; init; }

        [JsonPropertyName("onboardingTips")]
        public List<OnboardingTip> OnboardingTips { get; init; } = new();

        [JsonPropertyName("suggestedFirstActions")]
        public List<SuggestedBuddyAction> SuggestedFirstActions { get; init; } = new();

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; init; }

        [JsonPropertyName("validationErrors")]
        public Dictionary<string, string[]>? ValidationErrors { get; init; }

        public static Result SuccessResult(string buddyId, BuddyDocument buddy, List<OnboardingTip> tips, List<SuggestedBuddyAction> actions) =>
            new() 
            { 
                Success = true, 
                BuddyId = buddyId, 
                Buddy = buddy,
                OnboardingTips = tips,
                SuggestedFirstActions = actions
            };

        public static Result Failure(string errorMessage) =>
            new() { Success = false, ErrorMessage = errorMessage };

        public static Result ValidationFailure(Dictionary<string, string[]> validationErrors) =>
            new() { Success = false, ValidationErrors = validationErrors };
    }

    // 3. Supporting Models
    public record BuddyCustomization
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("personalityArchetype")]
        public string? PersonalityArchetype { get; init; }

        [JsonPropertyName("communicationTone")]
        public string? CommunicationTone { get; init; }

        [JsonPropertyName("formality")]
        public double? Formality { get; init; }

        [JsonPropertyName("proactiveness")]
        public double? Proactiveness { get; init; }

        [JsonPropertyName("useEmojis")]
        public bool? UseEmojis { get; init; }

        [JsonPropertyName("useHumor")]
        public bool? UseHumor { get; init; }
    }

    public record OnboardingTip
    {
        [JsonPropertyName("title")]
        public string Title { get; init; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; init; } = string.Empty;

        [JsonPropertyName("action")]
        public string? Action { get; init; }

        [JsonPropertyName("priority")]
        public int Priority { get; init; } = 1;
    }

    public record SuggestedBuddyAction
    {
        [JsonPropertyName("type")]
        public string Type { get; init; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; init; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; init; } = string.Empty;

        [JsonPropertyName("example")]
        public string? Example { get; init; }

        [JsonPropertyName("capabilities")]
        public List<string> RequiredCapabilities { get; init; } = new();
    }

    // 4. Validator (Input Validation)
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required")
                .MaximumLength(100).WithMessage("User ID must not exceed 100 characters");

            RuleFor(x => x.PersonaId)
                .NotEmpty().WithMessage("Persona ID is required")
                .MaximumLength(100).WithMessage("Persona ID must not exceed 100 characters");

            RuleFor(x => x.BuddyType)
                .NotEmpty().WithMessage("Buddy type is required")
                .Must(BeValidBuddyType).WithMessage("Invalid buddy type");

            RuleFor(x => x.Language)
                .NotEmpty().WithMessage("Language is required")
                .Length(2, 5).WithMessage("Language must be a valid language code");

            RuleFor(x => x.Customization!.Name)
                .MaximumLength(50).WithMessage("Buddy name must not exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.Customization?.Name));

            RuleFor(x => x.Customization!.Formality)
                .InclusiveBetween(0.0, 1.0).WithMessage("Formality must be between 0.0 and 1.0")
                .When(x => x.Customization?.Formality.HasValue == true);

            RuleFor(x => x.Customization!.Proactiveness)
                .InclusiveBetween(0.0, 1.0).WithMessage("Proactiveness must be between 0.0 and 1.0")
                .When(x => x.Customization?.Proactiveness.HasValue == true);
        }

        private static bool BeValidBuddyType(string buddyType)
        {
            var validTypes = new[] 
            { 
                "DailyCompanion", "LearningMentor", "WorkAssistant", "HealthCoach", 
                "CreativePartner", "StudyBuddy", "LifeOrganizer", "BusinessAdvisor",
                "PhotoExpert", "SocialMediaHelper", "FinanceGuide", "TravelCompanion"
            };
            return validTypes.Contains(buddyType);
        }
    }

    // 5. Handler (Business Logic + Data Access)
    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ICosmosRepository<BuddyDocument> _buddyRepository;
        private readonly ICosmosRepository<PersonaDocument> _personaRepository;
        private readonly ICosmosRepository<UserDocument> _userRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(
            ICosmosRepository<BuddyDocument> buddyRepository,
            ICosmosRepository<PersonaDocument> personaRepository,
            ICosmosRepository<UserDocument> userRepository,
            ILogger<Handler> logger)
        {
            _buddyRepository = buddyRepository;
            _personaRepository = personaRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating buddy for user: {UserId}, PersonaId: {PersonaId}, Type: {BuddyType}", 
                    request.UserId, request.PersonaId, request.BuddyType);

                // 1. Verify user exists
                var users = await _userRepository.QueryAsync(
                    u => u.UserId == request.UserId,
                    maxResults: 1,
                    cancellationToken: cancellationToken);

                if (!users.Any())
                {
                    _logger.LogWarning("Buddy creation attempted for non-existent user: {UserId}", request.UserId);
                    return Result.Failure("User not found");
                }

                var user = users.First();

                // 2. Verify persona exists and belongs to user
                var personas = await _personaRepository.QueryAsync(
                    p => p.PersonaId == request.PersonaId && p.UserId == request.UserId,
                    maxResults: 1,
                    cancellationToken: cancellationToken);

                if (!personas.Any())
                {
                    _logger.LogWarning("Buddy creation attempted for non-existent persona: {PersonaId}", request.PersonaId);
                    return Result.Failure("Persona not found or doesn't belong to user");
                }

                var persona = personas.First();

                // 3. Check if user already has a buddy of this type for this persona
                var existingBuddies = await _buddyRepository.QueryAsync(
                    b => b.UserId == request.UserId && b.PersonaId == request.PersonaId && b.BuddyType == request.BuddyType,
                    maxResults: 1,
                    cancellationToken: cancellationToken);

                if (existingBuddies.Any())
                {
                    _logger.LogWarning("User already has buddy of type {BuddyType} for persona {PersonaId}", 
                        request.BuddyType, request.PersonaId);
                    return Result.Failure($"You already have a {request.BuddyType} buddy for this persona");
                }

                // 4. Create buddy document
                var buddyId = $"buddy_{request.UserId}_{Guid.NewGuid():N}";
                var buddy = await CreateBuddyFromPersona(buddyId, request, persona, user, cancellationToken);

                // 5. Save buddy to Cosmos DB
                await _buddyRepository.CreateOrUpdateAsync(
                    buddy,
                    request.UserId,
                    cancellationToken);

                // 6. Update persona with buddy relationship
                await UpdatePersonaWithBuddyRelationship(persona, buddy, cancellationToken);

                // 7. Generate onboarding tips
                var onboardingTips = GenerateOnboardingTips(buddy, request.Language);

                // 8. Generate suggested first actions
                var suggestedActions = GenerateSuggestedFirstActions(buddy, persona, request.Language);

                _logger.LogInformation("Buddy created successfully: {BuddyId} for user: {UserId}", 
                    buddyId, request.UserId);

                return Result.SuccessResult(buddyId, buddy, onboardingTips, suggestedActions);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating buddy for user: {UserId}, Type: {BuddyType}", 
                    request.UserId, request.BuddyType);
                return Result.Failure("An error occurred creating the buddy. Please try again.");
            }
        }

        private async Task<BuddyDocument> CreateBuddyFromPersona(
            string buddyId, 
            Command request, 
            PersonaDocument persona, 
            UserDocument user,
            CancellationToken cancellationToken)
        {
            // Generate buddy name and avatar based on type and persona
            var basicInfo = CreateBuddyBasicInfo(request.BuddyType, request.Customization, request.Language);

            // Configure personality based on persona and user preferences
            var personality = CreateBuddyPersonality(request.BuddyType, persona, request.Customization);

            // Set up capabilities based on requested features
            var capabilities = CreateBuddyCapabilities(request.RequestedCapabilities, persona.PrivacySettings);

            // Initialize learning data
            var learningData = InitializeLearningData(persona);

            var buddy = new BuddyDocument
            {
                Id = $"buddy_{buddyId}",
                UserId = request.UserId,
                BuddyId = buddyId,
                PersonaId = request.PersonaId,
                BuddyType = request.BuddyType,
                BasicInfo = basicInfo,
                Personality = personality,
                Capabilities = capabilities,
                LearningData = learningData,
                MemoryReferences = new List<string>(),
                CreatedAt = DateTime.UtcNow,
                LastActiveAt = DateTime.UtcNow
            };

            return await Task.FromResult(buddy);
        }

        private BuddyBasicInfo CreateBuddyBasicInfo(string buddyType, BuddyCustomization? customization, string language)
        {
            var name = customization?.Name ?? GenerateDefaultBuddyName(buddyType, language);
            var description = GenerateBuddyDescription(buddyType, language);
            var avatar = GenerateAvatarForType(buddyType);

            return new BuddyBasicInfo
            {
                Name = name,
                Avatar = avatar,
                Description = description
            };
        }

        private string GenerateDefaultBuddyName(string buddyType, string language)
        {
            var names = language switch
            {
                "de" => buddyType switch
                {
                    "DailyCompanion" => "Alex",
                    "LearningMentor" => "Prof. Weise",
                    "WorkAssistant" => "Assistent",
                    "HealthCoach" => "Vita",
                    "CreativePartner" => "Kreatia",
                    "StudyBuddy" => "Studienfreund",
                    "LifeOrganizer" => "Organizer",
                    "BusinessAdvisor" => "Berater",
                    "PhotoExpert" => "Foto-Experte",
                    "SocialMediaHelper" => "Social-Helfer",
                    "FinanceGuide" => "Finanz-Guide",
                    "TravelCompanion" => "Reisebegleiter",
                    _ => "Assistent"
                },
                "es" => buddyType switch
                {
                    "DailyCompanion" => "Amigo",
                    "LearningMentor" => "Mentor",
                    "WorkAssistant" => "Asistente",
                    "HealthCoach" => "Coach Salud",
                    "CreativePartner" => "Creativo",
                    "StudyBuddy" => "Compañero",
                    "LifeOrganizer" => "Organizador",
                    "BusinessAdvisor" => "Consejero",
                    "PhotoExpert" => "Foto Experto",
                    "SocialMediaHelper" => "Social Helper",
                    "FinanceGuide" => "Guía Financiero",
                    "TravelCompanion" => "Compañero Viaje",
                    _ => "Asistente"
                },
                _ => buddyType switch
                {
                    "DailyCompanion" => "Buddy",
                    "LearningMentor" => "Mentor",
                    "WorkAssistant" => "Assistant",
                    "HealthCoach" => "Coach",
                    "CreativePartner" => "Creative",
                    "StudyBuddy" => "Scholar",
                    "LifeOrganizer" => "Planner",
                    "BusinessAdvisor" => "Advisor",
                    "PhotoExpert" => "Photo Pro",
                    "SocialMediaHelper" => "Social Helper",
                    "FinanceGuide" => "Finance Guide",
                    "TravelCompanion" => "Travel Guide",
                    _ => "Assistant"
                }
            };

            return names;
        }

        private string GenerateBuddyDescription(string buddyType, string language)
        {
            return language switch
            {
                "de" => buddyType switch
                {
                    "DailyCompanion" => "Dein freundlicher täglicher Begleiter für alle Lebensbereiche",
                    "LearningMentor" => "Ein weiser Mentor für Lernen und persönliche Entwicklung",
                    "WorkAssistant" => "Dein professioneller Arbeitsassistent",
                    "HealthCoach" => "Dein persönlicher Gesundheits- und Fitness-Coach",
                    "CreativePartner" => "Dein kreativer Partner für Projekte und Ideen",
                    "StudyBuddy" => "Dein motivierender Lernpartner",
                    "LifeOrganizer" => "Dein Organisationsexperte für ein strukturiertes Leben",
                    _ => "Dein hilfreicher KI-Assistent"
                },
                "es" => buddyType switch
                {
                    "DailyCompanion" => "Tu compañero amigable para todos los aspectos de la vida",
                    "LearningMentor" => "Un mentor sabio para el aprendizaje y desarrollo personal",
                    "WorkAssistant" => "Tu asistente profesional de trabajo",
                    "HealthCoach" => "Tu entrenador personal de salud y fitness",
                    "CreativePartner" => "Tu socio creativo para proyectos e ideas",
                    "StudyBuddy" => "Tu compañero de estudio motivador",
                    "LifeOrganizer" => "Tu experto en organización para una vida estructurada",
                    _ => "Tu asistente de IA útil"
                },
                _ => buddyType switch
                {
                    "DailyCompanion" => "Your friendly daily companion for all aspects of life",
                    "LearningMentor" => "A wise mentor for learning and personal development",
                    "WorkAssistant" => "Your professional work assistant",
                    "HealthCoach" => "Your personal health and fitness coach",
                    "CreativePartner" => "Your creative partner for projects and ideas",
                    "StudyBuddy" => "Your motivating study companion",
                    "LifeOrganizer" => "Your organization expert for a structured life",
                    _ => "Your helpful AI assistant"
                }
            };
        }

        private string GenerateAvatarForType(string buddyType)
        {
            return buddyType switch
            {
                "DailyCompanion" => "friendly-companion",
                "LearningMentor" => "wise-mentor",
                "WorkAssistant" => "professional-assistant",
                "HealthCoach" => "health-coach",
                "CreativePartner" => "creative-artist",
                "StudyBuddy" => "study-buddy",
                "LifeOrganizer" => "organizer",
                "BusinessAdvisor" => "business-advisor",
                "PhotoExpert" => "photo-expert",
                "SocialMediaHelper" => "social-helper",
                "FinanceGuide" => "finance-guide",
                "TravelCompanion" => "travel-guide",
                _ => "friendly-assistant"
            };
        }

        private BuddyPersonality CreateBuddyPersonality(string buddyType, PersonaDocument persona, BuddyCustomization? customization)
        {
            var archetype = customization?.PersonalityArchetype ?? GetDefaultArchetypeForType(buddyType);
            var traits = CreatePersonalityTraits(buddyType, persona, customization);
            var communicationStyle = CreateCommunicationStyle(buddyType, persona, customization);

            return new BuddyPersonality
            {
                Archetype = archetype,
                Traits = traits,
                CommunicationStyle = communicationStyle
            };
        }

        private string GetDefaultArchetypeForType(string buddyType)
        {
            return buddyType switch
            {
                "DailyCompanion" => "Helpful",
                "LearningMentor" => "Mentor",
                "WorkAssistant" => "Professional",
                "HealthCoach" => "Motivational",
                "CreativePartner" => "Creative",
                "StudyBuddy" => "Encouraging",
                "LifeOrganizer" => "Organized",
                "BusinessAdvisor" => "Advisory",
                _ => "Helpful"
            };
        }

        private PersonalityTraits CreatePersonalityTraits(string buddyType, PersonaDocument persona, BuddyCustomization? customization)
        {
            // Base traits by buddy type
            var baseTraits = buddyType switch
            {
                "DailyCompanion" => new PersonalityTraits { Friendliness = 0.9, Professionalism = 0.5, Proactiveness = 0.7, Formality = 0.3 },
                "LearningMentor" => new PersonalityTraits { Friendliness = 0.7, Professionalism = 0.8, Proactiveness = 0.6, Formality = 0.6 },
                "WorkAssistant" => new PersonalityTraits { Friendliness = 0.6, Professionalism = 0.9, Proactiveness = 0.8, Formality = 0.7 },
                "HealthCoach" => new PersonalityTraits { Friendliness = 0.8, Professionalism = 0.7, Proactiveness = 0.8, Formality = 0.4 },
                "CreativePartner" => new PersonalityTraits { Friendliness = 0.8, Professionalism = 0.6, Proactiveness = 0.9, Formality = 0.2 },
                "StudyBuddy" => new PersonalityTraits { Friendliness = 0.9, Professionalism = 0.6, Proactiveness = 0.7, Formality = 0.3 },
                _ => new PersonalityTraits { Friendliness = 0.8, Professionalism = 0.7, Proactiveness = 0.6, Formality = 0.4 }
            };

            // Adjust based on persona age and preferences
            if (persona.BasicInfo.Age <= 25)
            {
                baseTraits.Friendliness = Math.Min(1.0, baseTraits.Friendliness + 0.1);
                baseTraits.Formality = Math.Max(0.0, baseTraits.Formality - 0.2);
            }
            else if (persona.BasicInfo.Age >= 60)
            {
                baseTraits.Formality = Math.Min(1.0, baseTraits.Formality + 0.2);
                baseTraits.Professionalism = Math.Min(1.0, baseTraits.Professionalism + 0.1);
            }

            // Apply customizations
            if (customization != null)
            {
                if (customization.Formality.HasValue)
                    baseTraits.Formality = customization.Formality.Value;
                if (customization.Proactiveness.HasValue)
                    baseTraits.Proactiveness = customization.Proactiveness.Value;
            }

            return baseTraits;
        }

        private CommunicationStyle CreateCommunicationStyle(string buddyType, PersonaDocument persona, BuddyCustomization? customization)
        {
            var tone = customization?.CommunicationTone ?? GetDefaultToneForType(buddyType);
            var verbosity = GetVerbosityForPersona(persona);
            var useEmojis = customization?.UseEmojis ?? (persona.BasicInfo.Age <= 40);
            var useHumor = customization?.UseHumor ?? (buddyType == "DailyCompanion" || buddyType == "CreativePartner");

            return new CommunicationStyle
            {
                Tone = tone,
                Verbosity = verbosity,
                UseEmojis = useEmojis,
                UseHumor = useHumor
            };
        }

        private string GetDefaultToneForType(string buddyType)
        {
            return buddyType switch
            {
                "DailyCompanion" => "friendly",
                "LearningMentor" => "encouraging",
                "WorkAssistant" => "professional",
                "HealthCoach" => "motivating",
                "CreativePartner" => "enthusiastic",
                "StudyBuddy" => "supportive",
                "LifeOrganizer" => "helpful",
                "BusinessAdvisor" => "professional",
                _ => "friendly"
            };
        }

        private string GetVerbosityForPersona(PersonaDocument persona)
        {
            // Adjust verbosity based on persona preferences
            if (persona.TypeLeapConfig.UIComplexityLevel == "Simple")
                return "concise";
            else if (persona.TypeLeapConfig.UIComplexityLevel == "Advanced")
                return "detailed";
            else
                return "normal";
        }

        private BuddyCapabilities CreateBuddyCapabilities(List<string> requestedCapabilities, PrivacySettings privacySettings)
        {
            var capabilities = new BuddyCapabilities();

            // Enable task management by default (safe capability)
            capabilities.TaskManagement.Enabled = true;
            capabilities.TaskManagement.Permissions = new List<string> { "view_tasks", "create_tasks", "update_tasks" };

            // Email management - only if explicitly requested and privacy allows
            if (requestedCapabilities.Contains("email_management") && privacySettings.EmailAccess != "None")
            {
                capabilities.EmailManagement.Enabled = true;
                capabilities.EmailManagement.Permissions = privacySettings.EmailAccess == "ReadOnly" 
                    ? new List<string> { "read_emails", "categorize_emails" }
                    : new List<string> { "read_emails", "categorize_emails", "send_emails", "manage_emails" };
            }

            // Calendar integration - only if requested
            if (requestedCapabilities.Contains("calendar_integration"))
            {
                capabilities.CalendarIntegration.Enabled = true;
                capabilities.CalendarIntegration.Permissions = new List<string> { "view_calendar", "create_events", "update_events" };
            }

            return capabilities;
        }

        private BuddyLearningData InitializeLearningData(PersonaDocument persona)
        {
            return new BuddyLearningData
            {
                UserPreferences = new LearnedUserPreferences
                {
                    PreferredResponseTime = "anytime",
                    CommunicationFrequency = "moderate",
                    TopicInterests = persona.BasicInfo.Interests.ToList()
                },
                AdaptationHistory = new List<AdaptationRecord>
                {
                    new()
                    {
                        Timestamp = DateTime.UtcNow,
                        Adaptation = "Initial personality configuration",
                        Reason = "Based on user persona preferences"
                    }
                }
            };
        }

        private async Task UpdatePersonaWithBuddyRelationship(PersonaDocument persona, BuddyDocument buddy, CancellationToken cancellationToken)
        {
            var relationship = new BuddyRelationship
            {
                BuddyId = buddy.BuddyId,
                Relationship = buddy.BuddyType,
                TrustLevel = 0.5, // Start with moderate trust
                Permissions = buddy.Capabilities.TaskManagement.Permissions.ToList(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            persona.BuddyRelationships.Add(relationship);
            persona.UpdatedAt = DateTime.UtcNow;

            await _personaRepository.CreateOrUpdateAsync(persona, persona.UserId, cancellationToken);
        }

        private List<OnboardingTip> GenerateOnboardingTips(BuddyDocument buddy, string language)
        {
            var tips = new List<OnboardingTip>();

            // Always include introduction tip
            tips.Add(new OnboardingTip
            {
                Title = GetLocalizedText("Say Hello", language),
                Description = GetLocalizedText($"Start by introducing yourself to {buddy.BasicInfo.Name}. They're excited to meet you!", language),
                Action = "start_chat",
                Priority = 1
            });

            // Capability-based tips
            if (buddy.Capabilities.TaskManagement.Enabled)
            {
                tips.Add(new OnboardingTip
                {
                    Title = GetLocalizedText("Create Your First Task", language),
                    Description = GetLocalizedText("Try saying 'Remind me to call my mom tomorrow' to see how AI understands natural language", language),
                    Action = "create_task",
                    Priority = 2
                });
            }

            if (buddy.Capabilities.EmailManagement.Enabled)
            {
                tips.Add(new OnboardingTip
                {
                    Title = GetLocalizedText("Connect Your Email", language),
                    Description = GetLocalizedText("Let your buddy help organize and summarize your emails", language),
                    Action = "connect_email",
                    Priority = 3
                });
            }

            // Personality adjustment tip
            tips.Add(new OnboardingTip
            {
                Title = GetLocalizedText("Customize Personality", language),
                Description = GetLocalizedText("Adjust how formal or casual your buddy is to match your preference", language),
                Action = "customize_personality",
                Priority = 4
            });

            return tips;
        }

        private List<SuggestedBuddyAction> GenerateSuggestedFirstActions(BuddyDocument buddy, PersonaDocument persona, string language)
        {
            var actions = new List<SuggestedBuddyAction>();

            // Chat action
            actions.Add(new SuggestedBuddyAction
            {
                Type = "chat",
                Title = GetLocalizedText("Start a Conversation", language),
                Description = GetLocalizedText("Begin chatting with your buddy to see their personality in action", language),
                Example = GetLocalizedText("Hello! How can you help me today?", language),
                RequiredCapabilities = new List<string>()
            });

            // Task creation action
            if (buddy.Capabilities.TaskManagement.Enabled)
            {
                actions.Add(new SuggestedBuddyAction
                {
                    Type = "create_task",
                    Title = GetLocalizedText("Create a Task", language),
                    Description = GetLocalizedText("Use natural language to create tasks and reminders", language),
                    Example = GetLocalizedText("Remind me to exercise tomorrow at 7 AM", language),
                    RequiredCapabilities = new List<string> { "task_management" }
                });
            }

            // Interest-based suggestions
            if (persona.BasicInfo.Interests.Contains("learning"))
            {
                actions.Add(new SuggestedBuddyAction
                {
                    Type = "learning_plan",
                    Title = GetLocalizedText("Create Learning Plan", language),
                    Description = GetLocalizedText("Set up study goals and learning schedules", language),
                    Example = GetLocalizedText("Help me plan to learn Spanish in 3 months", language),
                    RequiredCapabilities = new List<string> { "task_management" }
                });
            }

            return actions;
        }

        private string GetLocalizedText(string text, string language)
        {
            return language switch
            {
                "de" => text switch
                {
                    "Say Hello" => "Sag Hallo",
                    var t when t.StartsWith("Start by introducing yourself to") => t.Replace("Start by introducing yourself to", "Stelle dich vor bei").Replace("They're excited to meet you!", "Sie freuen sich darauf, dich kennenzulernen!"),
                    "Create Your First Task" => "Erstelle deine erste Aufgabe",
                    "Try saying 'Remind me to call my mom tomorrow' to see how AI understands natural language" => "Versuche zu sagen 'Erinnere mich daran, morgen meine Mutter anzurufen', um zu sehen, wie KI natürliche Sprache versteht",
                    "Connect Your Email" => "E-Mail verbinden",
                    "Let your buddy help organize and summarize your emails" => "Lass deinen Buddy dabei helfen, deine E-Mails zu organisieren und zusammenzufassen",
                    "Customize Personality" => "Persönlichkeit anpassen",
                    "Adjust how formal or casual your buddy is to match your preference" => "Passe an, wie formal oder locker dein Buddy ist, um deiner Vorliebe zu entsprechen",
                    "Start a Conversation" => "Gespräch beginnen",
                    "Begin chatting with your buddy to see their personality in action" => "Beginne zu chatten mit deinem Buddy, um ihre Persönlichkeit in Aktion zu sehen",
                    "Hello! How can you help me today?" => "Hallo! Wie kannst du mir heute helfen?",
                    "Create a Task" => "Aufgabe erstellen",
                    "Use natural language to create tasks and reminders" => "Verwende natürliche Sprache, um Aufgaben und Erinnerungen zu erstellen",
                    "Remind me to exercise tomorrow at 7 AM" => "Erinnere mich daran, morgen um 7 Uhr zu trainieren",
                    "Create Learning Plan" => "Lernplan erstellen",
                    "Set up study goals and learning schedules" => "Lernziele und Lernpläne einrichten",
                    "Help me plan to learn Spanish in 3 months" => "Hilf mir dabei, in 3 Monaten Spanisch zu lernen",
                    _ => text
                },
                "es" => text switch
                {
                    "Say Hello" => "Di Hola",
                    var t when t.StartsWith("Start by introducing yourself to") => t.Replace("Start by introducing yourself to", "Comienza presentándote a").Replace("They're excited to meet you!", "¡Están emocionados de conocerte!"),
                    "Create Your First Task" => "Crea tu primera tarea",
                    "Try saying 'Remind me to call my mom tomorrow' to see how AI understands natural language" => "Intenta decir 'Recuérdame llamar a mi mamá mañana' para ver cómo la IA entiende el lenguaje natural",
                    "Connect Your Email" => "Conectar tu email",
                    "Let your buddy help organize and summarize your emails" => "Deja que tu compañero te ayude a organizar y resumir tus emails",
                    "Customize Personality" => "Personalizar personalidad",
                    "Adjust how formal or casual your buddy is to match your preference" => "Ajusta qué tan formal o casual es tu compañero según tu preferencia",
                    "Start a Conversation" => "Iniciar conversación",
                    "Begin chatting with your buddy to see their personality in action" => "Comienza a chatear con tu compañero para ver su personalidad en acción",
                    "Hello! How can you help me today?" => "¡Hola! ¿Cómo puedes ayudarme hoy?",
                    "Create a Task" => "Crear una tarea",
                    "Use natural language to create tasks and reminders" => "Usa lenguaje natural para crear tareas y recordatorios",
                    "Remind me to exercise tomorrow at 7 AM" => "Recuérdame hacer ejercicio mañana a las 7 AM",
                    "Create Learning Plan" => "Crear plan de aprendizaje",
                    "Set up study goals and learning schedules" => "Configurar objetivos de estudio y horarios de aprendizaje",
                    "Help me plan to learn Spanish in 3 months" => "Ayúdame a planificar aprender español en 3 meses",
                    _ => text
                },
                _ => text
            };
        }
    }

    // 6. Endpoint (HTTP API)
    [ApiController]
    [Route("api/buddies")]
    public class Endpoint : ControllerBase
    {
        private readonly IMediator _mediator;

        public Endpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a new AI buddy companion
        /// </summary>
        /// <param name="command">Buddy creation parameters</param>
        /// <returns>Created buddy with onboarding information</returns>
        [HttpPost("create")]
        [ProducesResponseType(typeof(Result), 201)]
        [ProducesResponseType(typeof(Result), 400)]
        [ProducesResponseType(typeof(Result), 404)]
        [ProducesResponseType(typeof(Result), 409)]
        public async Task<ActionResult<Result>> CreateBuddy([FromBody] Command command)
        {
            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                if (result.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound(result);
                }

                if (result.ErrorMessage?.Contains("already have") == true)
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
                nameof(GetBuddy.Endpoint.GetById), // Reference to future GetBuddy slice
                new { id = result.BuddyId },
                result);
        }
    }
}

// Future reference for GetBuddy slice (placeholder)
public static class GetBuddy
{
    public static class Endpoint
    {
        public static string GetById => "GetBuddyById";
    }
}