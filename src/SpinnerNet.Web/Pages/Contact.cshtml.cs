using SpinnerNet.Web.Pages.Shared;
using SpinnerNet.Shared.Localization;

namespace SpinnerNet.Web.Pages;

public class ContactModel : LocalizedPageModel
{
    public ContactModel(ILocalizationService localizationService, IConfiguration configuration) 
        : base(localizationService, configuration)
    {
    }

    public void OnGet()
    {
        ViewData["Title"] = GetText("Contact_Title", "Kontakt");
        ViewData["Description"] = GetText("Contact_Description", 
            "Kontaktieren Sie die Bamberger Spinnerei. Haben Sie Fragen zu unserem Innovation Hub, der Zeitsparkasse oder möchten Sie Teil unserer Community werden?");
    }
}