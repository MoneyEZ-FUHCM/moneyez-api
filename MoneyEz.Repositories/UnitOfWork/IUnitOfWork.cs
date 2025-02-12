using MoneyEz.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        // add interface repository here

        IUserRepository UsersRepository { get; }

        //spending model
        ISpendingModelRepository SpendingModelRepository { get; }

        //spending model category
        ISpendingModelCategoryRepository SpendingModelCategoryRepository { get; }

        //category
        ICategoriesRepository CategoriesRepository { get; }

        //subcategory
        ISubcategoryRepository SubcategoryRepository { get; }

        //group
        IGroupRepository GroupRepository { get; }
        IGroupFundLogRepository GroupFundLogRepository { get; }
        IGroupMemberRepository GroupMemberRepository { get; }

        //asset and liability
        IAssetRepository AssetRepository { get; }
        ILiabilityRepository LiabilityRepository { get; }


        int Save();
        void Commit();
        void Rollback();
        Task SaveAsync();
    }
}
