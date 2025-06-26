using SpinnerNet.Core.Data.CosmosDb;
using SpinnerNet.Personas.Services;
using SpinnerNet.Shared.DTOs;
using SpinnerNet.Shared.Models;
using SpinnerNet.Shared.Models.CosmosDb;
using System.Text.Json;

namespace SpinnerNet.Core.Features.Personas;

public class CosmosDbPersonaService : IPersonaService
{
    private readonly ICosmosRepository<PersonaDocument> _personaRepository;

    public CosmosDbPersonaService(ICosmosRepository<PersonaDocument> personaRepository)
    {
        _personaRepository = personaRepository;
    }

    public async Task<UserPersona> CreatePersonaFromOnboardingAsync(string userId, OnboardingRequest request)
    {
        var personaId = Guid.NewGuid().ToString();
        var documentId = $"persona_{userId}_{personaId}";

        var personaDocument = new PersonaDocument
        {
            id = documentId,
            UserId = userId,
            personaId = personaId,
            isDefault = true, // First persona is default
            basicInfo = new PersonaBasicInfo
            {
                displayName = request.DisplayName,
                age = request.Age,
                culturalBackground = request.CulturalBackground,
                occupation = "",
                interests = request.Interests.ToList(),
                languages = new LanguageInfo
                {
                    motherTongue = request.PrimaryLanguage,
                    preferred = request.PrimaryLanguage,
                    spoken = new List<string> { request.PrimaryLanguage }.Concat(request.AdditionalLanguages).Distinct().ToList(),
                    proficiency = new Dictionary<string, string> { { request.PrimaryLanguage, "Native" } }
                }
            },
            typeLeapConfig = new TypeLeapConfiguration
            {
                uiComplexityLevel = request.UIComplexity,
                fontSizePreferences = "Medium",
                colorPreferences = request.ColorScheme,
                enableAnimations = request.EnableAnimations,
                navigationStyle = "Standard"
            },
            learningPreferences = new LearningPreferences
            {
                preferredLearningStyle = request.LearningStyle,
                pacePreference = "SelfPaced",
                difficultyLevel = "Intermediate"
            },
            privacySettings = new PrivacySettings
            {
                dataSharing = request.PrivacyLevel == "High" ? "None" : 
                             request.PrivacyLevel == "Low" ? "Open" : "Selective",
                aiInteraction = "Standard",
                emailAccess = "None",
                consentTimestamp = DateTime.UtcNow
            },
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow
        };

        await _personaRepository.CreateOrUpdateAsync(personaDocument, userId);

        return MapToUserPersona(personaDocument);
    }

    public async Task<UserPersona?> GetPersonaAsync(int personaId)
    {
        var personaDocuments = await _personaRepository.QueryWithSqlAsync(
            $"SELECT * FROM c WHERE c.personaid = '{personaId}'");

        var personaDocument = personaDocuments.FirstOrDefault();
        return personaDocument != null ? MapToUserPersona(personaDocument) : null;
    }

    public async Task<UserPersona?> GetActivePersonaAsync(string userId)
    {
        var personaDocuments = await _personaRepository.QueryWithSqlAsync(
            $"SELECT * FROM c WHERE c.UserId = '{userId}' AND c.isDefault = true",
            partitionKey: userId);

        var personaDocument = personaDocuments.FirstOrDefault();
        return personaDocument != null ? MapToUserPersona(personaDocument) : null;
    }

