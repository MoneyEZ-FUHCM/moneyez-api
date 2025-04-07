using AutoMapper;
using MoneyEz.Services.BusinessModels.KnowledgeModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig : Profile
    {
        partial void KnowledgeMapperConfig()
        {
            CreateMap<ResponseKnowledgeModel, KnowledgeModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.Parse(src.CreatedDate)));
        }
    }
}
