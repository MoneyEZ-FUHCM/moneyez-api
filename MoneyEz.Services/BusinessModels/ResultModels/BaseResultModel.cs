using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.ResultModels
{
    public class BaseResultModel<T>
    {
        public required int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public T Data { get; set; } = default(T);
    }
}
