using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.SpendingModelModels
{
    public class RemoveCategoriesFromSpendingModelModel
    {
        public required Guid SpendingModelId { get; set; }
        [Required(ErrorMessage = "Danh sách các danh mục là bắt buộc.")]
        public required List<Guid> CategoryIds { get; set; }
    }
}
