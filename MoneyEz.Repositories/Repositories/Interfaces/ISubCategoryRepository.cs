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
        Task<List<Subcategory>> GetAllAsync();// get all subcategories
        Task<Subcategory?> GetByIdAsync(Guid id);//get subcategory by id
    }
}
