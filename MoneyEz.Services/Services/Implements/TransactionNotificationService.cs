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


        public async Task NotifyTransactionApprovalRequestAsync(GroupFund group, Transaction transaction, User requester)
        {
            var leader = group.GroupMembers.FirstOrDefault(m => m.Role == RoleGroup.LEADER);
            if (leader != null)
            {
                await _notificationService.AddNotificationByUserId(leader.UserId, new Notification
                {
                    Title = "Yêu cầu phê duyệt giao dịch",
                    Message = $"Thành viên {requester.FullName} vừa tạo giao dịch cần phê duyệt: {transaction.Description}.",
                    Type = NotificationType.GROUP,
                    EntityId = transaction.Id
                });
            }
        }

        public async Task NotifyTransactionCreatedAsync(GroupFund group, Transaction transaction, User creator)
        {
            foreach (var member in group.GroupMembers)
            {
                await _notificationService.AddNotificationByUserId(member.UserId, new Notification
                {
                    Title = "Giao dịch mới",
                    Message = $"Giao dịch '{transaction.Description}' đã được tạo bởi {creator.FullName}.",
                    Type = NotificationType.GROUP,
                    EntityId = transaction.Id
                });
            }
        }

        public async Task NotifyGoalCompletedAsync(FinancialGoal goal)
        {
            await _notificationService.AddNotificationByUserId(goal.UserId, new Notification
            {
                Title = "Hoàn thành mục tiêu tài chính!",
                Message = $"Mục tiêu '{goal.Name}' đã đạt được!",
                Type = NotificationType.USER,
                EntityId = goal.Id
            });
        }
    }

}
