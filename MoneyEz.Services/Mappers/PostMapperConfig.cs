using AutoMapper;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.BankAccountModels;
using MoneyEz.Services.BusinessModels.PostModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig : Profile
    {
        partial void PostMapperConfig() 
        { 
            CreateMap<CreatePostModel, Post>();

            CreateMap<Post, PostModel>();

            CreateMap<Pagination<Post>, Pagination<PostModel>>().ConvertUsing<PaginationConverter<Post, PostModel>>();
        }
    }
}
