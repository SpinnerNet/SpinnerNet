using SpinnerNet.Web.Pages.Shared;
using SpinnerNet.Shared.Localization;

namespace SpinnerNet.Web.Pages;

public class BetaModel : LocalizedPageModel
{
    public BetaModel(ILocalizationService localizationService, IConfiguration configuration) 
        : base(localizationService, configuration)
    {
    }

    public void OnGet()
    {
        ViewData["Title"] = GetText("Beta_Title", "Beta Zugang");
        ViewData["Description"] = GetText("Beta_Description", 
            "Werden Sie Beta-Tester für Spinner.Net und erleben Sie als Erste unsere innovativen Features wie AI-Buddys, Zeitsparkasse und Community-Tools.");
    }
}