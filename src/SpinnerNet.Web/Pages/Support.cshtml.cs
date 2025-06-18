using SpinnerNet.Web.Pages.Shared;
using SpinnerNet.Shared.Localization;

namespace SpinnerNet.Web.Pages;

public class SupportModel : LocalizedPageModel
{
    public SupportModel(ILocalizationService localizationService, IConfiguration configuration) 
        : base(localizationService, configuration)
    {
    }

    public void OnGet()
    {
        ViewData["Title"] = GetText("Support_Title", "Support");
        ViewData["Description"] = GetText("Support_Description", 
            "Erhalten Sie Hilfe und Support für die Bamberger Spinnerei. FAQ, technischer Support, Community-Hilfe und Notfall-Support für alle Ihre Fragen.");
    }
}