using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace SpinnerNet.Shared.Models;

/// <summary>
/// Represents a user in the Spinner.Net platform
/// Enhanced for Sprint 1 with age protection and data sovereignty
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    [Key]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// User's display name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Password hash for local authentication
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// User's birth date (for age verification)
    /// </summary>
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// User's calculated age
    /// </summary>
    public int Age { get; set; }

    /// <summary>
    /// Whether the user is under 18 years old
    /// </summary>
    public bool IsMinor { get; set; }

    /// <summary>
    /// User's profile image URL
    /// </summary>
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// Brief description or bio of the user
    /// </summary>
    [StringLength(500)]
    public string? Bio { get; set; }

    /// <summary>
    /// ISO 639-1 language code for the user's preferred language
    /// </summary>
    [StringLength(10)]
    public string PreferredLanguage { get; set; } = "en";

    /// <summary>
    /// User's timezone
    /// </summary>
    [StringLength(50)]
    public string TimeZone { get; set; } = "UTC";

    /// <summary>
    /// User's cultural background
    /// </summary>
    [StringLength(100)]
    public string? CulturalBackground { get; set; }

    /// <summary>
    /// Indicates whether the user prefers the dark theme
    /// </summary>
    public bool UseDarkTheme { get; set; } = false;

    /// <summary>
    /// Whether the user account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when the user was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the user was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the user last logged in
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Indicates whether the user has completed the onboarding process
    /// </summary>
    public bool HasCompletedOnboarding { get; set; } = false;

    /// <summary>
    /// ID of the active persona associated with this user
    /// </summary>
    public int? ActivePersonaId { get; set; }

    // Data Sovereignty Settings (JSON columns for SQLite)
    
    /// <summary>
    /// Data residency preference (stored as JSON)
    /// </summary>
    public string DataResidencyPreference { get; set; } = "Local";

    /// <summary>
    /// Data sovereignty settings (stored as JSON)
    /// </summary>
    public string DataSovereigntySettingsJson { get; set; } = "{}";

    /// <summary>
    /// User safety settings (stored as JSON)
    /// </summary>
    public string SafetySettingsJson { get; set; } = "{}";

    /// <summary>
    /// Parental control settings for minors (stored as JSON)
    /// </summary>
    public string? ParentalControlsJson { get; set; }

    /// <summary>
    /// Parent's email address (for minors)
    /// </summary>
    public string? ParentalEmail { get; set; }

    /// <summary>
    /// When parental consent was verified
    /// </summary>
    public DateTime? ParentalConsentVerifiedAt { get; set; }

    // Helper methods to work with JSON properties

    /// <summary>
    /// Get data sovereignty settings as object
    /// </summary>
    [NotMapped]
    public DataSovereigntySettings DataSovereigntySettings
    {
        get => string.IsNullOrEmpty(DataSovereigntySettingsJson) 
            ? new DataSovereigntySettings() 
            : JsonSerializer.Deserialize<DataSovereigntySettings>(DataSovereigntySettingsJson) ?? new DataSovereigntySettings();
        set => DataSovereigntySettingsJson = JsonSerializer.Serialize(value);
    }

    /// <summary>
    /// Get safety settings as object
    /// </summary>
    [NotMapped]
    public SafetySettings SafetySettings
    {
        get => string.IsNullOrEmpty(SafetySettingsJson) 
            ? new SafetySettings() 
            : JsonSerializer.Deserialize<SafetySettings>(SafetySettingsJson) ?? new SafetySettings();
        set => SafetySettingsJson = JsonSerializer.Serialize(value);
    }

    /// <summary>
    /// Get parental controls as object
    /// </summary>
    [NotMapped]
    public ParentalControls? ParentalControls
    {
        get => string.IsNullOrEmpty(ParentalControlsJson) 
            ? null 
            : JsonSerializer.Deserialize<ParentalControls>(ParentalControlsJson);
        set => ParentalControlsJson = value == null ? null : JsonSerializer.Serialize(value);
    }
}

/// <summary>
/// Data sovereignty settings for EF model
/// </summary>
public class DataSovereigntySettings
{
    public string PreferredRegion { get; set; } = "local";
    public string DataResidency { get; set; } = "local_first";
    public string AiProcessingLocation { get; set; } = "local_preferred";
    public bool EncryptionRequired { get; set; } = true;
    public bool ThirdPartyDataSharing { get; set; } = false;
    public int DataRetentionDays { get; set; } = 365;
}

/// <summary>
/// Safety settings for EF model
/// </summary>
public class SafetySettings
{
    public string MaxContentLevel { get; set; } = "safe";
    public bool ContentFilteringEnabled { get; set; } = true;
    public bool SafeModeEnabled { get; set; } = false;
    public bool ParentalNotificationsEnabled { get; set; } = false;
    public List<string> RestrictedTopics { get; set; } = new();
    public List<string> AllowedInteractionTypes { get; set; } = new();
}

/// <summary>
/// Parental controls for EF model
/// </summary>
public class ParentalControls
{
    public string? ParentEmail { get; set; }
    public DateTime? ConsentVerifiedAt { get; set; }
    public string NotificationLevel { get; set; } = "important";
    public bool RequiresOversight { get; set; } = true;
    public List<string> AllowedContentCategories { get; set; } = new();
}