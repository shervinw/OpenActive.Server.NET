using OpenActive.NET;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET
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

    //TODO: Put this with store stuff and/or refactor to inherrit from BookingFlowContext
    public class StoreBookingFlowContext<TOrder>
    {
        public BookingFlowContext<TOrder> FlowContext { get; set; }
        public SellerIdComponents SellerIdComponents { get; set; }
        public SingleValues<Organization, Person> Customer { get; set; }
        public Organization Broker { get; internal set; }
        public BookingService BookingSystem { get; internal set; }
    }
}
