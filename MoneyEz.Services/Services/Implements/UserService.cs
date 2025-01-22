using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Services.BusinessModels.AuthenModels;
using MoneyEz.Services.BusinessModels.OtpModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.UserModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.Utils;
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
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork,
            IOtpService otpService,
            IConfiguration configuration,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _otpService = otpService;
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<BaseResultModel> VerifyEmail(ConfirmOtpModel confirmOtpModel)
        {
            try
            {
                bool checkOtp = await _otpService.ValidateOtpAsync(confirmOtpModel.Email, confirmOtpModel.OtpCode);

                if (checkOtp)
                {
                    // return accesstoken

                    var existUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(confirmOtpModel.Email);

                    if (existUser == null)
                    {
                        return new BaseResultModel
                        {
                            Status = StatusCodes.Status401Unauthorized,
                            ErrorCode = MessageConstants.ACCOUNT_NOT_EXIST
                        };
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
            catch
            {
                throw;
            }
        }

        public Task<UserModel> CreateUserAsync(CreateUserModel model)
        {
            throw new NotImplementedException();
        }

        public Task<UserModel> DeleteUserAsync(int id, string currentEmail)
        {
            throw new NotImplementedException();
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
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.ACCOUNT_NOT_EXIST
                };
            }
        }

        public async Task<BaseResultModel> GetUserPaginationAsync(PaginationParameter paginationParameter)
        {
            var userList = await _unitOfWork.UsersRepository.ToPagination(paginationParameter);
            var userModels = _mapper.Map<List<UserModel>>(userList);

            var users = new Pagination<UserModel>(userModels,
                userList.TotalCount,
                userList.CurrentPage,
                userList.PageSize);

            var metaData = new
            {
                userList.TotalCount,
                userList.PageSize,
                userList.CurrentPage,
                userList.TotalPages,
                userList.HasNext,
                userList.HasPrevious
            };

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = new ModelPaging
                {
                    Data = users,
                    MetaData = metaData
                }
            };
        }

        // authen

        public async Task<BaseResultModel> LoginWithEmailPassword(string email, string password)
        {
            try
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
                            Message = MessageConstants.ACCOUNT_NEED_CONFIRM_EMAIL
                        };
                    }

                    var accessToken = AuthenTokenUtils.GenerateAccessToken(email, existUser, _configuration);
                    var refreshToken = AuthenTokenUtils.GenerateRefreshToken(email, _configuration);

                    //_unitOfWork.Save();

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
            catch
            {
                throw;
            }
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
            try
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
                    throw new DefaultException(MessageConstants.ACCOUNT_EXISTED);
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
            catch
            {
                throw;
            }
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
                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status400BadRequest,
                        ErrorCode = MessageConstants.OLD_PASSWORD_INVALID
                    };
                }
            }
            else
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.ACCOUNT_NOT_EXIST
                };
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
                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status400BadRequest,
                        ErrorCode = MessageConstants.RESET_PASSWORD_FAILED
                    };
                }
            }
            else
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.ACCOUNT_NOT_EXIST
                };
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
                    Message = MessageConstants.REQUEST_RESET_PASSWORD_CONFIRM_SUCCESS,
                };
            }

            return new BaseResultModel
            {
                Status = StatusCodes.Status400BadRequest,
                ErrorCode = MessageConstants.OTP_INVALID
            };
        }

        public async Task<BaseResultModel> ExecuteResetPassword(ResetPasswordModel resetPasswordModel)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(resetPasswordModel.Email);
            if (user != null)
            {
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
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.ACCOUNT_NOT_EXIST
                };
            }
        }
    }
}

