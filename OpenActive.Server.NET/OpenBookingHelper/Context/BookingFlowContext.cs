using OpenActive.NET;
using OpenActive.Server.NET.OpenBookingHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET.OpenBookingHelper
{
    public class BookingFlowContext
    {
        public FlowStage Stage { get; internal set; }
        public OrderIdTemplate OrderIdTemplate { get; internal set; }
        public OrderIdComponents OrderId { get; internal set; }
        public TaxPayeeRelationship TaxPayeeRelationship { get; internal set; }
        public ILegalEntity Payer { get; internal set; }
        public ILegalEntity Seller { get; internal set; }
        public SellerIdComponents SellerId { get; internal set; }
    }
}
