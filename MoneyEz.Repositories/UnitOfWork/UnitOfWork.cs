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

        //user spending model
        private IUserSpendingModelRepository _userSpendingModelRepository;

        //financial goal
        private IFinancialGoalRepository _financialGoalRepository;

        //finacnial repỏt
        private IFinancialReportRepository _financialReportRepository;

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

        //recurring transaction
        private IRecurringTransactionRepository _recurringTransactionRepository;
        //vote
        private ITransactionVoteRepository _transactionVoteRepository;

        //image
        private IImageRepository _imageRepository;

        //group
        private IGroupFundRepository _groupFundRepository;
        //group fund log
        private IGroupFundLogRepository _groupFundLogRepository;
        //group member
        private IGroupMemberRepository _groupMemberRepository;
        private ISubscriptionRepository _subscriptionRepository;

        // chat
        private IChatHistoryRepository _chatHistoryRepository;
        private IChatMessageRepository _chatMessageRepository;

        //asset and liability
        private IAssetRepository _assetRepository;
        private ILiabilityRepository _liabilityRepository;

        // notification
        private INotificationRepository _notificationRepository;

        // bank account
        private IBankAccountRepository _bankAccountRepository;

        // quiz
        private IQuizRepository _quizRepository;
        private IAnswerOptionRepository _answerOptionRepository;
        private IUserQuizResultRepository _userQuizResultRepository;
        private IQuestionRepository _questionRepository;
        private IUserQuizAnswerRepository _userQuizAnswerRepository;

        // post
        private IPostRepository _postRepository;

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

        public IFinancialGoalRepository FinancialGoalRepository
        {
            get
            {
                return _financialGoalRepository ??= new FinancialGoalRepository(_context);
            }
        }

        public IFinancialReportRepository FinancialReportRepository
        {
            get
            {
                return _financialReportRepository ??= new FinancialReportRepository(_context);
            }
        }

        public IUserSpendingModelRepository UserSpendingModelRepository
        {
            get
            {
                return _userSpendingModelRepository ??= new UserSpendingModelRepository(_context);
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

        public IRecurringTransactionRepository RecurringTransactionRepository
        {
            get
            {
                return _recurringTransactionRepository ??= new RecurringTransactionRepository(_context);
            }
        }

        public ITransactionVoteRepository TransactionVoteRepository
        {
            get
            {
                return _transactionVoteRepository ??= new TransactionVoteRepository(_context);
            }
        }

        public IImageRepository ImageRepository
        {
            get
            {
                return _imageRepository ??= new ImageRepository(_context);
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

        public IBankAccountRepository BankAccountRepository
        {
            get
            {
                return _bankAccountRepository ??= new BankAccountRepository(_context);
            }
        }

        public IQuizRepository QuizRepository
        {
            get
            {
                return _quizRepository ??= new QuizRepository(_context);
            }
        }

        public IAnswerOptionRepository AnswerOptionRepository
        {
            get
            {
                return _answerOptionRepository ??= new AnswerOptionRepository(_context);
            }
        }

        public IUserQuizResultRepository UserQuizResultRepository
        {
            get
            {
                return _userQuizResultRepository ??= new UserQuizResultRepository(_context);
            }
        }

        public IQuestionRepository QuestionRepository
        {
            get
            {
                return _questionRepository ??= new QuestionRepository(_context);
            }
        }

        public IUserQuizAnswerRepository UserQuizAnswerRepository
        {
            get
            {
                return _userQuizAnswerRepository ??= new UserQuizAnswerRepository(_context);
            }
        }

        public IPostRepository PostRepository
        {
            get
            {
                return _postRepository ??= new PostRepository(_context);
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
