using AutoMapper;
using Microsoft.AspNetCore.Http;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.BusinessModels.AssetModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.UserModels;
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
    public class AssetService : IAssetService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimsService _claimsService;


        public AssetService(IUnitOfWork unitOfWork, IMapper mapper, IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsService = claimsService;
        }

        public async Task<BaseResultModel> GetAssetByIdAsync(Guid id)
        {
            var asset = await _unitOfWork.AssetRepository.GetByIdAsync(id);
            if (asset != null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = _mapper.Map<AssetModel>(asset)
                };
            }
            else
            {
                throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);
            }
        }

        public async Task<BaseResultModel> GetAllAssetsPaginationAsync(PaginationParameter paginationParameter)
        {
            var assets = await _unitOfWork.AssetRepository.ToPagination(paginationParameter);
            var result = _mapper.Map<Pagination<AssetModel>>(assets);

            return new BaseResultModel { 
                Status = StatusCodes.Status200OK, 
                Message = MessageConstants.ASSET_LIST_GET_SUCCESS_MESSAGE,
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

        public async Task<BaseResultModel> GetAssetsByUserAsync(Guid userId, PaginationParameter paginationParameter)
        {
            // check exist user id
            var user = await _unitOfWork.UsersRepository.GetByIdAsync(userId);
            if (user == null) {
                throw new NotExistException(MessageConstants.USER_NOT_FOUND_MESSAGE);
            }

            var assets = await _unitOfWork.AssetRepository.GetAssetsByUserIdAsync(userId, paginationParameter);
            var result = _mapper.Map<Pagination<AssetModel>>(assets);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.ASSET_LIST_GET_SUCCESS_MESSAGE,
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

        public async Task<BaseResultModel> CreateAssetAsync(CreateAssetModel model)
        {
            // get current user id
            var userId = _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail).Result.Id;

            // check exist user id
            var user = await _unitOfWork.UsersRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotExistException(MessageConstants.USER_NOT_FOUND_MESSAGE);
            }

            // check exist subcategory
            var subcate = await _unitOfWork.SubcategoryRepository.GetByIdAsync(model.SubcategoryId);
            if (subcate == null)
            {
                throw new NotExistException(MessageConstants.SUBCATEGORY_NOT_FOUND);
            }

            var asset = _mapper.Map<Asset>(model);
            asset.NameUnsign = StringUtils.ConvertToUnSign(model.Name);
            asset.UserId = userId;

            await _unitOfWork.AssetRepository.AddAsync(asset);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = MessageConstants.ASSET_CREATED_SUCCESS,
                Data = _mapper.Map<AssetModel>(asset)
            };
        }

        public async Task<BaseResultModel> UpdateAssetAsync(UpdateAssetModel model)
        {
            // check exist asset id
            if (await _unitOfWork.AssetRepository.GetByIdAsync(model.Id) == null) throw new NotExistException(MessageConstants.ASSET_NOT_FOUND);

            // check exist subcategory
            if (await _unitOfWork.SubcategoryRepository.GetByIdAsync(model.SubcategoryId) == null) throw new NotExistException(MessageConstants.SUBCATEGORY_NOT_FOUND);

            var asset = _mapper.Map<Asset>(model);
            asset.UpdatedDate = CommonUtils.GetCurrentTime();

            _unitOfWork.AssetRepository.UpdateAsync(asset);
            await _unitOfWork.SaveAsync();
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.ASSET_UPDATED_SUCCESS,
                Data = _mapper.Map<AssetModel>(asset)
            };
        }

        public async Task<BaseResultModel> DeleteAssetAsync(Guid id)
        {
            var asset = await _unitOfWork.AssetRepository.GetByIdAsync(id);
            if (asset == null) throw new NotExistException(MessageConstants.ASSET_NOT_FOUND);

            _unitOfWork.AssetRepository.SoftDeleteAsync(asset);
            await _unitOfWork.SaveAsync();
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.ASSET_DELETED_SUCCESS,
            };
        }
        
    }
}
