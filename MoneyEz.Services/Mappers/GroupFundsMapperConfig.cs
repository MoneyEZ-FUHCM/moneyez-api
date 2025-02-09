using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.GroupFund;
using MoneyEz.Services.BusinessModels.GroupMember;

namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig : Profile
    {

        partial void GroupFundConfig()
        {
            CreateMap<CreateGroupModel, GroupFund>();
            CreateMap<GroupFund, GroupFundModel>();
        }
    }
}