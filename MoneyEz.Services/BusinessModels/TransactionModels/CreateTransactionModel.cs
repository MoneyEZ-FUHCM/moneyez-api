using MoneyEz.Repositories.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.TransactionModels
{
    public class CreateTransactionModel
    {
        [Required]
        public decimal Amount { get; set; }

        [Required]
        public TransactionType Type { get; set; } // INCOME, EXPENSE

        [Required]
        public DateOnly Date { get; set; }

        public string Description { get; set; } = string.Empty;

        public Guid? SubcategoryId { get; set; } // Chỉ dùng cho giao dịch cá nhân

        public Guid? UserId { get; set; } // Bắt buộc trong cả giao dịch nhóm & cá nhân

        public Guid? GroupId { get; set; } // Chỉ dùng cho giao dịch nhóm

        public bool ApprovalRequired { get; set; } = false; // Mặc định không cần duyệt
    }
}
