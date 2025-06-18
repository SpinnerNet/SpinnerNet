using System.Globalization;

namespace SpinnerNet.Shared.Extensions;

/// <summary>
/// Extension methods for string manipulation
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts a string to title case (first letter of each word capitalized)
    /// </summary>
    /// <param name="text">The text to convert</param>
    /// <returns>Title case string</returns>
    public static string ToTitleCase(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(text.ToLower());
    }
}