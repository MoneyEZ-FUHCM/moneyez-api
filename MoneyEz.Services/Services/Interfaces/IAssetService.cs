using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.AssetModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IAssetService
    {
        Task<BaseResultModel> GetAssetByIdAsync(Guid id);
        Task<BaseResultModel> GetAllAssetsPaginationAsync(PaginationParameter paginationParameter);
        Task<BaseResultModel> GetAssetsByUserAsync(Guid userId, PaginationParameter paginationParameter);
        Task<BaseResultModel> CreateAssetAsync(CreateAssetModel model);
        Task<BaseResultModel> UpdateAssetAsync(UpdateAssetModel model);
        Task<BaseResultModel> DeleteAssetAsync(Guid id);
    }
}
