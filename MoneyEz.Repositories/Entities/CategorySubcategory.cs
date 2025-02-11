using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Entities
{
    public partial class CategorySubcategory : BaseEntity
    {
        public Guid CategoryId { get; set; }
        public Guid SubcategoryId { get; set; }

        public virtual Category Category { get; set; }
        public virtual Subcategory Subcategory { get; set; }
    }
}
