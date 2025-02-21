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
            var subcategories = await _unitOfWork.SubcategoryRepository.ToPaginationIncludeAsync(
               paginationParameter,
               include: query => query.Include(s => s.CategorySubcategories).ThenInclude(cs => cs.Category)
           );

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
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.DUPLICATE_SUBCATEGORY_NAMES
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

            var validCategoryIds = existingCategories.Select(c => c.Id).ToHashSet();
            var validSubcategoryIds = existingSubcategories.Select(s => s.Id).ToHashSet();

            var invalidCategoryIds = model.Assignments.Select(a => a.CategoryId).Except(validCategoryIds).ToList();
            if (invalidCategoryIds.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_CATEGORY_IDS,
                    Message = $"The following category IDs do not exist: {string.Join(", ", invalidCategoryIds)}"
                };
            }

            var allSubcategoryIds = model.Assignments.SelectMany(a => a.SubcategoryIds).Distinct().ToList();
            var invalidSubcategoryIds = allSubcategoryIds.Except(validSubcategoryIds).ToList();
            if (invalidSubcategoryIds.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_SUBCATEGORY_IDS,
                    Message = $"The following subcategory IDs do not exist: {string.Join(", ", invalidSubcategoryIds)}"
                };
            }

            var existingCategorySubcategories = await _unitOfWork.CategorySubcategoryRepository.GetAllAsync();
            var existingLinks = existingCategorySubcategories
                .Select(cs => (cs.CategoryId, cs.SubcategoryId))
                .ToHashSet();

            var newCategorySubcategories = new List<CategorySubcategory>();

            foreach (var assignment in model.Assignments)
            {
                foreach (var subcategoryId in assignment.SubcategoryIds)
                {
                    if (!existingLinks.Contains((assignment.CategoryId, subcategoryId)))
                    {
                        newCategorySubcategories.Add(new CategorySubcategory
                        {
                            CategoryId = assignment.CategoryId,
                            SubcategoryId = subcategoryId
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