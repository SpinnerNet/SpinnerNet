using System.Globalization;
using System.Resources;
using Microsoft.AspNetCore.Http;

namespace SpinnerNet.Shared.Localization;

public class LocalizationService : ILocalizationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ResourceManager _resourceManager;
    
    public LocalizationService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _resourceManager = new ResourceManager("SpinnerNet.Shared.Resources.Strings", typeof(LocalizationService).Assembly);
    }

    public CultureInfo CurrentCulture
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                // Check URL parameter first
                if (httpContext.Request.Query.ContainsKey("lang"))
                {
                    var langParam = httpContext.Request.Query["lang"].ToString();
                    if (langParam == "en" || langParam == "de" || langParam == "fr" || langParam == "es")
                    {
                        httpContext.Items["Culture"] = langParam;
                        return new CultureInfo(langParam);
                    }
                }
                
                // Check Items cache
                var culture = httpContext.Items["Culture"] as string;
                if (culture != null)
                {
                    return new CultureInfo(culture);
                }
                
                // Check cookie
                if (httpContext.Request.Cookies.ContainsKey("Culture"))
                {
                    var cookieCulture = httpContext.Request.Cookies["Culture"];
                    if (cookieCulture == "en" || cookieCulture == "de" || cookieCulture == "fr" || cookieCulture == "es")
                    {
                        httpContext.Items["Culture"] = cookieCulture;
                        return new CultureInfo(cookieCulture);
                    }
                }
            }
            
            // Default to German
            return new CultureInfo("de");
        }
    }

    public string GetString(string key, params object[] arguments)
    {
        try
        {
            var value = _resourceManager.GetString(key, CurrentCulture);
            if (string.IsNullOrEmpty(value))
            {
                // Fallback to English if translation not found
                value = _resourceManager.GetString(key, CultureInfo.GetCultureInfo("en"));
            }
            
            if (string.IsNullOrEmpty(value))
            {
                return $"[{key}]"; // Return key in brackets if no translation found
            }

            return arguments.Any() ? string.Format(value, arguments) : value;
        }
        catch
        {
            return $"[{key}]";
        }
    }

    public IEnumerable<CultureInfo> GetSupportedCultures()
    {
        return new[]
        {
            new CultureInfo("de"),
            new CultureInfo("en"),
            new CultureInfo("fr"),
            new CultureInfo("es")
        };
    }

    public Task SetCultureAsync(string culture)
    {
        if (_httpContextAccessor.HttpContext != null)
        {
            _httpContextAccessor.HttpContext.Items["Culture"] = culture;
            
            // Set cookie for persistence
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = false,
                Secure = _httpContextAccessor.HttpContext.Request.IsHttps,
                SameSite = SameSiteMode.Lax
            };
            
            _httpContextAccessor.HttpContext.Response.Cookies.Append("Culture", culture, cookieOptions);
        }
        
        return Task.CompletedTask;
    }

    public string GetEnumTranslation<T>(T enumValue) where T : Enum
    {
        var key = $"{typeof(T).Name}_{enumValue}";
        return GetString(key);
    }

    public string FormatDate(DateTime date, string? format = null)
    {
        format ??= CurrentCulture.DateTimeFormat.ShortDatePattern;
        return date.ToString(format, CurrentCulture);
    }

    public string FormatNumber(decimal number)
    {
        return number.ToString("N", CurrentCulture);
    }
}