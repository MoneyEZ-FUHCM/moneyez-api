using MoneyEz.Repositories.Commons;
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

        //public Task<UserModel> GetLoginUserInformationAsync(string email);

        //public Task<AuthenModel> LoginWithGoogle(string credental);

        //public Task<bool> CancelEmailConfrimAsync(string email);

        // manager user

        public Task<BaseResultModel> GetUserByIdAsync(Guid id);

        public Task<BaseResultModel> GetUserPaginationAsync(PaginationParameter paginationParameter);

        //public Task<BaseResultModel> CreateUserAsync(CreateUserModel model);

        //public Task<UserModel> UpdateUserAsync(UpdateUserModel model);

        public Task<UserModel> DeleteUserAsync(int id, string currentEmail);
    }
}
