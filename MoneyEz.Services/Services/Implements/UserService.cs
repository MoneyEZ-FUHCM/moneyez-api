using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Services.BusinessModels.AuthenModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.UserModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, 
            IConfiguration configuration, 
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _mapper = mapper;
        }
        public Task<UserModel> CreateUserAsync(CreateUserModel model)
        {
            throw new NotImplementedException();
        }

        public Task<UserModel> DeleteUserAsync(int id, string currentEmail)
        {
            throw new NotImplementedException();
        }

        public Task<UserModel> GetUserByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Pagination<UserModel>> GetUserPaginationAsync(PaginationParameter paginationParameter)
        {
            throw new NotImplementedException();
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

                var verifyUser = PasswordUtils.VerifyPassword(password, existUser.PasswordHash);

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

                    //if (existUser.IsEmailConfirmed == false)
                    //{
                    //    // send otp email
                    //    await _otpService.CreateOtpAsync(existUser.Email, "confirm", existUser.FullName);

                    //    _unitOfWork.Save();

                    //    return new BaseResultModel<AuthenModel>
                    //    {
                    //        Status = StatusCodes.Status401Unauthorized,
                    //        Message = MessageConstants.ACCOUNT_NEED_CONFIRM_EMAIL
                    //    };
                    //}

                    var accessToken = AuthenTokenUtils.GenerateAccessToken(email, existUser, _configuration);
                    var refreshToken = AuthenTokenUtils.GenerateRefreshToken(email, _configuration);

                    //_unitOfWork.Save();

                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status200OK,
                        Message = MessageConstants.LOGIN_SUCCESS,
                        Data = new AuthenModel
                        {
                            AccessToken = accessToken,
                            RefreshToken = refreshToken
                        }
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

        public Task<AuthenModel> RefreshToken(string jwtToken)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResultModel> RegisterAsync(SignUpModel model)
        {
            try
            {
                User newUser = new User
                {
                    Email = model.Email,
                    FullName = model.FullName,
                    UnsignFullName = StringUtils.ConvertToUnSign(model.FullName),
                    PhoneNumber = model.PhoneNumber,
                    Role = RolesEnum.USER,
                    Status = CommonsStatus.ACTIVE,
                    IsEmailConfirmed = true
                };

                var existUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(model.Email);

                if (existUser != null)
                {
                    throw new DefaultException(MessageConstants.ACCOUNT_EXISTED);
                }

                // hash password
                newUser.PasswordHash = PasswordUtils.HashPassword(model.Password);

                await _unitOfWork.UsersRepository.AddAsync(newUser);

                // send otp email
                //await _otpService.CreateOtpAsync(newUser.Email, "confirm", newUser.FullName);

                _unitOfWork.Save();
                return new BaseResultModel
                {
                    Data = _mapper.Map<UserModel>(newUser),
                    Status = StatusCodes.Status200OK,
                    ErrorCode = MessageConstants.REGISTER_SUCCESS,
                };
            }
            catch
            {
                throw;
            }
        }
    }
}
