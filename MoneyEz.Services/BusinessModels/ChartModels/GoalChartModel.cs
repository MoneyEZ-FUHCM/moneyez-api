using System;
using System.Collections.Generic;

namespace MoneyEz.Services.BusinessModels.ChartModels
{
    public class GoalChartModel
    {
        public string GoalName { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal CompletionPercentage { get; set; }
        public List<GoalChartDataPoint> ChartData { get; set; } = new List<GoalChartDataPoint>();
    }

    public class GoalChartDataPoint
    {
        public string Label { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
