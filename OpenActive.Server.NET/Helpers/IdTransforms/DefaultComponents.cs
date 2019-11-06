using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET
{
    // Note in future we may make these more flexible (and configurable), but for now they are set for the simple case

    public class SellerIdComponents
    {
        public Uri BaseUrl { get; set; }
        public string SellerIdString { get; set; }
        public long? SellerIdLong { get; set; }
    }

    public class OrderIdComponents
    {
        public Uri BaseUrl { get; set; }
        public OrderType OrderType { get; set; }
        public string uuid { get; set; }
    }

    // TODO: Add resolve Order ID via enumeration, and add paths (e.g. 'order-quote-template') to the below
    public enum OrderType { OrderQuoteTemplate, OrderQuote, Order }
}
