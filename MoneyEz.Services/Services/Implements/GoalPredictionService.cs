using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.BusinessModels.FinancialGoalModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Services.Interfaces;

namespace MoneyEz.Services.Services.Implements
{
    public class GoalPredictionService : IGoalPredictionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private const decimal MIN_DAILY_PROGRESS = 0.001m; // 0.1% minimum daily progress

        public GoalPredictionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GoalPredictionModel> PredictGoalCompletion(Guid goalId, bool isSaving)
        {
            var goal = await _unitOfWork.FinancialGoalRepository.GetByIdAsync(goalId)
                ?? throw new NotExistException("", MessageConstants.FINANCIAL_GOAL_NOT_FOUND);

            var transactions = await _unitOfWork.TransactionsRepository.GetByConditionAsync(
                filter: t => t.UserId == goal.UserId
                            && t.SubcategoryId == goal.SubcategoryId
                            && t.TransactionDate >= goal.StartDate
                            && t.TransactionDate <= goal.Deadline
                            && t.Status == TransactionStatus.APPROVED
                            && !t.IsDeleted,
                orderBy: q => q.OrderBy(t => t.TransactionDate)
            );

            var now = CommonUtils.GetCurrentTime();
            var remainingDays = Math.Max(1, (goal.Deadline - now).Days) + 1;
            var dailyChanges = CalculateDailyChanges(transactions.ToList());
            var averageChangePerDay = dailyChanges.Any() ? dailyChanges.Average() : 0m;
            var remainingAmount = goal.TargetAmount - goal.CurrentAmount;
            
            // Calculate required daily change to meet deadline
            var requiredDailyChange = Math.Abs(remainingAmount) / remainingDays;
            
            // If average change is too small, use alternative calculation
            var effectiveChangePerDay = Math.Abs(averageChangePerDay) < MIN_DAILY_PROGRESS ? 
                requiredDailyChange : averageChangePerDay;

            if (isSaving)
            {
                var projectedDays = (int)Math.Ceiling(remainingAmount / effectiveChangePerDay);
                // Cap maximum projection at 2x the goal duration
                var maxDays = (goal.Deadline - goal.StartDate).Days * 2;
                projectedDays = Math.Min(projectedDays, maxDays);

                var predictedDate = now.AddDays(projectedDays);
                
                return new GoalPredictionModel
                {
                    AverageChangePerDay = decimal.Round(averageChangePerDay, 2),
                    ProjectedDaysToCompletion = projectedDays,
                    PredictedCompletionDate = predictedDate,
                    CurrentTrend = decimal.Round(CalculateCurrentTrend(dailyChanges), 2),
                    IsOnTrack = predictedDate <= goal.Deadline,
                    TrendDescription = GetSavingTrendDescription(
                        averageChangePerDay,
                        requiredDailyChange,
                        predictedDate,
                        goal.Deadline),
                    RequiredDailyChange = decimal.Round(requiredDailyChange, 2),
                    TotalProgress = decimal.Round((goal.CurrentAmount / goal.TargetAmount) * 100, 2),
                    RemainingDays = remainingDays
                };
            }
            else 
            {
                var projectedDays = (int)Math.Ceiling(remainingAmount / Math.Abs(effectiveChangePerDay));
                // Cap maximum projection at 2x the goal duration
                var maxDays = (goal.Deadline - goal.StartDate).Days * 2; 
                projectedDays = Math.Min(projectedDays, maxDays);

                var predictedDate = now.AddDays(projectedDays);

                return new GoalPredictionModel
                {
                    AverageChangePerDay = decimal.Round(averageChangePerDay, 2),
                    ProjectedDaysToCompletion = projectedDays,
                    PredictedCompletionDate = predictedDate,
                    CurrentTrend = decimal.Round(CalculateCurrentTrend(dailyChanges), 2),
                    IsOnTrack = predictedDate > goal.Deadline,
                    TrendDescription = GetSpendingTrendDescription(
                        averageChangePerDay,
                        requiredDailyChange,
                        predictedDate, 
                        goal.Deadline),
                    RequiredDailyChange = decimal.Round(requiredDailyChange, 2),
                    TotalProgress = decimal.Round((goal.CurrentAmount / goal.TargetAmount) * 100, 2),
                    RemainingDays = remainingDays
                };
            }
        }

