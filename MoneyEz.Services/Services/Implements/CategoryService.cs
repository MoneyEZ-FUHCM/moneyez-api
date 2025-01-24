using AutoMapper;
using Microsoft.AspNetCore.Http;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.CategoryModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Repositories.Commons;

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
            var categories = await _unitOfWork.CategoriesRepository.ToPagination(paginationParameter);

            var result = _mapper.Map<Pagination<CategoryModel>>(categories);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.CATEGORY_LIST_FETCHED_SUCCESS,
                Data = result
            };
        }

        public async Task<BaseResultModel> GetCategoryByIdAsync(Guid id)
        {
            var category = await _unitOfWork.CategoriesRepository.GetByIdAsync(id);
            if (category == null || category.IsDeleted)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.CATEGORY_NOT_FOUND
                };
            }

            var result = _mapper.Map<CategoryModel>(category);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.CATEGORY_FETCHED_SUCCESS,
                Data = result
            };
        }

        public async Task<BaseResultModel> AddCategoriesAsync(List<CreateCategoryModel> models)
        {
            if (models == null || !models.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.CATEGORY_LIST_EMPTY,
                    Message = "The category list is empty."
                };
            }

            var allCategories = await _unitOfWork.CategoriesRepository.GetAllAsync();
            var existingNames = allCategories.Where(c => !c.IsDeleted)
                                              .Select(c => c.NameUnsign)
                                              .ToHashSet();

            var newCategories = new List<Category>();
            var duplicateNames = new List<string>();

            foreach (var model in models)
            {
                var unsignName = model.NameUnsign;

                //check duplicate
                if (existingNames.Contains(unsignName))
                {
                    duplicateNames.Add(model.Name);
                    continue;
                }

                var category = _mapper.Map<Category>(model);
                newCategories.Add(category);
            }

            // if duplicate names found
            if (duplicateNames.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.CATEGORY_DUPLICATE_IN_LIST,
                    Message = $"The following categories already exist: {string.Join(", ", duplicateNames)}"
                };
            }

            // if no valid categories to add
            if (!newCategories.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.CATEGORY_LIST_EMPTY,
                    Message = "No valid categories to add."
                };
            }

            // add new categories
            await _unitOfWork.CategoriesRepository.AddRangeAsync(newCategories);
            _unitOfWork.Save();

            // return success message
            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = $"{newCategories.Count} categories were added successfully."
            };
        }

        public async Task<BaseResultModel> UpdateCategoryAsync(Guid id, UpdateCategoryModel model)
        {
            var category = await _unitOfWork.CategoriesRepository.GetByIdAsync(id);
            if (category == null || category.IsDeleted)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.CATEGORY_NOT_FOUND
                };
            }

            var unsignName = model.NameUnsign;

            // Check duplicate
            var allCategories = await _unitOfWork.CategoriesRepository.GetAllAsync();
            if (allCategories.Any(c => c.NameUnsign == unsignName && c.Id != id))
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.CATEGORY_ALREADY_EXISTS
                };
            }

            _mapper.Map(model, category);

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
            var category = await _unitOfWork.CategoriesRepository.GetByIdAsync(id);
            if (category == null || category.IsDeleted)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.CATEGORY_NOT_FOUND
                };
            }

            // check dependencies
            if (category.Subcategories.Any() || category.SpendingModelCategories.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.CATEGORY_HAS_DEPENDENCIES
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
