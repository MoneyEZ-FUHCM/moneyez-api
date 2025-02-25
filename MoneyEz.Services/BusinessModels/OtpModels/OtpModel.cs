using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.OtpModels
{
    public class OtpModel
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = "";

        public string OtpCode { get; set; } = "";

        public DateTime ExpiryTime { get; set; }

        public bool IsValidate { get; set; } = false;
    }
}
