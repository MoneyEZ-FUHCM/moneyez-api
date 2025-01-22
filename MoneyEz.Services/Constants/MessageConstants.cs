using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Constants
{
    public class MessageConstants
    {
        // syntax error code: <CONSTANT_NAME> = "MessagePascalCaseCode";
        // syntax message: <CONSTANT_NAME_MESSAGE> = "Message here";

        // authen

        public const string ACCOUNT_NEED_CONFIRM_EMAIL = "AccountDoesNotVerifyEmail";
        public const string WRONG_PASSWORD = "PasswordIsIncorrect";
        public const string TOKEN_NOT_VALID = "TokenNotValid";
        public const string OTP_INVALID = "OtpInvalid";
        public const string OLD_PASSWORD_INVALID = "OldPasswordInvalid";
        public const string RESET_PASSWORD_FAILED = "CanNotResetPassword";

        public const string LOGIN_SUCCESS_MESSAGE = "Login successfully";
        public const string TOKEN_REFRESH_SUCCESS_MESSAGE = "Token refresh successfully";
        public const string REGISTER_SUCCESS_MESSAGE = "The OTP has been sent to your email. Please verify it to log in.";
        public const string CHANGE_PASSWORD_SUCCESS = "Change password successfully";
        public const string REQUEST_RESET_PASSWORD_SUCCESS = "The OTP reset password has been sent to your email";
        public const string REQUEST_RESET_PASSWORD_CONFIRM_SUCCESS = "You can reset password now";

        // account
        public const string ACCOUNT_NOT_EXIST = "Account Not Exist";
        public const string ACCOUNT_EXISTED = "Account Already Existed";
        public const string ACCOUNT_BLOCKED = "Account Was Blocked";

        // group
        public const string GROUP_NOT_EXIST = "Group Not Exist";
        public const string GROUP_EXISTED = "Group Already Existed";
        public const string GROUP_MEMBER_NOT_EXIST = "Group Member Not Exist";
        public const string GROUP_CREATE_SUCCESS = "Group created successfully";
        public const string GROUP_UPDATE_SUCCESS = "Group updated successfully";
        public const string GROUP_DELETE_SUCCESS = "Group deleted successfully";
    }
}
