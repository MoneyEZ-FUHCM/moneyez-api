using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface ISubCategoryRepository : IGenericRepository<Subcategory>
    {
        Task<Pagination<Subcategory>> GetPaginatedSubcategoriesAsync(int pageIndex, int pageSize);
        Task<Subcategory?> GetByIdAsync(Guid id);//get subcategory by id
    }
}
