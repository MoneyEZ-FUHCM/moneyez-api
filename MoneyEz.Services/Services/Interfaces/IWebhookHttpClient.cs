using MoneyEz.Services.BusinessModels.WebhookModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IWebhookHttpClient
    {
        public Task<HttpResponseMessage> RegisterWebhookAsync(WebhookRequestModel request);

        public Task<HttpResponseMessage> CancelWebhookAsync(string secret);

        public Task<HttpResponseMessage> ValidateBankAccount(ValidateBankAccountRequestModel request);
    }
}
