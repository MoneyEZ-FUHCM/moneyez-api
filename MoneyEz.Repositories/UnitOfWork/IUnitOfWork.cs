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

        //user spending model   
        IUserSpendingModelRepository UserSpendingModelRepository { get; }

        //spending model category
        ISpendingModelCategoryRepository SpendingModelCategoryRepository { get; }

        //financialgoal
        IFinancialGoalRepository FinancialGoalRepository { get; } 

        //category
        ICategoriesRepository CategoriesRepository { get; }

        //subcategory
        ISubcategoryRepository SubcategoryRepository { get; }

        //categorysubcategory
        ICategorySubcategoryRepository CategorySubcategoryRepository { get; }

        //transaction
        ITransactionRepository TransactionsRepository { get; }

        //group
        IGroupFundRepository GroupFundRepository { get; }
        IGroupFundLogRepository GroupFundLogRepository { get; }
        IGroupMemberRepository GroupMemberRepository { get; }
        IGroupMemberLogRepository GroupMemberLogRepository { get; }

        //asset and liability
        IAssetRepository AssetRepository { get; }
        ILiabilityRepository LiabilityRepository { get; }


        //subscription
        ISubscriptionRepository SubscriptionRepository { get; }

        // chat
        IChatHistoryRepository ChatHistoryRepository { get; }
        IChatMessageRepository ChatMessageRepository { get; }

        // notification
        INotificationRepository NotificationRepository { get; }

        // bank account
        IBankAccountRepository BankAccountRepository { get; }

        int Save();
        void Commit();
        void Rollback();
        Task SaveAsync();
    }
}
