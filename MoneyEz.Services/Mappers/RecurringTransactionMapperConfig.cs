using AutoMapper;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.RecurringTransactionModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Mappers
{
    public class RecurringTransactionMapperConfig : Profile
    {
        public RecurringTransactionMapperConfig()
        {
            CreateMap<RecurringTransaction, RecurringTransactionModel>()
                .ForMember(dest => dest.SubcategoryName, opt => opt.MapFrom(src => src.Subcategory.Name));

            CreateMap<CreateRecurringTransactionModel, RecurringTransaction>();

            CreateMap<UpdateRecurringTransactionModel, RecurringTransaction>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
