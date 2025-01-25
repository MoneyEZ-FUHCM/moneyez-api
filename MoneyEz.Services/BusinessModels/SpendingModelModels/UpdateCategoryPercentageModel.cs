using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.SpendingModelModels
{
    public class UpdateCategoryPercentageModel
    {
        [Required(ErrorMessage = "Danh sách danh mục là bắt buộc.")]
        public List<CategoryPercentageModel> Categories { get; set; }
    }

    public class CategoryPercentageModel
    {
        [Required(ErrorMessage = "ID danh mục là bắt buộc.")]
        public Guid CategoryId { get; set; }

        [Range(0, 100, ErrorMessage = "Tỷ lệ phần trăm phải nằm trong khoảng từ 0 đến 100.")]
        public decimal PercentageAmount { get; set; }
    }
}
