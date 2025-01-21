using MoneyEz.Services.BusinessModels.OtpModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IOtpService
    {
        public Task<OtpModel> CreateOtpAsync(string email, string type, string fullName);

        public Task<bool> ValidateOtpAsync(string email, string otpCode);
    }
}
