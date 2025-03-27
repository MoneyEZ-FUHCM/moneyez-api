using Microsoft.AspNetCore.Mvc;
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
        [FromQuery(Name = "command")]
        public string Command { get; set; } = string.Empty;

        [FromQuery(Name = "query")]
        public string? Query { get; set; }

        [FromQuery(Name = "data")]
        public object? Data { get; set; } = new object();
    }
}
