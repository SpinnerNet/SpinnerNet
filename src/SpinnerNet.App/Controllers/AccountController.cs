using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SpinnerNet.App.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(LoginCallback), "Account", new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet("login-callback")]
    public IActionResult LoginCallback(string? returnUrl = null)
    {
        if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
        {
            returnUrl = "/";
        }
        
        return Redirect(returnUrl);
    }

    [HttpGet("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("LogoutCallback", "Account")
        };
        return SignOut(properties, OpenIdConnectDefaults.AuthenticationScheme, "Cookies");
    }

    [HttpGet("logout-callback")]
    public IActionResult LogoutCallback()
    {
        return Redirect("/");
    }
}