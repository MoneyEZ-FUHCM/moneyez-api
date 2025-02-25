using MoneyEz.Services.Utils;
using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.SubcategoryModels
{
    public class UpdateSubcategoryModel : CreateSubcategoryModel
    {
        [Required(ErrorMessage = "ID danh mục con là bắt buộc.")]
        public Guid Id { get; set; }
    }
}
