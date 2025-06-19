using SpinnerNet.Web.Pages.Shared;
using SpinnerNet.Shared.Localization;

namespace SpinnerNet.Web.Pages;

public class SpinnenModel : LocalizedPageModel
{
    public SpinnenModel(ILocalizationService localizationService, IConfiguration configuration) 
        : base(localizationService, configuration)
    {
    }

    public void OnGet()
    {
        ViewData["Title"] = GetText("Spinnen_Title");
        ViewData["Description"] = GetText("Spinnen_Description");
    }
}