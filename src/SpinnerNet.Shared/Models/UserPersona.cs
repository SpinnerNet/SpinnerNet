using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpinnerNet.Shared.Models;

/// <summary>
/// Represents a user's core persona in the Spinner.Net system
/// </summary>
public class UserPersona
{
    /// <summary>
    /// Unique identifier for the persona
    /// </summary>
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// The ID of the user that owns this persona
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// The digital name of the persona
    /// </summary>
    [Required]
    [StringLength(100)]
    public string DigitalName { get; set; } = string.Empty;
    
    /// <summary>
    /// User's age (for TypeLeap adaptation)
    /// </summary>
    public int Age { get; set; }
    
    /// <summary>
    /// User's date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; set; }
    
    /// <summary>
    /// User's cultural background
    /// </summary>
    [StringLength(100)]
    public string CulturalBackground { get; set; } = string.Empty;
    
    /// <summary>
    /// Primary languages (JSON array)
    /// </summary>
    public string PrimaryLanguages { get; set; } = "[\"en\"]";
    
    /// <summary>
    /// Primary timezone
    /// </summary>
    [StringLength(50)]
    public string PrimaryTimeZone { get; set; } = "UTC";
    
    /// <summary>
    /// User's passions and interests
    /// </summary>
    public string PassionsAndInterests { get; set; } = string.Empty;
    
    /// <summary>
    /// Preferred learning style
    /// </summary>
    [StringLength(50)]
    public string LearningStyle { get; set; } = "Visual";
    
    /// <summary>
    /// Communication preferences
    /// </summary>
    [StringLength(50)]
    public string CommunicationPreferences { get; set; } = "Friendly";
    
    /// <summary>
    /// UI complexity level
    /// </summary>
    [StringLength(20)]
    public string UIComplexityLevel { get; set; } = "simplified";
    
    /// <summary>
    /// Color preferences
    /// </summary>
    [StringLength(20)]
    public string ColorPreferences { get; set; } = "auto";
    
    /// <summary>
    /// Font size preferences
    /// </summary>
    [StringLength(20)]
    public string FontSizePreferences { get; set; } = "medium";
    
    /// <summary>
    /// Enable animations
    /// </summary>
    public bool EnableAnimations { get; set; } = true;
    
    /// <summary>
    /// Navigation style
    /// </summary>
    [StringLength(20)]
    public string NavigationStyle { get; set; } = "simple";
    
    /// <summary>
    /// Data sovereignty level
    /// </summary>
    [StringLength(20)]
    public string DataSovereigntyLevel { get; set; } = "high";
    
    /// <summary>
    /// Default sharing profile
    /// </summary>
    [StringLength(20)]
    public string DefaultSharingProfile { get; set; } = "Selective";
    
    /// <summary>
    /// Require explicit consent
    /// </summary>
    public bool RequireExplicitConsent { get; set; } = true;
    
    /// <summary>
    /// Enable proactive assistance
    /// </summary>
    public bool EnableProactiveAssistance { get; set; } = true;
    
    /// <summary>
    /// Companion personality
    /// </summary>
    [StringLength(50)]
    public string CompanionPersonality { get; set; } = "Helpful";
    
    /// <summary>
    /// Preferred interaction style
    /// </summary>
    [StringLength(50)]
    public string PreferredInteractionStyle { get; set; } = "Conversational";
    
    /// <summary>
    /// When the persona was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the persona was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Personality profile
    /// </summary>
    [StringLength(200)]
    public string PersonalityProfile { get; set; } = string.Empty;
    
    /// <summary>
    /// Data retention preference
    /// </summary>
    [StringLength(50)]
    public string DataRetentionPreference { get; set; } = "Standard";
    
    /// <summary>
    /// Navigation property to User (for EF Core)
    /// </summary>
    public User? User { get; set; }
    
    /// <summary>
    /// Persona ID for compatibility with components
    /// </summary>
    [NotMapped]
    public string PersonaId => Id.ToString();
    
