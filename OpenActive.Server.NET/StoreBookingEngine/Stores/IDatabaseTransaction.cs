using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET.StoreBooking
{
    public interface IDatabaseTransaction : IDisposable
    {
        void Commit();
        void Rollback();
    }
}
