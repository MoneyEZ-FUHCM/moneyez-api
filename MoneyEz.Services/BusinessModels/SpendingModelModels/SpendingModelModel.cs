using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.CategoryModels;
using System;
using System.Collections.Generic;

namespace MoneyEz.Services.BusinessModels.SpendingModelModels
{
    public class SpendingModelModel : BaseEntity
    {
        public required string Name { get; set; }
        public string NameUnsign { get; set; }
        public string Description { get; set; }
        public bool? IsTemplate { get; set; }
        public List<SpendingModelCategoryModel> Categories { get; set; }
    }
}
