﻿using AutoMapper;
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
            // spending model mapper
            SpendingModelMapperConfig();
            // user spending model mapper
            UserSpendingModelMapperConfig();

            //financialgoal
            FinancialGoalMapperConfig();

            //category mapper
            CategoryMapperConfig();
            //subcategory mapper
            SubcategoryMapperConfig();
            //transaction mapper
            TransactionMapperConfig();
            // group fund mapper
            GroupFundConfig();

            // report
            FinancialReportMapperConfig();
            // chat mapper
            ChatMapperConfig();

            // asset mapper
            AssetMapperConfig();

            // liability mapper
            LiabilityMapperConfig();

            // notification mapper
            NotificationMapperConfig();

            // bank account mapper
            BankAccountMapperConfig();

            // image mapper
            ImageMapperConfig();

            // group fund log
            GroupFundLogConfig();
        }

        partial void UserMapperConfig();
        partial void SpendingModelMapperConfig();
        partial void UserSpendingModelMapperConfig();
        partial void FinancialGoalMapperConfig();
        partial void FinancialReportMapperConfig();
        partial void CategoryMapperConfig();
        partial void SubcategoryMapperConfig();
        partial void TransactionMapperConfig();
        partial void GroupFundConfig();
        partial void ChatMapperConfig();
        partial void AssetMapperConfig();
        partial void LiabilityMapperConfig();

        partial void NotificationMapperConfig();

        partial void BankAccountMapperConfig();

        partial void ImageMapperConfig();
        partial void GroupFundLogConfig();
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
