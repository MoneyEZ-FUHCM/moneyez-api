using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.AuthenModels
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Old password is required!")]
        public string OldPassword { get; set; } = "";

        [Required(ErrorMessage = "New password is required!")]
        [Display(Name = "New password")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "Mật khẩu phải chứa từ 4 đến 20 kí tự.")]
        [PasswordPropertyText]
        public string NewPassword { get; set; } = "";
    }
}
