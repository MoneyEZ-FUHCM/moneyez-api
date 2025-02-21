using AutoMapper;
using FirebaseAdmin.Auth;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.BusinessModels.AuthenModels;
using MoneyEz.Services.BusinessModels.EmailModels;
using MoneyEz.Services.BusinessModels.OtpModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.UserModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.Utils;
using MoneyEz.Services.Utils.Email;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOtpService _otpService;
        private readonly IMailService _mailService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork,
            IOtpService otpService,
            IConfiguration configuration,
            IMailService mailService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _otpService = otpService;
            _mailService = mailService;
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<BaseResultModel> VerifyEmail(ConfirmOtpModel confirmOtpModel)
        {
            bool checkOtp = await _otpService.ValidateOtpAsync(confirmOtpModel.Email, confirmOtpModel.OtpCode);

            if (checkOtp)
            {
                // return accesstoken

                var existUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(confirmOtpModel.Email);

                if (existUser == null)
                {
                    throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
                }

                // check account was verified
                if (existUser.IsVerified == true)
                {
                    throw new DefaultException("", MessageConstants.ACCOUNT_VERIFIED);
                }

                // update verify email for user
                existUser.IsVerified = true;
                _unitOfWork.UsersRepository.UpdateAsync(existUser);

                var accessToken = AuthenTokenUtils.GenerateAccessToken(existUser.Email, existUser, _configuration);
                var refreshToken = AuthenTokenUtils.GenerateRefreshToken(existUser.Email, _configuration);

                _unitOfWork.Save();

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = new AuthenModel
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken
                    },
                    Message = MessageConstants.LOGIN_SUCCESS_MESSAGE
                };
            }

            return new BaseResultModel
            {
                Status = StatusCodes.Status401Unauthorized,
                ErrorCode = MessageConstants.OTP_INVALID
            };
        }

        public async Task<BaseResultModel> CreateUserAsync(CreateUserModel model)
        {
            User newUser = _mapper.Map<User>(model);
            newUser.Status = CommonsStatus.ACTIVE;
            newUser.NameUnsign = StringUtils.ConvertToUnSign(model.FullName);
            newUser.Role = model.Role;
            newUser.Dob = model.Dob;
            newUser.IsVerified = false;

            // check age
            var userAge = CalculateAge(model.Dob);
            if (userAge < 16)
            {
                throw new DefaultException("", MessageConstants.ACCOUNT_NOT_ENOUGH_AGE);
            }

            var existUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(model.Email);

            if (existUser != null)
            {
                throw new DefaultException("", MessageConstants.ACCOUNT_VERIFIED);
            }

            if (CheckExistPhone(model.PhoneNumber).Result)
            {
                throw new DefaultException("", MessageConstants.DUPLICATE_PHONE_NUMBER);
            }

            // generate password
            string password = PasswordUtils.GeneratePassword();

            // hash password
            newUser.Password = PasswordUtils.HashPassword(password);

            await _unitOfWork.UsersRepository.AddAsync(newUser);
            _unitOfWork.Save();

            // send email password
            MailRequest passwordEmail = new MailRequest()
            {
                ToEmail = model.Email,
                Subject = "MoneyEz Welcome",
                Body = EmailCreateAccount.EmailSendCreateAccount(model.Email, password, model.FullName)
            };

            await _mailService.SendEmailAsync(passwordEmail);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = _mapper.Map<UserModel>(newUser),
                Message = MessageConstants.ACCOUNT_CREATED_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> DeleteUserAsync(Guid id, string currentEmail)
        {
            var existUser = await _unitOfWork.UsersRepository.GetByIdAsync(id);
            if (existUser != null)
            {
                // check current user
                if (existUser.Email == currentEmail)
                {
                    throw new DefaultException("Account is current user, can not delete", MessageConstants.ACCOUNT_CURRENT_USER);
                }

                // check confirm email
                if (existUser.IsVerified == true)
                {
                    _unitOfWork.UsersRepository.SoftDeleteAsync(existUser);
                    _unitOfWork.Save();

                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status200OK,
                        Data = _mapper.Map<UserModel>(existUser),
                        Message = MessageConstants.ACCOUNT_DELETE_SUCCESS_MESSAGE,
                    };
                }
                else
                {
                    _unitOfWork.UsersRepository.PermanentDeletedAsync(existUser);
                    _unitOfWork.Save();

                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status200OK,
                        Data = _mapper.Map<UserModel>(existUser),
                        Message = MessageConstants.ACCOUNT_DELETE_SUCCESS_MESSAGE,
                    };
                }
            }
            else
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }
        }

        public async Task<BaseResultModel> GetUserByIdAsync(Guid id)
        {
            var user = await _unitOfWork.UsersRepository.GetByIdAsync(id);
            if (user != null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = _mapper.Map<UserModel>(user)
                };
            }
            else
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }
        }

        public async Task<BaseResultModel> GetUserPaginationAsync(PaginationParameter paginationParameter)
        {
            var userList = await _unitOfWork.UsersRepository.ToPagination(paginationParameter);
            var userModels = _mapper.Map<List<UserModel>>(userList);

            //var users = new Pagination<UserModel>(userModels,
            //    userList.TotalCount,
            //    userList.CurrentPage,
            //    userList.PageSize);

            //var metaData = new
            //{
            //    userList.TotalCount,
            //    userList.PageSize,
            //    userList.CurrentPage,
            //    userList.TotalPages,
            //    userList.HasNext,
            //    userList.HasPrevious
            //};

            var paginatedResult = PaginationHelper.GetPaginationResult(userList, userModels);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = paginatedResult
            };
        }

        // authen

        public async Task<BaseResultModel> LoginWithEmailPassword(string email, string password)
        {
            var existUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(email);
            if (existUser == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status401Unauthorized,
                    ErrorCode = MessageConstants.ACCOUNT_NOT_EXIST
                };
            }

            var verifyUser = PasswordUtils.VerifyPassword(password, existUser.Password);

            if (verifyUser)
            {
                // check status user
                if (existUser.Status == CommonsStatus.BLOCKED || existUser.IsDeleted == true)
                {
                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        ErrorCode = MessageConstants.ACCOUNT_BLOCKED
                    };
                }

                if (existUser.IsVerified == false)
                {
                    // send otp email
                    await _otpService.CreateOtpAsync(existUser.Email, "confirm", existUser.FullName);

                    _unitOfWork.Save();

                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        ErrorCode = MessageConstants.ACCOUNT_NEED_CONFIRM_EMAIL
                    };
                }

                var accessToken = AuthenTokenUtils.GenerateAccessToken(email, existUser, _configuration);
                var refreshToken = AuthenTokenUtils.GenerateRefreshToken(email, _configuration);

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = new AuthenModel
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken
                    },
                    Message = MessageConstants.LOGIN_SUCCESS_MESSAGE
                };
            }
            return new BaseResultModel
            {
                Status = StatusCodes.Status401Unauthorized,
                ErrorCode = MessageConstants.WRONG_PASSWORD
            };
        }

        public async Task<BaseResultModel> RefreshToken(string jwtToken)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = authSigningKey,
                ValidateIssuer = true,
                ValidIssuer = _configuration["JWT:ValidIssuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["JWT:ValidAudience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            try
            {
                SecurityToken validatedToken;
                var principal = handler.ValidateToken(jwtToken, validationParameters, out validatedToken);
                var email = principal.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").Value;
                if (email != null)
                {
                    var existUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(email);
                    if (existUser != null)
                    {
                        var accessToken = AuthenTokenUtils.GenerateAccessToken(email, existUser, _configuration);
                        var refreshToken = AuthenTokenUtils.GenerateRefreshToken(email, _configuration);
                        return new BaseResultModel
                        {
                            Status = StatusCodes.Status200OK,
                            Data = new AuthenModel
                            {
                                AccessToken = accessToken,
                                RefreshToken = refreshToken
                            },
                            Message = MessageConstants.TOKEN_REFRESH_SUCCESS_MESSAGE
                        };
                    }
                }
                return new BaseResultModel
                {
                    Status = StatusCodes.Status401Unauthorized,
                    ErrorCode = MessageConstants.ACCOUNT_NOT_EXIST
                };
            }
            catch
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status401Unauthorized,
                    ErrorCode = MessageConstants.TOKEN_NOT_VALID
                };
            }
        }

        public async Task<BaseResultModel> RegisterAsync(SignUpModel model)
        {
            User newUser = new User
            {
                Email = model.Email,
                FullName = model.FullName,
                NameUnsign = StringUtils.ConvertToUnSign(model.FullName),
                PhoneNumber = model.PhoneNumber,
                Role = RolesEnum.USER,
                Status = CommonsStatus.ACTIVE,
                IsVerified = false
            };

            var existUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(model.Email);

            if (existUser != null)
            {
                throw new DefaultException("", MessageConstants.ACCOUNT_EXISTED);
            }

            if (CheckExistPhone(model.PhoneNumber).Result)
            {
                throw new DefaultException("", MessageConstants.DUPLICATE_PHONE_NUMBER);
            }

            // hash password
            newUser.Password = PasswordUtils.HashPassword(model.Password);

            await _unitOfWork.UsersRepository.AddAsync(newUser);

            // send otp email
            await _otpService.CreateOtpAsync(newUser.Email, "confirm", newUser.FullName);

            _unitOfWork.Save();
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.REGISTER_SUCCESS_MESSAGE,
            };
        }

        public async Task<BaseResultModel> ChangePasswordAsync(string email, ChangePasswordModel changePasswordModel)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(email);
            if (user != null)
            {
                bool checkPassword = PasswordUtils.VerifyPassword(changePasswordModel.OldPassword, user.Password);
                if (checkPassword)
                {
                    user.Password = PasswordUtils.HashPassword(changePasswordModel.NewPassword);
                    _unitOfWork.UsersRepository.UpdateAsync(user);
                    _unitOfWork.Save();
                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status200OK,
                        Message = MessageConstants.CHANGE_PASSWORD_SUCCESS,
                    };
                }
                else
                {
                    throw new DefaultException("", MessageConstants.OLD_PASSWORD_INVALID);
                }
            }
            else
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }
        }

        public async Task<BaseResultModel> RequestResetPassword(string email)
        {
            var existUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(email);

            if (existUser != null)
            {
                if (existUser.IsVerified == true)
                {
                    await _otpService.CreateOtpAsync(email, "reset", existUser.FullName);
                    _unitOfWork.Save();
                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status200OK,
                        Message = MessageConstants.CHANGE_PASSWORD_SUCCESS,
                    };
                }
                else
                {
                    throw new DefaultException("", MessageConstants.RESET_PASSWORD_FAILED);
                }
            }
            else
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }
        }

        public async Task<BaseResultModel> ConfirmResetPassword(ConfirmOtpModel confirmOtpModel)
        {
            var result = await _otpService.ValidateOtpAsync(confirmOtpModel.Email, confirmOtpModel.OtpCode);

            if (result)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = MessageConstants.REQUEST_RESET_PASSWORD_CONFIRM_SUCCESS_MESSAGE,
                };
            }

            throw new DefaultException("", MessageConstants.OTP_INVALID);
        }

        public async Task<BaseResultModel> ExecuteResetPassword(ResetPasswordModel resetPasswordModel)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(resetPasswordModel.Email);
            if (user != null)
            {
                // check request otp
                var key = "Otp_" + resetPasswordModel.Email;
                var otpExist = await _otpService.CheckEmailRequestOtp(resetPasswordModel.Email);
                if (otpExist == null)
                {
                    throw new DefaultException("Email not request otp code", MessageConstants.EMAIL_NOT_REQUEST_OTP);
                }

                // validate otp
                if (otpExist.OtpCode != resetPasswordModel.OtpCode)
                {
                    throw new DefaultException("", MessageConstants.OTP_INVALID);
                }

                await _otpService.RemoveOtpAsync(resetPasswordModel.Email);

                // change password
                user.Password = PasswordUtils.HashPassword(resetPasswordModel.Password);
                _unitOfWork.UsersRepository.UpdateAsync(user);
                _unitOfWork.Save();
                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = MessageConstants.CHANGE_PASSWORD_SUCCESS,
                };
            }
            else
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }
        }

        public async Task<BaseResultModel> UpdateUserAsync(UpdateUserModel model)
        {
            // check age
            var userAge = CalculateAge(model.Dob);
            if (userAge < 16)
            {
                throw new DefaultException(MessageConstants.ACCOUNT_NOT_ENOUGH_AGE);
            }

            // check duplicate phone number
            if (CheckExistPhone(model.PhoneNumber).Result)
            {
                throw new DefaultException("", MessageConstants.DUPLICATE_PHONE_NUMBER);
            }

            var existUser = await _unitOfWork.UsersRepository.GetByIdAsync(model.Id);

            if (existUser == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // check duplicate phone number
            if (model.PhoneNumber != existUser.PhoneNumber && CheckExistPhone(model.PhoneNumber).Result)
            {
                throw new DefaultException("", MessageConstants.DUPLICATE_PHONE_NUMBER);
            }

            existUser.FullName = model.FullName;
            existUser.NameUnsign = StringUtils.ConvertToUnSign(model.FullName);
            existUser.PhoneNumber = model.PhoneNumber;
            existUser.Address = model.Address;
            existUser.Dob = model.Dob;
            existUser.Gender = model.Gender;
            if (model.Avatar != null)
            {
                existUser.AvatarUrl = model.Avatar;
            }

            _unitOfWork.UsersRepository.UpdateAsync(existUser);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.ACCOUNT_UPDATE_SUCCESS_MESSAGE,
                Data = _mapper.Map<UserModel>(existUser)
            };
        }

        public async Task<BaseResultModel> BanUserAsync(Guid id, string currentEmail)
        {
            var existUser = await _unitOfWork.UsersRepository.GetByIdAsync(id);
            if (existUser != null)
            {
                // check current user
                if (existUser.Email == currentEmail)
                {
                    throw new DefaultException("Account is current user, can not ban", MessageConstants.ACCOUNT_CURRENT_USER);
                }

                // change status
                existUser.Status = CommonsStatus.BLOCKED;

                _unitOfWork.UsersRepository.UpdateAsync(existUser);
                _unitOfWork.Save();

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = _mapper.Map<UserModel>(existUser),
                    Message = MessageConstants.ACCOUNT_BAN_SUCCESS_MESSAGE,
                };
            }
            else
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }
        }
        private static int CalculateAge(DateTime birthDate)
        {
            DateTime today = CommonUtils.GetCurrentTime();
            int age = today.Year - birthDate.Year;

            if (birthDate > today.AddYears(-age))
            {
                age--;
            }
            return age;
        }

        private async Task<bool> CheckExistPhone(string phoneNumber)
        {
            var users = await _unitOfWork.UsersRepository.GetUserByPhoneAsync(phoneNumber);
            return users != null;
        }

        public async Task<BaseResultModel> LoginWithGoogleFireBase(string credental)
        {
            FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(credental);

            string uid = decodedToken.Uid;

            UserRecord userGoogle = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);

            if (userGoogle == null)
            {
                throw new DefaultException("", MessageConstants.TOKEN_NOT_VALID);
            }

            var existUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userGoogle.Email);

            if (existUser != null)
            {

                if (existUser.Status == CommonsStatus.BLOCKED || existUser.IsDeleted == true)
                {
                    throw new DefaultException("", MessageConstants.ACCOUNT_BLOCKED);
                }
                else
                {
                    var accessToken = AuthenTokenUtils.GenerateAccessToken(existUser.Email, existUser, _configuration);
                    var refreshToken = AuthenTokenUtils.GenerateRefreshToken(existUser.Email, _configuration);

                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status200OK,
                        Data = new AuthenModel
                        {
                            AccessToken = accessToken,
                            RefreshToken = refreshToken
                        },
                        Message = MessageConstants.LOGIN_SUCCESS_MESSAGE
                    };
                }
            }
            else
            {
                var newUser = new User
                {
                    Email = userGoogle.Email,
                    IsVerified = userGoogle.EmailVerified,
                    FullName = userGoogle.DisplayName,
                    NameUnsign = StringUtils.ConvertToUnSign(userGoogle.DisplayName),
                    AvatarUrl = userGoogle.PhotoUrl,
                    Status = CommonsStatus.ACTIVE,
                    GoogleId = userGoogle.Uid,
                    Role = RolesEnum.USER,
                };

                await _unitOfWork.UsersRepository.AddAsync(newUser);
                _unitOfWork.Save();

                // create accesstoken
                var accessToken = AuthenTokenUtils.GenerateAccessToken(newUser.Email, newUser, _configuration);
                var refreshToken = AuthenTokenUtils.GenerateRefreshToken(newUser.Email, _configuration);

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = new AuthenModel
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken
                    },
                    Message = MessageConstants.LOGIN_GOOGLE_SUCCESS_MESSAGE
                };

            }
        }

        public async Task<BaseResultModel> UpdateFcmTokenAsync(string email, string fcmToken)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(email);
            if (user != null && !fcmToken.IsNullOrEmpty())
            {
                if (user.DeviceToken != fcmToken)
                {
                    user.DeviceToken = fcmToken;

                    _unitOfWork.UsersRepository.UpdateAsync(user);
                    _unitOfWork.Save();

                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status200OK,
                        Message = MessageConstants.ACCOUNT_UPDATE_TOKEN_SUCCESS_MESSAGE
                    };
                }
            }
            throw new DefaultException("", MessageConstants.ACCOUNT_UPDATE_TOKEN_FAILED);
        }

        public async Task<BaseResultModel> LoginWithGoogleOAuth(string credental)
        {
            string cliendId = _configuration["GoogleCredential:ClientId"];

            if (string.IsNullOrEmpty(cliendId))
            {
                throw new DefaultException("", MessageConstants.TOKEN_NOT_VALID);
            }

            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string> { cliendId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(credental, settings);
            if (payload == null)
            {
                throw new DefaultException("", MessageConstants.TOKEN_NOT_VALID);
            }

            var existUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(payload.Email);

            if (existUser != null)
            {

                if (existUser.Status == CommonsStatus.BLOCKED || existUser.IsDeleted == true)
                {
                    throw new DefaultException("", MessageConstants.ACCOUNT_BLOCKED);
                }
                else
                {
                    var accessToken = AuthenTokenUtils.GenerateAccessToken(existUser.Email, existUser, _configuration);
                    var refreshToken = AuthenTokenUtils.GenerateRefreshToken(existUser.Email, _configuration);

                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status200OK,
                        Data = new AuthenModel
                        {
                            AccessToken = accessToken,
                            RefreshToken = refreshToken
                        },
                        Message = MessageConstants.LOGIN_SUCCESS_MESSAGE
                    };
                }
            }
            else
            {
                var newUser = new User
                {
                    Email = payload.Email,
                    IsVerified = payload.EmailVerified,
                    FullName = payload.Name,
                    NameUnsign = StringUtils.ConvertToUnSign(payload.Name),
                    AvatarUrl = payload.Picture,
                    Status = CommonsStatus.ACTIVE,
                    GoogleId = payload.JwtId,
                    Role = RolesEnum.USER,
                };

                await _unitOfWork.UsersRepository.AddAsync(newUser);
                _unitOfWork.Save();

                // create accesstoken
                var accessToken = AuthenTokenUtils.GenerateAccessToken(newUser.Email, newUser, _configuration);
                var refreshToken = AuthenTokenUtils.GenerateRefreshToken(newUser.Email, _configuration);

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = new AuthenModel
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken
                    },
                    Message = MessageConstants.LOGIN_GOOGLE_SUCCESS_MESSAGE
                };

            }
        }

        public async Task<BaseResultModel> GetCurrentUser(string email)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(email);
            if (user != null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = _mapper.Map<UserModel>(user)
                };
            }
            else
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }
        }
    }
}

