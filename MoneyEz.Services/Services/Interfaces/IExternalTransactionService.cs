using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Services.BusinessModels.ChatModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.WebhookModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IExternalTransactionService
    {
        Task<BaseResultModel> UpdateTransactionWebhook(WebhookPayload webhookPayload);
        Task<BaseResultModel> CreateTransactionPythonService(CreateTransactionPythonModel createTransactionPythonModel);
        Task<BaseResultModel> CreateTransactionPythonServiceV2(CreateTransactionPythonModelV2 createTransactionPythonModel);
        Task<BaseResultModel> GetTransactionHistorySendToPythons(Guid userId, TransactionFilter transactionFilter);
    }
}
