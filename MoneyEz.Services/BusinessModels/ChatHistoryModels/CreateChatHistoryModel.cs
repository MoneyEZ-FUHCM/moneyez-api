using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.ChatHistoryModels
{
    public class CreateChatHistoryModel
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string Message { get; set; } = "";

        public MessageType MessageType { get; set; }
    }
}
