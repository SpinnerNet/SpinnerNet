using System.Text.Json.Serialization;

namespace SpinnerNet.Shared.Models.CosmosDb;

/// <summary>
/// Cosmos DB document representing an AI buddy companion
/// Container: Personas, Partition Key: /userId
/// </summary>
public class BuddyDocument
{
    /// <summary>
    /// Document ID (buddy_${userId}_${buddyId})
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Document type for discriminator
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "buddy";

    /// <summary>
    /// User identifier (partition key)
    /// </summary>
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Buddy identifier
    /// </summary>
    [JsonPropertyName("buddyId")]
    public string BuddyId { get; set; } = string.Empty;

    /// <summary>
    /// Associated persona identifier
    /// </summary>
    [JsonPropertyName("personaId")]
    public string PersonaId { get; set; } = string.Empty;

    /// <summary>
    /// Type of buddy (DailyCompanion, PhotographyExpert, BusinessAdvisor, etc.)
    /// </summary>
    [JsonPropertyName("buddyType")]
    public string BuddyType { get; set; } = "DailyCompanion";

    /// <summary>
    /// Basic buddy information
    /// </summary>
    [JsonPropertyName("basicInfo")]
    public BuddyBasicInfo BasicInfo { get; set; } = new();

    /// <summary>
    /// Buddy's personality configuration
    /// </summary>
    [JsonPropertyName("personality")]
    public BuddyPersonality Personality { get; set; } = new();

    /// <summary>
    /// Buddy's capabilities and permissions
    /// </summary>
    [JsonPropertyName("capabilities")]
    public BuddyCapabilities Capabilities { get; set; } = new();

    /// <summary>
    /// Learning data and user preferences
    /// </summary>
    [JsonPropertyName("learningData")]
    public BuddyLearningData LearningData { get; set; } = new();

    /// <summary>
    /// References to memory documents
    /// </summary>
    [JsonPropertyName("memoryReferences")]
    public List<string> MemoryReferences { get; set; } = new();

    /// <summary>
    /// When the buddy was created
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the buddy was last active
    /// </summary>
    [JsonPropertyName("lastActiveAt")]
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Cosmos DB timestamp
    /// </summary>
    [JsonPropertyName("_ts")]
    public long Timestamp { get; set; }
}

/// <summary>
/// Basic buddy information
/// </summary>
public class BuddyBasicInfo
{
    /// <summary>
    /// Buddy's name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Avatar identifier or image URL
    /// </summary>
    [JsonPropertyName("avatar")]
    public string Avatar { get; set; } = "friendly-assistant";

    /// <summary>
    /// Brief description of the buddy
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Buddy personality configuration
/// </summary>
public class BuddyPersonality
{
    /// <summary>
    /// Personality archetype (Helpful, Professional, Friendly, Mentor, etc.)
    /// </summary>
    [JsonPropertyName("archetype")]
    public string Archetype { get; set; } = "Helpful";

    /// <summary>
    /// Personality traits (0.0 to 1.0 scale)
    /// </summary>
    [JsonPropertyName("traits")]
    public PersonalityTraits Traits { get; set; } = new();

    /// <summary>
    /// Communication style preferences
    /// </summary>
    [JsonPropertyName("communicationStyle")]
    public CommunicationStyle CommunicationStyle { get; set; } = new();
}

/// <summary>
/// Personality traits with 0.0 to 1.0 scaling
/// </summary>
public class PersonalityTraits
{
    /// <summary>
    /// How friendly the buddy is (0.0 = formal, 1.0 = very friendly)
    /// </summary>
    [JsonPropertyName("friendliness")]
    public double Friendliness { get; set; } = 0.8;

    /// <summary>
    /// How professional the buddy is (0.0 = casual, 1.0 = very professional)
    /// </summary>
    [JsonPropertyName("professionalism")]
    public double Professionalism { get; set; } = 0.7;

    /// <summary>
    /// How proactive the buddy is (0.0 = reactive only, 1.0 = very proactive)
    /// </summary>
    [JsonPropertyName("proactiveness")]
    public double Proactiveness { get; set; } = 0.6;

    /// <summary>
    /// How formal the buddy's language is (0.0 = very casual, 1.0 = very formal)
    /// </summary>
    [JsonPropertyName("formality")]
    public double Formality { get; set; } = 0.4;
}

/// <summary>
/// Communication style configuration
/// </summary>
public class CommunicationStyle
{
    /// <summary>
    /// Overall tone (friendly, professional, casual, formal)
    /// </summary>
    [JsonPropertyName("tone")]
    public string Tone { get; set; } = "friendly";

