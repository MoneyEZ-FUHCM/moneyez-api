using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Entities
{
    public partial class Liability : BaseEntity
    {
        public Guid UserId { get; set; }

        public string Name { get; set; }

        public string NameUnsign { get; set; }

        public Guid SubcategoryId { get; set; }

        public decimal Amount { get; set; }

        public DateTime? RecognitionDate { get; set; } // Ngày xuất hiện ghi nợ

        public DateTime? DueDate { get; set; } // Hạn thanh toán

        public DateTime? InterestPaymentDate { get; set; } // Hạn thanh toán

        public double? InterestRate { get; set; }

        public string? Description { get; set; }

        public OwnershipType? OwnershipType { get; set; }

        public virtual User User { get; set; }
    }
}
