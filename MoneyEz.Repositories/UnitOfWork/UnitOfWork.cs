using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Repositories.Implements;
using MoneyEz.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MoneyEzContext _context;
        private IDbContextTransaction _transaction;

        private IUserRepository _userRepository;

        //spending model
        private ISpendingModelRepository _spendingModelRepository;

        //spending model category
        private ISpendingModelCategoryRepository _spendingModelCategoryRepository;

        //category  
        private ICategoriesRepository _categoriesRepository;

        //subcategory
        private ISubcategoryRepository _subcategoryRepository;

        //subcategorycategory
        private ICategorySubcategoryRepository _categorySubcategoryRepository;

        //transaction
        private ITransactionRepository _transactionsRepository;
        //group
        private IGroupFundRepository _groupFundRepository;
        //group fund log
        private IGroupFundLogRepository _groupFundLogRepository;
        //group member
        private IGroupMemberRepository _groupMemberRepository;
        private IGroupMemberLogRepository _groupMemberLogRepository;
        private ISubscriptionRepository _subscriptionRepository;

        // chat
        private IChatHistoryRepository _chatHistoryRepository;
        private IChatMessageRepository _chatMessageRepository;

        //asset and liability
        private IAssetRepository _assetRepository;
        private ILiabilityRepository _liabilityRepository;

        // notification
        private INotificationRepository _notificationRepository;

        public UnitOfWork(MoneyEzContext context)
        {
            _context = context;
        }

        public IUserRepository UsersRepository
        {
            get
            {
                return _userRepository ??= new UserRepository(_context);

            }
        }

        public ISpendingModelRepository SpendingModelRepository
        {
            get
            {
                return _spendingModelRepository ??= new SpendingModelRepository(_context);
            }
        }

        public ISpendingModelCategoryRepository SpendingModelCategoryRepository
        {
            get
            {
                return _spendingModelCategoryRepository ??= new SpendingModelCategoryRepository(_context);
            }
        }

        public ICategoriesRepository CategoriesRepository
        {
            get
            {
                return _categoriesRepository ??= new CategoriesRepository(_context);
            }
        }

        public ISubcategoryRepository SubcategoryRepository
        {
            get
            {
                return _subcategoryRepository ??= new SubcategoryRepository(_context);
            }
        }

        public ICategorySubcategoryRepository CategorySubcategoryRepository
        {
            get { return _categorySubcategoryRepository ??= new CategorySubcategoryRepository(_context); }
        }

        public ITransactionRepository TransactionsRepository
        {
            get
            {
                return _transactionsRepository ??= new TransactionRepository(_context);
            }
        }

        public IGroupFundRepository GroupFundRepository
        {
            get
            {
                return _groupFundRepository ??= new GroupRepository(_context);
            }
        }
        public IGroupFundLogRepository GroupFundLogRepository
        {
            get
            {
                return _groupFundLogRepository ??= new GroupFundLogRepository(_context);
            }
        }
        public IGroupMemberRepository GroupMemberRepository
        {
            get
            {
                return _groupMemberRepository ??= new GroupMemberRepository(_context);
            }
        }

        public IGroupMemberLogRepository GroupMemberLogRepository
        {
            get
            {
                return _groupMemberLogRepository ??= new GroupMemberLogRepository(_context);
            }
        }

        public ISubscriptionRepository SubscriptionRepository
        {
            get { return _subscriptionRepository ??= new SubscriptionRepository(_context); }
        }

        public IChatHistoryRepository ChatHistoryRepository
        {
            get
            {
                return _chatHistoryRepository ??= new ChatHistoryRepository(_context);
            }
        }

        public IChatMessageRepository ChatMessageRepository
        {
            get
            {
                return _chatMessageRepository ??= new ChatMessageRepository(_context);
            }
        }


        public IAssetRepository AssetRepository
        {
            get
            {
                return _assetRepository ??= new AssetRepository(_context);
            }
        } 
        
        public ILiabilityRepository LiabilityRepository
        {
            get
            {
                return _liabilityRepository ??= new LiabilityRepository(_context);
            }
        }

        public INotificationRepository NotificationRepository
        {
            get
            {
                return _notificationRepository ??= new NotificationRepository(_context);
            }
        }

        public void Commit()
        {
            try
            {
                _context.SaveChanges();
                _transaction?.Commit();
            }
            catch (Exception)
            {
                _transaction?.Rollback();
                throw;
            }
            finally
            {
                _transaction?.Dispose();
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public void Rollback()
        {
            _transaction?.Rollback();
            _transaction?.Dispose();
        }

        public int Save()
        {
            return _context.SaveChanges();
        }

        public Task SaveAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
