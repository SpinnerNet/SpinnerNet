using SpinnerNet.Web.Pages.Shared;
using SpinnerNet.Shared.Localization;

namespace SpinnerNet.Web.Pages;

public class MarktplatzModel : LocalizedPageModel
{
    public MarktplatzModel(ILocalizationService localizationService, IConfiguration configuration) 
        : base(localizationService, configuration)
    {
    }

    public void OnGet()
    {
        ViewData["Title"] = GetText("Marktplatz_Title");
        ViewData["Description"] = GetText("Marktplatz_Description");
    }
}