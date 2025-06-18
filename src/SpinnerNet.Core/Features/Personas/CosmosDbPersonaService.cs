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
            Id = documentId,
            UserId = userId,
            PersonaId = personaId,
            IsDefault = true, // First persona is default
            BasicInfo = new PersonaBasicInfo
            {
                DisplayName = request.DisplayName,
                Age = request.Age,
                CulturalBackground = request.CulturalBackground,
                Occupation = "",
                Interests = request.Interests.ToList(),
                Languages = new LanguageInfo
                {
                    MotherTongue = request.PrimaryLanguage,
                    Preferred = request.PrimaryLanguage,
                    Spoken = new List<string> { request.PrimaryLanguage }.Concat(request.AdditionalLanguages).Distinct().ToList(),
                    Proficiency = new Dictionary<string, string> { { request.PrimaryLanguage, "Native" } }
                }
            },
            TypeLeapConfig = new TypeLeapConfiguration
            {
                UIComplexityLevel = request.UIComplexity,
                FontSizePreferences = "Medium",
                ColorPreferences = request.ColorScheme,
                EnableAnimations = request.EnableAnimations,
                NavigationStyle = "Standard"
            },
            LearningPreferences = new LearningPreferences
            {
                PreferredLearningStyle = request.LearningStyle,
                PacePreference = "SelfPaced",
                DifficultyLevel = "Intermediate"
            },
            PrivacySettings = new PrivacySettings
            {
                DataSharing = request.PrivacyLevel == "High" ? "None" : 
                             request.PrivacyLevel == "Low" ? "Open" : "Selective",
                AIInteraction = "Standard",
                EmailAccess = "None",
                ConsentTimestamp = DateTime.UtcNow
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _personaRepository.CreateOrUpdateAsync(personaDocument, userId);

        return MapToUserPersona(personaDocument);
    }

    public async Task<UserPersona?> GetPersonaAsync(int personaId)
    {
        var personaDocuments = await _personaRepository.QueryWithSqlAsync(
            $"SELECT * FROM c WHERE c.personaId = '{personaId}'");

        var personaDocument = personaDocuments.FirstOrDefault();
        return personaDocument != null ? MapToUserPersona(personaDocument) : null;
    }

    public async Task<UserPersona?> GetActivePersonaAsync(string userId)
    {
        var personaDocuments = await _personaRepository.QueryWithSqlAsync(
            $"SELECT * FROM c WHERE c.userId = '{userId}' AND c.isDefault = true",
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
            updatedDocument.UpdatedAt = DateTime.UtcNow;

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
                $"SELECT * FROM c WHERE c.userId = '{userId}'",
                partitionKey: userId);

            foreach (var persona in allPersonas)
            {
                if (persona.IsDefault)
                {
                    persona.IsDefault = false;
                    persona.UpdatedAt = DateTime.UtcNow;
                    await _personaRepository.CreateOrUpdateAsync(persona, userId);
                }
            }

            // Then set the specified persona as default
            var targetPersona = allPersonas.FirstOrDefault(p => p.PersonaId == personaId.ToString());
            if (targetPersona != null)
            {
                targetPersona.IsDefault = true;
                targetPersona.UpdatedAt = DateTime.UtcNow;
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
        var numericId = Math.Abs(document.PersonaId.GetHashCode());

        return new UserPersona
        {
            Id = numericId,
            UserId = document.UserId,
            DigitalName = document.BasicInfo.DisplayName,
            Age = document.BasicInfo.Age,
            DateOfBirth = null, // Not stored in PersonaDocument
            CulturalBackground = document.BasicInfo.CulturalBackground,
            PrimaryLanguages = JsonSerializer.Serialize(document.BasicInfo.Languages.Spoken),
            PrimaryTimeZone = "UTC", // Default since not stored
            PassionsAndInterests = string.Join(", ", document.BasicInfo.Interests),
            LearningStyle = document.LearningPreferences.PreferredLearningStyle,
            CommunicationPreferences = "Friendly", // Default since not stored in PersonaDocument
            UIComplexityLevel = document.TypeLeapConfig.UIComplexityLevel,
            ColorPreferences = document.TypeLeapConfig.ColorPreferences,
            FontSizePreferences = document.TypeLeapConfig.FontSizePreferences,
            EnableAnimations = document.TypeLeapConfig.EnableAnimations,
            NavigationStyle = document.TypeLeapConfig.NavigationStyle,
            DataSovereigntyLevel = "Balanced", // Default mapping
            DefaultSharingProfile = document.PrivacySettings.DataSharing,
            RequireExplicitConsent = document.PrivacySettings.ConsentTimestamp.HasValue,
            EnableProactiveAssistance = true, // Default since not stored
            CompanionPersonality = "Helpful", // Default since not stored
            PreferredInteractionStyle = "Conversational", // Default since not stored
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            PersonalityProfile = "", // Default since not stored
            DataRetentionPreference = "Standard" // Default since not stored
        };
    }

    private PersonaDocument MapToPersonaDocument(UserPersona persona, PersonaDocument existingDocument)
    {
        existingDocument.BasicInfo.DisplayName = persona.DigitalName;
        existingDocument.BasicInfo.Age = persona.Age;
        existingDocument.BasicInfo.CulturalBackground = persona.CulturalBackground;
        existingDocument.BasicInfo.Interests = persona.PassionsAndInterests.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

        existingDocument.TypeLeapConfig.UIComplexityLevel = persona.UIComplexityLevel;
        existingDocument.TypeLeapConfig.ColorPreferences = persona.ColorPreferences;
        existingDocument.TypeLeapConfig.FontSizePreferences = persona.FontSizePreferences;
        existingDocument.TypeLeapConfig.EnableAnimations = persona.EnableAnimations;
        existingDocument.TypeLeapConfig.NavigationStyle = persona.NavigationStyle;

        existingDocument.LearningPreferences.PreferredLearningStyle = persona.LearningStyle;

        existingDocument.PrivacySettings.DataSharing = persona.DefaultSharingProfile;

        return existingDocument;
    }
}