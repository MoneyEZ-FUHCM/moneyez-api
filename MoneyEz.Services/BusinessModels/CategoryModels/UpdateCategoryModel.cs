using MoneyEz.Services.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.CategoryModels
{
    public class UpdateCategoryModel : CreateCategoryModel
    {
        public required Guid Id { get; set; }
    }
}