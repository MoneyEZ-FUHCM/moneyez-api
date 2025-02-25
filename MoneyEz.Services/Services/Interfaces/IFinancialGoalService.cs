using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.FinancialGoalModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using System;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IFinancialGoalService
    {
        Task<BaseResultModel> AddPersonalFinancialGoalAsync(AddPersonalFinancialGoalModel model);
        Task<BaseResultModel> GetPersonalFinancialGoalsAsync(PaginationParameter paginationParameter);
        Task<BaseResultModel> GetPersonalFinancialGoalByIdAsync(GetPersonalFinancialGoalDetailModel model);
        Task<BaseResultModel> UpdatePersonalFinancialGoalAsync(UpdatePersonalFinancialGoalModel model);
        Task<BaseResultModel> DeletePersonalFinancialGoalAsync(DeleteFinancialGoalModel model);

        Task<BaseResultModel> AddGroupFinancialGoalAsync(AddGroupFinancialGoalModel model);
        Task<BaseResultModel> GetGroupFinancialGoalsAsync(GetGroupFinancialGoalsModel model);
        Task<BaseResultModel> GetGroupFinancialGoalByIdAsync(GetGroupFinancialGoalDetailModel model);
        Task<BaseResultModel> UpdateGroupFinancialGoalAsync(UpdateGroupFinancialGoalModel model);
        Task<BaseResultModel> DeleteGroupFinancialGoalAsync(DeleteFinancialGoalModel model);
    }
}
