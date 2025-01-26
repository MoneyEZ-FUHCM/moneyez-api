using MoneyEz.Services.Utils;
using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.SpendingModelModels
{
    public class UpdateSpendingModelModel
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Tên mô hình là bắt buộc.")]
        public required string Name { get; set; }

        public string Description { get; set; }

        public bool? IsTemplate { get; set; }

        public string NameUnsign => StringUtils.ConvertToUnSign(Name);
    }
}
