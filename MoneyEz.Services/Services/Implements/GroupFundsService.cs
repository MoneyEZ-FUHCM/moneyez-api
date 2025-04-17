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
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.Utils;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.BusinessModels.GroupFund;
using System.Web;
using MoneyEz.Services.BusinessModels.EmailModels;
using MoneyEz.Services.Exceptions;
using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.GroupFund.GroupInvite;
using MoneyEz.Services.BusinessModels.BankAccountModels;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Services.BusinessModels.TransactionModels;
using MoneyEz.Services.BusinessModels.ImageModels;
using MoneyEz.Repositories.Commons.Filters;
using Microsoft.AspNetCore.Http.HttpResults;
using MoneyEz.Services.BusinessModels.GroupFundLogModels;
using MoneyEz.Services.BusinessModels.FinancialReportModels;
using MoneyEz.Services.BusinessModels.GroupMemLogModels;
using MoneyEz.Services.BusinessModels.TransactionModels.Group;
using MoneyEz.Services.Utils.Email;
using System.Text.RegularExpressions;
namespace MoneyEz.Services.Services.Implements
{
    public class GroupFundsService : IGroupFundsService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;

        public GroupFundsService(IMapper mapper,
            IUnitOfWork unitOfWork,
            IClaimsService claimsService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
        }

        public async Task<BaseResultModel> CreateGroupFundsAsync(CreateGroupModel model)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (user == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            var bankAccount = await _unitOfWork.BankAccountRepository.GetByIdAsync(model.AccountBankId);
            if (bankAccount == null)
            {
                throw new NotExistException("", MessageConstants.BANK_ACCOUNT_NOT_FOUND);
            }

            // Map the model to a new GroupFund entity and set its Id to the one generated for groupEntity
            var groupFund = _mapper.Map<GroupFund>(model);
            groupFund.NameUnsign = StringUtils.ConvertToUnSign(model.Name);
            groupFund.Status = GroupStatus.ACTIVE;
            groupFund.Visibility = VisibilityEnum.PRIVATE;
            groupFund.CreatedBy = user.Email;
            groupFund.AccountBankId = model.AccountBankId;

            groupFund.GroupMembers = new List<GroupMember>
            {
                new GroupMember
                {
                    UserId = user.Id,
                    ContributionPercentage = 100,
                    Role = RoleGroup.LEADER,
                    Status = GroupMemberStatus.ACTIVE,
                    CreatedDate = CommonUtils.GetCurrentTime(),
                    CreatedBy = user.Email,
                }
            };

            groupFund.GroupFundLogs = new List<GroupFundLog>
            {
                new GroupFundLog
                {
                    ChangedBy = user.FullName,
                    ChangeDescription = "đã tạo nhóm",
                    Action = GroupAction.CREATED.ToString(),
                    CreatedDate = CommonUtils.GetCurrentTime(),
                    CreatedBy = user.Email
                }
            };
            // Add the groupFund to the repository and save changes again
            await _unitOfWork.GroupFundRepository.AddAsync(groupFund);
            _unitOfWork.Save();

            // add image

            Image newImage = null;

            if (model.Image != null)
            {
                newImage = new Image
                {
                    EntityId = groupFund.Id,
                    EntityName = EntityName.GROUP.ToString(),
                    ImageUrl = model.Image,
                    CreatedBy = user.Email
                };

                await _unitOfWork.ImageRepository.AddAsync(newImage);
                _unitOfWork.Save();
            }

            var result = _mapper.Map<GroupFundModel>(groupFund);
            result.GroupMembers = new List<GroupMemberModel>();
            result.ImageUrl = newImage?.ImageUrl != null ? newImage.ImageUrl : null;

            // Return a success result with the created groupFund
            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Data = result,
                Message = MessageConstants.GROUP_CREATE_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> GetAllGroupFunds(PaginationParameter paginationParameters, GroupFilter filter)
        {
            // check current user
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (currentUser == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            if (currentUser.Role == RolesEnum.ADMIN)
            {
                // Get all groupFunds
                var groupFunds = await _unitOfWork.GroupFundRepository.GetGroupFundsFilterAsync(paginationParameters, filter);
                var groupFundModels = _mapper.Map<List<GroupFundModel>>(groupFunds);

                // Get and map images for each group fund
                var images = await _unitOfWork.ImageRepository.GetImagesByEntityNameAsync(EntityName.GROUP.ToString());
                foreach (var groupFund in groupFundModels)
                {
                    var image = images.FirstOrDefault(i => i.EntityId == groupFund.Id);
                    groupFund.ImageUrl = image?.ImageUrl != null ? image.ImageUrl : null;
                }

                var groupPagingResult = PaginationHelper.GetPaginationResult(groupFunds, groupFundModels);

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = groupPagingResult
                };
            }
            else
            {
                // setup filter
                filter.UserId = currentUser.Id;

                // Get all group's user
                var groupFunds = await _unitOfWork.GroupFundRepository.GetGroupFundsFilterAsync(
                    paginationParameters,
                    filter,
                    include: q => q
                        .Include(x => x.GroupMembers)
                );

                // remove group member in list
                foreach (var group in groupFunds)
                {
                    group.GroupMembers = null;
                }

                var groupFundModels = _mapper.Map<List<GroupFundModel>>(groupFunds);

                // Get and map images for each group fund
                var images = await _unitOfWork.ImageRepository.GetImagesByEntityNameAsync(EntityName.GROUP.ToString());
                foreach (var groupFund in groupFundModels)
                {
                    var image = images.FirstOrDefault(i => i.EntityId == groupFund.Id);
                    groupFund.ImageUrl = image?.ImageUrl != null ? image.ImageUrl : null;
                }

                var groupPagingResult = PaginationHelper.GetPaginationResult(groupFunds, groupFundModels);

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = groupPagingResult
                };
            }

        }

