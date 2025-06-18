using System.Globalization;

namespace SpinnerNet.Shared.Localization;

public interface ILocalizationService
{
    string GetString(string key, params object[] arguments);
    CultureInfo CurrentCulture { get; }
    IEnumerable<CultureInfo> GetSupportedCultures();
    Task SetCultureAsync(string culture);
    string GetEnumTranslation<T>(T enumValue) where T : Enum;
    string FormatDate(DateTime date, string? format = null);
    string FormatNumber(decimal number);
}