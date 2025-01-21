using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Entities
{
    public class Image : BaseEntity
    {
        public Guid EntityId { get; set; }

        public string EntityName { get; set; } = "";

        public string ImageUrl { get; set; } = "";
    }
}
