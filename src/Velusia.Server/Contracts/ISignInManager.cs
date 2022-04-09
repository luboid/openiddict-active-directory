using System.Security.Claims;
using System.Threading.Tasks;

namespace Velusia.Server.Contracts
{
    public interface ISignInManager
    {
        string AuthenticationScheme { get; }

        Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent);

        Task<ClaimsPrincipal> CreateUserPrincipalAsync(User user);

        Task<bool> CanSignInAsync(User user);

        Task SignOutAsync();

        Task<Claim[]> GetRolesAsync(User user);
    }
}