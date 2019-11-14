using OpenActive.DatasetSite.NET;
using OpenActive.Server.NET.OpenBookingHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingSystem.AspNetFramework
{
    /// <summary>
    /// These classes must be created by the booking system, the below are some simple examples.
    /// These should be created alongside the IdConfiguration and OpenDataFeeds settings, as the two work together
    /// 
    /// They can be completely customised to match the preferred ID structure of the booking system
    /// 
    /// There is a choice of `string`, `long?` and `Uri` available for each component of the ID
    /// </summary>
    public class FacilityOpportunity : IBookableIdComponents
    {
        public Uri BaseUrl { get; set; }
        public OpportunityType? OpportunityType { get; set; }
        public string FacilityUseId { get; set; }
        public long? SlotId { get; set; }
        public long? OfferId { get; set; }

    }
}
