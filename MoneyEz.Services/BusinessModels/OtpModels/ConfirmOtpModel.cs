using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.OtpModels
{
    public class ConfirmOtpModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email."), EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [Display(Name = "Email")]

        public string Email { get; set; } = "";

        [StringLength(6)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "OTP phải là số.")]
        public string OtpCode { get; set; } = "";
    }
}
