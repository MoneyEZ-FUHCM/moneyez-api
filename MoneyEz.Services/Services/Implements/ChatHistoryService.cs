using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.BusinessModels.ChatHistoryModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Implements
{
    public class ChatHistoryService : IChatHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ChatHistoryService(IUnitOfWork unitOfWork, IMapper mapper) 
        { 
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ChatHistoryModel> AddMessageToConversation(CreateChatHistoryModel model)
        {
            var sender = await _unitOfWork.UsersRepository.GetUserByEmailAsync(model.Email);
            if (sender == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            var listChatHistory = await _unitOfWork.ChatHistoryRepository.GetAllAsync();

            var userChat = listChatHistory.FirstOrDefault(x => x.UserId == sender.Id);

            if (userChat != null)
            {
                var userChatDetail = await _unitOfWork.ChatHistoryRepository
                    .GetByIdIncludeAsync(userChat.Id, query => query.Include(x => x.ChatMessages));

                if (userChatDetail != null)
                {
                    var newMessage = new ChatMessage
                    {
                        Message = model.Message,
                        Type = model.MessageType,
                        CreatedDate = CommonUtils.GetCurrentTime()
                    };

                    userChatDetail.ChatMessages.Add(newMessage);
                    _unitOfWork.ChatHistoryRepository.UpdateAsync(userChatDetail);

                    await _unitOfWork.SaveAsync();

                    return _mapper.Map<ChatHistoryModel>(userChatDetail);
                }
            }

            return null;
        }

        public async Task<ChatHistoryModel> CreateAndUpdateConversation(CreateChatHistoryModel model)
        {
            var sender = await _unitOfWork.UsersRepository.GetUserByEmailAsync(model.Email);
            if (sender == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            var listChatHistory = await _unitOfWork.ChatHistoryRepository.GetAllAsync();

            var userChat = listChatHistory.FirstOrDefault(x => x.UserId == sender.Id);
            if (userChat != null)
            {
                return await AddMessageToConversation(model);
            }

            var newHistory = new ChatHistory
            {
                Intent = $"Chat conversation of {sender.FullName}",
                IntentUnsign = StringUtils.ConvertToUnSign($"Chat conversation of {sender.FullName}"),
                UserId = sender.Id,
                ChatMessages = new List<ChatMessage>()
                {
                    new ChatMessage
                    {
                        Message = model.Message,
                        Type = model.MessageType,
                        CreatedDate = CommonUtils.GetCurrentTime()
                    }
                }
            };

            await _unitOfWork.ChatHistoryRepository.AddAsync(newHistory);

            await _unitOfWork.SaveAsync();

            return _mapper.Map<ChatHistoryModel>(newHistory);
        }
    }
}
