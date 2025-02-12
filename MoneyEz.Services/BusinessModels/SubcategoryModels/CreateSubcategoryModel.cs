using MoneyEz.Services.Utils;
using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.SubcategoryModels
{
    public class CreateSubcategoryModel
    {
        [Required(ErrorMessage = "Tên danh mục con là bắt buộc.")]
        [MaxLength(100, ErrorMessage = "Tên danh mục con không được vượt quá 100 ký tự.")]
        public required string Name { get; set; }

        [MaxLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự.")]
        public string Description { get; set; }
        public List<Guid> CategoryIds { get; set; } = new List<Guid>();

        public string NameUnsign => StringUtils.ConvertToUnSign(Name);
    }
}
