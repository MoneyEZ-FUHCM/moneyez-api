using AutoMapper;
using Microsoft.AspNetCore.Http;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Services.BusinessModels.NotificationModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.BusinessModels.UserModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Implements
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimsService _claimsService;

        public NotificationService(IUnitOfWork unitOfWork, IMapper mapper, IClaimsService claimsService) 
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsService = claimsService;
        }

        public async Task<BaseResultModel> AddNotificationByUserId(Guid userId, Notification notification)
        {
            var user = await _unitOfWork.UsersRepository.GetByIdAsync(userId);

            if (user != null)
            {
                if (notification != null)
                {
                    notification.UserId = user.Id;
                    await PushMessageFirebase(notification.Title, notification.Message, user.Id);
                    var newNotification = await _unitOfWork.NotificationRepository.AddAsync(notification);
                    await _unitOfWork.SaveAsync();
                    if (newNotification != null)
                    {
                        return new BaseResultModel
                        {
                            Status = StatusCodes.Status200OK,
                            Data = _mapper.Map<NotificationModel>(newNotification),
                            Message = "Push notification to user successfully"
                        };
                    }
                }
            }
            throw new DefaultException("Can not push notification to user", "");
        }

        public async Task<BaseResultModel> AddNotificationByListUser(List<Guid> userIds, Notification notification)
        {
            var users = await _unitOfWork.UsersRepository.GetUsersByUserIdsAsync(userIds);
            if (users.Any())
            {
                List<Notification> notificationList = new List<Notification>();
                foreach (Guid userId in userIds)
                {
                    var newNoti = new Notification
                    {
                        UserId = userId,
                        Type = NotificationType.SYSTEM,
                        Title = notification.Title,
                        Message = notification.Message,
                    };
                    notificationList.Add(newNoti);
                }
                await _unitOfWork.NotificationRepository.AddRangeAsync(notificationList);
                await _unitOfWork.SaveAsync();

                // push notification
                var userTokens = users.Where(x => x.DeviceToken != null).Select(x => x.DeviceToken).ToList();
                await PushListMessageFirebase(notification.Title, notification.Message, userTokens);

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = _mapper.Map<NotificationModel>(notification),
                    Message = "Push notification to list users successfully"
                };
            }
            throw new DefaultException("Can not push notification to list users", "");
        }

        public Task<BaseResultModel> AddNotificationByRoleAsync(RolesEnum roleEnums, Notification notificationModel)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResultModel> AddNotificationByUserId(int userId, Notification notificationModel)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResultModel> GetNotificationById(Guid id)
        {
            var noti = await _unitOfWork.NotificationRepository.GetByIdAsync(id);
            if (noti == null)
            {
                throw new NotExistException("", MessageConstants.NOTI_NOT_EXIST);
            }
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = _mapper.Map<NotificationModel>(noti)
            };
        }

        public async Task<BaseResultModel> GetNotificationsByUser(PaginationParameter paginationParameter)
        {
            var currentEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(currentEmail);
            if (user == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            var notiList = await _unitOfWork.NotificationRepository.ToPagination(paginationParameter);
            var notiListModels = _mapper.Map<List<UserModel>>(notiList);

            var notifications = new Pagination<UserModel>(notiListModels,
                notiList.TotalCount,
                notiList.CurrentPage,
                notiList.PageSize);

            var metaData = new
            {
                notiList.TotalCount,
                notiList.PageSize,
                notiList.CurrentPage,
                notiList.TotalPages,
                notiList.HasNext,
                notiList.HasPrevious
            };

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = new ModelPaging
                {
                    Data = notifications,
                    MetaData = metaData
                }
            };
        }

        public async Task<BaseResultModel> MarkAllUserNotificationIsReadAsync(string email)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            var notifications = await _unitOfWork.NotificationRepository.GetAllNotificationsByUserIdAsync(user.Id);
            if (!notifications.Any())
            {
                throw new DefaultException("", MessageConstants.NOTI_UNREAD_EMPTY);
            }

            var unreadNotifications = notifications.Where(n => n.IsRead == false);
            List<Notification> updateNotification = new List<Notification>();

            // mark read notification
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                updateNotification.Add(notification);
            }

            _unitOfWork.NotificationRepository.UpdateRangeAsync(updateNotification);
            _unitOfWork.Save();

            return new BaseResultModel 
            { 
                Status = StatusCodes.Status200OK, 
                Message = "Mark all read notifications successfully" 
            };
        }

        public async Task<BaseResultModel> MarkNotificationIsReadById(Guid notificationId)
        {
            var notification = await _unitOfWork.NotificationRepository.GetByIdAsync(notificationId);
            if (notification != null)
            {
                if (notification.IsRead)
                {
                    throw new DefaultException("", MessageConstants.NOTI_CANNOT_MARK_READ);
                }

                notification.IsRead = true;
                _unitOfWork.NotificationRepository.UpdateAsync(notification);
                _unitOfWork.Save();

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Mark read notification successfully"
                };
            }
            throw new NotExistException("", MessageConstants.NOTI_NOT_EXIST);
        }

        public async Task<bool> PushListMessageFirebase(string title, string body, List<string> fcmTokens)
        {
            if (fcmTokens.Any())
            {
                await FirebaseLibrary.SendRangeMessageFireBase(title, body, fcmTokens);
                return true;
            }
            return false;
        }

        public async Task<bool> PushMessageFirebase(string title, string body, Guid userId)
        {
            var user = await _unitOfWork.UsersRepository.GetByIdAsync(userId);
            if (user != null)
            {
                var fcmToken = user.DeviceToken;
                if (fcmToken != null)
                {
                    await FirebaseLibrary.SendMessageFireBase(title, body, fcmToken);
                    return true;
                }
                return false;
            }
            throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
        }
    }
}
