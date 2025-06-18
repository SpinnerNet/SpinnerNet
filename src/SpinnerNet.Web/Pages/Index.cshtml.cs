using SpinnerNet.Web.Pages.Shared;
using SpinnerNet.Shared.Localization;

namespace SpinnerNet.Web.Pages;

public class IndexModel : LocalizedPageModel
{
    public IndexModel(ILocalizationService localizationService, IConfiguration configuration) 
        : base(localizationService, configuration)
    {
    }

    public void OnGet()
    {
        ViewData["Title"] = GetText("Index_Hero_Title", "Bamberger Spinnerei");
        ViewData["Description"] = GetText("Index_Hero_Description", 
            "Die Bamberger Spinnerei schafft Raum, Zeit und Vertrauen, damit Menschen Ideen entwickeln, verwirklichen und Gemeinschaft gestalten können.");
    }
}