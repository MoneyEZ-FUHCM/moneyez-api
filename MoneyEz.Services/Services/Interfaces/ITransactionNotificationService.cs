using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface ITransactionNotificationService
    {
        Task NotifyBudgetExceededAsync(User user, Category category, decimal exceededAmount, TransactionType type);
        Task NotifyGoalAchievedAsync(User user, FinancialGoal goal);
        Task NotifyGoalCompletedAsync(FinancialGoal activeGoal);
        Task NotifyGoalProgressTrackingAsync(User user, FinancialGoal goal);
        Task NotifyTransactionApprovalRequestAsync(GroupFund group, Transaction transaction, User user);
        Task NotifyTransactionCreatedAsync(GroupFund group, Transaction transaction, User user);
    }

}
