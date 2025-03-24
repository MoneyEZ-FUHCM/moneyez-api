using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.WebhookModels
{
    public class WebhookRegisterModel
    {
        [Required(ErrorMessage = "Account bank Id is require")]
        public required Guid AccountBankId { get; set; }
    }
}