    public async Task<bool> UpdatePersonaAsync(UserPersona persona)
    {
        try
        {
            var documentId = $"persona_{persona.UserId}_{persona.PersonaId}";
            var existingDocument = await _personaRepository.GetAsync(documentId, persona.UserId);

            if (existingDocument == null)
                return false;

            var updatedDocument = MapToPersonaDocument(persona, existingDocument);
            updatedDocument.updatedAt = DateTime.UtcNow;

            await _personaRepository.CreateOrUpdateAsync(updatedDocument, persona.UserId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SetActivePersonaAsync(string userId, int personaId)
    {
        try
        {
            // First, set all personas for this user to non-default
            var allPersonas = await _personaRepository.QueryWithSqlAsync(
                $"SELECT * FROM c WHERE c.UserId = '{userId}'",
                partitionKey: userId);

            foreach (var persona in allPersonas)
            {
                if (persona.isDefault)
                {
                    persona.isDefault = false;
                    persona.updatedAt = DateTime.UtcNow;
                    await _personaRepository.CreateOrUpdateAsync(persona, userId);
                }
            }

            // Then set the specified persona as default
            var targetPersona = allPersonas.FirstOrDefault(p => p.personaId == personaId.ToString());
            if (targetPersona != null)
            {
                targetPersona.isDefault = true;
                targetPersona.updatedAt = DateTime.UtcNow;
                await _personaRepository.CreateOrUpdateAsync(targetPersona, userId);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<PersonaUIConfiguration> GetPersonaUIConfigurationAsync(int personaId)
    {
        var persona = await GetPersonaAsync(personaId);
        if (persona == null)
        {
            return new PersonaUIConfiguration();
        }

        return new PersonaUIConfiguration
        {
            ComplexityLevel = persona.UIComplexityLevel,
            ColorScheme = persona.ColorPreferences,
            FontSize = persona.FontSizePreferences,
            EnableAnimations = persona.EnableAnimations,
            NavigationStyle = persona.NavigationStyle,
            Theme = persona.ColorPreferences == "Dark" ? "Dark" : "Light",
            Language = persona.PrimaryLanguages.Contains("\"en\"") ? "en" : "en", // Parse JSON or default to en
            CulturalAdaptations = new Dictionary<string, object>
            {
                ["culturalBackground"] = persona.CulturalBackground
            },
            AgeAdaptations = new Dictionary<string, object>
            {
                ["age"] = persona.Age,
                ["ageGroup"] = persona.Age < 18 ? "Youth" : persona.Age < 65 ? "Adult" : "Senior"
            },
            InterestCustomizations = new Dictionary<string, object>
            {
                ["interests"] = persona.PassionsAndInterests.Split(',', StringSplitOptions.RemoveEmptyEntries)
            }
        };
    }

    private UserPersona MapToUserPersona(PersonaDocument document)
    {
        // Generate a numeric ID from PersonaId string for compatibility
        var numericId = Math.Abs(document.personaId.GetHashCode());

        return new UserPersona
        {
            Id = numericId,
            UserId = document.UserId,
            DigitalName = document.basicInfo.displayName,
            Age = document.basicInfo.age,
            DateOfBirth = null, // Not stored in PersonaDocument
            CulturalBackground = document.basicInfo.culturalBackground,
            PrimaryLanguages = JsonSerializer.Serialize(document.basicInfo.languages.spoken),
            PrimaryTimeZone = "UTC", // Default since not stored
            PassionsAndInterests = string.Join(", ", document.basicInfo.interests),
            LearningStyle = document.learningPreferences.preferredLearningStyle,
            CommunicationPreferences = "Friendly", // Default since not stored in PersonaDocument
            UIComplexityLevel = document.typeLeapConfig.uiComplexityLevel,
            ColorPreferences = document.typeLeapConfig.colorPreferences,
            FontSizePreferences = document.typeLeapConfig.fontSizePreferences,
            EnableAnimations = document.typeLeapConfig.enableAnimations,
            NavigationStyle = document.typeLeapConfig.navigationStyle,
            DataSovereigntyLevel = "Balanced", // Default mapping
            DefaultSharingProfile = document.privacySettings.dataSharing,
            RequireExplicitConsent = document.privacySettings.consentTimestamp != default(DateTime),
            EnableProactiveAssistance = true, // Default since not stored
            CompanionPersonality = "Helpful", // Default since not stored
            PreferredInteractionStyle = "Conversational", // Default since not stored
            CreatedAt = document.createdAt,
            UpdatedAt = document.updatedAt,
            PersonalityProfile = "", // Default since not stored
            DataRetentionPreference = "Standard" // Default since not stored
        };
    }

    private PersonaDocument MapToPersonaDocument(UserPersona persona, PersonaDocument existingDocument)
    {
        existingDocument.basicInfo.displayName = persona.DigitalName;
        existingDocument.basicInfo.age = persona.Age;
        existingDocument.basicInfo.culturalBackground = persona.CulturalBackground;
        existingDocument.basicInfo.interests = persona.PassionsAndInterests.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

        existingDocument.typeLeapConfig.uiComplexityLevel = persona.UIComplexityLevel;
        existingDocument.typeLeapConfig.colorPreferences = persona.ColorPreferences;
        existingDocument.typeLeapConfig.fontSizePreferences = persona.FontSizePreferences;
        existingDocument.typeLeapConfig.enableAnimations = persona.EnableAnimations;
        existingDocument.typeLeapConfig.navigationStyle = persona.NavigationStyle;

        existingDocument.learningPreferences.preferredLearningStyle = persona.LearningStyle;

        existingDocument.privacySettings.dataSharing = persona.DefaultSharingProfile;

        return existingDocument;
    }
}