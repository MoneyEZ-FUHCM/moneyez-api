﻿using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.ImageModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.GroupFund
{
    public class GroupFundModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? NameUnsign { get; set; }
        public string? Description { get; set; }
        public decimal? CurrentBalance { get; set; }
        public string? Status { get; set; }
        public string? Visibility { get; set; }
        public string? ImageUrl { get; set; }
        public List<GroupMemberModel> GroupMembers { get; set; } = new List<GroupMemberModel>();
        //public ImageModel? Image { get; set; }
    }
}
