using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyEz.Repositories.Enums;

namespace MoneyEz.Services.BusinessModels.GroupFund
{
    public class GetAllGroupFundModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? CurrentBalance { get; set; }
        public CommonsStatus? Status { get; set; }
        public VisibilityEnum? Visibility { get; set; }
    }
}