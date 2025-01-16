using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.ResultModels;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/fixed-transactions")]
    [ApiController]
    public class FixedTransactionsController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public FixedTransactionsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Add New Fixed Transaction
        [HttpPost]
        public Task<IActionResult> AddFixedTransaction([FromBody] FixedTransaction fixedTransaction)
        {
            return ValidateAndExecute(async () =>
            {
                if (!ModelState.IsValid)
                {
                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Invalid fixed transaction data."
                    };
                }

                await _unitOfWork.FixedTransactions.AddAsync(fixedTransaction);
                _unitOfWork.Save();

                return new BaseResultModel
                {
                    Status = StatusCodes.Status201Created,
                    Message = "Fixed transaction created successfully.",
                    Data = fixedTransaction
                };
            });
        }

        // Get All Fixed Transactions with Pagination
        [HttpGet]
        public Task<IActionResult> GetAllFixedTransactions([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            return ValidateAndExecute(async () =>
            {
                var paginatedResult = await _unitOfWork.FixedTransactions.GetPaginatedFixedTransactionsAsync(pageIndex, pageSize);
                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = paginatedResult
                };
            });
        }

        // Get Fixed Transaction by ID
        [HttpGet("{id:guid}")]
        public Task<IActionResult> GetFixedTransactionById(Guid id)
        {
            return ValidateAndExecute(async () =>
            {
                var fixedTransaction = await _unitOfWork.FixedTransactions.GetByIdAsync(id);

                if (fixedTransaction == null)
                {
                    return new BaseResultModel
                    {
                        Status = StatusCodes.Status404NotFound,
                        Message = "Fixed transaction not found."
                    };
                }

                return new BaseResultModel
                {
                    Status = StatusCodes.Status200OK,
                    Data = fixedTransaction
                };
            });
        }
    }
}
