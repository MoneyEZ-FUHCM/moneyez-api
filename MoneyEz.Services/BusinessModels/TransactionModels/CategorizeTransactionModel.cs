using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.TransactionModels
{
    public class CategorizeTransactionModel
    {
        [Required(ErrorMessage = "Mã giao dịch là bắt buộc")]
        public Guid TransactionId { get; set; }

        public Guid? GroupId { get; set; }

        [Required(ErrorMessage = "Loại giao dịch là bắt buộc")]
        public CategorizeTransaction CategorizeTransaction { get; set; }

        public Guid? SubcategoryId { get; set; }
    }
}
