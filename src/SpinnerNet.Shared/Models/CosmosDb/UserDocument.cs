using System.Text.Json.Serialization;

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
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Document type for discriminator
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "user";

    /// <summary>
    /// User identifier (partition key)
    /// </summary>
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's display name
    /// </summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// User's age (calculated from birth date)
    /// </summary>
    [JsonPropertyName("age")]
    public int Age { get; set; }

    /// <summary>
    /// User's birth date (for age verification)
    /// </summary>
    [JsonPropertyName("birthDate")]
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// Whether the user is under 18 years old
    /// </summary>
    [JsonPropertyName("isMinor")]
    public bool IsMinor { get; set; }

    /// <summary>
    /// When the user was created
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last login timestamp
    /// </summary>
    [JsonPropertyName("lastLoginAt")]
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Whether the user account is active
    /// </summary>
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Authentication providers for this user
    /// </summary>
    [JsonPropertyName("authProviders")]
    public List<AuthProvider> AuthProviders { get; set; } = new();

    /// <summary>
    /// User preferences
    /// </summary>
    [JsonPropertyName("preferences")]
    public UserPreferences Preferences { get; set; } = new();

    /// <summary>
    /// Data sovereignty and privacy settings
    /// </summary>
    [JsonPropertyName("dataSovereignty")]
    public DataSovereigntySettings DataSovereignty { get; set; } = new();

    /// <summary>
    /// Content safety and age protection settings
    /// </summary>
    [JsonPropertyName("safetySettings")]
    public UserSafetySettings SafetySettings { get; set; } = new();

    /// <summary>
    /// Parental control settings (only for minors)
    /// </summary>
    [JsonPropertyName("parentalControls")]
    public ParentalControlSettings? ParentalControls { get; set; }

    /// <summary>
    /// Cosmos DB timestamp
    /// </summary>
    [JsonPropertyName("_ts")]
    public long Timestamp { get; set; }
}

/// <summary>
/// Authentication provider information
/// </summary>
public class AuthProvider
{
    /// <summary>
    /// Provider name (local, google, microsoft, etc.)
    /// </summary>
    [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Provider-specific user identifier
    /// </summary>
    [JsonPropertyName("providerId")]
    public string ProviderId { get; set; } = string.Empty;

    /// <summary>
    /// Email associated with this provider
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Password hash (for local provider only)
    /// </summary>
    [JsonPropertyName("passwordHash")]
    public string? PasswordHash { get; set; }
}

/// <summary>
/// User preferences and settings
/// </summary>
public class UserPreferences
{
    /// <summary>
    /// User's preferred language
    /// </summary>
    [JsonPropertyName("language")]
    public string Language { get; set; } = "en";

    /// <summary>
    /// User's timezone
    /// </summary>
    [JsonPropertyName("timezone")]
    public string Timezone { get; set; } = "UTC";

    /// <summary>
    /// Notification preferences
    /// </summary>
    [JsonPropertyName("notifications")]
    public NotificationPreferences Notifications { get; set; } = new();
}

/// <summary>
/// Notification preferences
/// </summary>
public class NotificationPreferences
{
    /// <summary>
    /// Enable email notifications
    /// </summary>
    [JsonPropertyName("email")]
    public bool Email { get; set; } = true;

    /// <summary>
    /// Enable push notifications
    /// </summary>
    [JsonPropertyName("push")]
    public bool Push { get; set; } = false;
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
    [JsonPropertyName("preferredRegion")]
    public string PreferredRegion { get; set; } = "local";

    /// <summary>
    /// Data residency preference
    /// </summary>
    [JsonPropertyName("dataResidency")]
    public string DataResidency { get; set; } = "local_first";

    /// <summary>
    /// Where AI processing should occur
    /// </summary>
    [JsonPropertyName("aiProcessingLocation")]
    public string AiProcessingLocation { get; set; } = "local_preferred";

    /// <summary>
    /// Whether encryption is required for user data
    /// </summary>
    [JsonPropertyName("encryptionRequired")]
    public bool EncryptionRequired { get; set; } = true;

    /// <summary>
    /// Whether third-party data sharing is allowed
    /// </summary>
    [JsonPropertyName("thirdPartyDataSharing")]
    public bool ThirdPartyDataSharing { get; set; } = false;

    /// <summary>
    /// Data retention period in days
    /// </summary>
    [JsonPropertyName("dataRetentionDays")]
    public int DataRetentionDays { get; set; } = 365;
}

/// <summary>
/// User safety settings for content protection
/// </summary>
public class UserSafetySettings
{
    /// <summary>
    /// Maximum content level allowed for this user
    /// </summary>
    [JsonPropertyName("maxContentLevel")]
    public string MaxContentLevel { get; set; } = "safe";

    /// <summary>
    /// Whether content filtering is enabled
    /// </summary>
    [JsonPropertyName("contentFilteringEnabled")]
    public bool ContentFilteringEnabled { get; set; } = true;

    /// <summary>
    /// Whether safe mode is enabled (stricter filtering)
    /// </summary>
    [JsonPropertyName("safeModeEnabled")]
    public bool SafeModeEnabled { get; set; } = false;

    /// <summary>
    /// Whether parental notifications are enabled
    /// </summary>
    [JsonPropertyName("parentalNotificationsEnabled")]
    public bool ParentalNotificationsEnabled { get; set; } = false;

    /// <summary>
    /// Topics that are restricted for this user
    /// </summary>
    [JsonPropertyName("restrictedTopics")]
    public List<string> RestrictedTopics { get; set; } = new();

    /// <summary>
    /// Allowed interaction types for this user
    /// </summary>
    [JsonPropertyName("allowedInteractionTypes")]
    public List<string> AllowedInteractionTypes { get; set; } = new();
}

/// <summary>
/// Parental control settings for minor users
/// </summary>
public class ParentalControlSettings
{
    /// <summary>
    /// Parent's email address
    /// </summary>
    [JsonPropertyName("parentEmail")]
    public string? ParentEmail { get; set; }

    /// <summary>
    /// When parental consent was verified
    /// </summary>
    [JsonPropertyName("consentVerifiedAt")]
    public DateTime? ConsentVerifiedAt { get; set; }

    /// <summary>
    /// Level of notifications sent to parent
    /// </summary>
    [JsonPropertyName("notificationLevel")]
    public string NotificationLevel { get; set; } = "important";

    /// <summary>
    /// Whether this account requires parental oversight
    /// </summary>
    [JsonPropertyName("requiresOversight")]
    public bool RequiresOversight { get; set; } = true;

    /// <summary>
    /// Time restrictions for account usage
    /// </summary>
    [JsonPropertyName("timeRestrictions")]
    public List<TimeRestriction> TimeRestrictions { get; set; } = new();

    /// <summary>
    /// Content categories allowed by parent
    /// </summary>
    [JsonPropertyName("allowedContentCategories")]
    public List<string> AllowedContentCategories { get; set; } = new();
}

/// <summary>
/// Time restriction settings
/// </summary>
public class TimeRestriction
{
    /// <summary>
    /// Day of week (0 = Sunday, 1 = Monday, etc.)
    /// </summary>
    [JsonPropertyName("dayOfWeek")]
    public int DayOfWeek { get; set; }

    /// <summary>
    /// Start time (24-hour format)
    /// </summary>
    [JsonPropertyName("startTime")]
    public string StartTime { get; set; } = "00:00";

    /// <summary>
    /// End time (24-hour format)
    /// </summary>
    [JsonPropertyName("endTime")]
    public string EndTime { get; set; } = "23:59";

    /// <summary>
    /// Whether this restriction is active
    /// </summary>
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;
}