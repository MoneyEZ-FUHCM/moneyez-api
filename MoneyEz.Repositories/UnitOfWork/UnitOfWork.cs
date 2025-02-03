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
        private IGroupFundRepository _groupFundRepository;
        private IGroupFundLogRepository _groupFundLogRepository;
        private IGroupMemberRepository _groupMemberRepository;

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
