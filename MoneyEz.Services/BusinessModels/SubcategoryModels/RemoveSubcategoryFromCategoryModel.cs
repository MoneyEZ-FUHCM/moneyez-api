using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.SubcategoryModels
{
    public class RemoveSubcategoryFromCategoryModel
    {
        [Required(ErrorMessage = "ID danh mục là bắt buộc.")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "ID danh mục con là bắt buộc.")]
        public Guid SubcategoryId { get; set; }
    }
}
