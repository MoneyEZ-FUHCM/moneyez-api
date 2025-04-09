using MoneyEz.Services.BusinessModels.ResultModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IWebhookService
    {
        public Task<BaseResultModel> RegisterWebhookAsync(Guid accountBankId, string serverUri);
        public Task<BaseResultModel> CancelWebhookAsync(Guid accountBankId, string serverUri);
    }
}
