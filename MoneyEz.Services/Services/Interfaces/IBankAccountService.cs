using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels;
using MoneyEz.Services.BusinessModels.BankAccountModels;
using MoneyEz.Services.BusinessModels.ResultModels;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IBankAccountService
    {
        Task<BaseResultModel> GetAllBankAccountsPaginationAsync(PaginationParameter paginationParameter);
        Task<BaseResultModel> GetBankAccountByIdAsync(Guid id);
        Task<BaseResultModel> GetBankAccountsByUserAsync(PaginationParameter paginationParameter);
        Task<BaseResultModel> CreateBankAccountAsync(CreateBankAccountModel model);
        Task<BaseResultModel> UpdateBankAccountAsync(UpdateBankAccountModel model);
        Task<BaseResultModel> DeleteBankAccountAsync(Guid id);
    }
}
