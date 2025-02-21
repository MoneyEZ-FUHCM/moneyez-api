using MoneyEz.Services.Utils;
using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.SubcategoryModels
{
    public class UpdateSubcategoryModel
    {
        [Required(ErrorMessage = "ID danh mục con là bắt buộc.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục con là bắt buộc.")]
        [MaxLength(100, ErrorMessage = "Tên danh mục con không được vượt quá 100 ký tự.")]
        public string Name { get; set; }

        [MaxLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự.")]
        public string Description { get; set; }

        public string NameUnsign => StringUtils.ConvertToUnSign(Name);
    }
}
