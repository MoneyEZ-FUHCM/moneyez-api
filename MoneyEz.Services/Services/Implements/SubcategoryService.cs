using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
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
        private readonly IClaimsService _claimsService;

        public SubcategoryService(IUnitOfWork unitOfWork, IMapper mapper, IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsService = claimsService;
        }

        public async Task<BaseResultModel> GetSubcategoriesPaginationAsync(PaginationParameter paginationParameter, SubcategoryFilter subcategoryFilter)
        {
            var subcategories = await _unitOfWork.SubcategoryRepository.GetSubcategoriesByFilter(paginationParameter, subcategoryFilter);

            var subcategoryModels = _mapper.Map<Pagination<SubcategoryModel>>(subcategories);
            var result = PaginationHelper.GetPaginationResult(subcategories, subcategoryModels);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.SUBCATEGORY_LIST_FETCHED_SUCCESS,
                Data = result
            };
        }

        public async Task<BaseResultModel> GetSubcategoryByIdAsync(Guid id)
        {
            var subcategory = await _unitOfWork.SubcategoryRepository.GetByIdIncludeAsync(id,
               include: query => query.Include(s => s.CategorySubcategories).ThenInclude(cs => cs.Category));

            if (subcategory == null || subcategory.IsDeleted)
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SUBCATEGORY_NOT_FOUND
                };

            var result = _mapper.Map<SubcategoryModel>(subcategory);
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.SUBCATEGORY_FETCHED_SUCCESS,
                Data = result
            };
        }

        public async Task<BaseResultModel> CreateSubcategoriesAsync(List<CreateSubcategoryModel> models)
        {
            if (models == null || !models.Any())
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.EMPTY_SUBCATEGORY_LIST
                };

            var existingNames = (await _unitOfWork.SubcategoryRepository.GetAllAsync())
                .Where(s => !s.IsDeleted)
                .Select(s => s.NameUnsign)
                .ToHashSet();

            var existingCodes = (await _unitOfWork.SubcategoryRepository.GetAllAsync())
                .Where(s => !s.IsDeleted)
                .Select(s => s.Code)
                .ToHashSet();

            var newSubcategories = new List<Subcategory>();
            var duplicateNames = new List<string>();
            var duplicateCodes = new List<string>();

            foreach (var model in models)
            {
                var unsignName = model.NameUnsign;
                if (existingNames.Contains(unsignName) || newSubcategories.Any(s => s.NameUnsign == unsignName))
                {
                    duplicateNames.Add(model.Name);
                    continue;
                }

                if (existingCodes.Contains(model.Code) || newSubcategories.Any(s => s.Code == model.Code))
                {
                    duplicateNames.Add(model.Name);
                    continue;
                }

                var subcategory = _mapper.Map<Subcategory>(model);
                subcategory.NameUnsign = unsignName;
                subcategory.CreatedBy = _claimsService.GetCurrentUserEmail;
                newSubcategories.Add(subcategory);
            }

            if (duplicateNames.Any())
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.DUPLICATE_SUBCATEGORY_NAMES
                };

            if (duplicateCodes.Any())
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.DUPLICATE_SUBCATEGORY_CODES
                };

            await _unitOfWork.SubcategoryRepository.AddRangeAsync(newSubcategories);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = $"{newSubcategories.Count} subcategories were added successfully."
            };
        }

        public async Task<BaseResultModel> UpdateSubcategoryByIdAsync(UpdateSubcategoryModel model)
        {
            var subcategory = await _unitOfWork.SubcategoryRepository.GetByIdAsync(model.Id);

            if (subcategory == null || subcategory.IsDeleted)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SUBCATEGORY_NOT_FOUND,
                    Message = "Subcategory does not exist or has been deleted."
                };
            }

            var unsignName = model.NameUnsign;
            var allSubcategories = await _unitOfWork.SubcategoryRepository.GetAllAsync();

            if (allSubcategories.Any(s => s.NameUnsign == unsignName && s.Id != model.Id))
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.DUPLICATE_SUBCATEGORY_NAME_GLOBAL,
                    Message = $"Subcategory '{model.Name}' already exists in the system."
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

        public async Task<BaseResultModel> AddSubcategoriesToCategoriesAsync(AssignSubcategoryModel model)
        {
            var existingCategories = await _unitOfWork.CategoriesRepository.GetAllAsync();
            var existingSubcategories = await _unitOfWork.SubcategoryRepository.GetAllAsync();

            var validCategoryCodes = existingCategories.Select(c => c.Code).ToHashSet();
            var validSubcategoryCodes = existingSubcategories.Select(s => s.Code).ToHashSet();

            // Validate category codes
            var invalidCategoryCodes = model.Assignments.Select(a => a.CategoryCode).Except(validCategoryCodes).ToList();
            if (invalidCategoryCodes.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_CATEGORY_IDS,
                    Message = $"The following category codes do not exist: {string.Join(", ", invalidCategoryCodes)}"
                };
            }

            // Validate subcategory codes
            var allSubcategoryCodes = model.Assignments.SelectMany(a => a.SubcategoryCodes).Distinct().ToList();
            var invalidSubcategoryCodes = allSubcategoryCodes.Except(validSubcategoryCodes).ToList();
            if (invalidSubcategoryCodes.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_SUBCATEGORY_IDS,
                    Message = $"The following subcategory codes do not exist: {string.Join(", ", invalidSubcategoryCodes)}"
                };
            }

            // Get existing category-subcategory relationships
            var existingCategorySubcategories = await _unitOfWork.CategorySubcategoryRepository
                .GetByConditionAsync(include: query => query
                    .Include(x => x.Subcategory)
                    .Include(x => x.Category));

            var existingLinks = existingCategorySubcategories
                .Select(cs => (cs.Category.Code, cs.Subcategory.Code))
                .ToHashSet();

            var newCategorySubcategories = new List<CategorySubcategory>();

            // Create dictionary for quick lookup of IDs by code
            var categoryIdsByCode = existingCategories.ToDictionary(c => c.Code, c => c.Id);
            var subcategoryIdsByCode = existingSubcategories.ToDictionary(s => s.Code, s => s.Id);

            foreach (var assignment in model.Assignments)
            {
                var categoryId = categoryIdsByCode[assignment.CategoryCode];
                
                foreach (var subcategoryCode in assignment.SubcategoryCodes)
                {
                    if (!existingLinks.Contains((assignment.CategoryCode, subcategoryCode)))
                    {
                        var subcategoryId = subcategoryIdsByCode[subcategoryCode];
                        newCategorySubcategories.Add(new CategorySubcategory
                        {
                            CategoryId = categoryId,
                            SubcategoryId = subcategoryId,
                            CreatedBy = _claimsService.GetCurrentUserEmail
                        });
                    }
                }
            }

            if (newCategorySubcategories.Any())
            {
                await _unitOfWork.CategorySubcategoryRepository.AddRangeAsync(newCategorySubcategories);
                _unitOfWork.Save();
            }

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = "Subcategories assigned to categories successfully."
            };
        }

        public async Task<BaseResultModel> DeleteSubcategoryAsync(Guid id)
        {
            var subcategory = await _unitOfWork.SubcategoryRepository.GetByIdIncludeAsync(id,
                include: query => query.Include(sc => sc.Transactions).Include(sc => sc.RecurringTransactions));

            if (subcategory == null || subcategory.IsDeleted)
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SUBCATEGORY_NOT_FOUND
                };

            if (subcategory.Transactions.Any() || subcategory.RecurringTransactions.Any())
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.SUBCATEGORY_HAS_DEPENDENCIES
                };

            subcategory.UpdatedBy = _claimsService.GetCurrentUserEmail;
            _unitOfWork.SubcategoryRepository.SoftDeleteAsync(subcategory);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.SUBCATEGORY_DELETED_SUCCESS
            };
        }

        public async Task<BaseResultModel> RemoveSubcategoriesFromCategoriesAsync(RemoveSubcategoryFromCategoryModel model)
        {
            var category = await _unitOfWork.CategoriesRepository.GetByIdAsync(model.CategoryId);
            if (category == null || category.IsDeleted)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.CATEGORY_NOT_FOUND,
                    Message = "Category does not exist."
                };
            }

            var subcategory = await _unitOfWork.SubcategoryRepository.GetByIdAsync(model.SubcategoryId);
            if (subcategory == null || subcategory.IsDeleted)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SUBCATEGORY_NOT_FOUND,
                    Message = "Subcategory does not exist."
                };
            }

            var categorySubcategoryList = await _unitOfWork.CategorySubcategoryRepository.GetAllAsync();
            var categorySubcategory = categorySubcategoryList
                .FirstOrDefault(cs => cs.CategoryId == model.CategoryId && cs.SubcategoryId == model.SubcategoryId);

            if (categorySubcategory == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SUBCATEGORY_NOT_FOUND_IN_CATEGORY,
                    Message = "The subcategory is not found in the selected category."
                };
            }

            _unitOfWork.CategorySubcategoryRepository.PermanentDeletedAsync(categorySubcategory);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Subcategory removed from category successfully."
            };
        }
    }
}