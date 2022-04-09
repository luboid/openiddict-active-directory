using System.Security.Claims;
using System.Threading.Tasks;

namespace Velusia.Server.Contracts
{
    public interface IUserManager
    {
        Task<User> GetUserAsync(string userName, string password);

        Task<User> GetUserAsync(ClaimsPrincipal principal);

        Task<string[]> GetRolesAsync(User user);
    }
}
