using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SpinnerNet.Web.Areas.Account.Pages;

public class RegisterModel : PageModel
{
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(ILogger<RegisterModel> logger)
    {
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(name = "Email")]
        public string Email { get; set; } = "";

        [Required]
        [StringLength(100, Errormessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(name = "Password")]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(name = "Confirm password")]
        [Compare("Password", Errormessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = "";
    }

    public void OnGet()
    {
        // Clear any existing messages
        Errormessage = null;
        Successmessage = null;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // For prototype: Simple registration (in production, use proper user store)
            if (await RegisterUserAsync(Input.Email, Input.Password))
            {
                // Automatically sign in the user after registration
                await SignInUserAsync(Input.Email);
                _logger.LogInformation("New user {Email} registered and signed in", Input.Email);
                return Redirect("/app");
            }
            else
            {
                Errormessage = "A user with this email already exists.";
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            Errormessage = "An error occurred during registration. Please try again.";
            return Page();
        }
    }

    private async Task<bool> RegisterUserAsync(string email, string password)
    {
        // Prototype user storage - in production, use proper database
        // For now, we'll just log the registration and allow it
        // In a real implementation, you would:
        // 1. Check if user already exists
        // 2. Hash the password
        // 3. Store user in database
        
        _logger.LogInformation("User registration attempt for {Email}", email);
        
        // For prototype, always allow registration
        return true;
    }

    private async Task SignInUserAsync(string email)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, GetDisplayNameFromEmail(email)),
            new Claim(ClaimTypes.Email, email)
        };

        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = false,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity), authProperties);
    }

    private string GetDisplayNameFromEmail(string email)
    {
        if (email.StartsWith("admin@"))
            return "Admin User";
        
        // Extract name from email (simple approach for prototype)
        var localPart = email.Split('@')[0];
        return localPart.Replace(".", " ").Replace("_", " ");
    }
}