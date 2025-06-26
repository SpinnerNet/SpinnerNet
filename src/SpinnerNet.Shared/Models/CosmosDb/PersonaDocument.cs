namespace SpinnerNet.Shared.Models.CosmosDb;

/// <summary>
/// Cosmos DB document representing a user persona with buddy relationships
/// Container: Personas, Partition Key: /UserId
/// Following Microsoft's NoSQL pattern - no JsonPropertyName attributes, direct property names
/// </summary>
public class PersonaDocument
{
    /// <summary>
    /// Document ID (persona_${userId}_${personaId})
    /// </summary>
    public string id { get; set; } = string.Empty;

    /// <summary>
    /// Document type for discriminator
    /// </summary>
    public string type { get; set; } = "persona";

    /// <summary>
    /// User identifier (partition key)
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Persona identifier
    /// </summary>
    public string personaId { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the default persona for the user
    /// </summary>
    public bool isDefault { get; set; } = false;

    /// <summary>
    /// Basic persona information
    /// </summary>
    public PersonaBasicInfo basicInfo { get; set; } = new();

    /// <summary>
    /// TypeLeap UI adaptation configuration
    /// </summary>
    public TypeLeapConfiguration typeLeapConfig { get; set; } = new();

    /// <summary>
    /// Learning preferences
    /// </summary>
    public LearningPreferences learningPreferences { get; set; } = new();

    /// <summary>
    /// Privacy and data sovereignty settings
    /// </summary>
    public PrivacySettings privacySettings { get; set; } = new();

    /// <summary>
    /// AI buddy relationships
    /// </summary>
    public List<BuddyRelationship> buddyRelationships { get; set; } = new();

    /// <summary>
    /// When the persona was created
    /// </summary>
    public DateTime createdAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the persona was last updated
    /// </summary>
    public DateTime updatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Basic persona information
/// </summary>
public class PersonaBasicInfo
{
    /// <summary>
    /// Display name for the persona
    /// </summary>
    public string displayName { get; set; } = string.Empty;

    /// <summary>
    /// User's age
    /// </summary>
    public int age { get; set; } = 0;

    /// <summary>
    /// Cultural background
    /// </summary>
    public string culturalBackground { get; set; } = string.Empty;

    /// <summary>
    /// Occupation or role
    /// </summary>
    public string occupation { get; set; } = string.Empty;

    /// <summary>
    /// Areas of interest
    /// </summary>
    public List<string> interests { get; set; } = new();

    /// <summary>
    /// Language information and proficiency
    /// </summary>
    public LanguageInfo languages { get; set; } = new();
}

/// <summary>
/// Language information for a persona
/// </summary>
public class LanguageInfo
{
    /// <summary>
    /// Mother tongue language code (ISO 639-1)
    /// </summary>
    public string motherTongue { get; set; } = "en";

    /// <summary>
    /// Preferred language for interface
    /// </summary>
    public string preferred { get; set; } = "en";

    /// <summary>
    /// List of languages the user can speak
    /// </summary>
    public List<string> spoken { get; set; } = new() { "en" };

    /// <summary>
    /// Language proficiency levels
    /// </summary>
    public Dictionary<string, string> proficiency { get; set; } = new();
}

/// <summary>
/// TypeLeap UI adaptation configuration
/// </summary>
public class TypeLeapConfiguration
{
    /// <summary>
    /// UI complexity level (Simple, Standard, Advanced)
    /// </summary>
    public string uiComplexityLevel { get; set; } = "Standard";

    /// <summary>
    /// Font size preferences (Small, Medium, Large, ExtraLarge)
    /// </summary>
    public string fontSizePreferences { get; set; } = "Medium";

    /// <summary>
    /// Color preferences (Default, HighContrast, Dark, Bright)
    /// </summary>
    public string colorPreferences { get; set; } = "Default";

    /// <summary>
    /// Whether to enable animations
    /// </summary>
    public bool enableAnimations { get; set; } = true;

    /// <summary>
    /// Navigation style (Simple, Standard, Full)
    /// </summary>
    public string navigationStyle { get; set; } = "Standard";

    /// <summary>
    /// Age-specific adaptations
    /// </summary>
    public Dictionary<string, string> ageAdaptations { get; set; } = new();
}

/// <summary>
/// Learning preferences
/// </summary>
public class LearningPreferences
{
    /// <summary>
    /// Preferred learning style (Visual, Auditory, Kinesthetic, ReadingWriting)
    /// </summary>
    public string preferredLearningStyle { get; set; } = "Visual";

    /// <summary>
    /// Learning pace preference (SelfPaced, Guided, Structured)
    /// </summary>
    public string pacePreference { get; set; } = "SelfPaced";

    /// <summary>
    /// Difficulty level preference (Beginner, Intermediate, Advanced)
    /// </summary>
    public string difficultyLevel { get; set; } = "Intermediate";
}

/// <summary>
/// Privacy and data sovereignty settings
/// </summary>
public class PrivacySettings
{
    /// <summary>
    /// Data sharing preference (None, Selective, Open)
    /// </summary>
    public string dataSharing { get; set; } = "Selective";

    /// <summary>
    /// AI interaction level (Basic, Standard, Enhanced)
    /// </summary>
    public string aiInteraction { get; set; } = "Standard";

    /// <summary>
    /// Email access permission (None, ReadOnly, Authorized)
    /// </summary>
    public string emailAccess { get; set; } = "None";

    /// <summary>
    /// When consent was given
    /// </summary>
    public DateTime consentTimestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Relationship with an AI buddy
/// </summary>
public class BuddyRelationship
{
    /// <summary>
    /// Buddy identifier
    /// </summary>
    public string buddyId { get; set; } = string.Empty;

    /// <summary>
    /// Type of relationship (DailyCompanion, SpecializedExpert, etc.)
    /// </summary>
    public string relationship { get; set; } = "DailyCompanion";

    /// <summary>
    /// Trust level between user and buddy (0.0 to 1.0)
    /// </summary>
    public double trustLevel { get; set; } = 0.5;

    /// <summary>
    /// Permissions granted to this buddy
    /// </summary>
    public List<string> permissions { get; set; } = new();

    /// <summary>
    /// When the relationship was created
    /// </summary>
    public DateTime createdAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the relationship is currently active
    /// </summary>
    public bool isActive { get; set; } = true;
}