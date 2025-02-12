using AutoMapper;
using MoneyEz.Repositories.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Utils
{
    public class PaginationUtils
    {
        private readonly IMapper _mapper;

        public PaginationUtils(IMapper mapper)
        {
            _mapper = mapper;
        }

        public (Pagination<TDestination> pagination, object metaData) CreatePagedResponse<TSource, TDestination>(Pagination<TSource> source)
        {
            var mappedItems = _mapper.Map<List<TDestination>>(source);

            var pagination = new Pagination<TDestination>(
                mappedItems,
                source.TotalCount,
                source.CurrentPage,
                source.PageSize
            );

            var metaData = new
            {
                source.TotalCount,
                source.PageSize,
                source.CurrentPage,
                source.TotalPages,
                source.HasNext,
                source.HasPrevious
            };

            return (pagination, metaData);
        }
    }
}
