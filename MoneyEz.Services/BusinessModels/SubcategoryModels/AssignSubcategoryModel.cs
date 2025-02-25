using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.SubcategoryModels
{
    public class AssignSubcategoryModel
    {
        [Required(ErrorMessage = "Danh sách danh mục không được để trống.")]
        public List<CategorySubcategoryAssignment> Assignments { get; set; } = new List<CategorySubcategoryAssignment>();
    }

    public class CategorySubcategoryAssignment
    {
        [Required(ErrorMessage = "Mã danh mục là bắt buộc.")]
        public required string CategoryCode { get; set; }

        [Required(ErrorMessage = "Danh sách danh mục con không được để trống.")]
        public List<string> SubcategoryCodes { get; set; } = new List<string>();
    }
}
