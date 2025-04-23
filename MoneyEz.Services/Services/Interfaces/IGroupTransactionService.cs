using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.GroupFund;
using MoneyEz.Services.BusinessModels.ResultModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IGroupTransactionService
    {
        Task<BaseResultModel> CreateFundraisingRequest(CreateFundraisingModel createFundraisingModel);
        Task<BaseResultModel> CreateFundWithdrawalRequest(CreateFundWithdrawalModel createFundWithdrawalModel);
        Task<BaseResultModel> RemindFundraisingAsync(RemindFundraisingModel remindFundraisingModel);
        Task<BaseResultModel> GetPendingRequestsAsync(Guid groupId, PaginationParameter paginationParameters);
        Task<BaseResultModel> GetPendingRequestDetailAsync(Guid requestId);
    }
}
