using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SpinnerNet.Web.Areas.Account.Pages;

public class LogoutModel : PageModel
{
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(ILogger<LogoutModel> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userName = User.Identity.Name ?? "Unknown";
            await HttpContext.SignOutAsync("Cookies");
            _logger.LogInformation("User {UserName} signed out", userName);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userName = User.Identity.Name ?? "Unknown";
            await HttpContext.SignOutAsync("Cookies");
            _logger.LogInformation("User {UserName} signed out", userName);
        }

        return RedirectToPage("/");
    }
}