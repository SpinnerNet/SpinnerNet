using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpinnerNet.Shared.DTOs;
using SpinnerNet.Shared.Models;
using System.Text.Json;

namespace SpinnerNet.Personas.Services;

/// <summary>
/// Service for managing user personas
/// </summary>
public class PersonaService : IPersonaService
{
    private readonly ILogger<PersonaService> _logger;
    private readonly DbContext _dbContext;
    
    public PersonaService(ILogger<PersonaService> logger, DbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }
    
    /// <inheritdoc />
    public async Task<UserPersona> CreatePersonaFromOnboardingAsync(string userId, OnboardingRequest request)
    {
        try
        {
            _logger.LogInformation("Creating persona for user {UserId}", userId);
            
            var persona = new UserPersona
            {
                UserId = userId,
                DigitalName = request.DisplayName,
                Age = request.Age,
                DateOfBirth = request.DateOfBirth,
                CulturalBackground = request.CulturalBackground,
                PrimaryLanguages = JsonSerializer.Serialize(new[] { request.PrimaryLanguage }.Concat(request.AdditionalLanguages)),
                PrimaryTimeZone = request.TimeZone,
                PassionsAndInterests = string.Join(", ", request.Interests),
                LearningStyle = request.LearningStyle,
                CommunicationPreferences = request.CommunicationStyle,
                UIComplexityLevel = AdaptUIComplexityForAge(request.Age, request.UIComplexity),
                ColorPreferences = AdaptColorPreferencesForAge(request.Age, request.ColorScheme),
                FontSizePreferences = AdaptFontSizeForAge(request.Age),
                EnableAnimations = AdaptAnimationsForAge(request.Age, request.EnableAnimations),
                NavigationStyle = AdaptNavigationForAge(request.Age),
                DataSovereigntyLevel = request.PrivacyLevel,
                DefaultSharingProfile = "Selective",
                RequireExplicitConsent = true,
                EnableProactiveAssistance = request.EnableProactiveAssistance,
                CompanionPersonality = request.CompanionPersonality,
                PreferredInteractionStyle = "Conversational",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            _dbContext.Set<UserPersona>().Add(persona);
            await _dbContext.SaveChangesAsync();
            
            // Set as active persona for the user
            var user = await _dbContext.Set<User>().FindAsync(userId);
            if (user != null)
            {
                user.ActivePersonaId = persona.Id;
                user.HasCompletedOnboarding = true;
                user.Age = request.Age;
                user.CulturalBackground = request.CulturalBackground;
                user.TimeZone = request.TimeZone;
                user.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }
            
            _logger.LogInformation("Successfully created persona {PersonaId} for user {UserId}", persona.Id, userId);
            return persona;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating persona for user {UserId}", userId);
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task<UserPersona?> GetPersonaAsync(int personaId)
    {
        try
        {
            return await _dbContext.Set<UserPersona>()
                .FirstOrDefaultAsync(p => p.Id == personaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting persona {PersonaId}", personaId);
            return null;
        }
    }
    
    /// <inheritdoc />
    public async Task<UserPersona?> GetActivePersonaAsync(string userId)
    {
        try
        {
            var user = await _dbContext.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == userId);
                
            if (user?.ActivePersonaId == null)
            {
                // Try to find first persona for user
                var firstPersona = await _dbContext.Set<UserPersona>()
                    .FirstOrDefaultAsync(p => p.UserId == userId);
                    
                if (firstPersona != null && user != null)
                {
                    user.ActivePersonaId = firstPersona.Id;
                    await _dbContext.SaveChangesAsync();
                    return firstPersona;
                }
                
                return null;
            }
            
            return await _dbContext.Set<UserPersona>()
                .FirstOrDefaultAsync(p => p.Id == user.ActivePersonaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active persona for user {UserId}", userId);
            return null;
        }
    }
    
    /// <inheritdoc />
    public async Task<bool> UpdatePersonaAsync(UserPersona persona)
    {
        try
        {
            persona.UpdatedAt = DateTime.UtcNow;
            _dbContext.Set<UserPersona>().Update(persona);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating persona {PersonaId}", persona.Id);
            return false;
        }
    }
    
    /// <inheritdoc />
    public async Task<bool> SetActivePersonaAsync(string userId, int personaId)
    {
        try
        {
            var user = await _dbContext.Set<User>().FindAsync(userId);
            if (user == null) return false;
            
            // Verify persona belongs to user
            var persona = await _dbContext.Set<UserPersona>()
                .FirstOrDefaultAsync(p => p.Id == personaId && p.UserId == userId);
                
            if (persona == null) return false;
            
            user.ActivePersonaId = personaId;
            user.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting active persona {PersonaId} for user {UserId}", personaId, userId);
            return false;
        }
    }
    
    /// <inheritdoc />
    public async Task<PersonaUIConfiguration> GetPersonaUIConfigurationAsync(int personaId)
    {
        try
        {
            var persona = await GetPersonaAsync(personaId);
            if (persona == null)
            {
                return new PersonaUIConfiguration();
            }
            
            var config = new PersonaUIConfiguration
            {
                ComplexityLevel = persona.UIComplexityLevel,
                ColorScheme = persona.ColorPreferences,
                FontSize = persona.FontSizePreferences,
                EnableAnimations = persona.EnableAnimations,
                NavigationStyle = persona.NavigationStyle,
                Theme = persona.User?.UseDarkTheme == true ? "Dark" : "Light",
                Language = JsonSerializer.Deserialize<string[]>(persona.PrimaryLanguages)?.FirstOrDefault() ?? "en"
            };
            
            // Add age-based adaptations
            config.AgeAdaptations = GetAgeAdaptations(persona.Age);
            
            // Add cultural adaptations
            if (!string.IsNullOrEmpty(persona.CulturalBackground))
            {
                config.CulturalAdaptations = GetCulturalAdaptations(persona.CulturalBackground);
            }
            
            // Add interest-based customizations
            if (!string.IsNullOrEmpty(persona.PassionsAndInterests))
            {
                config.InterestCustomizations = GetInterestCustomizations(persona.PassionsAndInterests);
            }
            
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting UI configuration for persona {PersonaId}", personaId);
            return new PersonaUIConfiguration();
        }
    }
    
    #region Private Helper Methods
    
    private string AdaptUIComplexityForAge(int age, string requested)
    {
        return age switch
        {
            <= 12 => "Simple",
            <= 17 => "Standard",
            <= 65 => requested,
            _ => "Simple"
        };
    }
    
    private string AdaptColorPreferencesForAge(int age, string requested)
    {
        return age switch
        {
            <= 12 => "Bright",
            <= 17 => "Vibrant",
            <= 65 => requested,
            _ => "HighContrast"
        };
    }
    
    private string AdaptFontSizeForAge(int age)
    {
        return age switch
        {
            <= 12 => "Large",
            <= 17 => "Medium",
            <= 65 => "Medium",
            _ => "Large"
        };
    }
    
    private bool AdaptAnimationsForAge(int age, bool requested)
    {
        return age switch
        {
            <= 12 => true,
            <= 17 => requested,
            <= 65 => requested,
            _ => false
        };
    }
    
    private string AdaptNavigationForAge(int age)
    {
        return age switch
        {
            <= 12 => "Simple",
            <= 17 => "Standard",
            <= 65 => "Standard",
            _ => "Simple"
        };
    }
    
    private Dictionary<string, object> GetAgeAdaptations(int age)
    {
        var adaptations = new Dictionary<string, object>();
        
        if (age <= 12)
        {
            adaptations["iconSize"] = "large";
            adaptations["showGuidance"] = true;
            adaptations["parentalControls"] = true;
        }
        else if (age <= 17)
        {
            adaptations["socialFeatures"] = "enhanced";
            adaptations["gamification"] = true;
        }
        else if (age > 65)
        {
            adaptations["iconSize"] = "large";
            adaptations["showTooltips"] = true;
            adaptations["simplifiedNavigation"] = true;
        }
        
        return adaptations;
    }
    
    private Dictionary<string, object> GetCulturalAdaptations(string culture)
    {
        var adaptations = new Dictionary<string, object>();
        
        // Basic cultural adaptations
        if (culture.Contains("Arab") || culture.Contains("Hebrew"))
        {
            adaptations["textDirection"] = "rtl";
        }
        
        if (culture.Contains("Asian") || culture.Contains("Chinese") || culture.Contains("Japanese"))
        {
            adaptations["respectfulTone"] = true;
            adaptations["formalGreetings"] = true;
        }
        
        return adaptations;
    }
    
    private Dictionary<string, object> GetInterestCustomizations(string interests)
    {
        var customizations = new Dictionary<string, object>();
        var interestList = interests.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(i => i.Trim().ToLower()).ToList();
        
        if (interestList.Any(i => i.Contains("photo") || i.Contains("camera")))
        {
            customizations["highlightVisualFeatures"] = true;
            customizations["showImagePreview"] = true;
        }
        
        if (interestList.Any(i => i.Contains("art") || i.Contains("design")))
        {
            customizations["enhancedColorPicker"] = true;
            customizations["showCreativeTools"] = true;
        }
        
        return customizations;
    }
    
    #endregion
}