using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface ICategoriesRepository : IGenericRepository<Category>
    {
        Task<Pagination<Category>> GetCategoriesByFilter(PaginationParameter paginationParameter, CategoryFilter filter);
    }
}
