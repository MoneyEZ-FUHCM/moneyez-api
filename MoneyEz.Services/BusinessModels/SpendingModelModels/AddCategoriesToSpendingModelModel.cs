using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.SpendingModelModels
{
    public class AddCategoriesToSpendingModelModel
    {
        public Guid SpendingModelId { get; set; } 
        public required List<CategoryPercentageModel> Categories { get; set; }
    }

}

public class CategoryPercentageModel
{
    public Guid CategoryId { get; set; }
    public decimal PercentageAmount { get; set; }
}
