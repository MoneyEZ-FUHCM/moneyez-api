using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.Subscription;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface ISubscriptionService
    {
        Task<BaseResultModel> CreateSubscriptionAsync(CreateSubscriptionModel model);
    }
}
