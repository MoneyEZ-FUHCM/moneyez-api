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
        public required Guid Id { get; set; }
        [Required(ErrorMessage = "Tên danh mục là bắt buộc.")]
        public required string Name { get; set; }
        public string Description { get; set; }

        public string NameUnsign => StringUtils.ConvertToUnSign(Name);
    }
}