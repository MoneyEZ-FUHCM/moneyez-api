using MoneyEz.Repositories.Enums;
using MoneyEz.Services.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.CategoryModels
{
    public class CreateCategoryModel
    {
        [Required(ErrorMessage = "Tên danh mục là bắt buộc.")]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ0-9\s,]+$",
            ErrorMessage = "Tên danh mục không được chứa ký tự đặc biệt.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Mã danh mục là bắt buộc.")]
        [RegularExpression(@"^[a-zA-Z0-9\s,.-]+$",
            ErrorMessage = "Mã danh mục không được chứa ký tự đặc biệt.")]
        public required string Code { get; set; }
        public required string Icon { get; set; }
        public string? Description { get; set; }
        public TransactionType Type { get; set; }
        public bool IsSaving { get; set; } = false;

        public string NameUnsign => StringUtils.ConvertToUnSign(Name);
    }
}
