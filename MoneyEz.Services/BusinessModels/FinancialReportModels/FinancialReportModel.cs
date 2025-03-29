using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.Utils;

namespace MoneyEz.Services.BusinessModels.FinancialReportModels
{
    public class FinancialReportModel : BaseEntity
    {
        public Guid? GroupId { get; set; }
        public Guid? UserId { get; set; }
        public string Name { get; set; }
        public string NameUnsign => StringUtils.ConvertToUnSign(Name);
        public ReportType? ReportType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetBalance => TotalIncome - TotalExpense;
    }
}
