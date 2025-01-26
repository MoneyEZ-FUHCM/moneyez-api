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
        [Required(ErrorMessage = "Tên danh mục là bắt buộc.")] // Ràng buộc không được để trống
        public string Name { get; set; } // Tên danh mục

        public string Description { get; set; } // Mô tả danh mục

        // `NameUnsign` sẽ được xử lý tự động, không cần client truyền lên
        public string NameUnsign => StringUtils.ConvertToUnSign(Name); // Chuyển đổi tên không dấu
    }
}
