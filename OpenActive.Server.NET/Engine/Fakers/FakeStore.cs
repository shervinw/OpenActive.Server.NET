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
    public class ScheduledSessionOpportunity : IBookableIdComponents
    {
        public Uri BaseUrl { get; set; }
        public string SessionSeriesId { get; set; }
        public long? ScheduledSessionId { get; set; }
        public long? OfferId { get; set; }
    }

    public class SlotOpportunity : IBookableIdComponents
    {
        public Uri BaseUrl { get; set; }
        public string FacilityUseId { get; set; }
        public long? SlotId { get; set; }

        public long? OfferId { get; set; }
    }

    public class DefaultSellerIdComponents
    {
        public long? SellerId { get; set; }
    }

    class FakeStore : IOpportunityStore<DefaultSellerIdComponents>
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
