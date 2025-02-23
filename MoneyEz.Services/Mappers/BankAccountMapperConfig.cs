using AutoMapper;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels;
using MoneyEz.Services.BusinessModels.BankAccountModels;
using MoneyEz.Services.BusinessModels.ChatHistoryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig : Profile
    {
        partial void BankAccountMapperConfig()
        {
            CreateMap<BankAccount, BankAccountModel>();
            CreateMap<CreateBankAccountModel, BankAccount>();
            CreateMap<Pagination<BankAccount>, Pagination<BankAccountModel>>().ConvertUsing<PaginationConverter<BankAccount, BankAccountModel>>();
        }
    }
}
