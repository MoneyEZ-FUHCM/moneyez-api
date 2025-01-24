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
                Data = result,
                Message = MessageConstants.CATEGORY_LIST_FETCHED_SUCCESS
            };
        }

        public async Task<BaseResultModel> GetCategoryByIdAsync(Guid id)
        {
            var category = await _unitOfWork.CategoriesRepository.GetByIdAsync(id);
            if (category == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.CATEGORY_NOT_FOUND
                };
            }

            var result = _mapper.Map<CategoryModel>(category);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = result,
                Message = MessageConstants.CATEGORY_FETCHED_SUCCESS
            };
        }

        public async Task<BaseResultModel> AddCategoryAsync(CreateCategoryModel model)
        {
            var unsignName = model.NameUnsign;
            var existingCategory = await _unitOfWork.CategoriesRepository
                .FindByConditionAsync(c => c.NameUnsign == unsignName);

            if (existingCategory != null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    Message = MessageConstants.CATEGORY_ALREADY_EXISTS
                };
            }

            var category = _mapper.Map<Category>(model);

            await _unitOfWork.CategoriesRepository.AddAsync(category);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = MessageConstants.CATEGORY_CREATED_SUCCESS
            };
        }

        public async Task<BaseResultModel> UpdateCategoryAsync(Guid id, UpdateCategoryModel model)
        {
            var category = await _unitOfWork.CategoriesRepository.GetByIdAsync(id);
            if (category == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.CATEGORY_NOT_FOUND
                };
            }

            var unsignName = model.NameUnsign;
            var duplicateCategory = await _unitOfWork.CategoriesRepository
                .FindByConditionAsync(c => c.NameUnsign == unsignName && c.Id != id);

            if (duplicateCategory != null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    Message = MessageConstants.CATEGORY_ALREADY_EXISTS
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
            if (category == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.CATEGORY_NOT_FOUND
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
