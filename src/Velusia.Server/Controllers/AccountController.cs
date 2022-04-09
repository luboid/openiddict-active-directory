/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Velusia.Server.Contracts;
using Velusia.Server.Helpers;
using Velusia.Server.ViewModels.Account;

namespace Velusia.Server.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly ISignInManager _signInManager;
    private readonly ILogger<LoginModel> _logger;

    public AccountController(ISignInManager signInManager, ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    [TempData]
    public string ErrorMessage { get; set; }

    [HttpGet]
    public IActionResult Login([FromQuery] string returnUrl = null)
    {
        if (!string.IsNullOrWhiteSpace(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        ViewData["ReturnUrl"] = returnUrl ?? Url.Content("~/");

        return View(new LoginModel());
    }

    [HttpPost]
    public async Task<IActionResult> LoginAsync([FromForm] LoginModel input, [FromQuery] string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl ??= Url.Content("~/");

        if (ModelState.IsValid)
        {
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(input.UserName, input.Password, input.RememberMe);
            if (result.Succeeded)
            {
                _logger.LogInformation(LoggerEventIds.UserLogin, "User logged in.");
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning(LoggerEventIds.UserLockout, "User account locked out.");
                return RedirectToAction("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Invalid login attempt.");
            }
        }

        // If we got this far, something failed, redisplay form
        return View(input);
    }

    [HttpGet]
    public IActionResult Logout([FromQuery] string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl ?? Url.Content("~/");

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> LogoutAsync([FromQuery] string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl ??= Url.Content("~/");

        await _signInManager.SignOutAsync();
        _logger.LogInformation(LoggerEventIds.UserLoggedOut, "User logged out.");
        if (returnUrl != null)
        {
            return LocalRedirect(returnUrl);
        }
        else
        {
            // This needs to be a redirect so that the browser performs a new
            // request and the identity for the user gets updated.
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public IActionResult Lockout()
    {
        return View();
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}