using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Entities
{
    public partial class Asset : BaseEntity
    {
        public Guid UserId { get; set; }

        public string Name { get; set; }

        public string? NameUnsign { get; set; }

        public Guid SubcategoryId { get; set; }

        public decimal Amount { get; set; }

        public DateTime? AcquisitionDate { get; set; } // Ngày mua

        public DateTime? DepreciationDate { get; set; } // Ngày bắt đầu khấu hao

        public DateTime? RevaluationDate { get; set; } // Ngày tái định giá (cho tài sản cố định)

        public DateTime? DisposalDate { get; set; } // Ngày thanh lý

        public DateTime? MaturityDate { get; set; } // Ngày đáo hạn

        public double? Rate { get; set; } // Tỉ số lãi suất hoặc khấu hao (+/-)

        public string? Description { get; set; }

        public OwnershipType? OwnershipType { get; set; }

        public virtual User User { get; set; }
    }
}