    /// <summary>
    /// Basic info for component compatibility
    /// </summary>
    [NotMapped]
    public BasicInfo BasicInfo => new()
    {
        Name = DigitalName,
        Languages = new LanguagePreferences
        {
            Preferred = PrimaryLanguages.Contains("en") ? "en" : 
                       PrimaryLanguages.Contains("de") ? "de" : "en"
        }
    };
    
    /// <summary>
    /// TypeLeap configuration for UI adaptation
    /// </summary>
    [NotMapped]
    public TypeLeapConfig TypeLeapConfig => new()
    {
        InterfaceComplexity = UIComplexityLevel,
        ColorScheme = ColorPreferences
    };
}

/// <summary>
/// Basic information container
/// </summary>
public class BasicInfo
{
    public string Name { get; set; } = string.Empty;
    public LanguagePreferences Languages { get; set; } = new();
}

/// <summary>
/// Language preferences
/// </summary>
public class LanguagePreferences
{
    public string Preferred { get; set; } = "en";
}

/// <summary>
/// TypeLeap UI configuration
/// </summary>
public class TypeLeapConfig
{
    public string InterfaceComplexity { get; set; } = "simplified";
    public string ColorScheme { get; set; } = "auto";
    
    /// <summary>
    /// Primary timezone
    /// </summary>
    [StringLength(50)]
    public string PrimaryTimeZone { get; set; } = "UTC";
    
    /// <summary>
    /// Personality profile (JSON blob)
    /// </summary>
    public string PersonalityProfile { get; set; } = "{}";
    
    /// <summary>
    /// Passions and interests (comma-separated for search)
    /// </summary>
    public string PassionsAndInterests { get; set; } = string.Empty;
    
    /// <summary>
    /// Learning style preference
    /// </summary>
    [StringLength(50)]
    public string LearningStyle { get; set; } = "Visual";
    
    /// <summary>
    /// Communication preferences
    /// </summary>
    [StringLength(50)]
    public string CommunicationPreferences { get; set; } = "Friendly";
    
    /// <summary>
    /// TypeLeap UI complexity level
    /// </summary>
    [StringLength(20)]
    public string UIComplexityLevel { get; set; } = "Standard";
    
    /// <summary>
    /// Color preferences for UI
    /// </summary>
    [StringLength(50)]
    public string ColorPreferences { get; set; } = "Default";
    
    /// <summary>
    /// Font size preferences
    /// </summary>
    [StringLength(20)]
    public string FontSizePreferences { get; set; } = "Medium";
    
    /// <summary>
    /// Enable animations in UI
    /// </summary>
    public bool EnableAnimations { get; set; } = true;
    
    /// <summary>
    /// Navigation style preference
    /// </summary>
    [StringLength(20)]
    public string NavigationStyle { get; set; } = "Standard";
    
    /// <summary>
    /// Data sovereignty level
    /// </summary>
    [StringLength(20)]
    public string DataSovereigntyLevel { get; set; } = "Balanced";
    
    /// <summary>
    /// Default sharing profile
    /// </summary>
    [StringLength(20)]
    public string DefaultSharingProfile { get; set; } = "Selective";
    
    /// <summary>
    /// Require explicit consent for data sharing
    /// </summary>
    public bool RequireExplicitConsent { get; set; } = true;
    
    /// <summary>
    /// Data retention preference in days
    /// </summary>
    public int DataRetentionPreference { get; set; } = 365;
    
    /// <summary>
    /// Enable proactive assistance from AI buddies
    /// </summary>
    public bool EnableProactiveAssistance { get; set; } = true;
    
    /// <summary>
    /// Preferred interaction style with AI companions
    /// </summary>
    [StringLength(20)]
    public string PreferredInteractionStyle { get; set; } = "Conversational";
    
    /// <summary>
    /// Companion personality type preference
    /// </summary>
    [StringLength(50)]
    public string CompanionPersonality { get; set; } = "Helpful";
    
    /// <summary>
    /// Timestamp when the persona was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Timestamp when the persona was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Navigation property for the associated user
    /// </summary>
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}