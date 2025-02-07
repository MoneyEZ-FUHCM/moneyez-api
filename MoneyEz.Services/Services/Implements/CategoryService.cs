using AutoMapper;
using Microsoft.AspNetCore.Http;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.CategoryModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.Exceptions;
using Microsoft.EntityFrameworkCore;
using MoneyEz.Services.Utils;

namespace MoneyEz.Services.Services.Implements
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BaseResultModel> GetCategoryPaginationAsync(PaginationParameter paginationParameter)
        {
            var categories = await _unitOfWork.CategoriesRepository.ToPaginationIncludeAsync(
                paginationParameter,
                include: query => query.Include(c => c.Subcategories)
            );

            var result = _mapper.Map<Pagination<CategoryModel>>(categories);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.CATEGORY_LIST_FETCHED_SUCCESS,
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

        public async Task<BaseResultModel> GetCategoryByIdAsync(Guid id)
        {
            var category = await _unitOfWork.CategoriesRepository.GetByIdIncludeAsync(
                id,
                include: query => query.Include(c => c.Subcategories)
            );

            if (category == null || category.IsDeleted)
            {
                throw new NotExistException(MessageConstants.CATEGORY_NOT_FOUND);
            }

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.CATEGORY_FETCHED_SUCCESS,
                Data = _mapper.Map<CategoryModel>(category)
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

            var newCategories = new List<Category>();
            var duplicateNames = new List<string>();

            foreach (var model in models)
            {
                var unsignName = StringUtils.ConvertToUnSign(model.Name);
                if (existingUnsignNames.Contains(unsignName) || newCategories.Any(c => c.NameUnsign == unsignName))
                {
                    duplicateNames.Add(model.Name);
                    continue;
                }

                var category = _mapper.Map<Category>(model);
                category.NameUnsign = unsignName;
                newCategories.Add(category);
            }

            if (duplicateNames.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.CATEGORY_ALREADY_EXISTS,
                    Message = $"The following categories already exist: {string.Join(", ", duplicateNames)}"
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

            _mapper.Map(model, category);
            category.NameUnsign = unsignName;
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
                include: query => query.Include(c => c.Subcategories)
            );

            if (category == null || category.IsDeleted)
            {
                throw new NotExistException(MessageConstants.CATEGORY_NOT_FOUND);
            }

            if (category.Subcategories.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.CATEGORY_HAS_DEPENDENCIES,
                    Message = "The category has dependent subcategories and cannot be deleted."
                };
            }

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