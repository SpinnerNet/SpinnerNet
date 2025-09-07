using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SpinnerNet.Web.Areas.Account.Pages;

public class LoginModel : PageModel
{
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(ILogger<LoginModel> logger)
    {
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public void OnGet()
    {
        // Clear any existing error message
        ErrorMessage = null;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            // Check if demo login was requested
            if (Request.Form.ContainsKey("demoLogin"))
            {
                await SignInDemoUserAsync();
                return Redirect("/app");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // For prototype: Simple email/password validation
            // In production, this would integrate with a proper user store
            if (ValidateUser(Input.Email, Input.Password))
            {
                await SignInUserAsync(Input.Email, Input.RememberMe);
                _logger.LogInformation("User {Email} signed in successfully", Input.Email);
                return Redirect("/app");
            }
            else
            {
                ErrorMessage = "Invalid email or password.";
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login attempt");
            ErrorMessage = "An error occurred during login. Please try again.";
            return Page();
        }
    }

    private bool ValidateUser(string email, string password)
    {
        // Prototype validation - in production, use proper authentication
        var validCredentials = new Dictionary<string, string>
        {
            { "demo@example.com", "demo123" },
            { "admin@example.com", "admin123" },
            { "test@example.com", "test123" }
        };

        return validCredentials.ContainsKey(email.ToLowerInvariant()) && 
               validCredentials[email.ToLowerInvariant()] == password;
    }

    private async Task SignInUserAsync(string email, bool rememberMe)
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
            IsPersistent = rememberMe,
            ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity), authProperties);
    }

    private async Task SignInDemoUserAsync()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "demo-user-123"),
            new Claim(ClaimTypes.Name, "Demo User"),
            new Claim(ClaimTypes.Email, "demo@spinnernet.dev")
        };

        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = false,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity), authProperties);
        _logger.LogInformation("Demo user signed in");
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