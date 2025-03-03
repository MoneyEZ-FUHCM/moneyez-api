using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Services.BusinessModels.CategoryModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Implements
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimsService _claimsService;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsService = claimsService;
        }

        public async Task<BaseResultModel> GetCategoryPaginationAsync(PaginationParameter paginationParameter)
        {
            var categories = await _unitOfWork.CategoriesRepository.ToPaginationIncludeAsync(
                paginationParameter,
                include: query => query.Include(c => c.CategorySubcategories)
                                       .ThenInclude(cs => cs.Subcategory)
            );

            var categoryModels = _mapper.Map<Pagination<CategoryModel>>(categories);
            var result = PaginationHelper.GetPaginationResult(categoryModels, categoryModels);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.CATEGORY_LIST_FETCHED_SUCCESS,
                Data = result
            };
        }

        public async Task<BaseResultModel> GetCategoryByIdAsync(Guid id)
        {
            var category = await _unitOfWork.CategoriesRepository.GetByIdIncludeAsync(
                id,
                include: query => query.Include(c => c.CategorySubcategories)
                                       .ThenInclude(cs => cs.Subcategory)
            );

            if (category == null || category.IsDeleted)
            {
                throw new NotExistException(MessageConstants.CATEGORY_NOT_FOUND);
            }

            var categoryModel = _mapper.Map<CategoryModel>(category);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.CATEGORY_FETCHED_SUCCESS,
                Data = categoryModel
            };
        }

        public async Task<BaseResultModel> AddCategoriesAsync(List<CreateCategoryModel> models)
        {
            if (models == null || !models.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.EMPTY_CATEGORY_LIST,
                    Message = "The list of categories cannot be empty."
                };
            }

            var existingCategories = await _unitOfWork.CategoriesRepository.GetAllAsync();
            var existingUnsignNames = existingCategories
                .Where(c => !c.IsDeleted)
                .Select(c => c.NameUnsign)
                .ToHashSet();

            var existingCodes = existingCategories
                .Where(c => !c.IsDeleted)
                .Select(c => c.Code)
                .ToHashSet();

            var newCategories = new List<Category>();
            var duplicateNames = new List<string>();
            var duplicateCodes = new List<string>();

            foreach (var model in models)
            {
                var unsignName = StringUtils.ConvertToUnSign(model.Name);
                if (existingUnsignNames.Contains(unsignName) || newCategories.Any(c => c.NameUnsign == unsignName))
                {
                    duplicateNames.Add(model.Name);
                    continue;
                }

                if (newCategories.Any(c => c.Code == model.Code))
                {
                    duplicateCodes.Add(model.Code);
                    continue;
                }

                var category = _mapper.Map<Category>(model);
                category.NameUnsign = unsignName;
                category.CreatedBy = _claimsService.GetCurrentUserEmail;
                newCategories.Add(category);
            }

            if (duplicateNames.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.CATEGORY_ALREADY_EXISTS,
                    Message = $"The following categories name already exist: {string.Join(", ", duplicateNames)}"
                };
            }

            if (duplicateNames.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.CATEGORY_ALREADY_EXISTS,
                    Message = $"The following categories code already exist: {string.Join(", ", duplicateCodes)}"
                };
            }

            await _unitOfWork.CategoriesRepository.AddRangeAsync(newCategories);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = MessageConstants.CATEGORY_CREATED_SUCCESS
            };
        }

        public async Task<BaseResultModel> UpdateCategoryAsync(UpdateCategoryModel model)
        {
            var category = await _unitOfWork.CategoriesRepository.GetByIdAsync(model.Id);
            if (category == null || category.IsDeleted)
            {
                throw new NotExistException(MessageConstants.CATEGORY_NOT_FOUND);
            }

            var allCategories = await _unitOfWork.CategoriesRepository.GetAllAsync();
            var unsignName = StringUtils.ConvertToUnSign(model.Name);
            if (allCategories.Any(c => c.NameUnsign == unsignName && c.Id != model.Id))
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.CATEGORY_ALREADY_EXISTS,
                    Message = $"A category with the name '{model.Name}' already exists."
                };
            }

            if (allCategories.Any(c => c.Code == model.Code && c.Id != model.Id))
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.CATEGORY_ALREADY_EXISTS,
                    Message = $"A category with the code '{model.Code}' already exists."
                };
            }

            _mapper.Map(model, category);
            category.NameUnsign = unsignName;
            category.Code = model.Code;
            category.UpdatedBy = _claimsService.GetCurrentUserEmail;
            _unitOfWork.CategoriesRepository.UpdateAsync(category);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.CATEGORY_UPDATED_SUCCESS
            };
        }

        public async Task<BaseResultModel> DeleteCategoryAsync(Guid id)
        {
            var category = await _unitOfWork.CategoriesRepository.GetByIdIncludeAsync(
                id,
                include: query => query.Include(c => c.CategorySubcategories)
                                       .ThenInclude(cs => cs.Subcategory)
            );

            if (category == null || category.IsDeleted)
            {
                throw new NotExistException(MessageConstants.CATEGORY_NOT_FOUND);
            }

            if (category.CategorySubcategories.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.CATEGORY_HAS_DEPENDENCIES,
                    Message = "The category has dependent subcategories and cannot be deleted."
                };
            }

            category.UpdatedBy = _claimsService.GetCurrentUserEmail;
            _unitOfWork.CategoriesRepository.SoftDeleteAsync(category);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.CATEGORY_DELETED_SUCCESS
            };
        }
    }
}
