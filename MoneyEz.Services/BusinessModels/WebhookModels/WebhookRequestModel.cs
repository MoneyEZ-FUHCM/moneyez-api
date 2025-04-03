using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.WebhookModels
{
    public class WebhookRequestModel
    {
        public required string Url { get; set; }

        public required string Secret { get; set; }

        public required string AccountNumber { get; set; }

        public required string AccountHolder { get; set; }
    }

    public class ValidateBankAccountRequestModel
    {
        public required string AccountHolder { get; set; }
        public required string AccountNumber { get; set; }
    }
}
