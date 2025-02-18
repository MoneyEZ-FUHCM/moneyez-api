using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.CategoryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.SubcategoryModels
{
    public class SubcategoryModel : BaseEntity
    {
        public required string Name { get; set; }
        public required string NameUnsign { get; set; }
        public required string Description { get; set; }

        public List<CategoryModel> Categories { get; set; } = new List<CategoryModel>();
    }
}
