using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenActive.FakeDatabase.NET
{
    public class FakeDatabaseTransaction : IDisposable
    {
        internal List<string> OrdersIds = new List<string>();

        public FakeDatabaseTransaction(FakeDatabase database)
        {
            Database = database;
        }

        public void CommitTransaction()
        {
            // No action required
        }

        public void RollbackTransaction()
        {
            foreach (var orderIds in OrdersIds)
            {
                Database.RollbackOrder(orderIds);
            }
        }

        public void Dispose()
        {
            // No implementation required if not disposable
        }

        public FakeDatabase Database { get; set; }
    }

}
