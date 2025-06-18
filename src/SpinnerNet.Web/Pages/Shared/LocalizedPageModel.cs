using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Filters;
using SpinnerNet.Shared.Localization;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace SpinnerNet.Web.Pages.Shared;

public abstract class LocalizedPageModel : PageModel
{
    protected readonly ILocalizationService _localizationService;
    protected readonly IConfiguration _configuration;

    protected LocalizedPageModel(ILocalizationService localizationService, IConfiguration configuration)
    {
        _localizationService = localizationService;
        _configuration = configuration;
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        // Check for language parameter in URL
        if (Request.Query.ContainsKey("lang"))
        {
            var lang = Request.Query["lang"].ToString();
            if (lang == "en" || lang == "de" || lang == "fr" || lang == "es")
            {
                await _localizationService.SetCultureAsync(lang);
            }
        }
        // Check for culture cookie
        else if (Request.Cookies.ContainsKey("Culture"))
        {
            var cookieCulture = Request.Cookies["Culture"];
            if (cookieCulture == "en" || cookieCulture == "de" || cookieCulture == "fr" || cookieCulture == "es")
            {
                await _localizationService.SetCultureAsync(cookieCulture);
            }
        }

        // Load configuration values into ViewData
        LoadConfigurationToViewData();

        await next();
    }

    /// <summary>
    /// Get localized text by key
    /// </summary>
    public string GetText(string key, params object[] arguments)
    {
        return _localizationService.GetString(key, arguments);
    }

    /// <summary>
    /// Get localized text by key with fallback
    /// </summary>
    public string GetText(string key, string fallback, params object[] arguments)
    {
        var result = _localizationService.GetString(key, arguments);
        
        // If the result is in the format [key], use the fallback
        if (result.StartsWith('[') && result.EndsWith(']'))
        {
            return arguments.Any() ? string.Format(fallback, arguments) : fallback;
        }
        
        return result;
    }

    /// <summary>
    /// Get current culture
    /// </summary>
    public CultureInfo CurrentCulture => _localizationService.CurrentCulture;

    /// <summary>
    /// Get current language code
    /// </summary>
    public string CurrentLanguage => _localizationService.CurrentCulture.TwoLetterISOLanguageName;

    /// <summary>
    /// Load configuration values into ViewData
    /// </summary>
    private void LoadConfigurationToViewData()
    {
        // Load contact emails
        ViewData["InfoEmail"] = _configuration["ContactEmails:Info"];
        ViewData["ContactEmail"] = _configuration["ContactEmails:Contact"];
        ViewData["BetaEmail"] = _configuration["ContactEmails:Beta"];
        ViewData["EmergencyEmail"] = _configuration["ContactEmails:Emergency"];
        ViewData["ZeitsparkasseEmail"] = _configuration["ContactEmails:Zeitsparkasse"];
        ViewData["TechEmail"] = _configuration["ContactEmails:Tech"];
        ViewData["CommunityEmail"] = _configuration["ContactEmails:Community"];
        ViewData["EducationEmail"] = _configuration["ContactEmails:Education"];

        // Load site configuration
        ViewData["SiteAuthor"] = _configuration["Site:Author"];
        ViewData["BlazorAppUrl"] = _configuration["Site:BlazorAppUrl"];
    }
}