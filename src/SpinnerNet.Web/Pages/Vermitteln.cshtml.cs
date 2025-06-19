using SpinnerNet.Web.Pages.Shared;
using SpinnerNet.Shared.Localization;

namespace SpinnerNet.Web.Pages;

public class VermittelnModel : LocalizedPageModel
{
    public VermittelnModel(ILocalizationService localizationService, IConfiguration configuration) 
        : base(localizationService, configuration)
    {
    }

    public void OnGet()
    {
        ViewData["Title"] = GetText("Vermitteln_Title");
        ViewData["Description"] = GetText("Vermitteln_Description");
    }
}