using Microsoft.AspNetCore.Mvc;
using SpinnerNet.Web.Pages.Shared;
using SpinnerNet.Shared.Localization;

namespace SpinnerNet.Web.Pages
{
    public class ImpressumModel : LocalizedPageModel
    {
        public ImpressumModel(ILocalizationService localizationService, IConfiguration configuration) 
            : base(localizationService, configuration)
        {
        }

        public void OnGet()
        {
        }
    }
}