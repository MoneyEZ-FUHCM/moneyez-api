using MoneyEz.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.SubcategoryModels
{
    public class SubcategoryModel : BaseEntity
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; }
        public string NameUnsign { get; set; }
        public string Description { get; set; }
    }
}
