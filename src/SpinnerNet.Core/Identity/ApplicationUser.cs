using Microsoft.AspNetCore.Identity;

namespace SpinnerNet.Core.Data;

/// <summary>
/// Application user extending Identity user
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// User's display name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Indicates whether the user has completed onboarding
    /// </summary>
    public bool HasCompletedOnboarding { get; set; } = false;
    
    /// <summary>
    /// User's age
    /// </summary>
    public int? Age { get; set; }
    
    /// <summary>
    /// User's cultural background
    /// </summary>
    public string? CulturalBackground { get; set; }
    
    /// <summary>
    /// User's timezone
    /// </summary>
    public string TimeZone { get; set; } = "UTC";
    
    /// <summary>
    /// When the user was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the user was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Active persona ID
    /// </summary>
    public int? ActivePersonaId { get; set; }
}