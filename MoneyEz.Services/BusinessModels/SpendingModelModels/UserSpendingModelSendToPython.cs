using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.SpendingModelModels
{
    public class UserSpendingModelSendToPython
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<CategorySendToPython> Categories { get; set; } = new List<CategorySendToPython>();
    }

    public class CategorySendToPython
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
