//using AutoMapper;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;
//using MoneyEz.Repositories.Commons;
//using MoneyEz.Repositories.Entities;
//using MoneyEz.Repositories.Enums;
//using MoneyEz.Repositories.UnitOfWork;
//using MoneyEz.Services.BusinessModels.AuthenModels;
//using MoneyEz.Services.BusinessModels.ResultModels;
//using MoneyEz.Services.BusinessModels.UserModels;
//using MoneyEz.Services.Constants;
//using MoneyEz.Services.Exceptions;
//using MoneyEz.Services.Services.Interfaces;
//using MoneyEz.Services.Utils;
//using System;
//using System.Collections.Generic;
//using System.IdentityModel.Tokens.Jwt;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;

//namespace MoneyEz.Services.Services.Implements
//{
//    public class UserService : IUserService
//    {
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly IConfiguration _configuration;
//        private readonly IMapper _mapper;

//        public UserService(IUnitOfWork unitOfWork,
//            IConfiguration configuration,
//            IMapper mapper)
//        {
//            _unitOfWork = unitOfWork;
//            _configuration = configuration;
//            _mapper = mapper;
//        }
//        public Task<UserModel> CreateUserAsync(CreateUserModel model)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<UserModel> DeleteUserAsync(int id, string currentEmail)
//        {
//            throw new NotImplementedException();
//        }

//        public async Task<BaseResultModel> GetUserByIdAsync(Guid id)
//        {
//            var user = await _unitOfWork.UsersRepository.GetByIdAsync(id);
//            if (user != null)
//            {
//                return new BaseResultModel
//                {
//                    Status = StatusCodes.Status200OK,
//                    Data = _mapper.Map<UserModel>(user)
//                };
//            }
//            else
//            {
//                return new BaseResultModel
//                {
//                    Status = StatusCodes.Status404NotFound,
//                    ErrorCode = MessageConstants.ACCOUNT_NOT_EXIST
//                };
//            }
//        }

//        public async Task<BaseResultModel> GetUserPaginationAsync(PaginationParameter paginationParameter)
//        {
//            var userList = await _unitOfWork.UsersRepository.ToPagination(paginationParameter);
//            var userModels = _mapper.Map<List<UserModel>>(userList);

//            var users = new Pagination<UserModel>(userModels,
//                userList.TotalCount,
//                userList.CurrentPage,
//                userList.PageSize);

//            var metaData = new
//            {
//                userList.TotalCount,
//                userList.PageSize,
//                userList.CurrentPage,
//                userList.TotalPages,
//                userList.HasNext,
//                userList.HasPrevious
//            };

//            return new BaseResultModel
//            {
//                Status = StatusCodes.Status200OK,
//                Data = new ModelPaging 
//                { 
//                    Data = users,
//                    MetaData = metaData
//                }
//            };
//        }

//        // authen

//        public async Task<BaseResultModel> LoginWithEmailPassword(string email, string password)
//        {
//            try
//            {
//                var existUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(email);
//                if (existUser == null)
//                {
//                    return new BaseResultModel
//                    {
//                        Status = StatusCodes.Status401Unauthorized,
//                        ErrorCode = MessageConstants.ACCOUNT_NOT_EXIST
//                    };
//                }

//                var verifyUser = PasswordUtils.VerifyPassword(password, existUser.PasswordHash);

//                if (verifyUser)
//                {
//                    // check status user
//                    if (existUser.Status == CommonsStatus.BLOCKED || existUser.IsDeleted == true)
//                    {
//                        return new BaseResultModel
//                        {
//                            Status = StatusCodes.Status401Unauthorized,
//                            ErrorCode = MessageConstants.ACCOUNT_BLOCKED
//                        };
//                    }

//                    //if (existUser.IsEmailConfirmed == false)
//                    //{
//                    //    // send otp email
//                    //    await _otpService.CreateOtpAsync(existUser.Email, "confirm", existUser.FullName);

//                    //    _unitOfWork.Save();

//                    //    return new BaseResultModel<AuthenModel>
//                    //    {
//                    //        Status = StatusCodes.Status401Unauthorized,
//                    //        Message = MessageConstants.ACCOUNT_NEED_CONFIRM_EMAIL
//                    //    };
//                    //}

