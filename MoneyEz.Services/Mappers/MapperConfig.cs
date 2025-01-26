using AutoMapper;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
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
        public MapperConfig()
        {
            // user mapper
            UserMapperConfig();
            //category mapper
            CategoryMapperConfig();
            // group fund mapper
            GroupFundConfig();

            // transaction mapper
        }

        partial void UserMapperConfig();
        partial void CategoryMapperConfig();
        partial void GroupFundConfig();
    }

    public class PaginationConverter<TSource, TDestination> : ITypeConverter<Pagination<TSource>, Pagination<TDestination>>
    {
        public Pagination<TDestination> Convert(Pagination<TSource> source, Pagination<TDestination> destination, ResolutionContext context)
        {
            var mappedItems = context.Mapper.Map<List<TDestination>>(source);
            return new Pagination<TDestination>(mappedItems, source.TotalCount, source.CurrentPage, source.PageSize);
        }
    }
}
