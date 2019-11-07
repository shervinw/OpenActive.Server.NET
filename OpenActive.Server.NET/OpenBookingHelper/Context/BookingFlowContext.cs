using OpenActive.NET;
using OpenActive.Server.NET.OpenBookingHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET.OpenBookingHelper
{
    public class BookingFlowContext<TOrder>
    {
        public FlowStage Stage { get; set; }
        public OrderIdComponents OrderIdComponents { get; set; }
        public SellerIdComponents SellerIdComponents { get; set; }
        public TOrder Order { get; set; }
        public TaxPayeeRelationship TaxPayeeRelationship { get; set; }
        public SingleValues<Organization, Person> Payer { get; set; }
    }
}
