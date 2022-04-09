using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;
using System.Threading.Tasks;
using Velusia.Server.Contracts;

namespace Velusia.Server.Controllers;

[Route("api")]
public class ResourceController : Controller
{
    private readonly IUserManager _userManager;

    public ResourceController(IUserManager userManager)
        => _userManager = userManager;

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("message")]
    public async Task<IActionResult> GetMessage()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return BadRequest();
        }

        return Content($"{user.DisplayName} has been successfully authenticated.");
    }
}
