using Microsoft.EntityFrameworkCore;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Repositories.Implements
{
    public class ImageRepository : GenericRepository<Image>, IImageRepository
    {
        private readonly MoneyEzContext _context;

        public ImageRepository(MoneyEzContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Image>> GetImagesByEntityAsync(Guid entityId, string entityName)
        {
            return await _context.Images
                                 .Where(i => i.EntityId == entityId && i.EntityName == entityName && !i.IsDeleted)
                                 .ToListAsync();
        }
    }
}
