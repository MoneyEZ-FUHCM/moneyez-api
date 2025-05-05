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
            bool isOverdue = goal.Deadline.Date <= now.Date;

            // Tính toán số ngày còn lại của goal
            int remainingDays = 1;
            if (goal.Deadline.Date > now.Date)
            {
                remainingDays = Math.Max(1, (goal.Deadline.Date - now.Date).Days);
            }

            // tính toán dailyChanges từ transactions
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
                // trường hợp goal hết hạn
                if (isOverdue)
                {
                    // Use actual progress for overdue goals rather than prediction
                    var totalProgress = decimal.Round((goal.CurrentAmount / goal.TargetAmount) * 100, 2);
                    var daysSinceStart = (now.Date - goal.StartDate.Date).Days;
                    var currentTrend = decimal.Round(CalculateCurrentTrend(dailyChanges), 2);

                    return new GoalPredictionModel
                    {
                        AverageChangePerDay = decimal.Round(averageChangePerDay, 2),
                        ProjectedDaysToCompletion = 0,
                        PredictedCompletionDate = goal.Deadline, // ngày hoàn thành là deadline
                        CurrentTrend = currentTrend,
                        IsOnTrack = false,
                        TrendDescription = $"Mục tiêu đã quá hạn. Tiến độ hiện tại đạt {totalProgress}% " +
                                         $"với mức tiết kiệm trung bình {Math.Abs(averageChangePerDay):N0}đ/ngày trong {daysSinceStart} ngày qua.",
                        RequiredDailyChange = decimal.Round(requiredDailyChange, 2),
                        TotalProgress = totalProgress,
                        RemainingDays = 0
                    };
                }

                // tính toán số ngày dự báo (projectedDays) và ngày dự báo (predictedDate)
                // giới hạn số ngày dự báo tối đa là 2 lần thời gian còn lại
                // số ngày dự báo = số tiền còn lại / đổi biến hiệu quả mỗi ngày
                var projectedDays = (int)Math.Ceiling(remainingAmount / effectiveChangePerDay);
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
                // xử lí ngân sách đã hết hạn
                if (isOverdue)
                {
                    // Use actual progress for overdue goals rather than prediction
                    var totalProgress = decimal.Round((goal.CurrentAmount / goal.TargetAmount) * 100, 2);
                    var daysSinceStart = (now.Date - goal.StartDate.Date).Days;
                    var currentTrend = decimal.Round(CalculateCurrentTrend(dailyChanges), 2);

                    return new GoalPredictionModel
                    {
                        AverageChangePerDay = decimal.Round(averageChangePerDay, 2),
                        ProjectedDaysToCompletion = 0,
                        PredictedCompletionDate = goal.Deadline,
                        CurrentTrend = currentTrend,
                        IsOnTrack = false,
                        TrendDescription = $"Ngân sách đã quá hạn. Mức chi tiêu hiện tại ở mức {totalProgress}% so với hạn mức, " +
                                         $"với chi tiêu trung bình {Math.Abs(averageChangePerDay):N0}đ/ngày trong {daysSinceStart} ngày qua.",
                        RequiredDailyChange = decimal.Round(requiredDailyChange, 2),
                        TotalProgress = totalProgress,
                        RemainingDays = 0
                    };
                }

                // Kiểm tra nếu đã vượt ngân sách (remainingAmount < 0)
                bool isBudgetExceeded = remainingAmount < 0;

                if (isBudgetExceeded)
                {
                    // Tính % vượt ngân sách
                    var totalProgress = decimal.Round((goal.CurrentAmount / goal.TargetAmount) * 100, 2);
                    var exceededAmount = Math.Abs(remainingAmount);
                    var exceededPercent = decimal.Round((exceededAmount / goal.TargetAmount) * 100, 2);
                    var daysSinceStart = (now.Date - goal.StartDate.Date).Days;
                    var currentTrend = decimal.Round(CalculateCurrentTrend(dailyChanges), 2);
                    var dailyExceedAmount = exceededAmount / Math.Max(daysSinceStart, 1);

                    return new GoalPredictionModel
                    {
                        AverageChangePerDay = decimal.Round(averageChangePerDay, 2),
                        ProjectedDaysToCompletion = 0,
                        PredictedCompletionDate = now,
                        CurrentTrend = currentTrend,
                        IsOnTrack = false,
                        TrendDescription = $"Bạn đã vượt ngân sách {exceededAmount:N0}đ " +
                                          $"(tương đương {exceededPercent}% vượt hạn mức). " +
                                          $"Với xu hướng chi tiêu trung bình {Math.Abs(averageChangePerDay):N0}đ/ngày, " +
                                          $"bạn đang vượt khoảng {dailyExceedAmount:N0}đ mỗi ngày. " +
                                          $"Hãy cân nhắc điều chỉnh chi tiêu để quản lý tài chính hiệu quả hơn.",
                        RequiredDailyChange = 0,
                        TotalProgress = totalProgress,
                        RemainingDays = remainingDays
                    };
                }

                // tính toán số ngày dự báo (projectedDays) và ngày dự báo (predictedDate)
                // giới hạn số ngày dự báo tối đa là 2 lần thời gian còn lại
                // số ngày dự báo = số tiền còn lại / đổi biến hiệu quả mỗi ngày
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
                        remainingAmount,
                        remainingDays,
                        predictedDate, 
                        goal.Deadline),
                    RequiredDailyChange = decimal.Round(requiredDailyChange, 2),
                    TotalProgress = decimal.Round((goal.CurrentAmount / goal.TargetAmount) * 100, 2),
                    RemainingDays = remainingDays
                };
            }
        }

        // hàm tính tổng số tiền của các giao dịch cho mỗi ngày và trả về danh sách các giá trị tổng đó theo thứ tự ngày
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
            // Tính số ngày từ thời điểm hiện tại đến ngày dự báo và đến hạn chót
            int daysToGoal = (predictedDate - CommonUtils.GetCurrentTime()).Days;
            int daysToDeadline = (deadline - CommonUtils.GetCurrentTime()).Days;

            // Case 1: Chưa có tiết kiệm tích lũy
            if (averageChange <= 0)
            {
                return $"Hiện tại, bạn chưa có mức tiết kiệm tích lũy nào. Để đạt được mục tiêu tài chính đúng hạn, " +
                       $"bạn cần tiết kiệm đều đặn với số tiền khoảng {requiredChange:N0}đ mỗi ngày.";
            }

            // Case 2: Dự báo hoàn thành mục tiêu trước hạn chót (tiết kiệm tốt)
            if (predictedDate < deadline)
            {
                // Nếu tiết kiệm vượt mục tiêu đáng kể (trên 20% so với requiredChange)
                if (averageChange >= requiredChange * 1.2m)
                {
                    return $"Xu hướng tiết kiệm của bạn rất ấn tượng với {averageChange:N0}đ/ngày. " +
                           $"Dự báo sẽ hoàn thành mục tiêu sau {daysToGoal} ngày, sớm hơn hạn chót {daysToDeadline - daysToGoal} ngày.";
                }
                // Nếu tiết kiệm đạt đúng hoặc hơi trên mức yêu cầu
                else if (averageChange >= requiredChange)
                {
                    return $"Xu hướng tiết kiệm của bạn đang ổn định với {averageChange:N0}đ/ngày, " +
                           $"dự báo sẽ hoàn thành mục tiêu sau {daysToGoal} ngày.";
                }
                // Nếu mức tiết kiệm hiện tại thấp hơn yêu cầu mặc dù dự báo trước hạn
                else
                {
                    return $"Mặc dù bạn sẽ hoàn thành mục tiêu trước hạn, nhưng với mức tiết kiệm hiện tại là {averageChange:N0}đ/ngày, " +
                           $"bạn cần tăng mức tiết kiệm lên {requiredChange:N0}đ/ngày để đảm bảo ổn định tài chính.";
                }
            }

            // Case 3: Dự báo hoàn thành mục tiêu đúng hạn
            if (predictedDate == deadline)
            {
                return $"Xu hướng tiết kiệm của bạn đang ở mức ổn định với {averageChange:N0}đ/ngày, " +
                       $"dự báo sẽ hoàn thành mục tiêu đúng hạn.";
            }

            // Case 4: Dự báo hoàn thành mục tiêu sau hạn chót
            if (predictedDate > deadline)
            {
                int delayDays = (predictedDate - deadline).Days;
                // Nếu tiết kiệm hiện tại vẫn đạt hoặc vượt yêu cầu
                if (averageChange >= requiredChange)
                {
                    return $"Mặc dù bạn đang tiết kiệm với {averageChange:N0}đ/ngày, " +
                           $"dự báo sẽ hoàn thành mục tiêu trễ hạn {delayDays} ngày. " +
                           $"Hãy cân nhắc tăng mức tiết kiệm lên ít nhất {requiredChange:N0}đ/ngày để rút ngắn thời gian.";
                }
                else
                {
                    return $"Xu hướng tiết kiệm của bạn hiện tại là {averageChange:N0}đ/ngày, " +
                           $"dự báo sẽ hoàn thành mục tiêu trễ hạn {delayDays} ngày. " +
                           $"Để đạt mục tiêu đúng hạn, bạn cần tăng mức tiết kiệm lên {requiredChange:N0}đ/ngày.";
                }
            }

            // Default case (an toàn)
            return "Hãy theo dõi sát sao xu hướng tiết kiệm của bạn và điều chỉnh mức tiết kiệm nếu cần để đạt được mục tiêu tài chính.";
        }


        private string GetSpendingTrendDescription(
            decimal averageChange,
            decimal requiredChange,
            decimal remainingBudget,
            int remainingDays,
            DateTime predictedDate,
            DateTime deadline)
        {
            // Tính toán các chỉ số bổ sung
            int daysUntilPrediction = (predictedDate - CommonUtils.GetCurrentTime()).Days;
            int extraDays = (predictedDate - deadline).Days;

            // Xử lý trường hợp projectedDailySpending âm (đã vượt quá ngân sách)
            bool isBudgetExceeded = remainingBudget < 0;

            // mức chi tiêu dự kiến hàng ngày dựa trên ngân sách còn lại
            decimal projectedDailySpending;

            // chênh lệch giữa mức chi tiêu dự kiến hàng ngày và mức chi tiêu mục tiêu hàng ngày
            decimal spendingDifference;

            // tổng chênh lệch tích lũy dự kiến trong thời gian còn lại
            decimal cumulativeDifference;

            if (isBudgetExceeded)
            {
                // Đã vượt quá ngân sách, tính toán mức vượt quá mỗi ngày
                projectedDailySpending = remainingBudget / remainingDays;
                spendingDifference = projectedDailySpending - requiredChange;
                cumulativeDifference = spendingDifference * remainingDays;
            }
            else
            {
                // Ngân sách vẫn còn dư, tính toán bình thường
                projectedDailySpending = remainingBudget / remainingDays;
                spendingDifference = projectedDailySpending - requiredChange;
                cumulativeDifference = spendingDifference * remainingDays;
            }

            //decimal projectedDailySpending = remainingBudget / remainingDays;
            //decimal spendingDifference = projectedDailySpending - requiredChange;
            //decimal cumulativeDifference = spendingDifference * remainingDays;

            // Khởi tạo thông báo dự báo
            string trendMessage = "";
            // Xử lý trường hợp đã vượt quá ngân sách
            if (isBudgetExceeded)
            {
                // Tính số tiền đã vượt quá ngân sách
                decimal exceededAmount = Math.Abs(remainingBudget);

                if (averageChange >= 0)
                {
                    trendMessage = $"Bạn đã vượt quá ngân sách {exceededAmount:N0}đ và xu hướng chi tiêu đang tăng " +
                        $"({averageChange:N0}đ/ngày). ";
                }
                else
                {
                    trendMessage = $"Mặc dù xu hướng chi tiêu đang giảm ({Math.Abs(averageChange):N0}đ/ngày), " +
                        $"nhưng bạn đã vượt quá ngân sách {exceededAmount:N0}đ. ";
                }

                // Thêm lời khuyên
                trendMessage += $"Hãy hạn chế chi tiêu ngay lập tức. Ước tính mỗi ngày bạn vượt {Math.Abs(projectedDailySpending):N0}đ " +
                    $"so với mục tiêu hàng ngày {requiredChange:N0}đ.";

                return trendMessage;
            }

            // Đánh giá dựa trên mức chi tiêu dự kiến so với mục tiêu (trường hợp bình thường)
            if (projectedDailySpending > requiredChange)
            {
                if (averageChange >= 0)
                {
                    trendMessage = $"Mức chi tiêu của bạn có xu hướng tăng (averageChange = {averageChange:N0}đ/ngày) " +
                        $"và ước tính {projectedDailySpending:N0}đ/ngày, " +
                        $"vượt mức mục tiêu {requiredChange:N0}đ/ngày. ";
                }
                else // averageChange < 0 nhưng vẫn vượt mục tiêu
                {
                    trendMessage = $"Dù xu hướng chi tiêu đang giảm (averageChange = {averageChange:N0}đ/ngày), " +
                        $"nhưng mức chi tiêu dự kiến {projectedDailySpending:N0}đ/ngày vẫn cao hơn mục tiêu {requiredChange:N0}đ/ngày. ";
                }
                trendMessage += $"Chênh lệch là {spendingDifference:N0}đ/ngày, tích lũy {cumulativeDifference:N0}đ trong tổng số {remainingDays} ngày còn lại.";
            }
            else if (projectedDailySpending < requiredChange)
            {
                trendMessage = $"Xu hướng chi tiêu của bạn rất khả quan, với mức chi tiêu dự kiến {projectedDailySpending:N0}đ/ngày " +
                    $"thấp hơn mục tiêu {requiredChange:N0}đ/ngày, giảm trung bình {Math.Abs(averageChange):N0}đ/ngày.";
            }
            else // projectedDailySpending == requiredChange
            {
                trendMessage = $"Mức chi tiêu dự kiến của bạn đang đạt đúng mục tiêu {requiredChange:N0}đ/ngày.";
            }

            // Đánh giá thêm dựa trên ngày dự báo so với hạn chót
            if (predictedDate == deadline)
            {
                trendMessage += " Dự báo sẽ đạt mục tiêu đúng hạn.";
            }
            else if (predictedDate > deadline)
            {
                if (extraDays >= 10)
                {
                    trendMessage += $" Dự kiến đạt mục tiêu muộn hơn hạn chót tới {extraDays} ngày, cho thấy bạn đang tiết kiệm rất tốt.";
                }
                else if (extraDays >= 5)
                {
                    trendMessage += $" Dự kiến đạt mục tiêu muộn hơn hạn chót {extraDays} ngày, điều này cho thấy bạn kiểm soát ngân sách khá tốt.";
                }
                else
                {
                    trendMessage += " Dự kiến sẽ đạt mục tiêu với chỉ một chút chậm trễ nhẹ.";
                }
            }
            else // predictedDate < deadline
            {
                if (daysUntilPrediction <= 0)
                {
                    trendMessage += " Tình hình chi tiêu của bạn đang vượt mức ngay lập tức. Hãy điều chỉnh ngay để tránh vượt ngân sách.";
                }
                else if (daysUntilPrediction <= 5)
                {
                    trendMessage += $" Nếu duy trì mức chi tiêu hiện tại, bạn sẽ vượt quá hạn mức trong vòng chỉ {daysUntilPrediction} ngày. Hãy điều chỉnh ngay.";
                }
                else if (daysUntilPrediction <= 10)
                {
                    trendMessage += $" Xu hướng chi tiêu cho thấy bạn sẽ đạt mục tiêu sớm, chỉ còn {daysUntilPrediction} ngày để điều chỉnh. Cần cẩn trọng.";
                }
                else
                {
                    trendMessage += $" Dự báo đạt mục tiêu sớm, nhưng với {remainingDays} ngày và {remainingBudget:N0} VND còn lại, " +
                        $"hãy duy trì sự kiểm soát để tối ưu hóa ngân sách.";
                }
            }

            return trendMessage;
        }

    }
}
