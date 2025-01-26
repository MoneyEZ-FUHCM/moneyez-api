using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.GroupMember;
using MoneyEz.Services.Services.Interfaces;
using AutoMapper;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Services.BusinessModels.ResultModels;
using Microsoft.AspNetCore.Http;
using MoneyEz.Services.Constants;

namespace MoneyEz.Services.Services.Implements
{
    public class GroupMemberService : IGroupMemberService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public GroupMemberService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<GroupMember> AddGroupMemberAsync(CreateGroupMemberModel model)
        {
            var groupMember = _mapper.Map<GroupMember>(model);

            await _unitOfWork.GroupMemberRepository.AddAsync(groupMember);

            await _unitOfWork.SaveAsync();
            return groupMember;
        }

    }
}