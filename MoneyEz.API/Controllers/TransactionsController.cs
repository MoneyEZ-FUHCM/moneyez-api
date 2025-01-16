using Microsoft.AspNetCore.Mvc;
using MoneyEz.API.Controllers;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Services.BusinessModels.ResultModels;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/transactions")]
    [ApiController]
    public class TransactionsController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Add new transaction for user
        [HttpPost("user")]
        public Task<IActionResult> AddTransactionForUser([FromBody] Transaction transaction)
        {
            return ValidateAndExecute(async () =>
            {
                if (!ModelState.IsValid)
                {
                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Invalid transaction data."
                    };
                }

                await _unitOfWork.Transactions.AddAsync(transaction);
                _unitOfWork.Save();

                return new BaseResultModel
                {
                    Status = StatusCodes.Status201Created,
                    Message = "Transaction created successfully for user.",
                    Data = transaction
                };
            });
        }

        // Add new transaction for group
        [HttpPost("group")]
        public Task<IActionResult> AddTransactionForGroup([FromBody] Transaction transaction)
        {
            return ValidateAndExecute(async () =>
            {
                if (!ModelState.IsValid)
                {
                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Invalid transaction data."
                    };
                }

                await _unitOfWork.Transactions.AddAsync(transaction);
                _unitOfWork.Save();

                return new BaseResultModel
                {
                    Status = StatusCodes.Status201Created,
                    Message = "Transaction created successfully for group.",
                    Data = transaction
                };
            });
        }

        // Get all transactions
        [HttpGet]
        public async Task<IActionResult> GetAllTransactions([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            return await ValidateAndExecute(async () =>
            {
                var paginatedTransactions = await _unitOfWork.Transactions.GetPaginatedTransactionsAsync(pageIndex, pageSize);
                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = paginatedTransactions
                };
            });
        }


        // Get transaction by ID
        [HttpGet("{id:guid}")]
        public Task<IActionResult> GetTransactionById(Guid id)
        {
            return ValidateAndExecute(async () =>
            {
                var transaction = await _unitOfWork.Transactions.GetTransactionByIdIncludeAsync(id);

                if (transaction == null)
                {
                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status404NotFound,
                        Message = "Transaction not found."
                    };
                }

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = transaction
                };
            });
        }
    }
}