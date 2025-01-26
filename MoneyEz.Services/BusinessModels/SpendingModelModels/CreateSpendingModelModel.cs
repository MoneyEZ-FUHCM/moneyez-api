using MoneyEz.Services.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.SpendingModelModels
{
    public class CreateSpendingModelModel
    {
        [Required(ErrorMessage = "Tên mô hình là bắt buộc.")]
        public required string Name { get; set; }

        public string Description { get; set; }

        public bool? IsTemplate { get; set; } = false; // Giá trị mặc định là false

        public string NameUnsign => StringUtils.ConvertToUnSign(Name);

    }
}

