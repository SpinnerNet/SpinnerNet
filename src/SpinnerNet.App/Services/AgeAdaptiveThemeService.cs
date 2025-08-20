using MudBlazor;
using Microsoft.Extensions.Logging;

namespace SpinnerNet.App.Services;

/// <summary>
/// Age-adaptive theme service for SpinnerNet
/// Following SpinnerNet's theming patterns with MudBlazor
/// </summary>
public interface IAgeAdaptiveThemeService
{
    MudTheme GetThemeForAge(int age);
    string GetThemeNameForAge(int age);
    MudTheme GetCustomTheme(string themeName, int age);
    Task<MudTheme> GetThemeAsync(int age, string? preferredTheme = null);
}

/// <summary>
/// Implementation of age-adaptive theming service
/// Creates distinct themes for different age groups following SpinnerNet design patterns
/// </summary>
public class AgeAdaptiveThemeService : IAgeAdaptiveThemeService
{
    private readonly ILogger<AgeAdaptiveThemeService> _logger;
    private readonly Dictionary<string, MudTheme> _baseThemes;

    public AgeAdaptiveThemeService(ILogger<AgeAdaptiveThemeService> logger)
    {
        _logger = logger;
        _baseThemes = InitializeBaseThemes();
    }

    public MudTheme GetThemeForAge(int age)
    {
        var themeName = GetThemeNameForAge(age);
        var baseTheme = _baseThemes[themeName];
        return ApplyAgeAdaptations(baseTheme, age);
    }

    public string GetThemeNameForAge(int age)
    {
        return age switch
        {
            < 13 => "PlayfulChild",
            >= 13 and < 18 => "VibrantTeen",
            >= 18 and < 65 => "ProfessionalAdult",
            _ => "ComfortableSenior"
        };
    }

    public MudTheme GetCustomTheme(string themeName, int age)
    {
        if (!_baseThemes.TryGetValue(themeName, out var theme))
        {
            _logger.LogWarning("Theme {ThemeName} not found, using default", themeName);
            theme = _baseThemes["ProfessionalAdult"];
        }

        return ApplyAgeAdaptations(theme, age);
    }

    public async Task<MudTheme> GetThemeAsync(int age, string? preferredTheme = null)
    {
        await Task.Delay(1); // Simulate async operation
        return string.IsNullOrEmpty(preferredTheme) ? GetThemeForAge(age) : GetCustomTheme(preferredTheme, age);
    }

    private Dictionary<string, MudTheme> InitializeBaseThemes()
    {
        return new Dictionary<string, MudTheme>
        {
            ["PlayfulChild"] = CreatePlayfulChildTheme(),
            ["VibrantTeen"] = CreateVibrantTeenTheme(),
            ["ProfessionalAdult"] = CreateProfessionalAdultTheme(),
            ["ComfortableSenior"] = CreateComfortableSeniorTheme()
        };
    }

    private MudTheme CreatePlayfulChildTheme()
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#FF6B35",      // Bright orange
                Secondary = "#4ECDC4",    // Turquoise
                Tertiary = "#FFE66D",     // Bright yellow
                Background = "#FEFEFE",   // Clean white
                Surface = "#F8F9FA",      // Soft gray
                AppbarBackground = "#FF6B35",
                DrawerBackground = "#FEFEFE"
            }
        };
    }

    private MudTheme CreateVibrantTeenTheme()
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#6C5CE7",      // Purple
                Secondary = "#A29BFE",    // Light purple
                Tertiary = "#FD79A8",     // Pink
                Background = "#FFFFFF",   // Pure white
                Surface = "#F1F2F6",      // Light gray
                AppbarBackground = "#6C5CE7",
                DrawerBackground = "#FFFFFF"
            }
        };
    }

    private MudTheme CreateProfessionalAdultTheme()
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#2E3440",      // Dark gray-blue
                Secondary = "#5E81AC",    // Steel blue
                Tertiary = "#88C0D0",     // Light blue
                Background = "#FFFFFF",   // White
                Surface = "#ECEFF4",      // Light gray
                AppbarBackground = "#2E3440",
                DrawerBackground = "#FFFFFF"
            }
        };
    }

    private MudTheme CreateComfortableSeniorTheme()
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#8B4513",      // Saddle brown
                Secondary = "#D2691E",    // Chocolate
                Tertiary = "#F4A460",     // Sandy brown
                Background = "#FFFEF7",   // Warm white
                Surface = "#F5F5DC",      // Beige
                AppbarBackground = "#8B4513",
                DrawerBackground = "#FFFEF7"
            }
        };
    }

    private MudTheme ApplyAgeAdaptations(MudTheme baseTheme, int age)
    {
        // Clone the theme and apply age-specific adaptations
        var adaptedTheme = new MudTheme
        {
            PaletteLight = baseTheme.PaletteLight,
            PaletteDark = baseTheme.PaletteDark
        };

        // Age-specific spacing and sizing would be handled via CSS classes
        // since MudBlazor doesn't expose these properties directly in the theme
        _logger.LogDebug("Applied age adaptations for age: {Age}", age);

        return adaptedTheme;
    }
}