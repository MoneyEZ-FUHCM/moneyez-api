using MoneyEz.Services.Utils;
using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.SubcategoryModels
{
    public class CreateSubcategoryModel
    {
        [Required(ErrorMessage = "Tên danh mục con là bắt buộc.")]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ0-9\s,]+$",
            ErrorMessage = "Tên danh mục con không được chứa ký tự đặc biệt.")]

        [MaxLength(100, ErrorMessage = "Tên danh mục con không được vượt quá 100 ký tự.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Mã danh mục con là bắt buộc.")]
        [RegularExpression(@"^[a-zA-Z0-9\s,.-]+$",
            ErrorMessage = "Mã danh mục con không được chứa ký tự đặc biệt.")]
        public required string Code { get; set; }

        public required string Icon { get; set; }

        [MaxLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự.")]
        public string? Description { get; set; }

        public string NameUnsign => StringUtils.ConvertToUnSign(Name);
    }
}