//                    var accessToken = AuthenTokenUtils.GenerateAccessToken(email, existUser, _configuration);
//                    var refreshToken = AuthenTokenUtils.GenerateRefreshToken(email, _configuration);

//                    //_unitOfWork.Save();

//                    return new BaseResultModel
//                    {
//                        Status = StatusCodes.Status200OK,
//                        Data = new AuthenModel
//                        {
//                            AccessToken = accessToken,
//                            RefreshToken = refreshToken
//                        },
//                        Message = MessageConstants.LOGIN_SUCCESS_MESSAGE
//                    };
//                }
//                return new BaseResultModel
//                {
//                    Status = StatusCodes.Status401Unauthorized,
//                    ErrorCode = MessageConstants.WRONG_PASSWORD
//                };
//            }
//            catch
//            {
//                throw;
//            }
//        }

//        public async Task<BaseResultModel> RefreshToken(string jwtToken)
//        {
//            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
//            var handler = new JwtSecurityTokenHandler();
//            var validationParameters = new TokenValidationParameters
//            {
//                ValidateIssuerSigningKey = true,
//                IssuerSigningKey = authSigningKey,
//                ValidateIssuer = true,
//                ValidIssuer = _configuration["JWT:ValidIssuer"],
//                ValidateAudience = true,
//                ValidAudience = _configuration["JWT:ValidAudience"],
//                ValidateLifetime = true,
//                ClockSkew = TimeSpan.Zero
//            };
//            try
//            {
//                SecurityToken validatedToken;
//                var principal = handler.ValidateToken(jwtToken, validationParameters, out validatedToken);
//                var email = principal.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").Value;
//                if (email != null)
//                {
//                    var existUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(email);
//                    if (existUser != null)
//                    {
//                        var accessToken = AuthenTokenUtils.GenerateAccessToken(email, existUser, _configuration);
//                        var refreshToken = AuthenTokenUtils.GenerateRefreshToken(email, _configuration);
//                        return new BaseResultModel
//                        {
//                            Status = StatusCodes.Status200OK,
//                            Data = new AuthenModel
//                            {
//                                AccessToken = accessToken,
//                                RefreshToken = refreshToken
//                            },
//                            Message = MessageConstants.TOKEN_REFRESH_SUCCESS_MESSAGE
//                        };
//                    }
//                }
//                return new BaseResultModel
//                {
//                    Status = StatusCodes.Status401Unauthorized,
//                    ErrorCode = MessageConstants.ACCOUNT_NOT_EXIST
//                };
//            }
//            catch
//            {
//                return new BaseResultModel
//                {
//                    Status = StatusCodes.Status401Unauthorized,
//                    ErrorCode = MessageConstants.TOKEN_NOT_VALID
//                };
//            }
//        }

//        public async Task<BaseResultModel> RegisterAsync(SignUpModel model)
//        {
//            try
//            {
//                User newUser = new User
//                {
//                    Email = model.Email,
//                    FullName = model.FullName,
//                    UnsignFullName = StringUtils.ConvertToUnSign(model.FullName),
//                    PhoneNumber = model.PhoneNumber,
//                    Role = RolesEnum.USER,
//                    Status = CommonsStatus.ACTIVE,
//                    IsEmailConfirmed = true
//                };

//                var existUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(model.Email);

//                if (existUser != null)
//                {
//                    throw new DefaultException(MessageConstants.ACCOUNT_EXISTED);
//                }

//                // hash password
//                newUser.PasswordHash = PasswordUtils.HashPassword(model.Password);

//                await _unitOfWork.UsersRepository.AddAsync(newUser);

//                // send otp email
//                //await _otpService.CreateOtpAsync(newUser.Email, "confirm", newUser.FullName);

//                _unitOfWork.Save();
//                return new BaseResultModel
//                {
//                    Data = _mapper.Map<UserModel>(newUser),
//                    Status = StatusCodes.Status200OK,
//                    Message = MessageConstants.REGISTER_SUCCESS_MESSAGE,
//                };
//            }
//            catch
//            {
//                throw;
//            }
//        }
//    }
//}
