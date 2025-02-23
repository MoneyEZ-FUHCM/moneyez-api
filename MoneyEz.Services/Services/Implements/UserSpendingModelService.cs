using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.SpendingModelModels;
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
    public class UserSpendingModelService : IUserSpendingModelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimsService _claimsService;

        public UserSpendingModelService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsService = claimsService;
        }

        public async Task<BaseResultModel> ChooseSpendingModelAsync(ChooseSpendingModelModel model)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var spendingModel = await _unitOfWork.SpendingModelRepository.GetByIdAsync(model.SpendingModelId)
                ?? throw new NotExistException(MessageConstants.SPENDING_MODEL_NOT_FOUND);

            if (spendingModel.IsTemplate == false)
            {
                throw new DefaultException("Selected spending model is not a template.", MessageConstants.SPENDING_MODEL_NOT_FOUND);
            }

            // Kiểm tra xem user đã có mô hình chi tiêu nào đang được sử dụng chưa
            var activeModels = await _unitOfWork.UserSpendingModelRepository.ToPaginationIncludeAsync(
                new PaginationParameter { PageSize = 1, PageIndex = 1 },
                filter: usm => usm.UserId == user.Id && usm.EndDate > CommonUtils.GetCurrentTime() && !usm.IsDeleted
            );

            if (activeModels.Any())
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.USER_ALREADY_HAS_ACTIVE_SPENDING_MODEL,
                    Message = "You already have an active spending model. Please switch or cancel it before choosing a new one."
                };
            }

            // Nếu StartDate không được nhập, mặc định là hôm nay
            var startDate = model.StartDate ?? CommonUtils.GetCurrentTime();

            var endDate = CalculateEndDate(startDate, model.PeriodUnit, model.PeriodValue);

            var userSpendingModel = new UserSpendingModel
            {
                UserId = user.Id,
                SpendingModelId = spendingModel.Id,
                PeriodUnit = (int)model.PeriodUnit,
                PeriodValue = model.PeriodValue,
                StartDate = startDate,
                EndDate = endDate
            };

            await _unitOfWork.UserSpendingModelRepository.AddAsync(userSpendingModel);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = "Spending model selected successfully."
            };
        }
        public async Task<BaseResultModel> SwitchSpendingModelAsync(SwitchSpendingModelModel model)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            // Kiểm tra xem có mô hình chi tiêu hiện tại chưa kết thúc không
            var currentModels = await _unitOfWork.UserSpendingModelRepository.ToPaginationIncludeAsync(
                new PaginationParameter { PageSize = 1, PageIndex = 1 },
                filter: usm => usm.UserId == user.Id && usm.EndDate > CommonUtils.GetCurrentTime() && !usm.IsDeleted
            );

            var currentModel = currentModels.FirstOrDefault();

            if (currentModel != null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = MessageConstants.CURRENT_SPENDING_MODEL_NOT_FINISHED,
                    Message = "You cannot switch to a new spending model until your current model has ended."
                };
            }

            var spendingModel = await _unitOfWork.SpendingModelRepository.GetByIdAsync(model.SpendingModelId)
                ?? throw new NotExistException(MessageConstants.SPENDING_MODEL_NOT_FOUND);

            if (spendingModel.IsTemplate == false)
            {
                throw new DefaultException("Selected spending model is not a template.", MessageConstants.SPENDING_MODEL_NOT_FOUND);
            }

            // Nếu không có StartDate, mặc định là ngày mai
            var startDate = model.StartDate ?? CommonUtils.GetCurrentTime().AddDays(1);

            var endDate = CalculateEndDate(startDate, model.PeriodUnit, model.PeriodValue);

            var newModel = new UserSpendingModel
            {
                UserId = user.Id,
                SpendingModelId = model.SpendingModelId,
                PeriodUnit = (int)model.PeriodUnit,
                PeriodValue = model.PeriodValue,
                StartDate = startDate,
                EndDate = endDate
            };

            await _unitOfWork.UserSpendingModelRepository.AddAsync(newModel);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Spending model switched successfully."
            };
        }

        public async Task<BaseResultModel> CancelSpendingModelAsync(Guid spendingModelId)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var spendingModels = await _unitOfWork.UserSpendingModelRepository.ToPaginationIncludeAsync(
                new PaginationParameter { PageSize = 1, PageIndex = 1 },
                filter: usm => usm.UserId == user.Id
                            && usm.SpendingModelId == spendingModelId
                            && usm.EndDate > CommonUtils.GetCurrentTime()
                            && !usm.IsDeleted
            );

            var spendingModel = spendingModels.FirstOrDefault();

            if (spendingModel == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SPENDING_MODEL_NOT_FOUND,
                    Message = "No active spending model found for cancellation."
                };
            }

            _unitOfWork.UserSpendingModelRepository.SoftDeleteAsync(spendingModel);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Spending model cancelled successfully."
            };
        }


        public async Task<BaseResultModel> GetCurrentSpendingModelAsync()
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var currentModels = await _unitOfWork.UserSpendingModelRepository.ToPaginationIncludeAsync(
                new PaginationParameter { PageSize = 1, PageIndex = 1 },
                filter: usm => usm.UserId == user.Id && usm.EndDate > CommonUtils.GetCurrentTime() && !usm.IsDeleted,
                include: query => query.Include(usm => usm.SpendingModel)
            );

            var currentModel = currentModels.FirstOrDefault();


            if (currentModel == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SPENDING_MODEL_NOT_FOUND
                };
            }

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = _mapper.Map<UserSpendingModelModel>(currentModel)
            };
        }

        public async Task<BaseResultModel> GetUsedSpendingModelByIdAsync(Guid id)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var spendingModels = await _unitOfWork.UserSpendingModelRepository.ToPaginationIncludeAsync(
                new PaginationParameter { PageSize = 1, PageIndex = 1 },
                filter: usm => usm.UserId == user.Id && usm.Id == id,
                include: query => query.Include(usm => usm.SpendingModel)
            );

            var spendingModel = spendingModels.FirstOrDefault();


            if (spendingModel == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    ErrorCode = MessageConstants.SPENDING_MODEL_NOT_FOUND
                };
            }

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = _mapper.Map<UserSpendingModelModel>(spendingModel)
            };
        }

        public async Task<BaseResultModel> GetUsedSpendingModelsPaginationAsync(PaginationParameter paginationParameter)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var usedSpendingModels = await _unitOfWork.UserSpendingModelRepository.ToPaginationIncludeAsync(
                paginationParameter,
                filter: usm => usm.UserId == user.Id,
                include: query => query.Include(usm => usm.SpendingModel)
            );

            var mappedResult = _mapper.Map<List<UserSpendingModelHistoryModel>>(usedSpendingModels);

            var paginatedResult = new Pagination<UserSpendingModelHistoryModel>(
                mappedResult,
                usedSpendingModels.TotalCount,
                usedSpendingModels.CurrentPage,
                usedSpendingModels.PageSize
            );

            var metaData = new
            {
                usedSpendingModels.TotalCount,
                usedSpendingModels.PageSize,
                usedSpendingModels.CurrentPage,
                usedSpendingModels.TotalPages,
                usedSpendingModels.HasNext,
                usedSpendingModels.HasPrevious
            };

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = new
                {
                    Data = paginatedResult,
                    MetaData = metaData
                }
            };
        }

        private DateTime CalculateEndDate(DateTime startDate, PeriodUnit periodUnit, int periodValue)
        {
            return periodUnit switch
            {
                PeriodUnit.DAY => startDate.AddDays(periodValue),
                PeriodUnit.WEEK => startDate.AddDays(periodValue * 7),
                PeriodUnit.MONTH => startDate.AddMonths(periodValue),
                PeriodUnit.YEAR => startDate.AddYears(periodValue),
                _ => throw new
                DefaultException(
                    "Invalid period unit",
                    MessageConstants.INVALID_PERIOD_UNIT
                    )
            };
        }
    }
}
