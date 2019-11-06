using BookingSystem.FakeDatabase;
using OpenActive.DatasetSite.NET;
using OpenActive.NET;
using OpenActive.NET.Rpde.Version1;
using OpenActive.Server.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingSystem.AspNetCore
{
    public class AcmeFacilityUseRPDEGenerator : RPDEFeedModifiedTimestampAndIDLong<FacilityOpportunity>
    {
        //public override string FeedPath { get; protected set; } = "example path override";

        protected override List<RpdeItem> GetRPDEItems(long? afterTimestamp, long? afterId)
        {
            throw new NotImplementedException();    
        }
    }

    public class AcmeFacilityUseSlotRPDEGenerator : RPDEFeedModifiedTimestampAndIDLong<FacilityOpportunity>
    {
        //public override string FeedPath { get; protected set; } = "example path override";

        protected override List<RpdeItem> GetRPDEItems(long? afterTimestamp, long? afterId)
        {
            throw new NotImplementedException();
        }
    }

    public class FacilityOpportunity : IBookableIdComponents
    {
        public Uri BaseUrl { get; set; }
        public OpportunityType? OpportunityType { get; set; }
        public string FacilityUseId { get; set; }
        public long? SlotId { get; set; }
        public long? OfferId { get; set; }

    }
}
