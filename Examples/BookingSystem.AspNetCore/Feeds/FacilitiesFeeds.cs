using OpenActive.NET.Rpde.Version1;
using OpenActive.Server.NET.OpenBookingHelper;
using System;
using System.Collections.Generic;
using System.Linq;

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


}
