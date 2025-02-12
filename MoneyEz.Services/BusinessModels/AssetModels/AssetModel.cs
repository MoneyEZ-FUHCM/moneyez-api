using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.AssetModels
{
    public class AssetModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string? NameUnsign { get; set; }
        public Guid SubcategoryId { get; set; }
        public decimal Amount { get; set; }
        public DateTime? AcquisitionDate { get; set; }
        public DateTime? DepreciationDate { get; set; }
        public DateTime? RevaluationDate { get; set; }
        public DateTime? DisposalDate { get; set; }
        public DateTime? MaturityDate { get; set; }
        public double? Rate { get; set; }
        public string? Description { get; set; }
        public int? OwnershipType { get; set; }
    }
}
