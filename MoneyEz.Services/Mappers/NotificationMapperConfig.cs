﻿using AutoMapper;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.NotificationModels;
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
        partial void NotificationMapperConfig()
        {
            CreateMap<Notification, NotificationModel>();
            CreateMap<Pagination<Notification>, Pagination<NotificationModel>>().ConvertUsing<PaginationConverter<Notification, NotificationModel>>();
        }
    }
}
