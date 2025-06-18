using System.Text.Json.Serialization;

namespace SpinnerNet.Shared.Models.CosmosDb;

/// <summary>
/// Cosmos DB document representing a user persona with buddy relationships
/// Container: Personas, Partition Key: /userId
/// </summary>
public class PersonaDocument
{
    /// <summary>
    /// Document ID (persona_${userId}_${personaId})
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Document type for discriminator
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "persona";

    /// <summary>
    /// User identifier (partition key)
    /// </summary>
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Persona identifier
    /// </summary>
    [JsonPropertyName("personaId")]
    public string PersonaId { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the default persona for the user
    /// </summary>
    [JsonPropertyName("isDefault")]
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Basic persona information
    /// </summary>
    [JsonPropertyName("basicInfo")]
    public PersonaBasicInfo BasicInfo { get; set; } = new();

    /// <summary>
    /// TypeLeap UI adaptation configuration
    /// </summary>
    [JsonPropertyName("typeLeapConfig")]
    public TypeLeapConfiguration TypeLeapConfig { get; set; } = new();

    /// <summary>
    /// Learning preferences
    /// </summary>
    [JsonPropertyName("learningPreferences")]
    public LearningPreferences LearningPreferences { get; set; } = new();

    /// <summary>
    /// Privacy and data sovereignty settings
    /// </summary>
    [JsonPropertyName("privacySettings")]
    public PrivacySettings PrivacySettings { get; set; } = new();

    /// <summary>
    /// AI buddy relationships
    /// </summary>
    [JsonPropertyName("buddyRelationships")]
    public List<BuddyRelationship> BuddyRelationships { get; set; } = new();

    /// <summary>
    /// When the persona was created
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the persona was last updated
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Cosmos DB timestamp
    /// </summary>
    [JsonPropertyName("_ts")]
    public long Timestamp { get; set; }
}

/// <summary>
/// Basic persona information
/// </summary>
public class PersonaBasicInfo
{
    /// <summary>
    /// Display name for the persona
    /// </summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// User's age
    /// </summary>
    [JsonPropertyName("age")]
    public int Age { get; set; } = 0;

    /// <summary>
    /// Cultural background
    /// </summary>
    [JsonPropertyName("culturalBackground")]
    public string CulturalBackground { get; set; } = string.Empty;

    /// <summary>
    /// Occupation or role
    /// </summary>
    [JsonPropertyName("occupation")]
    public string Occupation { get; set; } = string.Empty;

    /// <summary>
    /// Areas of interest
    /// </summary>
    [JsonPropertyName("interests")]
    public List<string> Interests { get; set; } = new();

    /// <summary>
    /// Language information and proficiency
    /// </summary>
    [JsonPropertyName("languages")]
    public LanguageInfo Languages { get; set; } = new();
}

/// <summary>
/// Language information for a persona
/// </summary>
public class LanguageInfo
{
    /// <summary>
    /// Mother tongue language code (ISO 639-1)
    /// </summary>
    [JsonPropertyName("motherTongue")]
    public string MotherTongue { get; set; } = "en";

    /// <summary>
    /// Preferred language for interface
    /// </summary>
    [JsonPropertyName("preferred")]
    public string Preferred { get; set; } = "en";

    /// <summary>
    /// List of languages the user can speak
    /// </summary>
    [JsonPropertyName("spoken")]
    public List<string> Spoken { get; set; } = new() { "en" };

    /// <summary>
    /// Language proficiency levels
    /// </summary>
    [JsonPropertyName("proficiency")]
    public Dictionary<string, string> Proficiency { get; set; } = new();
}

/// <summary>
/// TypeLeap UI adaptation configuration
/// </summary>
public class TypeLeapConfiguration
{
    /// <summary>
    /// UI complexity level (Simple, Standard, Advanced)
    /// </summary>
    [JsonPropertyName("uiComplexityLevel")]
    public string UIComplexityLevel { get; set; } = "Standard";

    /// <summary>
    /// Font size preferences (Small, Medium, Large, ExtraLarge)
    /// </summary>
    [JsonPropertyName("fontSizePreferences")]
    public string FontSizePreferences { get; set; } = "Medium";

    /// <summary>
    /// Color preferences (Default, HighContrast, Dark, Bright)
    /// </summary>
    [JsonPropertyName("colorPreferences")]
    public string ColorPreferences { get; set; } = "Default";

    /// <summary>
    /// Whether to enable animations
    /// </summary>
    [JsonPropertyName("enableAnimations")]
    public bool EnableAnimations { get; set; } = true;

    /// <summary>
    /// Navigation style (Simple, Standard, Full)
    /// </summary>
    [JsonPropertyName("navigationStyle")]
    public string NavigationStyle { get; set; } = "Standard";

    /// <summary>
    /// Age-specific adaptations
    /// </summary>
    [JsonPropertyName("ageAdaptations")]
    public Dictionary<string, object> AgeAdaptations { get; set; } = new();
}

/// <summary>
/// Learning preferences
/// </summary>
public class LearningPreferences
{
    /// <summary>
    /// Preferred learning style (Visual, Auditory, Kinesthetic, ReadingWriting)
    /// </summary>
    [JsonPropertyName("preferredLearningStyle")]
    public string PreferredLearningStyle { get; set; } = "Visual";

    /// <summary>
    /// Learning pace preference (SelfPaced, Guided, Structured)
    /// </summary>
    [JsonPropertyName("pacePreference")]
    public string PacePreference { get; set; } = "SelfPaced";

    /// <summary>
    /// Difficulty level preference (Beginner, Intermediate, Advanced)
    /// </summary>
    [JsonPropertyName("difficultyLevel")]
    public string DifficultyLevel { get; set; } = "Intermediate";
}

/// <summary>
/// Privacy and data sovereignty settings
/// </summary>
public class PrivacySettings
{
    /// <summary>
    /// Data sharing preference (None, Selective, Open)
    /// </summary>
    [JsonPropertyName("dataSharing")]
    public string DataSharing { get; set; } = "Selective";

    /// <summary>
    /// AI interaction level (Basic, Standard, Enhanced)
    /// </summary>
    [JsonPropertyName("aiInteraction")]
    public string AIInteraction { get; set; } = "Standard";

    /// <summary>
    /// Email access permission (None, ReadOnly, Authorized)
    /// </summary>
    [JsonPropertyName("emailAccess")]
    public string EmailAccess { get; set; } = "None";

    /// <summary>
    /// When consent was given
    /// </summary>
    [JsonPropertyName("consentTimestamp")]
    public DateTime? ConsentTimestamp { get; set; }
}

/// <summary>
/// Relationship with an AI buddy
/// </summary>
public class BuddyRelationship
{
    /// <summary>
    /// Buddy identifier
    /// </summary>
    [JsonPropertyName("buddyId")]
    public string BuddyId { get; set; } = string.Empty;

    /// <summary>
    /// Type of relationship (DailyCompanion, SpecializedExpert, etc.)
    /// </summary>
    [JsonPropertyName("relationship")]
    public string Relationship { get; set; } = "DailyCompanion";

    /// <summary>
    /// Trust level between user and buddy (0.0 to 1.0)
    /// </summary>
    [JsonPropertyName("trustLevel")]
    public double TrustLevel { get; set; } = 0.5;

    /// <summary>
    /// Permissions granted to this buddy
    /// </summary>
    [JsonPropertyName("permissions")]
    public List<string> Permissions { get; set; } = new();

    /// <summary>
    /// When the relationship was created
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the relationship is currently active
    /// </summary>
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;
}