    /// <summary>
    /// Verbosity level (concise, normal, detailed)
    /// </summary>
    [JsonPropertyName("verbosity")]
    public string Verbosity { get; set; } = "normal";

    /// <summary>
    /// Whether to use emojis
    /// </summary>
    [JsonPropertyName("useEmojis")]
    public bool UseEmojis { get; set; } = true;

    /// <summary>
    /// Whether to use humor
    /// </summary>
    [JsonPropertyName("useHumor")]
    public bool UseHumor { get; set; } = false;
}

/// <summary>
/// Buddy capabilities and permissions
/// </summary>
public class BuddyCapabilities
{
    /// <summary>
    /// Email management capabilities
    /// </summary>
    [JsonPropertyName("emailManagement")]
    public EmailManagementCapability EmailManagement { get; set; } = new();

    /// <summary>
    /// Calendar integration capabilities
    /// </summary>
    [JsonPropertyName("calendarIntegration")]
    public CalendarIntegrationCapability CalendarIntegration { get; set; } = new();

    /// <summary>
    /// Task management capabilities
    /// </summary>
    [JsonPropertyName("taskManagement")]
    public TaskManagementCapability TaskManagement { get; set; } = new();
}

/// <summary>
/// Email management capability configuration
/// </summary>
public class EmailManagementCapability
{
    /// <summary>
    /// Whether email management is enabled
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Permissions for email access
    /// </summary>
    [JsonPropertyName("permissions")]
    public List<string> Permissions { get; set; } = new();

    /// <summary>
    /// Connected email accounts
    /// </summary>
    [JsonPropertyName("accounts")]
    public List<ConnectedEmailAccount> Accounts { get; set; } = new();
}

/// <summary>
/// Connected email account information
/// </summary>
public class ConnectedEmailAccount
{
    /// <summary>
    /// Email provider (gmail, outlook, etc.)
    /// </summary>
    [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// When the account was connected
    /// </summary>
    [JsonPropertyName("connectedAt")]
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Connection status (active, inactive, error)
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "inactive";

    /// <summary>
    /// Specific permissions for this account
    /// </summary>
    [JsonPropertyName("permissions")]
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// Calendar integration capability
/// </summary>
public class CalendarIntegrationCapability
{
    /// <summary>
    /// Whether calendar integration is enabled
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Permissions for calendar access
    /// </summary>
    [JsonPropertyName("permissions")]
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// Task management capability
/// </summary>
public class TaskManagementCapability
{
    /// <summary>
    /// Whether task management is enabled
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Permissions for task management
    /// </summary>
    [JsonPropertyName("permissions")]
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// Buddy learning data and user preferences
/// </summary>
public class BuddyLearningData
{
    /// <summary>
    /// Learned user preferences
    /// </summary>
    [JsonPropertyName("userPreferences")]
    public LearnedUserPreferences UserPreferences { get; set; } = new();

    /// <summary>
    /// History of adaptations made
    /// </summary>
    [JsonPropertyName("adaptationHistory")]
    public List<AdaptationRecord> AdaptationHistory { get; set; } = new();
}

/// <summary>
/// User preferences learned by the buddy
/// </summary>
public class LearnedUserPreferences
{
    /// <summary>
    /// Preferred time for buddy responses
    /// </summary>
    [JsonPropertyName("preferredResponseTime")]
    public string PreferredResponseTime { get; set; } = "anytime";

    /// <summary>
    /// Preferred communication frequency
    /// </summary>
    [JsonPropertyName("communicationFrequency")]
    public string CommunicationFrequency { get; set; } = "moderate";

    /// <summary>
    /// Topics the user is most interested in
    /// </summary>
    [JsonPropertyName("topicInterests")]
    public List<string> TopicInterests { get; set; } = new();
}

/// <summary>
/// Record of an adaptation made by the buddy
/// </summary>
public class AdaptationRecord
{
    /// <summary>
    /// When the adaptation was made
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Description of the adaptation
    /// </summary>
    [JsonPropertyName("adaptation")]
    public string Adaptation { get; set; } = string.Empty;

    /// <summary>
    /// Reason for the adaptation
    /// </summary>
    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;
}