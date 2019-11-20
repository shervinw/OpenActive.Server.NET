using OpenActive.NET;
using OpenActive.Server.NET.CustomBooking;
using OpenActive.Server.NET.OpenBookingHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET.StoreBooking
{
    //TODO: Refactor to inherrit from BookingFlowContext (using constructor to copy params? Use Automapper?)
    public class StoreBookingFlowContext : BookingFlowContext
    {
        public StoreBookingFlowContext(BookingFlowContext bookingFlowContext)
        {
            if (bookingFlowContext == null) throw new ArgumentNullException(nameof(bookingFlowContext));
            base.Stage = bookingFlowContext.Stage;
            base.OrderIdTemplate = bookingFlowContext.OrderIdTemplate;
            base.OrderIdComponents = bookingFlowContext.OrderIdComponents;
            base.TaxPayeeRelationship = bookingFlowContext.TaxPayeeRelationship;
            base.Payer = bookingFlowContext.Payer;
        }

        public SellerIdComponents SellerIdComponents { get; set; }
        public ILegalEntity Customer { get; set; }
        public Organization Broker { get; internal set; }
        public BookingService BookingService { get; internal set; }
        public BrokerType? BrokerRole { get; internal set; }
    }
}
