using SpinnerNet.Web.Pages.Shared;
using SpinnerNet.Shared.Localization;

namespace SpinnerNet.Web.Pages;

public class AboutModel : LocalizedPageModel
{
    public AboutModel(ILocalizationService localizationService, IConfiguration configuration) 
        : base(localizationService, configuration)
    {
    }

    public void OnGet()
    {
        ViewData["Title"] = GetText("About_Title", "Über uns");
        ViewData["Description"] = GetText("About_Description", 
            "Erfahren Sie mehr über die Bamberger Spinnerei, unsere Vision, Mission und das Team, das Innovation und Community Building in Bamberg vorantreibt.");
    }
}