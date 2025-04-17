using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.GroupMember;
using MoneyEz.Services.Services.Interfaces;
using AutoMapper;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Services.BusinessModels.ResultModels;
using Microsoft.AspNetCore.Http;
using MoneyEz.Services.Constants;
using MoneyEz.Services.BusinessModels.GroupFund.GroupInvite;
using MoneyEz.Services.BusinessModels.GroupFund;
using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.BusinessModels.EmailModels;
using MoneyEz.Services.Utils.Email;
using MoneyEz.Services.Utils;
using System.Web;

namespace MoneyEz.Services.Services.Implements
{
    public class GroupMemberService : IGroupMemberService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;
        private readonly IRedisService _redisService;
        private readonly IMailService _mailService;
        private readonly INotificationService _notificationService;

        public GroupMemberService(IMapper mapper, 
            IUnitOfWork unitOfWork,
            IClaimsService claimsService,
            IRedisService redisService,
            IMailService mailService,
            INotificationService notificationService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
            _redisService = redisService;
            _mailService = mailService;
            _notificationService = notificationService;
        }

        public async Task<BaseResultModel> AcceptInvitationEmailAsync(string token)
        {
            // Retrieve the invitation token from Redis
            var groupInviteRedisModel = await _redisService.GetAsync<GroupInviteRedisModel>(token);
            if (groupInviteRedisModel == null || groupInviteRedisModel.UserId == Guid.Empty)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    Message = MessageConstants.INVALID_INVITATION_TOKEN_MESSAGE
                };
            }

            // Retrieve the group fund by its Id
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(groupInviteRedisModel.GroupId, include: query => query.Include(c => c.GroupMembers));
            if (groupFund == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_NOT_EXIST
                };
            }

            // Retrieve the user by email
            var userInvite = await _unitOfWork.UsersRepository.GetByIdAsync(groupInviteRedisModel.UserId);
            if (userInvite == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.ACCOUNT_NOT_EXIST
                };
            }

            // check member is exist
            var memberExist = groupFund.GroupMembers.FirstOrDefault(member => member.UserId == userInvite.Id);
            if (memberExist != null && memberExist.Status == GroupMemberStatus.ACTIVE)
            {
                throw new DefaultException("", MessageConstants.GROUP_MEMBER_EXIST);
            }
            else if (memberExist != null && memberExist.Status != GroupMemberStatus.ACTIVE)
            {
                memberExist.IsDeleted = false;
                memberExist.Status = GroupMemberStatus.ACTIVE;
                memberExist.UpdatedDate = CommonUtils.GetCurrentTime();

                // add log
                groupFund.GroupFundLogs.Add(new GroupFundLog
                {
                    ChangedBy = userInvite.FullName,
                    ChangeDescription = $"đã tham gia nhóm qua email",
                    Action = GroupAction.JOINED.ToString(),
                    CreatedDate = CommonUtils.GetCurrentTime(),
                    CreatedBy = userInvite.Email
                });

                // Save the changes to the repository
                groupFund.UpdatedBy = userInvite.Email;
                _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
                await _unitOfWork.SaveAsync();

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = MessageConstants.GROUP_INVITATION_ACCEPT_SUCCESS_MESSAGE
                };
            }
            else
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_MEMBER_NOT_FOUND
                };
            }
        }

        public async Task<BaseResultModel> AcceptInvitationQRCodeAsync(string token)
        {
            // Retrieve the invitation token from Redis
            var groupInviteRedisModel = await _redisService.GetAsync<GroupInviteRedisModel>(token);
            if (groupInviteRedisModel == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    Message = MessageConstants.INVALID_INVITATION_TOKEN_MESSAGE
                };
            }

            // Retrieve the group fund by its Id
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(groupInviteRedisModel.GroupId, include: query => query.Include(c => c.GroupMembers));
            if (groupFund == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_NOT_EXIST
                };
            }

            // get current user
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);

            // check member is exist
            var memberExist = groupFund.GroupMembers.FirstOrDefault(member => member.UserId == currentUser.Id);
            if (memberExist != null && memberExist.Status == GroupMemberStatus.ACTIVE)
            {
                throw new DefaultException("", MessageConstants.GROUP_MEMBER_EXIST);
            }
            else if (memberExist != null && memberExist.Status != GroupMemberStatus.ACTIVE)
            {
                memberExist.IsDeleted = false;
                memberExist.Status = GroupMemberStatus.ACTIVE;
                memberExist.UpdatedDate = CommonUtils.GetCurrentTime();

                // Add a log entry for the invite member action
                groupFund.GroupFundLogs.Add(new GroupFundLog
                {
                    ChangedBy = currentUser.FullName,
                    ChangeDescription = $"đã tham gia nhóm qua liên kết mời",
                    Action = GroupAction.JOINED.ToString(),
                    CreatedDate = CommonUtils.GetCurrentTime(),
                    CreatedBy = currentUser.Email
                });

                // Save the changes to the repository
                _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
                await _unitOfWork.SaveAsync();

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = MessageConstants.GROUP_INVITATION_ACCEPT_SUCCESS_MESSAGE
                };
            }

            // Add the member to the group
            var newMember = new GroupMember
            {
                UserId = currentUser.Id,
                ContributionPercentage = 0,
                Role = RoleGroup.MEMBER,
                Status = GroupMemberStatus.ACTIVE,
                CreatedDate = CommonUtils.GetCurrentTime()
            };

            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangedBy = currentUser.FullName,
                ChangeDescription = $"đã tham gia nhóm qua liên kết mời",
                Action = GroupAction.JOINED.ToString(),
                CreatedDate = CommonUtils.GetCurrentTime(),
                CreatedBy = currentUser.Email
            });

            groupFund.GroupMembers.Add(newMember);

            // Save the changes to the repository
            groupFund.UpdatedBy = currentUser.Email;
            _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.GROUP_INVITATION_ACCEPT_SUCCESS_MESSAGE,
                Data = new
                {
                    GroupId = groupFund.Id
                }
            };
        }

        public async Task<BaseResultModel> InviteMemberEmailAsync(InviteMemberModel inviteMemberModel)
        {
            // Retrieve the group fund by its Id
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(inviteMemberModel.GroupId, include: query => query.Include(c => c.GroupMembers));

            if (groupFund == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_NOT_EXIST
                };
            }

            // Check if the current user is the leader of the group
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            var isLeader = groupFund.GroupMembers.Any(member => member.UserId == currentUser.Id && member.Role == RoleGroup.LEADER);

            if (!isLeader)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    Message = MessageConstants.GROUP_INVITE_FORBIDDEN_MESSAGE
                };
            }

            var users = await _unitOfWork.UsersRepository.GetAllAsync();
            var emailSet = new HashSet<string>(inviteMemberModel.Emails);

            var invitedUsers = users.Where(u => emailSet.Contains(u.Email)).ToList();

            if (invitedUsers.Count != emailSet.Count)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // check list member
            foreach (var invitedUser in invitedUsers)
            {
                // check member is exist in group
                var memberExist = groupFund.GroupMembers.FirstOrDefault(member => member.UserId == invitedUser.Id);
                if (memberExist != null && memberExist.Status == GroupMemberStatus.ACTIVE)
                {
                    throw new DefaultException("", MessageConstants.GROUP_MEMBER_EXIST);
                }
                else if (memberExist == null)
                {
                    // Add the member to the group with a pending status
                    var pendingMember = new GroupMember
                    {
                        UserId = invitedUser.Id,
                        ContributionPercentage = 0,
                        Role = RoleGroup.MEMBER,
                        Status = GroupMemberStatus.PENDING,
                        CreatedDate = CommonUtils.GetCurrentTime(),
                        CreatedBy = currentUser.Email,
                    };
                    groupFund.GroupMembers.Add(pendingMember);
                }

                // Generate a raw invitation token
                var rawToken = Guid.NewGuid().ToString();

                // hash token use hmac sha256
                var hashedToken = StringUtils.HashToken(rawToken);

                var invitationLink = $"https://easymoney.anttravel.online/moneyez-web/accept-invitation?token={HttpUtility.UrlEncode(hashedToken)}";
                //var invitationLink = $"http://localhost:3000/moneyez-web/accept-invitation?token={HttpUtility.UrlEncode(invitationToken)}";

                // Save the invitation token to Redis
                var redisKey = hashedToken;
                var groupInviteRedisModel = new GroupInviteRedisModel
                {
                    GroupId = inviteMemberModel.GroupId,
                    UserId = invitedUser.Id
                };
                await _redisService.SetAsync(redisKey, groupInviteRedisModel, TimeSpan.FromDays(1));

                string defaultDescription = $"Bạn đã được '{currentUser.FullName}' mời vào nhóm '{groupFund.Name}'. Ấn vào link để tham gia: {invitationLink}";

                string emailBody = SendInviteGroupMember
                    .EmailSendInviteGroupMember(currentUser.FullName, invitedUser.FullName, groupFund.Name,
                        string.IsNullOrEmpty(inviteMemberModel.Description) ? defaultDescription : inviteMemberModel.Description, invitationLink);

                // send mail
                MailRequest newEmail = new MailRequest()
                {
                    ToEmail = invitedUser.Email,
                    Subject = $"[MoneyEz] Lời mời tham gia nhóm {groupFund.Name}",
                    Body = emailBody,
                };

                // send mail
                await _mailService.SendEmailAsync_v2(newEmail);

                // Add a log entry for the invite member action
                groupFund.GroupFundLogs.Add(new GroupFundLog
                {
                    ChangedBy = currentUser.FullName,
                    ChangeDescription = $"đã mời {invitedUser.FullName} vào nhóm qua email",
                    Action = GroupAction.INVITED.ToString(),
                    CreatedDate = CommonUtils.GetCurrentTime(),
                    CreatedBy = currentUser.Email
                });

                // Save the changes to the repository
                groupFund.UpdatedBy = currentUser.Email;
                _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);

                await _unitOfWork.SaveAsync();

                // send notification to member
                var newNotification = new Notification
                {
                    UserId = invitedUser.Id,
                    Title = $"Lời mời tham gia nhóm [{groupFund.Name}]",
                    Message = string.IsNullOrEmpty(inviteMemberModel.Description) ?
                        $"Bạn đã được '{currentUser.FullName}' mời vào nhóm '{groupFund.Name}'. Tham gia ngay!"
                        : $"{inviteMemberModel.Description}",
                    EntityId = groupFund.Id,
                    Href = invitationLink,
                    Type = NotificationType.GROUP_INVITE,
                    CreatedDate = CommonUtils.GetCurrentTime(),
                    CreatedBy = currentUser.Email
                };

                await _notificationService.AddNotificationByUserId(invitedUser.Id, newNotification);
            }

            // Return a success result
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.GROUP_INVITE_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> InviteMemberQRCodeAsync(InviteMemberModel inviteMemberModel)
        {
            // Retrieve the group fund by its Id
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(inviteMemberModel.GroupId, include: query => query.Include(c => c.GroupMembers));

            if (groupFund == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_NOT_EXIST
                };
            }

            // Check if the current user is the leader of the group
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            var isLeader = groupFund.GroupMembers.Any(member => member.UserId == currentUser.Id && member.Role == RoleGroup.LEADER);

            if (!isLeader)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    Message = MessageConstants.GROUP_INVITE_FORBIDDEN_MESSAGE
                };
            }

            // generate inivitation code
            var invitationCode = "";
            while (true)
            {
                invitationCode = StringUtils.GenerateInviteCode();
                var existingCode = await _redisService.GetAsync<GroupInviteRedisModel>(invitationCode);
                if (existingCode == null)
                {
                    break;
                }
            }

            // Save the invitation token to Redis
            var redisKey = invitationCode;
            var groupInviteRedisModel = new GroupInviteRedisModel
            {
                InviteToken = invitationCode,
                GroupId = inviteMemberModel.GroupId
            };
            await _redisService.SetAsync(redisKey, groupInviteRedisModel, TimeSpan.FromMinutes(10));

            // Add a log entry for the invite member action
            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangedBy = currentUser.FullName,
                ChangeDescription = $"đã tạo liên kết mời vào nhóm",
                Action = GroupAction.INVITED.ToString(),
                CreatedDate = CommonUtils.GetCurrentTime(),
                CreatedBy = currentUser.Email
            });

            // Save the changes to the repository
            groupFund.UpdatedBy = currentUser.Email;
            _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = new QRCodeInviteModel
                {
                    QRCode = invitationCode,
                    ExpiredTime = CommonUtils.GetCurrentTime().AddMinutes(10)
                },
                Message = "Đã tạo mã QRCode mời vào nhóm. Mã có hiệu lực trong 10 phút"
            };
        }

        public async Task<BaseResultModel> LeaveGroupAsync(Guid groupId)
        {
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (currentUser == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // Retrieve the group fund by its Id
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(groupId, include: query => query.Include(g => g.GroupMembers));
            if (groupFund == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_NOT_EXIST
                };
            }

            // Check if the current user is the leader of the group
            var isLeader = groupFund.GroupMembers.Any(member => member.UserId == currentUser.Id && member.Role == RoleGroup.LEADER);

            if (isLeader)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.GROUP_CAN_NOT_REMOVE_LEADER,
                    Message = "Cannot remove yourself from the group"
                };
            }

            // Find the member to be removed
            var memberToRemove = groupFund.GroupMembers
                .FirstOrDefault(member => member.UserId == currentUser.Id && member.Status == GroupMemberStatus.ACTIVE);
            if (memberToRemove == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_MEMBER_NOT_FOUND);
            }

            // kiểm tra % contribution đối với group có goal
            var groupGoal = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: g => g.GroupId == groupId && g.Status == FinancialGoalStatus.ACTIVE
            );

            if (groupGoal.Any())
            {
                if (memberToRemove.ContributionPercentage > 0)
                {
                    throw new DefaultException(MessageConstants.GROUP_MEMBER_HAVE_CONTRIBUTION_MESSAGE,
                        MessageConstants.GROUP_MEMBER_HAVE_CONTRIBUTION);
                }
            }

            // add log
            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangedBy = currentUser.FullName,
                ChangeDescription = $"{currentUser.FullName} đã rời khỏi nhóm",
                Action = GroupAction.LEFT.ToString(),
                CreatedDate = CommonUtils.GetCurrentTime(),
                CreatedBy = currentUser.Email
            });

            memberToRemove.Status = GroupMemberStatus.INACTIVE;
            memberToRemove.UpdatedBy = currentUser.Email;
            _unitOfWork.GroupMemberRepository.SoftDeleteAsync(memberToRemove);

            // Save the changes to the repository
            await _unitOfWork.SaveAsync();

            // Return a success result
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.GROUP_LEAVE_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> RemoveMemberByLeaderAsync(Guid groupId, Guid memberId)
        {
            // BR: leader chỉ có thể xóa thành viên nếu thành viên chưa đóng góp vào nhóm (chưa có transaction)
            // Retrieve the group fund by its Id
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(groupId, include: query => query.Include(g => g.GroupMembers));
            if (groupFund == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_NOT_EXIST
                };
            }

            // Check if the current user is the leader of the group
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            var leader = groupFund.GroupMembers.FirstOrDefault(member => member.UserId == currentUser.Id && member.Role == RoleGroup.LEADER);

            if (leader == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.GROUP_REMOVE_MEMBER_FORBIDDEN
                };
            }

            if (leader.UserId == memberId)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    ErrorCode = MessageConstants.GROUP_CAN_NOT_REMOVE_LEADER,
                    Message = "Cannot remove yourself from the group"
                };
            }


            // Find the member to be removed
            var memberToRemove = groupFund.GroupMembers
                .FirstOrDefault(member => member.UserId == memberId && member.Status == GroupMemberStatus.ACTIVE);
            if (memberToRemove == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_MEMBER_NOT_FOUND);
            }

            // kiểm tra % contribution đối với group có goal
            var groupGoal = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: g => g.GroupId == groupId && g.Status == FinancialGoalStatus.ACTIVE
            );

            if (groupGoal.Any())
            {
                if (memberToRemove.ContributionPercentage > 0)
                {
                    throw new DefaultException(MessageConstants.GROUP_MEMBER_HAVE_CONTRIBUTION_MESSAGE,
                        MessageConstants.GROUP_MEMBER_HAVE_CONTRIBUTION);
                }
            }

            // get remove member info
            var removeMember = await _unitOfWork.UsersRepository.GetByIdAsync(memberId);
            if (removeMember == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            if (memberToRemove.Status == GroupMemberStatus.PENDING)
            {
                _unitOfWork.GroupMemberRepository.PermanentDeletedAsync(memberToRemove);
                await _unitOfWork.SaveAsync();

                // Return a success result
                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = MessageConstants.GROUP_REMOVE_MEMBER_SUCCESS_MESSAGE
                };
            }
            else
            {
                // kiểm tra xem thành viên đã có giao dịch nào chưa
                var transactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                    filter: t => t.GroupId == groupId && t.UserId == memberToRemove.UserId && t.Status == TransactionStatus.APPROVED
                );
                if (transactions.Any())
                {
                    throw new DefaultException(MessageConstants.GROUP_MEMBER_HAVE_TRANSACTION_MESSAGE, MessageConstants.GROUP_MEMBER_HAVE_TRANSACTION);
                }

                groupFund.GroupFundLogs.Add(new GroupFundLog
                {
                    ChangedBy = currentUser.FullName,
                    ChangeDescription = $"đã xóa {removeMember.FullName} khỏi nhóm",
                    Action = GroupAction.UPDATED.ToString(),
                    CreatedDate = CommonUtils.GetCurrentTime(),
                    CreatedBy = currentUser.Email
                });

                memberToRemove.Status = GroupMemberStatus.INACTIVE;
                memberToRemove.UpdatedBy = currentUser.Email;
                _unitOfWork.GroupMemberRepository.SoftDeleteAsync(memberToRemove);

                await _unitOfWork.SaveAsync();

                // send notification to member
                var newNotification = new Notification
                {
                    UserId = memberId,
                    Title = $"Rời khỏi nhóm {groupFund.Name}",
                    Message = $"Bạn đã bị {currentUser.FullName} xóa khỏi nhóm '{groupFund.Name}'. Ấn vào thông báo để xem chi tiết.",
                    EntityId = groupFund.Id,
                    Type = NotificationType.GROUP,
                };

                await _notificationService.AddNotificationByUserId(memberId, newNotification);

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Message = MessageConstants.GROUP_REMOVE_MEMBER_SUCCESS_MESSAGE
                };
            }
        }

        /// <summary>
        /// cập nhật tỉ lệ đóng góp của các thành viên trong nhóm (chỉ leader mới có quyền cập nhật)
        /// 
        public async Task<BaseResultModel> SetGroupContribution(SetGroupContributionModel setGroupContributionModel)
        {
            // Retrieve the group fund by its Id including members
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(setGroupContributionModel.GroupId, include: query => query.Include(g => g.GroupMembers));
            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            // Check if the current user exists and is the leader of the group
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            var isLeader = groupFund.GroupMembers.Any(member => member.UserId == currentUser.Id && member.Role == RoleGroup.LEADER);

            if (!isLeader)
            {
                throw new DefaultException("Only group leader can set contribution percentages",
                    MessageConstants.GROUP_SET_CONTRIBUTION_FORBIDDEN);
            }

            // Validate total contribution equals 100%
            var totalContribution = setGroupContributionModel.MemberContributions.Sum(x => x.Contribution);
            if (totalContribution != 100)
            {
                throw new DefaultException("Total contribution percentage must equal 100%",
                    MessageConstants.GROUP_INVALID_TOTAL_CONTRIBUTION);
            }

            // Update contribution percentages for each member
            foreach (var memberContribution in setGroupContributionModel.MemberContributions)
            {
                var groupMember = groupFund.GroupMembers.FirstOrDefault(m =>
                    m.UserId == memberContribution.MemberId &&
                    m.Status == GroupMemberStatus.ACTIVE);

                if (groupMember == null)
                {
                    throw new NotExistException($"Member with ID {memberContribution.MemberId} not found in group",
                        MessageConstants.GROUP_MEMBER_CONTRIBUTION_NOT_FOUND);
                }

                groupMember.ContributionPercentage = memberContribution.Contribution;
                groupMember.UpdatedDate = CommonUtils.GetCurrentTime();
            }

            // Add log entry for contribution update
            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangedBy = currentUser.FullName,
                ChangeDescription = $"đã cập nhật tỷ lệ đóng góp cho các thành viên",
                Action = GroupAction.UPDATED.ToString(),
                CreatedDate = CommonUtils.GetCurrentTime(),
                CreatedBy = currentUser.Email
            });

            groupFund.UpdatedBy = currentUser.Email;
            _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
            await _unitOfWork.SaveAsync();

            // send notification to members

            var newNotification = new Notification
            {
                Title = $"Cập nhật tỷ lệ đóng góp nhóm {groupFund.Name}",
                Message = $"Tỷ lệ đóng góp của bạn trong nhóm '{groupFund.Name}' đã được cập nhật. Ấn vào thông báo để xem chi tiết.",
                EntityId = groupFund.Id,
                Type = NotificationType.GROUP,
            };

            var listUserId = groupFund.GroupMembers.Select(x => x.UserId).ToList();

            await _notificationService.AddNotificationByListUser(listUserId, newNotification);


            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.GROUP_SET_CONTRIBUTION_SUCCESS_MESSAGE
            };
        }

        /// <summary>
        /// cập nhật vai trò trong nhóm (chỉ leader mới có quyền cập nhật)
        /// 
        public async Task<BaseResultModel> SetMemberRoleAsync(SetRoleGroupModel setRoleGroupModel)
        {
            // Retrieve the group fund by its Id
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(setRoleGroupModel.GroupId, include: query => query.Include(g => g.GroupMembers));
            if (groupFund == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_NOT_EXIST
                };
            }

            // Check if the current user is the leader of the group
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            var isLeader = groupFund.GroupMembers.Any(member => member.UserId == currentUser.Id && member.Role == RoleGroup.LEADER);

            if (!isLeader)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    Message = MessageConstants.GROUP_SET_ROLE_FORBIDDEN
                };
            }

            // Find the member whose role is to be changed
            var memberToUpdate = groupFund.GroupMembers.FirstOrDefault(member => member.UserId == setRoleGroupModel.MemberId);
            if (memberToUpdate == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_MEMBER_NOT_FOUND
                };
            }

            if (memberToUpdate.UserId == currentUser.Id)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status403Forbidden,
                    Message = MessageConstants.GROUP_SET_ROLE_FORBIDDEN
                };
            }

            if (memberToUpdate.Role == setRoleGroupModel.RoleGroup)
            {
                throw new DefaultException($"Member already '{setRoleGroupModel.RoleGroup.ToString()}' on group",
                    MessageConstants.GROUP_MEMBER_ALREADY_ROLE);
            }

            memberToUpdate.Role = setRoleGroupModel.RoleGroup;

            var memberUpdateInfo = await _unitOfWork.UsersRepository.GetByIdAsync(memberToUpdate.UserId);

            groupFund.GroupFundLogs.Add(new GroupFundLog
            {
                ChangedBy = currentUser.FullName,
                ChangeDescription = $"đã thay đổi vai trò của " +
                    $"{memberUpdateInfo.FullName} thành {setRoleGroupModel.RoleGroup.ToString()}",
                Action = GroupAction.UPDATED.ToString(),
                CreatedDate = CommonUtils.GetCurrentTime(),
                CreatedBy = currentUser.Email
            });

            groupFund.UpdatedBy = currentUser.Email;

            _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
            await _unitOfWork.SaveAsync();

            // send notification to member
            var newNotification = new Notification
            {
                UserId = memberToUpdate.UserId,
                Title = $"Thay đổi vai trò trong nhóm {groupFund.Name}",
                Message = $"Vai trò của bạn trong nhóm '{groupFund.Name}' đã được thay đổi thành '{setRoleGroupModel.RoleGroup.ToString()}'. Ấn vào thông báo để xem chi tiết.",
                EntityId = groupFund.Id,
                Type = NotificationType.GROUP,
            };

            await _notificationService.AddNotificationByUserId(memberToUpdate.UserId, newNotification);

            // Return a success result
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.MEMBER_ROLE_UPDATE_SUCCESS_MESSAGE
            };
        }
    }
}