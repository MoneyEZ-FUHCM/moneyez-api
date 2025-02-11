using AutoMapper;
using Microsoft.AspNetCore.Http;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.SpendingModelModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Repositories.Commons;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MoneyEz.Services.BusinessModels.CategoryModels;

namespace MoneyEz.Services.Services.Implements
{
    public class SpendingModelService : ISpendingModelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SpendingModelService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BaseResultModel> GetSpendingModelsPaginationAsync(PaginationParameter paginationParameter)
        {
            // Lấy SpendingModel cùng các danh mục liên kết
            var spendingModels = await _unitOfWork.SpendingModelRepository.ToPaginationIncludeAsync(
                paginationParameter,
                include: query => query.Include(sm => sm.SpendingModelCategories)
                                       .ThenInclude(smc => smc.Category)
            );

            // Map từ SpendingModel sang SpendingModelModel
            var result = _mapper.Map<Pagination<SpendingModelModel>>(spendingModels);

            // Chuẩn bị metadata
            var response = new ModelPaging
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
            };

            // Trả về BaseResultModel
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.SPENDING_MODEL_LIST_FETCHED_SUCCESS,
                Data = response
            };
        }

        public async Task<BaseResultModel> GetSpendingModelByIdAsync(Guid id)
        {
            // Lấy SpendingModel từ repository cùng các danh mục liên quan
            var spendingModel = await _unitOfWork.SpendingModelRepository.GetByIdIncludeAsync(
                id,
                include: query => query.Include(sm => sm.SpendingModelCategories)
                                       .ThenInclude(smc => smc.Category)
            );

            if (spendingModel == null || spendingModel.IsDeleted)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SPENDING_MODEL_NOT_FOUND
                };
            }

            // Map từ SpendingModel sang SpendingModelModel
            var result = _mapper.Map<SpendingModelModel>(spendingModel);

            // Trả về kết quả
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.SPENDING_MODEL_FETCHED_SUCCESS,
                Data = result
            };
        }

        public async Task<BaseResultModel> AddSpendingModelsAsync(List<CreateSpendingModelModel> models)
        {
            if (models == null || !models.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.EMPTY_SPENDING_MODEL_LIST,
                    Message = "The list of spending models cannot be empty."
                };
            }

            var allSpendingModels = await _unitOfWork.SpendingModelRepository.GetAllAsync();
            var existingUnsignNames = allSpendingModels
                .Where(sm => !sm.IsDeleted)
                .Select(sm => sm.NameUnsign)
                .ToHashSet();

            var newSpendingModels = new List<SpendingModel>();
            var duplicateNames = new List<string>();

            foreach (var model in models)
            {
                var unsignName = model.NameUnsign; // Xử lý tự động từ model
                if (existingUnsignNames.Contains(unsignName) || newSpendingModels.Any(sm => sm.NameUnsign == unsignName))
                {
                    duplicateNames.Add(model.Name);
                    continue;
                }

                var spendingModel = _mapper.Map<SpendingModel>(model);
                spendingModel.NameUnsign = unsignName; // Đảm bảo gán NameUnsign
                newSpendingModels.Add(spendingModel);
            }

            if (duplicateNames.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.DUPLICATE_SPENDING_MODELS,
                    Message = $"The following spending models already exist: {string.Join(", ", duplicateNames)}"
                };
            }

            await _unitOfWork.SpendingModelRepository.AddRangeAsync(newSpendingModels);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = $"{newSpendingModels.Count} spending models were added successfully."
            };
        }
        public async Task<BaseResultModel> UpdateSpendingModelAsync(UpdateSpendingModelModel model)
        {
            var spendingModel = await _unitOfWork.SpendingModelRepository.GetByIdAsync(model.Id);
            if (spendingModel == null || spendingModel.IsDeleted)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SPENDING_MODEL_NOT_FOUND,
                    Message = "The spending model does not exist or has been deleted."
                };
            }

            var unsignName = model.NameUnsign; // Xử lý tự động từ model
            var allSpendingModels = await _unitOfWork.SpendingModelRepository.GetAllAsync();
            if (allSpendingModels.Any(sm => sm.NameUnsign == unsignName && sm.Id != model.Id))
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.SPENDING_MODEL_ALREADY_EXISTS,
                    Message = $"A spending model with the name '{model.Name}' already exists."
                };
            }

            _mapper.Map(model, spendingModel);
            spendingModel.NameUnsign = unsignName; // Đảm bảo gán NameUnsign
            _unitOfWork.SpendingModelRepository.UpdateAsync(spendingModel);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.SPENDING_MODEL_UPDATED_SUCCESS
            };
        }

        public async Task<BaseResultModel> DeleteSpendingModelAsync(Guid id)
        {
            var spendingModel = await _unitOfWork.SpendingModelRepository.GetByIdIncludeAsync(
                id,
                include: query => query.Include(sm => sm.SpendingModelCategories)
            );

            if (spendingModel == null || spendingModel.IsDeleted)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SPENDING_MODEL_NOT_FOUND,
                    Message = "The spending model does not exist or has been deleted."
                };
            }

            // Kiểm tra nếu SpendingModel có các danh mục phụ thuộc
            if (spendingModel.SpendingModelCategories.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.SPENDING_MODEL_HAS_DEPENDENCIES,
                    Message = "The spending model has dependent categories and cannot be deleted."
                };
            }

            _unitOfWork.SpendingModelRepository.SoftDeleteAsync(spendingModel);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.SPENDING_MODEL_DELETED_SUCCESS
            };
        }

        public async Task<BaseResultModel> AddCategoriesToSpendingModelAsync(AddCategoriesToSpendingModelModel model)
        {
            if (model == null || model.SpendingModelId == Guid.Empty || model.Categories == null || !model.Categories.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.EMPTY_CATEGORY_LIST,
                    Message = "The list of categories cannot be empty, and a valid SpendingModelId is required."
                };
            }

            if (model.Categories.Any(c => c.CategoryId == Guid.Empty))
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_CATEGORY_IDS,
                    Message = "One or more CategoryIds are invalid (empty GUIDs)."
                };
            }

            if (model.Categories.Any(c => c.PercentageAmount < 0))
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_PERCENTAGE_AMOUNT,
                    Message = "Percentage amounts cannot be negative."
                };
            }

            var duplicateCategoryIdsInRequest = model.Categories
                .GroupBy(c => c.CategoryId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateCategoryIdsInRequest.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.DUPLICATE_CATEGORY_IDS_IN_REQUEST,
                    Message = $"The following category IDs are duplicated in the request: {string.Join(", ", duplicateCategoryIdsInRequest)}"
                };
            }

            var spendingModel = await _unitOfWork.SpendingModelRepository.GetByIdIncludeAsync(
                model.SpendingModelId,
                include: query => query.Include(sm => sm.SpendingModelCategories)
            );

            if (spendingModel == null || spendingModel.IsDeleted)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SPENDING_MODEL_NOT_FOUND,
                    Message = "The spending model does not exist or has been deleted."
                };
            }

            var existingCategories = await _unitOfWork.CategoriesRepository.GetAllAsync();
            var validCategoryIds = existingCategories.Select(c => c.Id).ToHashSet();

            var invalidCategoryIds = model.Categories.Select(c => c.CategoryId).Except(validCategoryIds).ToList();
            if (invalidCategoryIds.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_CATEGORY_IDS,
                    Message = $"The following category IDs do not exist: {string.Join(", ", invalidCategoryIds)}"
                };
            }

            var existingCategoryIdsInModel = spendingModel.SpendingModelCategories.Select(smc => smc.CategoryId).ToHashSet();
            var newCategoriesData = model.Categories.Where(c => !existingCategoryIdsInModel.Contains(c.CategoryId)).ToList();

            if (!newCategoriesData.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.CATEGORIES_ALREADY_ADDED,
                    Message = "All categories in the list are already added to the spending model."
                };
            }

            if (!spendingModel.SpendingModelCategories.Any())
            {
                // Nếu đây là lần đầu thêm danh mục, `PercentageAmount` là bắt buộc
                if (newCategoriesData.Any(c => c.PercentageAmount == 0))
                {
                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status400BadRequest,
                        ErrorCode = MessageConstants.PERCENTAGE_REQUIRED,
                        Message = "Percentage amounts must be provided when adding categories for the first time."
                    };
                }

                var totalPercentage = newCategoriesData.Sum(c => c.PercentageAmount);
                if (totalPercentage != 100)
                {
                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status400BadRequest,
                        ErrorCode = MessageConstants.INVALID_TOTAL_PERCENTAGE,
                        Message = "The total percentage amount must equal 100% when adding categories for the first time."
                    };
                }
            }
            else
            {
                // Nếu đã có danh mục, các danh mục mới mặc định sẽ có tỷ lệ phần trăm là 0
                newCategoriesData.ForEach(c => c.PercentageAmount = 0);
            }

            var newCategories = newCategoriesData.Select(c => new SpendingModelCategory
            {
                SpendingModelId = model.SpendingModelId,
                CategoryId = c.CategoryId,
                PercentageAmount = c.PercentageAmount
            }).ToList();

            await _unitOfWork.SpendingModelCategoryRepository.AddRangeAsync(newCategories);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = $"{newCategories.Count} categories were successfully added to the spending model."
            };
        }

        public async Task<BaseResultModel> UpdateCategoryPercentageAsync(UpdateCategoryPercentageModel model)
        {
            var spendingModel = await _unitOfWork.SpendingModelRepository.GetByIdIncludeAsync(
                 model.SpendingModelId,
                include: query => query.Include(sm => sm.SpendingModelCategories)
            );

            if (spendingModel == null || spendingModel.IsDeleted)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SPENDING_MODEL_NOT_FOUND,
                    Message = "The spending model does not exist or has been deleted."
                };
            }

            foreach (var categoryPercentage in model.Categories)
            {
                var categoryInModel = spendingModel.SpendingModelCategories
                    .FirstOrDefault(c => c.CategoryId == categoryPercentage.CategoryId);

                if (categoryInModel == null)
                {
                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status400BadRequest,
                        ErrorCode = MessageConstants.CATEGORY_NOT_FOUND_IN_SPENDING_MODEL,
                        Message = $"Category with ID {categoryPercentage.CategoryId} not found in the spending model."
                    };
                }

                categoryInModel.PercentageAmount = categoryPercentage.PercentageAmount;
            }

            var totalPercentageAmount = spendingModel.SpendingModelCategories.Sum(c => c.PercentageAmount);
            if (totalPercentageAmount != 100)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.INVALID_TOTAL_PERCENTAGE,
                    Message = "The total percentage amount for all categories in the spending model must equal 100%."
                };
            }

            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Category percentages were successfully updated."
            };
        }

        public async Task<BaseResultModel> RemoveCategoriesFromSpendingModelAsync(RemoveCategoriesFromSpendingModelModel model)
        {
            // Lấy SpendingModel từ database cùng với các danh mục liên quan
            var spendingModel = await _unitOfWork.SpendingModelRepository.GetByIdIncludeAsync(
                model.SpendingModelId,
                include: query => query.Include(sm => sm.SpendingModelCategories)
            );

            if (spendingModel == null || spendingModel.IsDeleted)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SPENDING_MODEL_NOT_FOUND,
                    Message = "The spending model does not exist or has been deleted."
                };
            }

            // Lấy danh sách các danh mục hiện có trong SpendingModel
            var categoriesToRemove = spendingModel.SpendingModelCategories
                .Where(smc => model.CategoryIds.Contains(smc.CategoryId.Value))
                .ToList();

            if (!categoriesToRemove.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.CATEGORIES_NOT_FOUND_IN_SPENDING_MODEL,
                    Message = "None of the provided categories were found in the spending model."
                };
            }

            // Xóa các danh mục khỏi SpendingModel
            _unitOfWork.SpendingModelCategoryRepository.PermanentDeletedListAsync(categoriesToRemove);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = $"{categoriesToRemove.Count} categories were successfully removed from the spending model."
            };
        }


    }
}
