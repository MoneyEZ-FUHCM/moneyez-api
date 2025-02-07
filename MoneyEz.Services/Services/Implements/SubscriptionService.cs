using System;
using System.Threading.Tasks;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.Subscription;
using MoneyEz.Services.Services.Interfaces;
using AutoMapper;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Services.BusinessModels.ResultModels;
using Microsoft.AspNetCore.Http;
using MoneyEz.Services.Constants;

namespace MoneyEz.Services.Services.Implements
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public SubscriptionService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResultModel> CreateSubscriptionAsync(CreateSubscriptionModel model)
        {
            // Map the model to a new Subscription entity
            var subscription = _mapper.Map<Subscription>(model);

            // Add the subscription to the repository and save changes
            await _unitOfWork.SubscriptionRepository.AddAsync(subscription);
            await _unitOfWork.SaveAsync();

            // Return a success result with the created subscription
            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Data = subscription,
                Message = MessageConstants.SUBSCRIPTION_CREATE_SUCCESS_MESSAGE
            };
        }
    }
}
