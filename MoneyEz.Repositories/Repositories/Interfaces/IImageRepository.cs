using MoneyEz.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface IImageRepository : IGenericRepository<Image>
    {
        Task<List<Image>> GetImagesByEntityAsync(Guid entityId, string entityName);
    }
}
