using System;

namespace MoneyEz.Services.BusinessModels.AdminDashboardModels
{
    public class AdminStatisticsModel
    {
        public UserStatistics Users { get; set; } = new UserStatistics();
        public int TotalModels { get; set; }
        public int TotalCategories { get; set; }
        public int TotalGroups { get; set; }
    }

    public class UserStatistics
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Inactive { get; set; }
    }
}
