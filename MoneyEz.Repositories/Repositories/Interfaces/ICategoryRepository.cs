using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoneyEz.Repositories.Entities;


namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<Category?> GetByNameAsync(string name); // tìm danh mục theo tên không phân biệt hoa thường

        Task<bool> IsCategoryExistsAsync(string name); // kiểm tra xem danh mục đã tồn tại chưa

        Task<List<Category>> GetPagedCategoriesAsync(int pageIndex, int pageSize); // Lấy danh sách các mục có phân trang


    }
}
