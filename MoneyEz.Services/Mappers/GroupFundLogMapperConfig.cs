using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.GroupFundLogModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig
    {
        partial void GroupFundLogConfig()
        {
            CreateMap<GroupFundLog, GroupFundLogModel>();
            CreateMap(typeof(Pagination<>), typeof(Pagination<>))
                .ConvertUsing(typeof(PaginationConverter<,>));

        }
    }
}
