using System.ComponentModel.DataAnnotations;

namespace MoneyEz.API.ViewModels.RequestModels
{
    public class LoginModel
    {
        [Required]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";
    }
}
