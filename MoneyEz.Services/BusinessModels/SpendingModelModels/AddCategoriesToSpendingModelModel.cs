using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.SpendingModelModels
{
    public class AddCategoriesToSpendingModelModel
    {
        public Guid SpendingModelId { get; set; }

        [Required(ErrorMessage = "The list of category IDs cannot be empty.")]
        public List<Guid> CategoryIds { get; set; } // Danh sách các CategoryId cần thêm

        public List<decimal>? PercentageAmounts { get; set; } // Tùy chọn danh sách PercentageAmount (phải khớp số lượng với CategoryIds nếu có)

        public AddCategoriesToSpendingModelModel()
        {
            CategoryIds = new List<Guid>();
        }
    }
}
