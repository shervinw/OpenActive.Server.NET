using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET
{
    public class StoreBookingEngine
    {
        public StoreBookingEngine(IOrderStore orderStore, IOpportunityStore opportunityStore)
        {

        }


        public OrderItem GetOrderItem(BookableOpportunityClass opportunityClass, string opportunityId, string offerId, string sellerId)
        {

        }
            public OrderItem GetOrderItem(DefaultOpportunityOfferIdComponents opportunityOfferId, DefaultSellerIdComponents sellerId)
            {
                throw new NotImplementedException();
            }

        }
}
