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
            string title = !category.IsSaving
                ? "⚠️ Cảnh báo: Chi tiêu vượt hạn mức"
                : "🎉 Chúc mừng: Tiết kiệm thành công";

            string message = !category.IsSaving
                ? $"Bạn đã vượt quá ngân sách cho danh mục '{category.Name}' với số tiền {exceededAmount:N0} VNĐ. Hãy cân nhắc điều chỉnh chi tiêu!"
                : $"Chúc mừng! Bạn đã tiết kiệm thêm được {exceededAmount:N0} VNĐ cho danh mục '{category.Name}'. Tiếp tục duy trì bạn nhé!";

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
            string message;
            if (goal.IsSaving)
            {
                message = $"Chúc mừng! Bạn đã đạt được mục tiêu tiết kiệm '{goal.Name}' với tổng số tiền {goal.CurrentAmount:N0} VNĐ. " +
                    $"Thành quả này thật tuyệt vời!";
            }
            else
            {
                message = $"Thông báo! Bạn đã đạt đến giới hạn ngân sách chi tiêu '{goal.Name}' với số tiền {goal.CurrentAmount:N0} VNĐ. " +
                    $"Hãy tiếp tục quản lý tài chính hiệu quả!";
            }

            var notification = new Notification
            {
                UserId = user.Id,
                Title = goal.IsSaving ? "🎉 Chúc mừng hoàn thành mục tiêu!" : "⚠️ Cảnh báo giới hạn ngân sách chi tiêu",
                Message = message,
                EntityId = user.Id,
                Type = NotificationType.FINANCIAL_GOAL_PERSONAL,
                CreatedDate = CommonUtils.GetCurrentTime()
            };

            await _notificationService.AddNotificationByUserId(user.Id, notification);
        }

        public async Task NotifyGoalProgressTrackingAsync(User user, FinancialGoal goal)
        {
            decimal remaining = goal.TargetAmount - goal.CurrentAmount;
            string message;

            if (remaining <= 0)
            {
                await NotifyGoalAchievedAsync(user, goal);
                return;
            }

            if (goal.IsSaving)
            {
                if (remaining < goal.TargetAmount * 0.1m)
                {
                    message = $"Bạn sắp đạt được mục tiêu tiết kiệm '{goal.Name}'! Chỉ còn {remaining:N0} VNĐ nữa thôi!";
                }
                else
                {
                    message = $"Bạn đã tiết kiệm được {goal.CurrentAmount:N0} VNĐ cho mục tiêu '{goal.Name}'. Hãy tiếp tục cố gắng!";
                }
            }
            else
            {
                if (remaining < goal.TargetAmount * 0.1m)
                {
                    message = $"Nhắc nhở! Chỉ còn {remaining:N0} VNĐ nữa là bạn sẽ đạt đến giới hạn ngân sách chi tiêu '{goal.Name}'.";
                }
                else
                {
                    message = $"Bạn đã sử dụng {goal.CurrentAmount:N0} VNĐ trong ngân sách chi tiêu '{goal.Name}'. Hãy tiếp tục quản lý tài chính thông minh!";
                }
            }

            var notification = new Notification
            {
                UserId = user.Id,
                Title = "📊 Cập nhật tiến độ mục tiêu",
                Message = message,
                EntityId = user.Id,
                Type = NotificationType.FINANCIAL_GOAL_PERSONAL,
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
