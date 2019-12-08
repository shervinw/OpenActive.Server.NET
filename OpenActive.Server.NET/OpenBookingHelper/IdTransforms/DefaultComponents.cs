using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using OpenActive.NET;

namespace OpenActive.Server.NET.OpenBookingHelper
{
    // Note in future we may make these more flexible (and configurable), but for now they are set for the simple case

    public class SellerIdComponents
    {
        public long? SellerIdLong { get; set; }
        public string SellerIdString { get; set; }
    }

    public class OrderIdComponents
    {
        public OrderType? OrderType { get; set; }
        public string ClientId { get; set; }
        public string uuid { get; set; }
        public long? OrderItemIdLong { get; set; }
        public string OrderItemIdString { get; set; }
    }

    // TODO: Add resolve Order ID via enumeration, and add paths (e.g. 'order-quote-template') to the below
    public enum OrderType {

        [EnumMember(Value = "order-quote-templates")]
        OrderQuoteTemplate,

        [EnumMember(Value = "order-quotes")]
        OrderQuote,

        [EnumMember(Value = "orders")]
        Order
    }

}
