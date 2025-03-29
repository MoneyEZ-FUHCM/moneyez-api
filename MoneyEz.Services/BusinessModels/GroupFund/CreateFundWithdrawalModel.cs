using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.GroupFund
{
    public class CreateFundWithdrawalModel
    {
        public Guid GroupId { get; set; }

        public decimal Amount { get; set; }

        public string? Description { get; set; }

        public List<string> Images { get; set; } = new List<string>();
    }
}
