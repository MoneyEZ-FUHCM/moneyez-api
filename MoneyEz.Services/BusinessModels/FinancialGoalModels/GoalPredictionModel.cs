namespace MoneyEz.Services.BusinessModels.FinancialGoalModels
{
    public class GoalPredictionModel
    {
        public decimal AverageChangePerDay { get; set; }
        public int ProjectedDaysToCompletion { get; set; }
        public DateTime PredictedCompletionDate { get; set; }
        public decimal CurrentTrend { get; set; }
        public bool IsOnTrack { get; set; }
        public string TrendDescription { get; set; } = "";
        public decimal RequiredDailyChange { get; set; }
        public decimal TotalProgress { get; set; }
        public int RemainingDays { get; set; }
    }
}
