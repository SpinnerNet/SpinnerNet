using System.ComponentModel.DataAnnotations;

namespace SpinnerNet.Shared.DTOs;

/// <summary>
/// Request model for user onboarding and persona initialization
/// </summary>
public class OnboardingRequest
{
    /// <summary>
    /// User's display name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// User's age
    /// </summary>
    [Range(13, 120, ErrorMessage = "Age must be between 13 and 120")]
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
    /// Primary language
    /// </summary>
    [Required]
    [StringLength(10)]
    public string PrimaryLanguage { get; set; } = "en";
    
    /// <summary>
    /// Additional languages
    /// </summary>
    public List<string> AdditionalLanguages { get; set; } = new();
    
    /// <summary>
    /// User's timezone
    /// </summary>
    [Required]
    [StringLength(50)]
    public string TimeZone { get; set; } = "UTC";
    
    /// <summary>
    /// User's main interests and passions
    /// </summary>
    public List<string> Interests { get; set; } = new();
    
    /// <summary>
    /// Preferred learning style
    /// </summary>
    [StringLength(50)]
    public string LearningStyle { get; set; } = "Visual";
    
    /// <summary>
    /// Preferred communication style
    /// </summary>
    [StringLength(50)]
    public string CommunicationStyle { get; set; } = "Friendly";
    
    /// <summary>
    /// UI complexity preference
    /// </summary>
    [StringLength(20)]
    public string UIComplexity { get; set; } = "Standard";
    
    /// <summary>
    /// Preferred color scheme
    /// </summary>
    [StringLength(50)]
    public string ColorScheme { get; set; } = "Default";
    
    /// <summary>
    /// Enable animations
    /// </summary>
    public bool EnableAnimations { get; set; } = true;
    
    /// <summary>
    /// Data privacy level preference
    /// </summary>
    [StringLength(20)]
    public string PrivacyLevel { get; set; } = "Balanced";
    
    /// <summary>
    /// Enable proactive AI assistance
    /// </summary>
    public bool EnableProactiveAssistance { get; set; } = true;
    
    /// <summary>
    /// Preferred AI companion personality
    /// </summary>
    [StringLength(50)]
    public string CompanionPersonality { get; set; } = "Helpful";
}