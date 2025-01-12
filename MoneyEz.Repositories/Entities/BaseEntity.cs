using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Entities
{
    public class BaseEntity
    {
        public required string Id { get; set; } = "";

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; } = "";

        public DateTime UpdatedDate { get; set; }

        public string UpdatedBy { get; set; } = "";

        public bool IsDeleted { get; set; } = false;
    }
}
