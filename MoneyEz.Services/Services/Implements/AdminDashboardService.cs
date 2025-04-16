using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Services.BusinessModels.AdminDashboardModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Services.Interfaces;

namespace MoneyEz.Services.Services.Implements
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimsService _claimsService;

        public AdminDashboardService(IUnitOfWork unitOfWork, IMapper mapper, IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsService = claimsService;
        }

        public async Task<BaseResultModel> GetDashboardStatisticsAsync()
        {
            // Validate admin access
            var userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail);

            if (user == null || user.Role != RolesEnum.ADMIN)
            {
                throw new DefaultException("Access denied: Only administrators can access this information.", 
                    MessageConstants.PERMISSION_DENIED);
            }

            // Create the statistics model
            var statistics = new AdminStatisticsModel();

            // Get user statistics
            var allUsers = await _unitOfWork.UsersRepository.GetAllAsync();
            statistics.Users.Total = allUsers.Count;
            statistics.Users.Active = allUsers.Count(u => !u.IsDeleted && u.Status == CommonsStatus.ACTIVE);
            statistics.Users.Inactive = allUsers.Count(u => u.IsDeleted || u.Status == CommonsStatus.INACTIVE);

            // Get model statistics
            var allModels = await _unitOfWork.SpendingModelRepository.GetByConditionAsync(filter: m => !m.IsDeleted);
            statistics.TotalModels = allModels.Count;

            // Get category statistics
            var allCategories = await _unitOfWork.CategoriesRepository.GetByConditionAsync(filter: m => !m.IsDeleted);
            statistics.TotalCategories = allCategories.Count;

            // Get group statistics
            var allGroups = await _unitOfWork.GroupFundRepository.GetByConditionAsync(filter: m => !m.IsDeleted);
            statistics.TotalGroups = allGroups.Count;

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Dashboard statistics retrieved successfully",
                Data = statistics
            };
        }

        public async Task<BaseResultModel> GetModelUsageStatisticsAsync()
        {
            // Validate admin access
            var userEmail = _claimsService.GetCurrentUserEmail;
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(userEmail);

            if (user == null || user.Role != RolesEnum.ADMIN)
            {
                throw new DefaultException("Access denied: Only administrators can access this information.", 
                    MessageConstants.PERMISSION_DENIED);
            }

            var modelUsageStats = new ModelUsageStatisticsModel();
            
            // Get all spending models
            var spendingModels = await _unitOfWork.SpendingModelRepository.GetByConditionAsync(
                filter: m => !m.IsDeleted,
                include: query => query.Include(m => m.UserSpendingModels)
            );

            foreach (var model in spendingModels)
            {
                var modelUsage = new ModelUsage
                {
                    ModelId = model.Id,
                    ModelName = model.Name,
                    UserCount = model.UserSpendingModels?.Count(usm => !usm.IsDeleted) ?? 0
                };

                // Get transaction counts for this model
                var userSpendingModelIds = model.UserSpendingModels?.Where(usm => !usm.IsDeleted).Select(usm => usm.Id).ToList();
                
                if (userSpendingModelIds != null && userSpendingModelIds.Any())
                {
                    var transactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(filter:
                        t => userSpendingModelIds.Contains(t.UserSpendingModelId.Value) && !t.IsDeleted && t.GroupId == null
                    );

                    modelUsage.TransactionCount = transactions.Count;
                    modelUsage.TotalAmount = transactions.Sum(t => t.Amount);
                }

                modelUsageStats.Models.Add(modelUsage);
            }

            // Sort by user count (most popular first)
            modelUsageStats.Models = modelUsageStats.Models.OrderByDescending(m => m.UserCount).ToList();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Model usage statistics retrieved successfully",
                Data = modelUsageStats
            };
        }
    }
}
