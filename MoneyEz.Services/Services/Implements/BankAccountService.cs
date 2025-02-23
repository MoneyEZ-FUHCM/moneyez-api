using AutoMapper;
using Microsoft.AspNetCore.Http;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
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
            var bankAccounts = await _unitOfWork.BankAccountRepository.ToPagination(paginationParameter);
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
            var account = await _unitOfWork.BankAccountRepository.GetByIdAsync(id);
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
                .ToPaginationIncludeAsync(paginationParameter, filter: uid => uid.UserId == user.Id);
            var accountModels = _mapper.Map<List<BankAccountModel>>(accounts);
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
            var existingAccount = await _unitOfWork.BankAccountRepository
                .GetByConditionAsync(filter: x => x.AccountNumber == model.AccountNumber);
            if (existingAccount.Any())
            {
                throw new DefaultException("Bank account number already exists", 
                    MessageConstants.BANK_ACCOUNT_ALREADY_EXISTS);
            }

            var bankAccount = _mapper.Map<BankAccount>(model);
            bankAccount.UserId = user.Id;
            
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
                throw new DefaultException("Bank account is registered in group", MessageConstants.BANK_ACCOUNT_REGISTERED_IN_GROUP);
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
