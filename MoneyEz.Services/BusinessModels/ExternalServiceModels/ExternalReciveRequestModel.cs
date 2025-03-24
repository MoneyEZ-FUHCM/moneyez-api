using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.ExternalServiceModels
{
    public class ExternalReciveRequestModel
    {
        [Required(ErrorMessage = "Command is required")]
        public string Command { get; set; } = string.Empty;
        public object? Data { get; set; } = new object();
    }
}
