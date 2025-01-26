using MoneyEz.Services.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.CategoryModels
{
    public class UpdateCategoryModel
    {
        [Required(ErrorMessage = "Tên danh mục là bắt buộc.")]
        public string Name { get; set; } // Tên mới của danh mục

        public string Description { get; set; } // Mô tả mới

        public Guid? ModelId { get; set; } // Mô hình liên quan (nếu có)

        // Tương tự CreateCategoryModel, `NameUnsign` được xử lý tự động
        public string NameUnsign => StringUtils.ConvertToUnSign(Name);
    }
}
