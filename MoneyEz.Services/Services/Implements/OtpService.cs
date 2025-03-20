using MoneyEz.Repositories.Utils;
using MoneyEz.Services.BusinessModels.EmailModels;
using MoneyEz.Services.BusinessModels.OtpModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.Utils;
using MoneyEz.Services.Utils.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace MoneyEz.Services.Services.Implements
{
    public class OtpService : IOtpService
    {
        private readonly IRedisService _redisService;
        private readonly IMailService _mailService;

        public OtpService(IRedisService redisService, IMailService mailService) 
        {
            _redisService = redisService;
            _mailService = mailService;
        }

        public Task<OtpModel> CheckEmailRequestOtp(string email)
        {
            var key = "Otp_" + email;
            return _redisService.GetAsync<OtpModel>(key);
        }

        public async Task<OtpModel> CreateOtpAsync(string email, string type, string fullName)
        {
            // check if sent otp before
            var key = "Otp_" + email;
            var otpExist = await _redisService.GetAsync<OtpModel>(key);
            if (otpExist != null)
            {
                throw new DefaultException("Otp has been sent to your email. Please check your email.", MessageConstants.OTP_HAS_SENT);
            }

            // default ExpiryTime otp is 5 minutes
            OtpModel newOtp = new OtpModel()
            {
                Id = Guid.NewGuid(),
                Email = email,
                OtpCode = NumberUtils.GenerateFiveDigitNumber().ToString(),
                ExpiryTime = CommonUtils.GetCurrentTime().AddMinutes(5)
            };

            await _redisService.SetAsync<OtpModel>(key, newOtp, TimeSpan.FromMinutes(5));

            if (type == "confirm")
            {
                bool checkSendMail = await SendOtpAsync(newOtp, fullName);
                if (checkSendMail)
                {
                    return newOtp;
                }
                return null;
            }
            else
            {
                bool checkSendMail = await SendOtpResetPasswordAsync(newOtp, fullName);
                if (checkSendMail)
                {
                    return newOtp;
                }
                return null;
            }
        }

        public async Task<bool> RemoveOtpAsync(string email)
        {
            var key = "Otp_" + email;
            await _redisService.RemoveAsync(key);
            return true;
        }

        public async Task<bool> ValidateOtpAsync(string email, string otpCode)
        {
            var key = "Otp_" + email;
            var otpExist = await _redisService.GetAsync<OtpModel>(key);
            if (otpExist != null)
            {
                if (otpExist.Email == email && otpExist.ExpiryTime > CommonUtils.GetCurrentTime()
                    && otpExist.IsValidate == false && otpExist.OtpCode == otpCode)
                {
                    return true;
                }
            }
            return false;
        }

        private async Task<bool> SendOtpAsync(OtpModel otp, string fullName)
        {
            // create new email
            MailRequest newEmail = new MailRequest()
            {
                ToEmail = otp.Email,
                Subject = "Xác thực tài khoản MoneyEz",
                Body = SendOTPTemplate.EmailSendOTP(otp.Email, otp.OtpCode, fullName)
            };

            // send mail
            await _mailService.SendEmailAsync_v2(newEmail);
            return true;
        }

        private async Task<bool> SendOtpResetPasswordAsync(OtpModel otp, string fullName)
        {
            // create new email
            MailRequest newEmail = new MailRequest()
            {
                ToEmail = otp.Email,
                Subject = "Đặt lại mật khẩu MoneyEz",
                Body = SendOTPTemplate.EmailSendOTPResetPassword(otp.Email, otp.OtpCode, fullName)
            };

            // send mail
            await _mailService.SendEmailAsync_v2(newEmail);
            return true;
        }
    }
}
