using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Services.BusinessModels.AuthenModels;
using MoneyEz.Services.BusinessModels.OtpModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.UserModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IUserService
    {
        // authen

        public Task<BaseResultModel> RegisterAsync(SignUpModel model);

        public Task<BaseResultModel> LoginWithEmailPassword(string email, string password);

        public Task<BaseResultModel> RefreshToken(string jwtToken);

        public Task<BaseResultModel> ChangePasswordAsync(string email, ChangePasswordModel changePasswordModel);

        public Task<BaseResultModel> VerifyEmail(ConfirmOtpModel confirmOtpModel);

        public Task<BaseResultModel> RequestResetPassword(string email);

        public Task<BaseResultModel> ConfirmResetPassword(ConfirmOtpModel confirmOtpModel);

        public Task<BaseResultModel> ExecuteResetPassword(ResetPasswordModel resetPasswordModel);

        public Task<BaseResultModel> ResendOtpConfirmAsync(string email);

        //public Task<UserModel> GetLoginUserInformationAsync(string email);

        public Task<BaseResultModel> LoginWithGoogleFireBase(string credental);

        public Task<BaseResultModel> LoginWithGoogleOAuth(string credental);

        //public Task<bool> CancelEmailConfrimAsync(string email);

        // manager user

        public Task<BaseResultModel> GetUserByIdAsync(Guid id);

        public Task<BaseResultModel> GetUserPaginationAsync(PaginationParameter paginationParameter, UserFilter userFilter);

        public Task<BaseResultModel> CreateUserAsync(CreateUserModel model);

        public Task<BaseResultModel> UpdateUserAsync(UpdateUserModel model);

        public Task<BaseResultModel> DeleteUserAsync(Guid id, string currentEmail);

        public Task<BaseResultModel> BanUserAsync(Guid id, string currentEmail);

        public Task<BaseResultModel> UpdateFcmTokenAsync(string email, string fcmToken);

        public Task<BaseResultModel> GetCurrentUser(string email);
    }
}
