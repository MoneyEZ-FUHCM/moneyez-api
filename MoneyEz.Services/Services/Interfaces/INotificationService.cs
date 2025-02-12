using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.ResultModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface INotificationService
    {
        public Task<BaseResultModel> AddNotificationByUserId(Guid userId, Notification notificationModel);

        public Task<BaseResultModel> AddNotificationByRoleAsync(RolesEnum roleEnums, Notification notificationModel);

        public Task<BaseResultModel> AddNotificationByListUser(List<Guid> userIds, Notification notificationModel);

        public Task<BaseResultModel> GetNotificationById(Guid id);

        public Task<BaseResultModel> GetNotificationsByUser(PaginationParameter paginationParameter);

        public Task<bool> PushMessageFirebase(string title, string body, Guid userId);

        public Task<BaseResultModel> MarkAllUserNotificationIsReadAsync(string email);

        public Task<BaseResultModel> MarkNotificationIsReadById(Guid notificationId);

        public Task<bool> PushListMessageFirebase(string title, string body, List<string> fcmTokens);
    }
}
