using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface ISubcategoryRepository : IGenericRepository<Subcategory>
    {
        Task<Pagination<Subcategory>> GetSubcategoriesByFilter(PaginationParameter paginationParameter, SubcategoryFilter filter);
    }
}
