using OpenActive.DatasetSite.NET;
using OpenActive.Server.NET.OpenBookingHelper;

namespace BookingSystem
{
    /// <summary>
    /// These classes must be created by the booking system, the below are some simple examples.
    /// These should be created alongside the IdConfiguration and OpenDataFeeds settings, as the two work together
    /// 
    /// They can be completely customised to match the preferred ID structure of the booking system
    /// 
    /// There is a choice of `string`, `long?` and `Uri` available for each component of the ID
    /// </summary>
    public class SessionOpportunity : IBookableIdComponentsWithInheritance
    {
        public OpportunityType? OpportunityType { get; set; }
        public OpportunityType? OfferOpportunityType { get; set; }
        public long? SessionSeriesId { get; set; }
        public long? ScheduledSessionId { get; set; }
        public long? OfferId { get; set; }
    }
}
