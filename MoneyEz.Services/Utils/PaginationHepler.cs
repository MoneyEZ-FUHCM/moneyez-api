using AutoMapper;
using Microsoft.AspNetCore.Http;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using MoneyEz.Services.BusinessModels.ResultModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Utils
{
    public static class PaginationHelper
    {
        public static ModelPaging GetPaginationResult<TEntity, TModel>(Pagination<TEntity> entityList, List<TModel> modelList)
        {
            var paginatedResult = new Pagination<TModel>(modelList,
                entityList.TotalCount,
                entityList.CurrentPage,
                entityList.PageSize);

            var metaData = new
            {
                entityList.TotalCount,
                entityList.PageSize,
                entityList.CurrentPage,
                entityList.TotalPages,
                entityList.HasNext,
                entityList.HasPrevious
            };

            return new ModelPaging
            {
                Data = paginatedResult,
                MetaData = metaData
            };
        }
    }

}
