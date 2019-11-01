using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET
{
    /// <summary>
    /// Use the Settings pattern
    /// 
    /// TODO: Remove defaults here and set up a way of passing settings into the BookingEngine
    /// </summary>
    public class BookingEngineSettings
    {
        /// <summary>
        /// This Dictionary maps pairs of JSON-LD IDs to strongly typed classes containing their components.
        /// It is used by the booking engine to validate and transform IDs provided by the Broker.
        /// 
        /// The classes are POCO simply implementing the IBookablePairIdTemplate interface.
        /// 
        /// The first ID is for the opportunity, the second ID is for the offer.
        /// </summary>
        public Dictionary<BookableOpportunityClass, IBookablePairIdTemplate> IdConfiguration { get; }
            = new Dictionary<BookableOpportunityClass, IBookablePairIdTemplate> {
                {
                    BookableOpportunityClass.ScheduledSession,
                    new BookablePairIdTemplate<ScheduledSessionOpportunity>(
                        "{+BaseUrl}api/scheduled-sessions/{SessionSeriesId}/events/{ScheduledSessionId}",
                        "{+BaseUrl}api/scheduled-sessions/{SessionSeriesId}/events/{ScheduledSessionId}#/offers/{OfferId}"
                        )
                },
                {
                    BookableOpportunityClass.Slot,
                    new BookablePairIdTemplate<SlotOpportunity>(
                        "{+BaseUrl}api/facility-uses/{FacilityUseId}/slots/{SlotId}",
                        "{+BaseUrl}api/facility-uses/{FacilityUseId}/slots/{SlotId}#/offers/{OfferId}"
                        )
                }
        };

        public Uri OrderBaseUrl { get; set; } = new Uri("https://example.com/api/orders/");
        public SingleIdTemplate<OrderId> OrderIdTemplate = new SingleIdTemplate<OrderId>(
                        "{+BaseUrl}api/scheduled-sessions/{SessionSeriesId}/events/{ScheduledSessionId}"
                        );
    }


    /// <summary>
    /// These classes are created by the booking system, the below are temporary default examples.
    /// These should be created alongside the settings containing IdConfiguration, as the two work together
    /// 
    /// They can be completely customised to match the preferred ID structure of the booking system
    /// 
    /// There is a choice of `string` or `long?` available for each component of the ID
    /// </summary>

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
}
