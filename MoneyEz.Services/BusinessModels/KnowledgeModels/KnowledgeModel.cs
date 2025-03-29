using MoneyEz.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.KnowledgeModels
{
    public class KnowledgeModel : BaseEntity
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Size { get; set; }
    }
}
