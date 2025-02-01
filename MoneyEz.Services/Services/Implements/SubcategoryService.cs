using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.SubcategoryModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Implements
{
    public class SubcategoryService : ISubcategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SubcategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BaseResultModel> GetSubcategoriesPaginationAsync(PaginationParameter paginationParameter)
        {
            var subcategories = await _unitOfWork.SubcategoryRepository.ToPaginationIncludeAsync(paginationParameter);

            var result = _mapper.Map<Pagination<SubcategoryModel>>(subcategories);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.SUBCATEGORY_LIST_FETCHED_SUCCESS,
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

        public async Task<BaseResultModel> GetSubcategoryByIdAsync(Guid id)
        {
            var subcategory = await _unitOfWork.SubcategoryRepository.GetByIdAsync(id);

            if (subcategory == null || subcategory.IsDeleted)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SUBCATEGORY_NOT_FOUND
                };
            }

            var result = _mapper.Map<SubcategoryModel>(subcategory);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.SUBCATEGORY_FETCHED_SUCCESS,
                Data = result
            };
        }

        public async Task<BaseResultModel> AddSubcategoriesAsync(List<CreateSubcategoryModel> models)
        {
            if (models == null || !models.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.EMPTY_SUBCATEGORY_LIST,
                    Message = "The list of subcategories cannot be empty."
                };
            }

            // Kiểm tra xem model nào không có CategoryId
            if (models.Any(m => m.CategoryId == Guid.Empty))
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.CATEGORY_ID_REQUIRED,
                    Message = "Category ID is required for all subcategories."
                };
            }

            // Lấy danh sách danh mục chính hợp lệ từ DB
            var existingCategories = await _unitOfWork.CategoriesRepository.GetAllAsync();
            var validCategoryIds = existingCategories.Select(c => c.Id).ToHashSet();

            var invalidCategoryIds = models.Select(m => m.CategoryId).Except(validCategoryIds).ToList();
            if (invalidCategoryIds.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_CATEGORY_IDS,
                    Message = $"The following category IDs do not exist: {string.Join(", ", invalidCategoryIds)}"
                };
            }

            var existingSubcategories = await _unitOfWork.SubcategoryRepository.GetAllAsync();
            var existingNames = existingSubcategories
                .Where(s => !s.IsDeleted)
                .Select(s => s.NameUnsign)
                .ToHashSet();

            var newSubcategories = new List<Subcategory>();
            var duplicateNames = new List<string>();

            foreach (var model in models)
            {
                var unsignName = model.NameUnsign;
                if (existingNames.Contains(unsignName) || newSubcategories.Any(s => s.NameUnsign == unsignName))
                {
                    duplicateNames.Add(model.Name);
                    continue;
                }

                var subcategory = _mapper.Map<Subcategory>(model);
                subcategory.NameUnsign = unsignName;
                newSubcategories.Add(subcategory);
            }

            if (duplicateNames.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.DUPLICATE_SUBCATEGORY_NAMES,
                    Message = $"The following subcategory names already exist: {string.Join(", ", duplicateNames)}"
                };
            }

            await _unitOfWork.SubcategoryRepository.AddRangeAsync(newSubcategories);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = $"{newSubcategories.Count} subcategories were added successfully."
            };
        }

        public async Task<BaseResultModel> UpdateSubcategoryAsync(UpdateSubcategoryModel model)
        {
            if (model.CategoryId == Guid.Empty)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.CATEGORY_ID_REQUIRED,
                    Message = "Category ID is required."
                };
            }

            var subcategory = await _unitOfWork.SubcategoryRepository.GetByIdAsync(model.Id);
            if (subcategory == null || subcategory.IsDeleted)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SUBCATEGORY_NOT_FOUND
                };
            }

            // Kiểm tra danh mục chính có tồn tại không
            var existingCategories = await _unitOfWork.CategoriesRepository.GetAllAsync();
            var validCategoryIds = existingCategories.Select(c => c.Id).ToHashSet();
            if (!validCategoryIds.Contains(model.CategoryId))
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_CATEGORY_IDS,
                    Message = $"Category ID {model.CategoryId} does not exist."
                };
            }

            var unsignName = model.NameUnsign;
            var existingSubcategories = await _unitOfWork.SubcategoryRepository.GetAllAsync();
            if (existingSubcategories.Any(s => s.NameUnsign == unsignName && s.Id != model.Id))
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.SUBCATEGORY_ALREADY_EXISTS
                };
            }

            _mapper.Map(model, subcategory);
            subcategory.NameUnsign = unsignName;
            _unitOfWork.SubcategoryRepository.UpdateAsync(subcategory);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.SUBCATEGORY_UPDATED_SUCCESS
            };
        }

        public async Task<BaseResultModel> DeleteSubcategoryAsync(Guid id)
        {
            // Lấy Subcategory từ database cùng với các giao dịch liên quan
            var subcategory = await _unitOfWork.SubcategoryRepository.GetByIdIncludeAsync(
                id,
                include: query => query.Include(sc => sc.Transactions).Include(sc => sc.RecurringTransactions)
            );

            if (subcategory == null || subcategory.IsDeleted)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SUBCATEGORY_NOT_FOUND,
                    Message = "The subcategory does not exist or has been deleted."
                };
            }

            // Kiểm tra nếu Subcategory đang được sử dụng trong giao dịch
            if (subcategory.Transactions.Any() || subcategory.RecurringTransactions.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.SUBCATEGORY_HAS_DEPENDENCIES,
                    Message = "The subcategory has transactions associated with it and cannot be deleted."
                };
            }

            // Xóa mềm Subcategory
            _unitOfWork.SubcategoryRepository.SoftDeleteAsync(subcategory);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.SUBCATEGORY_DELETED_SUCCESS
            };
        }
    }
}
