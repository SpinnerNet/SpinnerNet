namespace SpinnerNet.Shared.DTOs;

/// <summary>
/// UI configuration adapted to a user's persona
/// </summary>
public class PersonaUIConfiguration
{
    /// <summary>
    /// UI complexity level
    /// </summary>
    public string ComplexityLevel { get; set; } = "Standard";
    
    /// <summary>
    /// Color scheme
    /// </summary>
    public string ColorScheme { get; set; } = "Default";
    
    /// <summary>
    /// Font size
    /// </summary>
    public string FontSize { get; set; } = "Medium";
    
    /// <summary>
    /// Enable animations
    /// </summary>
    public bool EnableAnimations { get; set; } = true;
    
    /// <summary>
    /// Navigation style
    /// </summary>
    public string NavigationStyle { get; set; } = "Standard";
    
    /// <summary>
    /// Theme preference
    /// </summary>
    public string Theme { get; set; } = "Light";
    
    /// <summary>
    /// Language code
    /// </summary>
    public string Language { get; set; } = "en";
    
    /// <summary>
    /// Cultural adaptations
    /// </summary>
    public Dictionary<string, object> CulturalAdaptations { get; set; } = new();
    
    /// <summary>
    /// Age-appropriate adjustments
    /// </summary>
    public Dictionary<string, object> AgeAdaptations { get; set; } = new();
    
    /// <summary>
    /// Interest-based customizations
    /// </summary>
    public Dictionary<string, object> InterestCustomizations { get; set; } = new();
}