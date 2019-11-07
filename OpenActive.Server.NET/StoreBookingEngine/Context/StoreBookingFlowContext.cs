using OpenActive.NET;
using OpenActive.Server.NET.CustomBooking;
using OpenActive.Server.NET.OpenBookingHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET.StoreBooking
{
    //TODO: Refactor to inherrit from BookingFlowContext (using constructor to copy params? Use Automapper?)
    public class StoreBookingFlowContext<TOrder>
    {
        public BookingFlowContext<TOrder> FlowContext { get; set; }
        public SellerIdComponents SellerIdComponents { get; set; }
        public SingleValues<Organization, Person> Customer { get; set; }
        public Organization Broker { get; internal set; }
        public BookingService BookingSystem { get; internal set; }
    }
}
