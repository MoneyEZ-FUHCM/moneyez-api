﻿using MoneyEz.Repositories.Entities;

namespace MoneyEz.Repositories.Repositories.Interfaces
{
    public interface IUserSpendingModelRepository : IGenericRepository<UserSpendingModel>
    {
        Task<UserSpendingModel?> GetCurrentSpendingModelByUserId(Guid userId);
    }
}
