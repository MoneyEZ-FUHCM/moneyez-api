using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.TransactionModels.Group
{
    public class CreateGroupTransactionModel
    {
        [Required(ErrorMessage = "GroupId là bắt buộc.")]
        public Guid GroupId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0.")]
        public decimal Amount { get; set; }

        [EnumDataType(typeof(TransactionType), ErrorMessage = "Loại giao dịch không hợp lệ.")]
        public TransactionType Type { get; set; }

        [Required(ErrorMessage = "Ngày giao dịch là bắt buộc.")]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự.")]
        public string? Description { get; set; } = string.Empty;

        public List<string>? Images { get; set; }

        public bool RequireVote { get; set; } = false;

        public InsertType InsertType { get; set; } = InsertType.MANUAL;
    }
}
