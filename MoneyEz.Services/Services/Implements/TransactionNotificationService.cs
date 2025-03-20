using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Implements
{
    public class TransactionNotificationService : ITransactionNotificationService
    {
        private readonly INotificationService _notificationService;

        public TransactionNotificationService(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task NotifyBudgetExceededAsync(User user, Category category, decimal exceededAmount, TransactionType type)
        {
            string title = type == TransactionType.EXPENSE
                ? "Chi tiêu vượt hạn mức!"
                : "Thu nhập vượt kế hoạch!";

            string message = type == TransactionType.EXPENSE
                ? $"Bạn đã chi vượt ngân sách cho danh mục '{category.Name}' thêm {exceededAmount:N0} VNĐ trong kỳ hiện tại."
                : $"Bạn đã tiết kiệm thêm được {exceededAmount:N0} VNĐ cho danh mục '{category.Name}'. Tuyệt vời!";

            var notification = new Notification
            {
                UserId = user.Id,
                Title = title,
                Message = message,
                EntityId = user.Id,
                Type = NotificationType.USER,
                CreatedDate = CommonUtils.GetCurrentTime()
            };

            await _notificationService.AddNotificationByUserId(user.Id, notification);
        }

        public async Task NotifyGoalAchievedAsync(User user, FinancialGoal goal)
        {
            var notification = new Notification
            {
                UserId = user.Id,
                Title = "Chúc mừng hoàn thành mục tiêu!",
                Message = $"Bạn đã hoàn thành mục tiêu: '{goal.Name}' với số tiền tích lũy {goal.CurrentAmount:N0} VNĐ.",
                EntityId = user.Id,
                Type = NotificationType.USER,
                CreatedDate = CommonUtils.GetCurrentTime()
            };
            await _notificationService.AddNotificationByUserId(user.Id, notification);
        }

        public async Task NotifyGoalProgressTrackingAsync(User user, FinancialGoal goal)
        {
            decimal remaining = goal.TargetAmount - goal.CurrentAmount;
            var notification = new Notification
            {
                UserId = user.Id,
                Title = "Tiến độ mục tiêu tài chính",
                Message = $"Mục tiêu '{goal.Name}' còn thiếu {remaining:N0} VNĐ để hoàn thành.",
                EntityId = user.Id,
                Type = NotificationType.USER,
                CreatedDate = CommonUtils.GetCurrentTime()
            };
            await _notificationService.AddNotificationByUserId(user.Id, notification);
        }
    }

}
