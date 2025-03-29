using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Services.BusinessModels.FinancialGoalModels;
using MoneyEz.Services.BusinessModels.FinancialGoalModels.CreatePersonnalGoal;
using MoneyEz.Services.BusinessModels.ResultModels;
using System;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IFinancialGoalService
    {
        Task<BaseResultModel> AddPersonalFinancialGoalAsync(AddPersonalFinancialGoalModel model);
        Task<BaseResultModel> GetPersonalFinancialGoalsAsync(PaginationParameter paginationParameter, FinancialGoalFilter filter);
        Task<BaseResultModel> GetPersonalFinancialGoalByIdAsync(Guid id);
        Task<BaseResultModel> GetChartPersonalFinacialGoalByIdAsync(Guid id, string type);
        Task<BaseResultModel> UpdatePersonalFinancialGoalAsync(UpdatePersonalFinancialGoalModel model);
        Task<BaseResultModel> DeletePersonalFinancialGoalAsync(DeleteFinancialGoalModel model);
        Task<BaseResultModel> GetUserLimitBugdetSubcategoryAsync(Guid subcategoryId);
        Task<BaseResultModel> GetUserTransactionsGoalAsync(Guid goalId, PaginationParameter paginationParameter);
        Task<BaseResultModel> GetUserFinancialGoalBySpendingModelAsync(Guid userSpendingModelId, PaginationParameter paginationParameter, FinancialGoalFilter filter);
        Task<BaseResultModel> GetAvailableCategoriesCreateGoalPersonalAsync();

        Task<BaseResultModel> AddGroupFinancialGoalAsync(AddGroupFinancialGoalModel model);
        Task<BaseResultModel> GetGroupFinancialGoalsAsync(GetGroupFinancialGoalsModel model);
        Task<BaseResultModel> GetGroupFinancialGoalByIdAsync(GetGroupFinancialGoalDetailModel model);
        Task<BaseResultModel> UpdateGroupFinancialGoalAsync(UpdateGroupFinancialGoalModel model);
        Task<BaseResultModel> DeleteGroupFinancialGoalAsync(DeleteFinancialGoalModel model);
        Task<BaseResultModel> ApproveGroupFinancialGoalAsync(ApproveGroupFinancialGoalRequestModel model);

    }
}