        public async Task<BaseResultModel> CloseGroupFundAsync(Guid groupId)
        {
            // Retrieve the group fund by its Id
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(groupId, include: q => q
                    .Include(x => x.GroupFundLogs)
                    .Include(x => x.GroupMembers));
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
                    Message = MessageConstants.GROUP_CLOSE_FORBIDDEN
                };
            }

            // Check if the group has any transactions
            if (groupFund.Transactions.Any())
            {
                // check group balance = 0
                if (groupFund.CurrentBalance > 0)
                {
                    throw new DefaultException(MessageConstants.GROUP_BALANCE_MUST_EQUAL_ZERO_MESSAGE, 
                        MessageConstants.GROUP_BALANCE_MUST_EQUAL_ZERO);
                }

                // Soft delete: mark the group as inactive
                groupFund.Status = GroupStatus.DISBANDED;
                _unitOfWork.GroupFundRepository.SoftDeleteAsync(groupFund);

                // Add a log entry for the disband group action
                groupFund.GroupFundLogs.Add(new GroupFundLog
                {
                    ChangedBy = currentUser.FullName,
                    ChangeDescription = $"đã chuyển nhóm vào chế độ lưu trữ",
                    Action = GroupAction.DISBANDED.ToString(),
                    CreatedDate = CommonUtils.GetCurrentTime(),
                    CreatedBy = currentUser.Email
                });
            }
            else
            {
                // Hard delete: remove the group from the database
                // BR: xóa cứng nhóm nếu chưa có transaction nào

                _unitOfWork.GroupFundLogRepository.PermanentDeletedListAsync(groupFund.GroupFundLogs.ToList());

                // get group member
                var groupMembers = groupFund.GroupMembers;

                // delete member
                _unitOfWork.GroupMemberRepository.PermanentDeletedListAsync(groupMembers.ToList());

                // delete group
                _unitOfWork.GroupFundRepository.PermanentDeletedAsync(groupFund);
            }



            // Save the changes to the repository
            await _unitOfWork.SaveAsync();

            // Return a success result
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.GROUP_CLOSE_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> GenerateFinancialHealthReportAsync(Guid groupId)
        {
            // Retrieve the group fund by its Id
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdAsync(groupId);
            if (groupFund == null)
            {
                return new BaseResultModel
                {
                    Status = StatusCodes.Status404NotFound,
                    Message = MessageConstants.GROUP_NOT_EXIST
                };
            }

            // Calculate financial ratios
            var totalIncome = groupFund.Transactions.Where(t => t.Type == TransactionType.INCOME).Sum(t => (int?)t.Amount) ?? 0;
            var totalDebt = groupFund.Transactions.Where(t => t.Type == TransactionType.EXPENSE).Sum(t => (int?)t.Amount) ?? 0;
            var totalSavings = totalIncome - totalDebt;

            var savingRatio = (totalSavings / totalIncome) * 100;
            var debtToIncomeRatio = (totalDebt / totalIncome) * 100;
            var netWorth = totalSavings;

            // Generate suggestions based on financial ratios 
            // Import AI generated suggestions here
            var suggestions = new List<string>();
            if (savingRatio < 10)
            {
                suggestions.Add("Tiết kiệm hiện tại là dưới 10%. Hãy đặt mục tiêu tiết kiệm ít nhất 20% thu nhập.");
            }
            if (debtToIncomeRatio > 50)
            {
                suggestions.Add("Nợ chiếm hơn 50% thu nhập. Ưu tiên thanh toán nợ tín dụng trước để giảm gánh nặng lãi suất.");
            }
            //
            // Create the financial health report
            var report = new FinancialHealthReport
            {
                SavingRatio = savingRatio,
                DebtToIncomeRatio = debtToIncomeRatio,
                NetWorth = netWorth,
                Suggestions = suggestions
            };

            // Return the report
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = report,
                Message = MessageConstants.REPORT_GENERATE_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> GetGroupFundById(Guid groupId)
        {
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(groupId, include: q => q.Include(c => c.GroupMembers).ThenInclude(gm => gm.User));

            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            var images = await _unitOfWork.ImageRepository.GetImagesByEntityAsync(groupFund.Id, EntityName.GROUP.ToString());
            var groupFundModel = _mapper.Map<GroupFundModel>(groupFund);
            groupFundModel.ImageUrl = images.FirstOrDefault()?.ImageUrl;

            // get goal
            var groupGoal = await _unitOfWork.FinancialGoalRepository.GetByConditionAsync(
                filter: g => g.GroupId == groupId && g.Status == FinancialGoalStatus.ACTIVE
            );

            if (groupGoal.Any())
            {
                groupFundModel.IsGoalActive = true;
            }

            var allGroupTransactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.GroupId == groupId && t.Type == TransactionType.INCOME && t.Status == TransactionStatus.APPROVED
            );

            // Calculate total group income from all approved income transactions
            decimal totalGroupIncome = allGroupTransactions.Sum(t => t.Amount);

            if (groupFundModel.GroupMembers != null && groupFundModel.GroupMembers.Any())
            {
                foreach (var member in groupFundModel.GroupMembers)
                {
                    var memberTransactions = allGroupTransactions.Where(t => t.UserId == member.UserId).ToList();

                    decimal totalContribution = memberTransactions.Sum(t => t.Amount);

                    member.TotalContribution = totalContribution;
                    member.TransactionCount = memberTransactions.Count;
                    
                    // Calculate contribution percentage for this member
                    if (totalGroupIncome > 0)
                    {
                        member.ContributionPercentage = Math.Round((totalContribution / totalGroupIncome) * 100, 2);
                    }
                    else
                    {
                        member.ContributionPercentage = 0;
                    }
                }
            }

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = groupFundModel
            };
        }

        public async Task<GroupMember> GetGroupLeader(Guid groupId)
        {
            // Get group with members
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(groupId, include: q => q
                    .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.User));

            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            // Find leader member
            var leader = groupFund.GroupMembers.FirstOrDefault(member =>
                member.Role == RoleGroup.LEADER &&
                member.Status == GroupMemberStatus.ACTIVE &&
                !member.IsDeleted);

            if (leader == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_LEADER_NOT_FOUND);
            }

            return leader;
        }
        
        public async Task<BaseResultModel> GetGroupFundLogs(Guid groupId, PaginationParameter paginationParameters, GroupLogFilter filter)
        {
            var currentUser = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (currentUser == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // Kiểm tra xem user có thuộc nhóm quỹ không
            var groupMembers = await _unitOfWork.GroupMemberRepository.GetByConditionAsync(
                filter: gm => gm.GroupId == groupId && gm.UserId == currentUser.Id,
                include: gm => gm.Include(gm => gm.User)
            );

            if (!groupMembers.Any())
            {
                throw new DefaultException("You can not access this group.", MessageConstants.GROUP_ACCESS_DENIED);
            }

            var logsPagination = await _unitOfWork.GroupFundLogRepository.GetGroupFundLogsFilter(
                   paginationParameters,
                   filter,
                   condition: log => log.GroupId == groupId
             );

            var groupFundLogModels = _mapper.Map<List<GroupFundLogModel>>(logsPagination);

            var allUser = await _unitOfWork.UsersRepository.GetAllAsync();
            foreach (var log in groupFundLogModels)
            {
                var user = allUser.FirstOrDefault(u => u.Email == log.CreatedBy);
                if (user != null)
                {
                    log.ImageUrl = user.AvatarUrl;
                }
            }

            var result = PaginationHelper.GetPaginationResult(logsPagination, groupFundLogModels);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Retrieved group fund logs successfully",
                Data = result
            };
        }

        public async Task<List<GroupMember>> GetGroupMembers(Guid groupId)
        {
            // Get group with members
            var groupFund = await _unitOfWork.GroupFundRepository
                .GetByIdIncludeAsync(groupId, include: q => q
                    .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.User));

            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            // Get all active members
            var members = groupFund.GroupMembers
                .Where(member =>
                    member.Status == GroupMemberStatus.ACTIVE &&
                    !member.IsDeleted)
                .ToList();

            return members;
        }

        public async Task<BaseResultModel> UpdateGroupFundsAsync(UpdateGroupModel model)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (user == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            var bankAccount = await _unitOfWork.BankAccountRepository.GetByIdAsync(model.AccountBankId);
            if (bankAccount == null)
            {
                throw new NotExistException("", MessageConstants.BANK_ACCOUNT_NOT_FOUND);
            }

            // get group fund
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdIncludeAsync(model.Id);
            if (groupFund == null)
            {
                throw new NotExistException("", MessageConstants.GROUP_NOT_EXIST);
            }

            // check user is leader
            var isLeader = groupFund.GroupMembers.Any(member => member.UserId == user.Id 
                && member.Role == RoleGroup.LEADER && member.Status == GroupMemberStatus.ACTIVE);

            if (!isLeader)
            {
                throw new DefaultException(MessageConstants.GROUP_UPDATE_FORBIDDEN_MESSAGE, MessageConstants.GROUP_UPDATE_FORBIDDEN);
            }

            // Map the model to the existing groupFund entity
            _mapper.Map(model, groupFund);
            groupFund.NameUnsign = StringUtils.ConvertToUnSign(model.Name);
            groupFund.UpdatedBy = user.Email;

            // group image
            var groupImages = await _unitOfWork.ImageRepository.GetImagesByEntityAsync(groupFund.Id, EntityName.GROUP.ToString());
            var groupImage = groupImages.FirstOrDefault();


            if (groupImage == null)
            {
                Image newImage = null;

                if (model.Image != null)
                {
                    newImage = new Image
                    {
                        EntityId = groupFund.Id,
                        EntityName = EntityName.GROUP.ToString(),
                        ImageUrl = model.Image,
                        CreatedBy = user.Email
                    };

                    await _unitOfWork.ImageRepository.AddAsync(newImage);
                    await _unitOfWork.SaveAsync();
                }
            }
            else
            {
                if (model.Image != null)
                {
                    // Update the existing image entity
                    groupImage.ImageUrl = model.Image;
                    groupImage.UpdatedBy = user.Email;
                    _unitOfWork.ImageRepository.UpdateAsync(groupImage);
                    await _unitOfWork.SaveAsync();
                }
            }

            groupFund.GroupFundLogs = new List<GroupFundLog>
            {
                new GroupFundLog
                {
                    ChangedBy = user.FullName,
                    ChangeDescription = "đã chỉnh sửa thông tin của nhóm",
                    Action = GroupAction.CREATED.ToString(),
                    CreatedDate = CommonUtils.GetCurrentTime(),
                    CreatedBy = user.Email
                }
            };
            // Add the groupFund to the repository and save changes again
            _unitOfWork.GroupFundRepository.UpdateAsync(groupFund);
            await _unitOfWork.SaveAsync();

            // Return a success result with the created groupFund
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.GROUP_UPDATE_SUCCESS_MESSAGE
            };
        }

        public async Task LogGroupFundChange(Guid groupId, string description, GroupAction action, string userEmail)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail)
                ?? throw new NotExistException("User not found", MessageConstants.ACCOUNT_NOT_EXIST);
            var groupFund = await _unitOfWork.GroupFundRepository.GetByIdAsync(groupId)
                ?? throw new NotExistException("Group not found", MessageConstants.GROUP_NOT_EXIST);
            var log = new GroupFundLog
            {
                GroupId = groupFund.Id,
                ChangedBy = user.FullName,
                ChangeDescription = description,
                Action = action.ToString(),
                CreatedDate = CommonUtils.GetCurrentTime(),
                CreatedBy = user.Email,
            };

            await _unitOfWork.GroupFundLogRepository.AddAsync(log);
            await _unitOfWork.SaveAsync();
        }
    }
}
