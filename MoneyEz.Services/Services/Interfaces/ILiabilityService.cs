using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.LiabilityModels;
using MoneyEz.Services.BusinessModels.ResultModels;


namespace MoneyEz.Services.Services.Interfaces
{
    public interface ILiabilityService
    {
        Task<BaseResultModel> GetLiabilityByIdAsync(Guid id);
        Task<BaseResultModel> GetAllLiabilitiesPaginationAsync(PaginationParameter paginationParameter);
        Task<BaseResultModel> GetLiabilitiesByUserAsync(PaginationParameter paginationParameter);
        Task<BaseResultModel> CreateLiabilityAsync(CreateLiabilityModel model);
        Task<BaseResultModel> UpdateLiabilityAsync(UpdateLiabilityModel model);
        Task<BaseResultModel> DeleteLiabilityAsync(Guid id);
    }
}
