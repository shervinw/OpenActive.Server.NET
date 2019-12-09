using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenActive.FakeDatabase.NET;
using OpenActive.Server.NET.StoreBooking;

namespace BookingSystem
{
    public sealed class OrderTransaction : IDatabaseTransaction
    {
        private FakeDatabaseTransaction _fakeDatabaseTransaction;

        public FakeDatabase Database { get => _fakeDatabaseTransaction.Database; }

        public OrderTransaction()
        {
            _fakeDatabaseTransaction = new FakeDatabaseTransaction(FakeBookingSystem.Database);
        }

        public void Commit()
        {
            _fakeDatabaseTransaction.CommitTransaction();
        }

        public void Rollback()
        {
            _fakeDatabaseTransaction.Database = null;
        }

        public void Dispose()
        {
            // Note dispose pattern of checking for null first,
            // to ensure Dispose() is not called twice
            if (_fakeDatabaseTransaction != null)
            {
                _fakeDatabaseTransaction.Dispose();
                _fakeDatabaseTransaction = null;
            }
        }
    }

    /*
    public sealed class EntityFrameworkOrderTransaction : IDatabaseTransaction
    {
        private OrderContext _context;
        private DbContextTransaction _dbContextTransaction;

        public EntityFrameworkOrderTransaction()
        {
            _context = new OrderContext();
            _dbContextTransaction = _context.Database.BeginTransaction();
        }

        public void Commit()
        {
            _context.SaveChanges();
            _dbContextTransaction.Commit();
        }

        public void Rollback()
        {
            _dbContextTransaction.Rollback();
        }

        public void Dispose()
        {
            // Note dispose pattern of checking for null first,
            // to ensure Dispose() is not called twice
            if (_dbContextTransaction != null)
            {
                _dbContextTransaction.Dispose();
                _dbContextTransaction = null;
            }

            if (_context != null)
            {
                _context.Dispose();
                _context = null;
            }
        }
    }
    */
}

