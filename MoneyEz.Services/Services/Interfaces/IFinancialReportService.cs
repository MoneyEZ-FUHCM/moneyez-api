using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.FinancialReportModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IFinancialReportService
    {
        Task<BaseResultModel> GetAllReportsForUserAsync(PaginationParameter paginationParameter);
        Task<BaseResultModel> GetUserReportByIdAsync(Guid reportId);
        Task<BaseResultModel> CreateUserReportAsync(CreateUserReportModel model);
        Task<BaseResultModel> UpdateUserReportAsync(UpdateUserReportModel model);
        Task<BaseResultModel> DeleteUserReportAsync(Guid reportId);

        Task<BaseResultModel> GetAllReportsForGroupAsync(PaginationParameter paginationParameter, Guid groupId);
        Task<BaseResultModel> GetGroupReportByIdAsync(Guid reportId);
        Task<BaseResultModel> CreateGroupReportAsync(CreateGroupReportModel model);
        Task<BaseResultModel> UpdateGroupReportAsync(UpdateGroupReportModel model);
        Task<BaseResultModel> DeleteGroupReportAsync(Guid reportId);
    }
}
