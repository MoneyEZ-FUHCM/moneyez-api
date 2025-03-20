using AutoMapper;
using Microsoft.AspNetCore.Http;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Services.BusinessModels.FinancialReportModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Implements
{
    public class FinancialReportService : IFinancialReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimsService _claimsService;

        public FinancialReportService(IUnitOfWork unitOfWork, IMapper mapper, IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsService = claimsService;
        }

        // --- User Reports ---
        public async Task<BaseResultModel> GetAllReportsForUserAsync(PaginationParameter paginationParameter)
        {
            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail);
            if (user == null)
                return new BaseResultModel { Status = StatusCodes.Status404NotFound, ErrorCode = MessageConstants.REPORT_USER_NOT_FOUND };

            var reports = await _unitOfWork.FinancialReportRepository.ToPaginationIncludeAsync(
                paginationParameter,
                filter: r => r.UserId == user.Id
            );

            var result = _mapper.Map<Pagination<FinancialReportModel>>(reports);
            var paginatedResult = PaginationHelper.GetPaginationResult(reports, result);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.REPORT_LIST_FETCHED_SUCCESS_MESSAGE,
                Data = paginatedResult
            };
        }

        public async Task<BaseResultModel> GetUserReportByIdAsync(Guid reportId)
        {
            var report = await _unitOfWork.FinancialReportRepository.GetByIdAsync(reportId);
            if (report == null || report.UserId == null)
                return new BaseResultModel 
                    { 
                        Status = StatusCodes.Status404NotFound, 
                        ErrorCode = MessageConstants.REPORT_NOT_FOUND 
                    };

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.REPORT_FETCHED_SUCCESS_MESSAGE,
                Data = _mapper.Map<FinancialReportModel>(report)
            };
        }

        public async Task<BaseResultModel> CreateUserReportAsync(CreateUserReportModel model)
        {
            if (model.StartDate >= model.EndDate)
                return new BaseResultModel { Status = StatusCodes.Status400BadRequest, ErrorCode = MessageConstants.INVALID_REPORT_DATE_RANGE };

            string userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail);
            if (user == null)
                return new BaseResultModel { Status = StatusCodes.Status404NotFound, ErrorCode = MessageConstants.REPORT_USER_NOT_FOUND };

            var report = _mapper.Map<FinancialReport>(model);
            report.UserId = user.Id;

            // Tính toán số liệu từ Transaction
            report.TotalIncome = await _unitOfWork.TransactionsRepository.GetTotalIncomeAsync(user.Id, null, model.StartDate, model.EndDate);
            report.TotalExpense = await _unitOfWork.TransactionsRepository.GetTotalExpenseAsync(user.Id, null, model.StartDate, model.EndDate);

            await _unitOfWork.FinancialReportRepository.AddAsync(report);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = MessageConstants.REPORT_GENERATE_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> UpdateUserReportAsync(UpdateUserReportModel model)
        {
            var report = await _unitOfWork.FinancialReportRepository.GetByIdAsync(model.Id);
            if (report == null)
                return new BaseResultModel { Status = StatusCodes.Status404NotFound, ErrorCode = MessageConstants.REPORT_NOT_FOUND };

            _mapper.Map(model, report);

            // Cập nhật số liệu
            report.TotalIncome = await _unitOfWork.TransactionsRepository.GetTotalIncomeAsync(report.UserId, null, model.StartDate, model.EndDate);
            report.TotalExpense = await _unitOfWork.TransactionsRepository.GetTotalExpenseAsync(report.UserId, null, model.StartDate, model.EndDate);

            _unitOfWork.FinancialReportRepository.UpdateAsync(report);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.REPORT_UPDATED_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> DeleteUserReportAsync(Guid reportId)
        {
            var report = await _unitOfWork.FinancialReportRepository.GetByIdAsync(reportId);
            if (report == null)
                return new BaseResultModel { Status = StatusCodes.Status404NotFound, ErrorCode = MessageConstants.REPORT_NOT_FOUND };

            _unitOfWork.FinancialReportRepository.SoftDeleteAsync(report);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.REPORT_DELETED_SUCCESS_MESSAGE
            };
        }

        // --- Group Reports ---
        public async Task<BaseResultModel> GetAllReportsForGroupAsync(PaginationParameter paginationParameter, Guid groupId)
        {
            var reports = await _unitOfWork.FinancialReportRepository.ToPaginationIncludeAsync(
                paginationParameter,
                filter: r => r.GroupId == groupId
            );

            var result = _mapper.Map<Pagination<FinancialReportModel>>(reports);
            var paginatedResult = PaginationHelper.GetPaginationResult(reports, result);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.REPORT_LIST_FETCHED_SUCCESS_MESSAGE,
                Data = paginatedResult
            };
        }

        public async Task<BaseResultModel> GetGroupReportByIdAsync(Guid reportId)
        {
            var report = await _unitOfWork.FinancialReportRepository.GetByIdAsync(reportId);
            if (report == null || report.GroupId == null)
                return new BaseResultModel { Status = StatusCodes.Status404NotFound, ErrorCode = MessageConstants.REPORT_NOT_FOUND };

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.REPORT_FETCHED_SUCCESS_MESSAGE,
                Data = _mapper.Map<FinancialReportModel>(report)
            };
        }

        public async Task<BaseResultModel> CreateGroupReportAsync(CreateGroupReportModel model)
        {
            if (model.StartDate >= model.EndDate)
                return new BaseResultModel { Status = StatusCodes.Status400BadRequest, ErrorCode = MessageConstants.INVALID_REPORT_DATE_RANGE };

            var report = _mapper.Map<FinancialReport>(model);

            // Tính toán số liệu từ Transaction của Group
            report.TotalIncome = await _unitOfWork.TransactionsRepository.GetTotalIncomeAsync(null, model.GroupId, model.StartDate, model.EndDate);
            report.TotalExpense = await _unitOfWork.TransactionsRepository.GetTotalExpenseAsync(null, model.GroupId, model.StartDate, model.EndDate);

            await _unitOfWork.FinancialReportRepository.AddAsync(report);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = MessageConstants.REPORT_GENERATE_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> UpdateGroupReportAsync(UpdateGroupReportModel model)
        {
            var report = await _unitOfWork.FinancialReportRepository.GetByIdAsync(model.Id);
            if (report == null)
                return new BaseResultModel { Status = StatusCodes.Status404NotFound, ErrorCode = MessageConstants.REPORT_NOT_FOUND };

            _mapper.Map(model, report);

            // Cập nhật số liệu
            report.TotalIncome = await _unitOfWork.TransactionsRepository.GetTotalIncomeAsync(null, model.GroupId, model.StartDate, model.EndDate);
            report.TotalExpense = await _unitOfWork.TransactionsRepository.GetTotalExpenseAsync(null, model.GroupId, model.StartDate, model.EndDate);

            _unitOfWork.FinancialReportRepository.UpdateAsync(report);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.REPORT_UPDATED_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> DeleteGroupReportAsync(Guid reportId)
        {
            var report = await _unitOfWork.FinancialReportRepository.GetByIdAsync(reportId);
            if (report == null)
                return new BaseResultModel { Status = StatusCodes.Status404NotFound, ErrorCode = MessageConstants.REPORT_NOT_FOUND };

            _unitOfWork.FinancialReportRepository.SoftDeleteAsync(report);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.REPORT_DELETED_SUCCESS_MESSAGE
            };
        }
    }
}
