using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Settings
{
    public class RedisSettings
    {
        public string ConnectionString { get; set; }
        public string InstanceName { get; set; }
        public int DefaultExpiryMinutes { get; set; }
    }
}