        private List<decimal> CalculateDailyChanges(List<Transaction> transactions)
        {
            var changes = new List<decimal>();
            var groupedByDate = transactions
                .GroupBy(t => t.TransactionDate)
                .OrderBy(g => g.Key);

            foreach (var group in groupedByDate)
            {
                changes.Add(group.Sum(t => t.Amount));
            }

            return changes;
        }

        private decimal CalculateCurrentTrend(List<decimal> dailyChanges)
        {
            if (dailyChanges.Count < 2) return 0;

            var recentDays = Math.Min(7, dailyChanges.Count);
            var recentChanges = dailyChanges.TakeLast(recentDays).ToList();
            return recentChanges.Average();
        }

        private string GetSavingTrendDescription(
            decimal averageChange,
            decimal requiredChange,
            DateTime predictedDate,
            DateTime deadline)
        {
            if (averageChange <= 0)
                return $"Hiện tại, bạn chưa có mức tiết kiệm tích lũy nào. Để đạt được mục tiêu tài chính đúng hạn, " +
                    $"bạn cần tiết kiệm đều đặn với số tiền khoảng {requiredChange:N0}đ mỗi ngày.";

            if (predictedDate <= deadline)
                return $"Xu hướng tiết kiệm của bạn đang ở mức ổn định với {averageChange:N0}đ/ngày, " +
                    $"dự báo sẽ hoàn thành mục tiêu sau {(predictedDate - CommonUtils.GetCurrentTime()).Days} ngày.";

            return $"Mặc dù bạn đã tiết kiệm được một khoản nhất định, nhưng để đảm bảo đạt mục tiêu đúng hạn, " +
                $"hãy cân nhắc tăng mức tiết kiệm lên {requiredChange:N0}đ mỗi ngày.";
        }

        private string GetSpendingTrendDescription(
            decimal averageChange,
            decimal requiredChange,
            DateTime predictedDate,
            DateTime deadline)
        {
            int daysDiff = (predictedDate - CommonUtils.GetCurrentTime()).Days;

            // Case 1: Nếu mức chi tiêu tăng (hoặc không giảm)
            if (averageChange >= 0)
                return $"Hiện tại, mức chi tiêu của bạn đang có xu hướng tăng. Để đảm bảo ngân sách được cân đối, " +
                    $"bạn cần điều chỉnh và giảm xuống dưới {requiredChange:N0}đ mỗi ngày.";

            // Case 2: Dự kiến hoàn thành chính xác đúng hạn
            if (predictedDate == deadline)
                return "Với mức chi tiêu hiện tại, bạn sẽ đạt được mục tiêu ngay đúng hạn. Hãy giữ vững phong độ hiện tại.";

            // Case 3: Nếu dự kiến hoàn thành sau hạn chót (tốt cho việc kiểm soát chi tiêu)
            if (predictedDate > deadline)
            {
                int extraDays = (predictedDate - deadline).Days;
                if (extraDays >= 5)
                    return $"Tình hình chi tiêu của bạn rất khả quan, bạn không chỉ kiểm soát tốt mà còn có dư phòng " +
                           $"trong ngân sách (dự kiến đạt mục tiêu muộn hơn hạn chót {extraDays} ngày).";
                else
                    return "Tình hình chi tiêu của bạn đang được kiểm soát hiệu quả, dự báo sẽ đạt được mục tiêu tài chính đúng hạn.";
            }

            // Case 4: Nếu dự kiến vượt hạn mức trước hạn chót
            if (predictedDate < deadline)
            {
                if (daysDiff > 10)
                    return $"Xu hướng chi tiêu hiện tại cho thấy bạn sẽ vượt quá hạn mức sớm, chỉ còn khoảng {daysDiff} ngày " +
                           $"để điều chỉnh. Hãy cân nhắc điều chỉnh chiến lược chi tiêu ngay từ bây giờ.";
                else
                    return $"Nếu tiếp tục duy trì mức chi tiêu hiện tại với {Math.Abs(averageChange):N0}đ/ngày, " +
                           $"bạn dự kiến sẽ vượt quá hạn mức chỉ sau {daysDiff} ngày. " +
                           $"Hãy nhanh chóng điều chỉnh để không vượt quá ngân sách.";
            }

            // Default case (nếu có trường hợp bất thường)
            return $"Dựa trên xu hướng hiện tại, hãy theo dõi và điều chỉnh mức chi tiêu để đảm bảo không vượt quá ngân sách " +
                   $"và đạt được mục tiêu đúng hạn.";
        }

    }
}
