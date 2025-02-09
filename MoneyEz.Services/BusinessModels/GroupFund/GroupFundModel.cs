using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.GroupFund
{
    public class GroupFundModel
    {
        public string? Name { get; set; }

        public string? NameUnsign { get; set; }

        public string? Description { get; set; }

        public decimal? CurrentBalance { get; set; }

        public CommonsStatus? Status { get; set; }

        public VisibilityEnum? Visibility { get; set; }
    }
}
