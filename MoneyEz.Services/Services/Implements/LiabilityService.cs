using AutoMapper;
using Microsoft.AspNetCore.Http;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.BusinessModels.LiabilityModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Implements
{
    public class LiabilityService : ILiabilityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimsService _claimsService;

        public LiabilityService(IUnitOfWork unitOfWork, IMapper mapper, IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsService = claimsService;
        }

        public async Task<BaseResultModel> GetLiabilityByIdAsync(Guid id)
        {
            var liability = await _unitOfWork.LiabilityRepository.GetByIdAsync(id);
            if (liability != null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = _mapper.Map<LiabilityModel>(liability)
                };
            }
            else
            {
                throw new NotExistException(MessageConstants.LIABILITY_NOT_FOUND);
            }
        }

        public async Task<BaseResultModel> GetAllLiabilitiesPaginationAsync(PaginationParameter paginationParameter)
        {
            var liabilities = await _unitOfWork.LiabilityRepository.ToPagination(paginationParameter);
            var result = _mapper.Map<Pagination<LiabilityModel>>(liabilities);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.LIABILITY_LIST_GET_SUCCESS_MESSAGE,
                Data = new ModelPaging
                {
                    Data = result,
                    MetaData = new
                    {
                        result.TotalCount,
                        result.PageSize,
                        result.CurrentPage,
                        result.TotalPages,
                        result.HasNext,
                        result.HasPrevious
                    }
                }
            };
        }

        public async Task<BaseResultModel> GetLiabilitiesByUserAsync(PaginationParameter paginationParameter)
        {
            // get current user id
            var userId = _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail).Result.Id;

            // check exist user id
            var user = await _unitOfWork.UsersRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);
            }

            var liabilities = await _unitOfWork.LiabilityRepository.GetLiabilitiesByUserIdAsync(userId, paginationParameter);
            var result = _mapper.Map<Pagination<LiabilityModel>>(liabilities);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.LIABILITY_LIST_GET_SUCCESS_MESSAGE,
                Data = new ModelPaging
                {
                    Data = result,
                    MetaData = new
                    {
                        result.TotalCount,
                        result.PageSize,
                        result.CurrentPage,
                        result.TotalPages,
                        result.HasNext,
                        result.HasPrevious
                    }
                }
            };
        }

        public async Task<BaseResultModel> CreateLiabilityAsync(CreateLiabilityModel model)
        {
            // get current user id
            var userId = _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail).Result.Id;

            // check exist user id
            var user = await _unitOfWork.UsersRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // check exist subcategory
            var subcate = await _unitOfWork.SubcategoryRepository.GetByIdAsync(model.SubcategoryId);
            if (subcate == null)
            {
                throw new NotExistException(MessageConstants.SUBCATEGORY_NOT_FOUND);
            }

            var liability = _mapper.Map<Liability>(model);
            liability.NameUnsign = StringUtils.ConvertToUnSign(model.Name);
            liability.UserId = userId;

            await _unitOfWork.LiabilityRepository.AddAsync(liability);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = MessageConstants.LIABILITY_CREATED_SUCCESS,
                Data = _mapper.Map<LiabilityModel>(liability)
            };
        }

        public async Task<BaseResultModel> UpdateLiabilityAsync(UpdateLiabilityModel model)
        {
            // check exist liability id
            if (await _unitOfWork.LiabilityRepository.GetByIdAsync(model.Id) == null) throw new NotExistException(MessageConstants.LIABILITY_NOT_FOUND);

            // check exist subcategory
            if (await _unitOfWork.SubcategoryRepository.GetByIdAsync(model.SubcategoryId) == null) throw new NotExistException(MessageConstants.SUBCATEGORY_NOT_FOUND);

            var liability = _mapper.Map<Liability>(model);
            liability.UpdatedDate = CommonUtils.GetCurrentTime();

            _unitOfWork.LiabilityRepository.UpdateAsync(liability);
            await _unitOfWork.SaveAsync();
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.LIABILITY_UPDATED_SUCCESS,
                Data = _mapper.Map<LiabilityModel>(liability)
            };
        }

        public async Task<BaseResultModel> DeleteLiabilityAsync(Guid id)
        {
            var liability = await _unitOfWork.LiabilityRepository.GetByIdAsync(id);
            if (liability == null) throw new NotExistException(MessageConstants.LIABILITY_NOT_FOUND);

            _unitOfWork.LiabilityRepository.SoftDeleteAsync(liability);
            await _unitOfWork.SaveAsync();
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.LIABILITY_DELETED_SUCCESS,
            };
        }
    }
}
