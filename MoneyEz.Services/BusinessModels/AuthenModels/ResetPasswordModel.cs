using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.AuthenModels
{
    public class ResetPasswordModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email."), EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [Display(Name = "Email")]

        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "Mật khẩu phải có từ 4 đến 20 kí tự.")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mã OTP.")]
        public string OtpCode { get; set; } = "";
    }
}
