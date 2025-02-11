using AutoMapper;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.ChatHistoryModels;
using MoneyEz.Services.BusinessModels.UserModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig : Profile
    {
        partial void ChatMapperConfig()
        {
            CreateMap<ChatMessage, ChatMessageModel>();
            CreateMap<ChatHistory, ChatHistoryModel>().ForMember(dest => dest.ChatMessages, opt => opt.MapFrom(src => src.ChatMessages));
            CreateMap<Pagination<ChatHistory>, Pagination<ChatHistoryModel>>().ConvertUsing<PaginationConverter<ChatHistory, ChatHistoryModel>>();
        }
    }
}
