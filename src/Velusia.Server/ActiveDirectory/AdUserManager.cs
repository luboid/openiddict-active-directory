using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Velusia.Server.Contracts;
using static OpenIddict.Abstractions.OpenIddictConstants;

#pragma warning disable CA1416 // Validate platform compatibility
namespace Velusia.Server.ActiveDirectory
{
    // using (var p = Principal.FindByIdentity(c, IdentityType.SamAccountName, user))
    // using (var p = Principal.FindByIdentity(c, IdentityType.SamAccountName, "DOMAIN\\" + user))
    // using (var p = Principal.FindByIdentity(c, IdentityType.UserPrincipalName, user + "@domain.com"))
    public class AdUserManager : IUserManager
    {
        private readonly IOptionsMonitor<AdSettings> _settings;
        private readonly ILogger _logger;

        public AdUserManager(IOptionsMonitor<AdSettings> settings, ILogger<AdUserManager> logger)
        {
            _settings = settings;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<User> GetUserAsync(string userName, string password)
        {
            User adUser = null;
            if (!(string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password)))
            {
                try
                {
                    using var c = new PrincipalContext(_settings.CurrentValue.ContextType, _settings.CurrentValue.Server, null, ContextOptions.Negotiate, userName, password);
                    using var user = Principal.FindByIdentity(c, IdentityType.SamAccountName, userName) as UserPrincipal;
                    adUser = CreateUserFromPrincipal(user);
                }
                catch (DirectoryServicesCOMException ex) when (ex.ExtendedError == -2146893044 || ex.ErrorCode == -2147023570)
                {
                    // The user name or password is incorrect.
                    _logger.LogInformation(ex, ex.Message);
                }
                catch (Exception ex)
                {
                    // PrincipalServerDownException
                    _logger.LogError(ex, ex.Message);
                }
            }

            return Task.FromResult(adUser);
        }

        public Task<User> GetUserAsync(ClaimsPrincipal principal)
        {
            User adUser = null;
            if (principal?.Identity?.IsAuthenticated == true)
            {
                try
                {
                    using var context = new PrincipalContext(_settings.CurrentValue.ContextType, _settings.CurrentValue.Server, null, ContextOptions.Negotiate);
                    using var user = Principal.FindByIdentity(context, IdentityType.UserPrincipalName, principal.FindFirstValue(Claims.Nickname)) as UserPrincipal;
                    adUser = CreateUserFromPrincipal(user);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }

            return Task.FromResult(adUser);
        }

        private static User CreateUserFromPrincipal(UserPrincipal principal)
        {
            if (principal == null)
            {
                return null;
            }

            var entry = principal.GetUnderlyingObject() as DirectoryEntry;

            return new User
            {
                Id = principal.Sid.ToString(),
                Name = principal.UserPrincipalName,
                DisplayName = principal.DisplayName,
                Enabled = principal.Enabled == true,
                LockedOut = principal.IsAccountLockedOut(),
                EmailAddress = principal.EmailAddress,
                PhoneNumber = principal.VoiceTelephoneNumber,
                Changed = Convert.ToDateTime(entry.Properties["whenChanged"].Value),
                PasswordChanged = principal.LastPasswordSet,
                Groups = principal.GetGroupList(),
            };
        }

        public Task<string[]> GetRolesAsync(User user)
        {
            string[] roles = null;
            if (user.Groups?.Length > 0 && _settings.CurrentValue.Roles?.Length > 0)
            {
                roles = _settings.CurrentValue
                    .Roles
                    .Where(r => user.Groups.Contains(r.GroupName, StringComparer.InvariantCultureIgnoreCase))
                    .Select(r => r.Name)
                    .ToArray();
            }

            return Task.FromResult(roles ?? Array.Empty<string>());
        }
    }
}
#pragma warning restore CA1416 // Validate platform compatibility
