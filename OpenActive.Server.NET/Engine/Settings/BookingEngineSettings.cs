using System;
using System.Collections.Generic;
using System.Text;
using OpenActive.DatasetSite.NET;
using OpenActive.NET;
using OpenActive.NET.Rpde.Version1;

namespace OpenActive.Server.NET
{
    /// <summary>
    /// QUESTION: Should this be an interface? How do we use the settings pattern?
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
        public List<IBookablePairIdTemplate> IdConfiguration { get; set;  }
        public Uri OrderBaseUrl { get; set; }
        public SingleIdTemplate<OrderId> OrderIdTemplate { get; set; }
        public Dictionary<OpportunityType, RPDEFeedGenerator> OpenDataFeeds { get; set; }
        public int RPDEPageSize { get; set; } = 500;
        public Uri JsonLdIdBaseUrl { get; set; }
    }


    /// <summary>
    /// These classes are created by the booking system, the below are temporary default examples.
    /// These should be created alongside the settings containing IdConfiguration, as the two work together
    /// 
    /// They can be completely customised to match the preferred ID structure of the booking system
    /// 
    /// There is a choice of `string` or `long?` available for each component of the ID
    /// </summary>

    public class SessionSeriesOpportunity : IBookableIdComponents
    {
        public Uri BaseUrl { get; set; }
        public long? SessionSeriesId { get; set; }
        public long? OfferId { get; set; }
        public OpportunityType? OpportunityType { get; set; }
    }

    public class ScheduledSessionOpportunity : IBookableIdComponents
    {
        public Uri BaseUrl { get; set; }
        public long? SessionSeriesId { get; set; }
        public long? ScheduledSessionId { get; set; }
        public long? OfferId { get; set; }
        public OpportunityType? OpportunityType { get; set; }
    }

    public class SlotOpportunity : IBookableIdComponents
    {
        public Uri BaseUrl { get; set; }
        public string FacilityUseId { get; set; }
        public long? SlotId { get; set; }

        public long? OfferId { get; set; }
        public OpportunityType? OpportunityType { get; set; }
    }

    public class DefaultSellerIdComponents
    {
        public long? SellerId { get; set; }
    }
}
