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

        private ICategoryRepository _categoryRepository;
        private ISubCategoryRepository _subCategoryRepository;

        private ITransactionRepository _transactionRepository;
        private IFixedTransactionRepository _fixedTransactionRepository;

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

        public ICategoryRepository Categories
        {
            get
            {
                return _categoryRepository ??= new CategoryRepository(_context);
            }
        }

        public ISubCategoryRepository SubCategories
        {
            get
            {
                return _subCategoryRepository ??= new SubCategoryRepository(_context);
            }
        }

        public ITransactionRepository Transactions
        {
            get
            {
                return _transactionRepository ??= new TransactionRepository(_context);
            }
        }

        public IFixedTransactionRepository FixedTransactions
        {
            get
            {
                return _fixedTransactionRepository ??= new FixedTransactionRepository(_context);
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
    }
}
