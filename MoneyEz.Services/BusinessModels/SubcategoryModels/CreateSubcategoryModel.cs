using MoneyEz.Services.Utils;
using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.SubcategoryModels
{
    public class CreateSubcategoryModel
    {
        [Required(ErrorMessage = "Tên danh mục con là bắt buộc.")]
        public required string Name { get; set; }
        public string Description { get; set; }

        [Required(ErrorMessage = "ID danh mục chính là bắt buộc.")]
        public Guid CategoryId { get; set; }

        public string NameUnsign => StringUtils.ConvertToUnSign(Name);
    }
}
