// Removed System.Text.Json.Serialization - using direct property names for Cosmos DB (Microsoft NoSQL pattern)

namespace SpinnerNet.Shared.Models.CosmosDb;

/// <summary>
/// Cosmos DB document representing a user in the Spinner.Net platform
/// Container: Users, Partition Key: /userId
/// </summary>
public class UserDocument
{
    /// <summary>
    /// Document ID (user_${userId})
    /// </summary>
    public string id { get; set; } = string.Empty;

    /// <summary>
    /// Document type for discriminator
    /// </summary>
    public string type { get; set; } = "user";

    /// <summary>
    /// User identifier (partition key)
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    public string email { get; set; } = string.Empty;

    /// <summary>
    /// User's display name
    /// </summary>
    public string displayName { get; set; } = string.Empty;

    /// <summary>
    /// User's age (calculated from birth date)
    /// </summary>
    public int age { get; set; }

    /// <summary>
    /// User's birth date (for age verification)
    /// </summary>
    public DateTime birthDate { get; set; }

    /// <summary>
    /// Whether the user is under 18 years old
    /// </summary>
    public bool isMinor { get; set; }

    /// <summary>
    /// When the user was created
    /// </summary>
    public DateTime createdAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? lastLoginAt { get; set; }

    /// <summary>
    /// Whether the user account is active
    /// </summary>
    public bool isActive { get; set; } = true;

    /// <summary>
    /// Authentication providers for this user
    /// </summary>
    public List<AuthProvider> authProviders { get; set; } = new();

    /// <summary>
    /// User preferences
    /// </summary>
    public UserPreferences preferences { get; set; } = new();

    /// <summary>
    /// Data sovereignty and privacy settings
    /// </summary>
    public DataSovereigntySettings dataSovereignty { get; set; } = new();

    /// <summary>
    /// Content safety and age protection settings
    /// </summary>
    public UserSafetySettings safetySettings { get; set; } = new();

    /// <summary>
    /// Parental control settings (only for minors)
    /// </summary>
    public ParentalControlSettings? parentalControls { get; set; }

    /// <summary>
    /// Cosmos DB timestamp
    /// </summary>
    public long _ts { get; set; }
}

/// <summary>
/// Authentication provider information
/// </summary>
public class AuthProvider
{
    /// <summary>
    /// Provider name (local, google, microsoft, etc.)
    /// </summary>
    public string provider { get; set; } = string.Empty;

    /// <summary>
    /// Provider-specific user identifier
    /// </summary>
    public string providerId { get; set; } = string.Empty;

    /// <summary>
    /// Email associated with this provider
    /// </summary>
    public string email { get; set; } = string.Empty;

    /// <summary>
    /// Password hash (for local provider only)
    /// </summary>
    public string? passwordHash { get; set; }
}

/// <summary>
/// User preferences and settings
/// </summary>
public class UserPreferences
{
    /// <summary>
    /// User's preferred language
    /// </summary>
    public string language { get; set; } = "en";

    /// <summary>
    /// User's timezone
    /// </summary>
    public string timezone { get; set; } = "UTC";

    /// <summary>
    /// Notification preferences
    /// </summary>
    public NotificationPreferences notifications { get; set; } = new();
}

/// <summary>
/// Notification preferences
/// </summary>
public class NotificationPreferences
{
    /// <summary>
    /// Enable email notifications
    /// </summary>
    public bool email { get; set; } = true;

    /// <summary>
    /// Enable push notifications
    /// </summary>
    public bool push { get; set; } = false;
}

/// <summary>
/// Data residency and sovereignty preferences
/// </summary>
public enum DataResidencyPreference
{
    Local = 0,
    EU = 1,
    US = 2,
    UserRegion = 3,
    Hybrid = 4
}

/// <summary>
/// Data sovereignty settings for user privacy control
/// </summary>
public class DataSovereigntySettings
{
    /// <summary>
    /// Preferred geographic region for data storage
    /// </summary>
    public string preferredRegion { get; set; } = "local";

    /// <summary>
    /// Data residency preference
    /// </summary>
    public string dataResidency { get; set; } = "local_first";

    /// <summary>
    /// Where AI processing should occur
    /// </summary>
    public string aiProcessingLocation { get; set; } = "local_preferred";

    /// <summary>
    /// Whether encryption is required for user data
    /// </summary>
    public bool encryptionRequired { get; set; } = true;

    /// <summary>
    /// Whether third-party data sharing is allowed
    /// </summary>
    public bool thirdPartyDataSharing { get; set; } = false;

    /// <summary>
    /// Data retention period in days
    /// </summary>
    public int dataRetentionDays { get; set; } = 365;
}

/// <summary>
/// User safety settings for content protection
/// </summary>
public class UserSafetySettings
{
    /// <summary>
    /// Maximum content level allowed for this user
    /// </summary>
    public string maxContentLevel { get; set; } = "safe";

    /// <summary>
    /// Whether content filtering is enabled
    /// </summary>
    public bool contentFilteringEnabled { get; set; } = true;

    /// <summary>
    /// Whether safe mode is enabled (stricter filtering)
    /// </summary>
    public bool safeModeEnabled { get; set; } = false;

    /// <summary>
    /// Whether parental notifications are enabled
    /// </summary>
    public bool parentalNotificationsEnabled { get; set; } = false;

    /// <summary>
    /// Topics that are restricted for this user
    /// </summary>
    public List<string> restrictedTopics { get; set; } = new();

    /// <summary>
    /// Allowed interaction types for this user
    /// </summary>
    public List<string> allowedInteractionTypes { get; set; } = new();
}

/// <summary>
/// Parental control settings for minor users
/// </summary>
public class ParentalControlSettings
{
    /// <summary>
    /// Parent's email address
    /// </summary>
    public string? parentEmail { get; set; }

    /// <summary>
    /// When parental consent was verified
    /// </summary>
    public DateTime? consentVerifiedAt { get; set; }

    /// <summary>
    /// Level of notifications sent to parent
    /// </summary>
    public string notificationLevel { get; set; } = "important";

    /// <summary>
    /// Whether this account requires parental oversight
    /// </summary>
    public bool requiresOversight { get; set; } = true;

    /// <summary>
    /// Time restrictions for account usage
    /// </summary>
    public List<TimeRestriction> timeRestrictions { get; set; } = new();

    /// <summary>
    /// Content categories allowed by parent
    /// </summary>
    public List<string> allowedContentCategories { get; set; } = new();
}

/// <summary>
/// Time restriction settings
/// </summary>
public class TimeRestriction
{
    /// <summary>
    /// Day of week (0 = Sunday, 1 = Monday, etc.)
    /// </summary>
    public int dayOfWeek { get; set; }

    /// <summary>
    /// Start time (24-hour format)
    /// </summary>
    public string startTime { get; set; } = "00:00";

    /// <summary>
    /// End time (24-hour format)
    /// </summary>
    public string endTime { get; set; } = "23:59";

    /// <summary>
    /// Whether this restriction is active
    /// </summary>
    public bool isActive { get; set; } = true;
}