using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.CategoryModels;
using System;
using System.Collections.Generic;

namespace MoneyEz.Services.BusinessModels.SubcategoryModels
{
    public class SubcategoryModel : BaseEntity
    {
        public string? Name { get; set; }
        public string? NameUnsign { get; set; }
        public string? Description { get; set; }
        public string? Code { get; set; }
        public string? Icon { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryCode { get; set; }
    }
}
