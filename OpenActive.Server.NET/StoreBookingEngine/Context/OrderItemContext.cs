using OpenActive.NET;
using OpenActive.Server.NET.OpenBookingHelper;
using System;
using System.Collections.Generic;
using System.Text;

/** DEPRECATED
namespace OpenActive.Server.NET.StoreBooking
{
    public class OrderItemContext
    {
        public IBookableIdComponents BookableIdComponents { get; set; }
        public OrderItem OrderItem { get; set; }
        public List<OpenBookingError> LeaseErrors { get; set; }
    }

    public class OrderItemContext<TComponents> where TComponents : class, IBookableIdComponents, new()
    {
        public TComponents BookableIdComponents { get; set; }
        public OrderItem OrderItem { get; set; }
        public List<OpenBookingError> LeaseErrors { get; set; }
    }
}
}
    */
