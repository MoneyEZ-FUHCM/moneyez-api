using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using System;

namespace MoneyEz.Services.BusinessModels.TransactionModels
{
    public class TransactionModel : BaseEntity
    {
        public decimal Amount { get; set; }

        public TransactionType Type { get; set; } // INCOME, EXPENSE

        public DateOnly Date { get; set; }

        public string Description { get; set; }

        public TransactionStatus Status { get; set; }

        public bool ApprovalRequired { get; set; } = false; // Mặc định không cần duyệt

        public Guid? SubcategoryId { get; set; } // Chỉ dùng cho giao dịch cá nhân

        public Subcategory Subcategory { get; set; } // Lấy thông tin danh mục con nếu có

        public Guid? UserId { get; set; } // Luôn có trong cả giao dịch nhóm & cá nhân

        public Guid? GroupId { get; set; } // Chỉ dùng cho giao dịch nhóm
    }
}
