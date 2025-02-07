using System;

namespace MoneyEz.Services.BusinessModels.Subscription
{
    public class CreateSubscriptionModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
