using System;
using System.Collections.Generic;

namespace MoneyEz.Services.BusinessModels.AdminDashboardModels
{
    public class ModelUsageStatisticsModel
    {
        public List<ModelUsage> Models { get; set; } = new List<ModelUsage>();
    }

    public class ModelUsage
    {
        public Guid ModelId { get; set; }
        public string? ModelName { get; set; }
        public int UserCount { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
