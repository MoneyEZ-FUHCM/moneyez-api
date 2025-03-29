using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.KnowledgeModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Services.Interfaces;

namespace MoneyEz.API.Controllers
{
    [Route("api/knowledges")]
    [ApiController]
    public class KnowledgesController : BaseController
    {
        private readonly IAIKnowledgeService _aIKnowledgeService;

        public KnowledgesController(IAIKnowledgeService aIKnowledgeService)
        {
            _aIKnowledgeService = aIKnowledgeService;
        }

        [HttpGet]
        public Task<IActionResult> GetKnowledgesAsync([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _aIKnowledgeService.GetKnowledgesAsync(paginationParameter));
        }

        [HttpPost]
        public async Task<IActionResult> CreateKnowledgeAsync(CreateKnowledgeModel model)
        {
            if (!model.ValidateFileType())
            {
                return BadRequest(new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = "INVALID_FILE_TYPE",
                    Message = "Invalid file type. Allowed types are: PDF, DOC, DOCX, and TXT"
                });
            }

            if (model.File.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new BaseResultModel
                {
                    Status = StatusCodes.Status400BadRequest,
                    ErrorCode = "FILE_TOO_LARGE",
                    Message = $"File size exceeds maximum limit of {10 * 1024 * 1024 / (1024 * 1024)}MB"
                });
            }

            return await ValidateAndExecute(() => _aIKnowledgeService.CreateKnowledgeAsync(model));
        }

        [HttpDelete("{id}")]
        public Task<IActionResult> DeleteKnowledgeAsync(Guid id)
        {
            return ValidateAndExecute(() => _aIKnowledgeService.DeleteKnowledgeAsync(id));
        }
    }
}
