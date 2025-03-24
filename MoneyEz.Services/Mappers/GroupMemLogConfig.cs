using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.GroupMemLogModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig
    {
        partial void GroupMemLogConfig()
        {
            CreateMap<GroupMemberLog, GroupMemLogModel>();
            CreateMap<GroupMemLogModel, GroupMemberLog>();

            // Sử dụng PaginationConverter để ánh xạ Pagination<GroupMemberLog> sang Pagination<GroupMemLogModel>
            CreateMap<Pagination<GroupMemberLog>, Pagination<GroupMemLogModel>>()
                .ConvertUsing<PaginationConverter<GroupMemberLog, GroupMemLogModel>>();

        }
    }
}
