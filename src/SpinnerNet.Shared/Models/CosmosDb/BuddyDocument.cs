/*
 * COMMENTED OUT FOR SPRINT 1 SIMPLIFICATION - BUDDY DOCUMENT IS FUTURE SPRINT
 * Buddy data model will be implemented in a later sprint
 * Focus: User registration + Persona creation only for Sprint 1
 */

/*
// Removed System.Text.Json.Serialization - using direct property names for Cosmos DB (Microsoft NoSQL pattern)

namespace SpinnerNet.Shared.Models.CosmosDb;

/// <summary>
/// Cosmos DB document representing an AI buddy companion
/// Container: Buddies, Partition Key: /UserId
/// </summary>
public class BuddyDocument
{
    /// <summary>
    /// Document ID (buddy_${userId}_${buddyId})
    /// </summary>
    public string id { get; set; } = string.Empty;

    /// <summary>
    /// Document type for discriminator
    /// </summary>
    public string type { get; set; } = "buddy";

    /// <summary>
    /// User identifier (partition key)
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Buddy identifier
    /// </summary>
    public string buddyId { get; set; } = string.Empty;

    /// <summary>
    /// Associated persona identifier
    /// </summary>
    public string personaId { get; set; } = string.Empty;

    /// <summary>
    /// Type of buddy (DailyCompanion, PhotographyExpert, BusinessAdvisor, etc.)
    /// </summary>
    public string buddyType { get; set; } = "DailyCompanion";

    /// <summary>
    /// Basic buddy information
    /// </summary>
    public BuddyBasicInfo basicInfo { get; set; } = new();

    /// <summary>
    /// Buddy's personality configuration
    /// </summary>
    public BuddyPersonality personality { get; set; } = new();

    /// <summary>
    /// Buddy's capabilities and permissions
    /// </summary>
    public BuddyCapabilities capabilities { get; set; } = new();

    /// <summary>
    /// Learning data and user preferences
    /// </summary>
    public BuddyLearningData learningData { get; set; } = new();

    /// <summary>
    /// References to memory documents
    /// </summary>
    public List<string> memoryReferences { get; set; } = new();

    /// <summary>
    /// When the buddy was created
    /// </summary>
    public DateTime createdAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the buddy was last active
    /// </summary>
    public DateTime lastActiveAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Cosmos DB timestamp
    /// </summary>
    public long _ts { get; set; }
}

/// <summary>
/// Basic buddy information
/// </summary>
public class BuddyBasicInfo
{
    /// <summary>
    /// Buddy's name
    /// </summary>
    public string name { get; set; } = string.Empty;

    /// <summary>
    /// Avatar identifier or image URL
    /// </summary>
    public string avatar { get; set; } = "friendly-assistant";

    /// <summary>
    /// Brief description of the buddy
    /// </summary>
    public string description { get; set; } = string.Empty;
}

/// <summary>
/// Buddy personality configuration
/// </summary>
public class BuddyPersonality
{
    /// <summary>
    /// Personality archetype (Helpful, Professional, Friendly, Mentor, etc.)
    /// </summary>
    public string archetype { get; set; } = "Helpful";

    /// <summary>
    /// Personality traits (0.0 to 1.0 scale)
    /// </summary>
    public PersonalityTraits traits { get; set; } = new();

    /// <summary>
    /// Communication style preferences
    /// </summary>
    public CommunicationStyle communicationStyle { get; set; } = new();
}

/// <summary>
/// Personality traits with 0.0 to 1.0 scaling
/// </summary>
public class PersonalityTraits
{
    /// <summary>
    /// How friendly the buddy is (0.0 = formal, 1.0 = very friendly)
    /// </summary>
    public double friendliness { get; set; } = 0.8;

    /// <summary>
    /// How professional the buddy is (0.0 = casual, 1.0 = very professional)
    /// </summary>
    public double professionalism { get; set; } = 0.7;

    /// <summary>
    /// How proactive the buddy is (0.0 = reactive only, 1.0 = very proactive)
    /// </summary>
    public double proactiveness { get; set; } = 0.6;

    /// <summary>
    /// How formal the buddy's language is (0.0 = very casual, 1.0 = very formal)
    /// </summary>
    public double formality { get; set; } = 0.4;
}

/// <summary>
/// Communication style configuration
/// </summary>
public class CommunicationStyle
{
    /// <summary>
    /// Overall tone (friendly, professional, casual, formal)
    /// </summary>
    public string tone { get; set; } = "friendly";

    /// <summary>
    /// Verbosity level (concise, normal, detailed)
    /// </summary>
    public string verbosity { get; set; } = "normal";

    /// <summary>
    /// Whether to use emojis
    /// </summary>
    public bool useEmojis { get; set; } = true;

    /// <summary>
    /// Whether to use humor
    /// </summary>
    public bool useHumor { get; set; } = false;
}

/// <summary>
/// Buddy capabilities and permissions
/// </summary>
public class BuddyCapabilities
{
    /// <summary>
    /// Email management capabilities
    /// </summary>
    public EmailManagementCapability emailManagement { get; set; } = new();

    /// <summary>
    /// Calendar integration capabilities
    /// </summary>
    public CalendarIntegrationCapability calendarIntegration { get; set; } = new();

    /// <summary>
    /// Task management capabilities
    /// </summary>
    public TaskManagementCapability taskManagement { get; set; } = new();
}

/// <summary>
/// Email management capability configuration
/// </summary>
public class EmailManagementCapability
{
    /// <summary>
    /// Whether email management is enabled
    /// </summary>
    public bool enabled { get; set; } = false;

    /// <summary>
    /// Permissions for email access
    /// </summary>
    public List<string> permissions { get; set; } = new();

    /// <summary>
    /// Connected email accounts
    /// </summary>
    public List<ConnectedEmailAccount> accounts { get; set; } = new();
}

/// <summary>
/// Connected email account information
/// </summary>
public class ConnectedEmailAccount
{
    /// <summary>
    /// Email provider (gmail, outlook, etc.)
    /// </summary>
    public string provider { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string email { get; set; } = string.Empty;

    /// <summary>
    /// When the account was connected
    /// </summary>
    public DateTime connectedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Connection status (active, inactive, error)
    /// </summary>
    public string status { get; set; } = "inactive";

    /// <summary>
    /// Specific permissions for this account
    /// </summary>
    public List<string> permissions { get; set; } = new();
}

/// <summary>
/// Calendar integration capability
/// </summary>
public class CalendarIntegrationCapability
{
    /// <summary>
    /// Whether calendar integration is enabled
    /// </summary>
    public bool enabled { get; set; } = false;

    /// <summary>
    /// Permissions for calendar access
    /// </summary>
    public List<string> permissions { get; set; } = new();
}

/// <summary>
/// Task management capability
/// </summary>
public class TaskManagementCapability
{
    /// <summary>
    /// Whether task management is enabled
    /// </summary>
    public bool enabled { get; set; } = false;

    /// <summary>
    /// Permissions for task management
    /// </summary>
    public List<string> permissions { get; set; } = new();
}

/// <summary>
/// Buddy learning data and user preferences
/// </summary>
public class BuddyLearningData
{
    /// <summary>
    /// Learned user preferences
    /// </summary>
    public LearnedUserPreferences userPreferences { get; set; } = new();

    /// <summary>
    /// History of adaptations made
    /// </summary>
    public List<AdaptationRecord> adaptationHistory { get; set; } = new();
}

/// <summary>
/// User preferences learned by the buddy
/// </summary>
public class LearnedUserPreferences
{
    /// <summary>
    /// Preferred time for buddy responses
    /// </summary>
    public string preferredResponseTime { get; set; } = "anytime";

    /// <summary>
    /// Preferred communication frequency
    /// </summary>
    public string communicationFrequency { get; set; } = "moderate";

    /// <summary>
    /// Topics the user is most interested in
    /// </summary>
    public List<string> topicInterests { get; set; } = new();
}

/// <summary>
/// Record of an adaptation made by the buddy
/// </summary>
public class AdaptationRecord
{
    /// <summary>
    /// When the adaptation was made
    /// </summary>
    public DateTime timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Description of the adaptation
    /// </summary>
    public string adaptation { get; set; } = string.Empty;

    /// <summary>
    /// Reason for the adaptation
    /// </summary>
    public string reason { get; set; } = string.Empty;
}
*/