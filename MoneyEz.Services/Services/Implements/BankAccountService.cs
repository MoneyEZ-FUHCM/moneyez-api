using AutoMapper;
using Microsoft.AspNetCore.Http;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.Repositories.Interfaces;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Services.BusinessModels;
using MoneyEz.Services.BusinessModels.BankAccountModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.Utils;
namespace MoneyEz.Services.Services.Implements
{
    public class BankAccountService : IBankAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimsService _claimsService;

        public BankAccountService(IUnitOfWork unitOfWork, IMapper mapper, IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsService = claimsService;
        }

        public async Task<BaseResultModel> GetAllBankAccountsPaginationAsync(PaginationParameter paginationParameter)
        {
            var bankAccounts = await _unitOfWork.BankAccountRepository.ToPaginationIncludeAsync(paginationParameter, filter: a => !a.IsDeleted);
            var accountModels = _mapper.Map<List<BankAccountModel>>(bankAccounts);
            var paginatedResult = PaginationHelper.GetPaginationResult(bankAccounts, accountModels);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = paginatedResult,
                Message = MessageConstants.BANK_ACCOUNT_LIST_GET_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> GetBankAccountByIdAsync(Guid id)
        {
            var account = await _unitOfWork.BankAccountRepository.GetByIdIncludeAsync(id, filter: a => !a.IsDeleted);
            if (account == null)
            {
                throw new NotExistException("Bank account not found", MessageConstants.BANK_ACCOUNT_NOT_FOUND);
            }

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = _mapper.Map<BankAccountModel>(account),
                Message = MessageConstants.BANK_ACCOUNT_GET_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> GetBankAccountsByUserAsync(PaginationParameter paginationParameter)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (user == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }
            var accounts = await _unitOfWork.BankAccountRepository
                .ToPaginationIncludeAsync(paginationParameter, filter: a => a.UserId == user.Id && !a.IsDeleted);
            var accountModels = _mapper.Map<List<BankAccountModel>>(accounts);
            
            // Check if user is a leader in any groups
            var groupsAsLeader = await _unitOfWork.GroupMemberRepository.GetByConditionAsync(
                filter: m => m.UserId == user.Id && 
                            m.Role == RoleGroup.LEADER && 
                            m.Status == GroupMemberStatus.ACTIVE && 
                            !m.IsDeleted);
            
            if (groupsAsLeader.Any())
            {
                // Get all group funds where the user is a leader
                var groupIds = groupsAsLeader.Select(g => g.GroupId).ToList();
                var groupFunds = await _unitOfWork.GroupFundRepository.GetByConditionAsync(
                    filter: gf => groupIds.Contains(gf.Id) && 
                                 gf.AccountBankId != null);
                
                // Create a hashset for faster lookups
                var accountsInGroups = groupFunds
                    .Where(gf => gf.AccountBankId.HasValue)
                    .Select(gf => gf.AccountBankId.Value)
                    .ToHashSet();
                
                // Update IsHasGroup property for each account
                foreach (var account in accountModels)
                {
                    account.IsHasGroup = accountsInGroups.Contains(account.Id);
                }
            }
            
            var paginatedResult = PaginationHelper.GetPaginationResult(accounts, accountModels);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = paginatedResult,
                Message = MessageConstants.BANK_ACCOUNT_LIST_GET_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> CreateBankAccountAsync(CreateBankAccountModel model)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (user == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            // Check for duplicate names
            var existingAccounts = await _unitOfWork.BankAccountRepository
                .GetByConditionAsync(filter: x => x.AccountNumber == model.AccountNumber);
            
            var existingAccount = existingAccounts.FirstOrDefault();

            if (existingAccount != null)
            {
                if (existingAccount.IsDeleted && existingAccount.UserId == user.Id)
                {
                    // update the deleted account
                    existingAccount.IsDeleted = false;
                    existingAccount.UpdatedBy = user.Email;
                    _unitOfWork.BankAccountRepository.UpdateAsync(existingAccount);
                    await _unitOfWork.SaveAsync();

                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status201Created,
                        Data = _mapper.Map<BankAccountModel>(existingAccount),
                        Message = MessageConstants.BANK_ACCOUNT_CREATE_SUCCESS_MESSAGE
                    };
                }
                else
                {
                    throw new DefaultException("Bank account number already exists", MessageConstants.BANK_ACCOUNT_ALREADY_EXISTS);
                }
            }

            var bankAccount = _mapper.Map<BankAccount>(model);
            bankAccount.UserId = user.Id;
            bankAccount.CreatedBy = user.Email;
            
            await _unitOfWork.BankAccountRepository.AddAsync(bankAccount);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Data = _mapper.Map<BankAccountModel>(bankAccount),
                Message = MessageConstants.BANK_ACCOUNT_CREATE_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> UpdateBankAccountAsync(UpdateBankAccountModel model)
        {
            var existingAccount = await _unitOfWork.BankAccountRepository.GetByIdAsync(model.Id);
            if (existingAccount == null)
            {
                throw new NotExistException("Bank account not found", MessageConstants.BANK_ACCOUNT_NOT_FOUND);
            }

            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (user == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            if (existingAccount.UserId != user.Id)
            {
                throw new DefaultException("Access denied", MessageConstants.BANK_ACCOUNT_ACCESS_DENIED);
            }

            _mapper.Map(model, existingAccount);
            existingAccount.UpdatedBy = user.Email;
            _unitOfWork.BankAccountRepository.UpdateAsync(existingAccount);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = _mapper.Map<BankAccountModel>(existingAccount),
                Message = MessageConstants.BANK_ACCOUNT_UPDATE_SUCCESS_MESSAGE
            };
        }

        public async Task<BaseResultModel> DeleteBankAccountAsync(Guid id)
        {
            var account = await _unitOfWork.BankAccountRepository.GetByIdAsync(id);
            if (account == null)
            {
                throw new NotExistException("Bank account not found", MessageConstants.BANK_ACCOUNT_NOT_FOUND);
            }

            if (account.WebhookSecretKey != null && account.WebhookUrl != null)
            {
                throw new DefaultException("Webhook is registered for this bank account. Can not delete.", MessageConstants.WEBHOOK_SECRET_EXISTED);
            }

            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (user == null)
            {
                throw new NotExistException("", MessageConstants.ACCOUNT_NOT_EXIST);
            }

            if (account.UserId != user.Id)
            {
                throw new DefaultException("Access denied", MessageConstants.BANK_ACCOUNT_ACCESS_DENIED);
            }

            // check bank account is register in group
            var groupFund = await _unitOfWork.GroupFundRepository.GetByConditionAsync(filter: a => a.AccountBankId == id);
            if (groupFund.Any())
            {
                throw new DefaultException("Bank account is registered in group. Can not delete.", MessageConstants.BANK_ACCOUNT_REGISTERED_IN_GROUP);
            }

            _unitOfWork.BankAccountRepository.SoftDeleteAsync(account);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.BANK_ACCOUNT_DELETE_SUCCESS_MESSAGE
            };
        }
    }
}
