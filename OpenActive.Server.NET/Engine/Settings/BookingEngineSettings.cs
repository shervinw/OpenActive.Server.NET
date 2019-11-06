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
        public Dictionary<OpportunityType, IRPDEFeedGenerator> OpenDataFeeds { get; set; }
        public int RPDEPageSize { get; set; } = 500;
        public Uri JsonLdIdBaseUrl { get; set; }
    }



    public class DefaultSellerIdComponents
    {
        public long? SellerId { get; set; }
    }
}
