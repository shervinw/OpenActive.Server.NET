using Newtonsoft.Json;
using OpenActive.NET;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Linq;

namespace OpenActive.Server.NET
{

    /// <summary>
    /// TODO: Move to BookingSystem.AspNetCore
    /// </summary>
    class AcmeStore : IOpenBookingStore
    {


        //public void CreateFakeEvent()
        public OrderItem GetOrderItem(IBookableIdComponents opportunityOfferId, DefaultSellerIdComponents sellerId)
        {
            // Note switch statement exists here as we need to handle booking for a single Order that contains different types of opportunity
            switch (opportunityOfferId)
            {
                case ScheduledSessionOpportunity scheduledSessionOpportunity:

                case SlotOpportunity slotOpportunity:

                case null:

                default:
                    break;
            }

            return null;
        }
    }

    
}
