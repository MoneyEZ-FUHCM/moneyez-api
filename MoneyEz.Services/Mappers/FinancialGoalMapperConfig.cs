﻿using AutoMapper;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.FinancialGoalModels;
using MoneyEz.Services.Utils;
using MoneyEz.Repositories.Enums;

namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig
    {
        partial void FinancialGoalMapperConfig()
        {
            // Mapping cho Financial Goal cá nhân
            CreateMap<FinancialGoal, PersonalFinancialGoalModel>()
                .ForMember(dest => dest.SubcategoryName, opt => opt.MapFrom(src => src.Subcategory.Name))
                .ForMember(dest => dest.SubcategoryIcon, opt => opt.MapFrom(src => src.Subcategory.Icon))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Prediction, opt => opt.Ignore());

            CreateMap<AddPersonalFinancialGoalModel, FinancialGoal>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => FinancialGoalStatus.ACTIVE))  
                .ForMember(dest => dest.ApprovalStatus, opt => opt.MapFrom(src => ApprovalStatus.APPROVED)); 

            CreateMap<UpdatePersonalFinancialGoalModel, FinancialGoal>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => StringUtils.ConvertToUnSign(src.Name)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => FinancialGoalStatus.ACTIVE)) 
                .ForMember(dest => dest.ApprovalStatus, opt => opt.MapFrom(src => ApprovalStatus.APPROVED));

            CreateMap<Pagination<FinancialGoal>, Pagination<PersonalFinancialGoalModel>>()
                .ConvertUsing<PaginationConverter<FinancialGoal, PersonalFinancialGoalModel>>();

            // Mapping cho Financial Goal nhóm
            CreateMap<FinancialGoal, GroupFinancialGoalModel>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => src.NameUnsign))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status)) 
                .ForMember(dest => dest.ApprovalStatus, opt => opt.MapFrom(src => src.ApprovalStatus)); 

            CreateMap<AddGroupFinancialGoalModel, FinancialGoal>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => StringUtils.ConvertToUnSign(src.Name)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => FinancialGoalStatus.PENDING)) 
                .ForMember(dest => dest.ApprovalStatus, opt => opt.MapFrom(src => ApprovalStatus.PENDING));

            CreateMap<UpdateGroupFinancialGoalModel, FinancialGoal>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => StringUtils.ConvertToUnSign(src.Name)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => FinancialGoalStatus.PENDING))  
                .ForMember(dest => dest.ApprovalStatus, opt => opt.MapFrom(src => ApprovalStatus.PENDING));

            CreateMap<FinancialGoal, GroupFinancialGoalDetailModel>()
                .IncludeBase<FinancialGoal, GroupFinancialGoalModel>()
                .ForMember(dest => dest.MemberContributions, opt => opt.Ignore())
                .ForMember(dest => dest.TotalCurrentAmount, opt => opt.Ignore())
                .ForMember(dest => dest.CompletionPercentage, opt => opt.Ignore());

            CreateMap<Pagination<FinancialGoal>, Pagination<GroupFinancialGoalModel>>()
                .ConvertUsing<PaginationConverter<FinancialGoal, GroupFinancialGoalModel>>();
        }
    }
}
