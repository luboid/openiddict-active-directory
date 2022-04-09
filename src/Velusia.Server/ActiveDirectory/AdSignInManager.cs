using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Velusia.Server.Contracts;
using static OpenIddict.Abstractions.OpenIddictConstants;

#pragma warning disable CA1416 // Validate platform compatibility
namespace Velusia.Server.ActiveDirectory
{
    public class AdSignInManager : ISignInManager
    {
        private readonly IOptionsMonitor<AdSettings> _settings;
        private readonly IOptionsMonitor<AuthenticationSettings> _authenticationSettings;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserManager _userManager;
        private readonly ILogger _logger;

        public AdSignInManager(
            IOptionsMonitor<AdSettings> settings,
            IOptionsMonitor<AuthenticationSettings> authenticationSettings,
            IHttpContextAccessor contextAccessor,
            IAuthenticationService authenticationService,
            IUserManager userManager,
            ILogger<AdSignInManager> logger)
        {
            _settings = settings;
            _authenticationSettings = authenticationSettings;
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string AuthenticationScheme
        {
            get
            {
                return _authenticationSettings.CurrentValue.Scheme;
            }
        }

        public async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent)
        {
            try
            {
                var settings = _settings.CurrentValue;
                var user = await _userManager.GetUserAsync(userName, password);
                if (user == null)
                {
                    return new SignInResult("Invalid user name or password.");
                }

                if (settings.AllowGroups?.Length > 0 && !settings.AllowGroups[0].Equals("*", StringComparison.OrdinalIgnoreCase) && !user.Groups.Any(groupName => settings.AllowGroups.Contains(groupName, StringComparer.InvariantCultureIgnoreCase)))
                {
                    return new SignInResult("User dosen't have permisions to login.");
                }

                if (!user.Enabled)
                {
                    return new SignInResult("User is disabled.");
                }

                if (user.LockedOut)
                {
                    return SignInResult.LockedOut;
                }

                var principal = await CreateUserPrincipalAsync(user);

                var properties = new AuthenticationProperties
                {
                    IsPersistent = isPersistent
                };

                await _authenticationService.SignInAsync(_contextAccessor.HttpContext, AuthenticationScheme, principal, properties);

                return SignInResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return SignInResult.Failed;
            }
        }

        public Task<ClaimsPrincipal> CreateUserPrincipalAsync(User user)
        {
            var identity = new ClaimsIdentity(AuthenticationScheme, Claims.Name, Claims.Role);
            identity.AddClaim(new Claim(Claims.AuthenticationMethodReference, AuthenticationScheme));
            identity.AddClaim(new Claim(Claims.Subject, user.Id));
            identity.AddClaim(new Claim(Claims.Name, user.DisplayName));
            identity.AddClaim(new Claim(Claims.Nickname, user.Name));

            // if (UserManager.SupportsUserSecurityStamp) Claims.UpdatedAt
            // {
            // 	ClaimsIdentity claimsIdentity = id;
            // 	string securityStampClaimType = Options.ClaimsIdentity.SecurityStampClaimType;
            // 	claimsIdentity.AddClaim(new Claim(securityStampClaimType, await UserManager.GetSecurityStampAsync(user)));
            // }

            return Task.FromResult(new ClaimsPrincipal(identity));
        }

        public Task<bool> CanSignInAsync(User user)
        {
            if (!user.Enabled || user.LockedOut)
            {
                _logger.LogWarning("User {userId} cannot sign, it is disabled.", user.Name);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public Task SignOutAsync()
        {
            return _authenticationService.SignOutAsync(
                _contextAccessor.HttpContext,
                AuthenticationScheme,
                null);
        }

        public async Task<Claim[]> GetRolesAsync(User user)
        {
            return (await _userManager.GetRolesAsync(user))
                .Select(role => new Claim(Claims.Role, role))
                .ToArray();
        }
    }
}
#pragma warning restore CA1416 // Validate platform compatibility
