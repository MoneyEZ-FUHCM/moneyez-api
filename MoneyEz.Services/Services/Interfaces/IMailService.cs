using MoneyEz.Services.BusinessModels.EmailModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IMailService
    {
        public Task SendEmailAsync(MailRequest mailRequest);

        public Task SendEmailAsync_v2(MailRequest mailRequest);
    }
}
