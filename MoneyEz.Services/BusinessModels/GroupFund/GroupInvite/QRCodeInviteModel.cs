using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.GroupFund.GroupInvite
{
    public class QRCodeInviteModel
    {
        public string QRCode { get; set; } = "";

        public DateTime ExpiredTime { get; set; }
    }
}
