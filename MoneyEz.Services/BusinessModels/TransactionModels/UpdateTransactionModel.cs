using MoneyEz.Repositories.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.TransactionModels
{
    public class UpdateTransactionModel
    {
        [Required]
        public Guid Id { get; set; } // ID giao dịch cần cập nhật

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        public DateOnly Date { get; set; }

        public string Description { get; set; } = string.Empty;

        public Guid? SubcategoryId { get; set; } // Chỉ dùng cho giao dịch cá nhân

        public bool ApprovalRequired { get; set; } = false; // Cập nhật yêu cầu duyệt nếu cần

        public TransactionStatus Status { get; set; } // Chỉ admin hoặc nhóm trưởng mới có quyền cập nhật trạng thái

        public Guid? UserId { get; set; }

        public Guid? GroupId { get; set; }
    }
}
