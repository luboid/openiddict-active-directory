using System.ComponentModel.DataAnnotations;

namespace Velusia.Server.ViewModels.Account
{
    public class LoginModel
    {
        [Required]
        public string UserName { get; set; } = default!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
