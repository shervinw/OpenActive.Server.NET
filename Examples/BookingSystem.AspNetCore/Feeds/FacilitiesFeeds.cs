using OpenActive.NET;
using OpenActive.NET.Rpde.Version1;
using OpenActive.Server.NET.OpenBookingHelper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BookingSystem
{
    public class AcmeFacilityUseRPDEGenerator : RPDEFeedModifiedTimestampAndIDLong<FacilityOpportunity, FacilityUse>
    {
        //public override string FeedPath { get; protected set; } = "example path override";

        protected override List<RpdeItem<FacilityUse>> GetRPDEItems(long? afterTimestamp, long? afterId)
        {
            throw new NotImplementedException();    
        }
    }

    public class AcmeFacilityUseSlotRPDEGenerator : RPDEFeedModifiedTimestampAndIDLong<FacilityOpportunity, Slot>
    {
        //public override string FeedPath { get; protected set; } = "example path override";

        protected override List<RpdeItem<Slot>> GetRPDEItems(long? afterTimestamp, long? afterId)
        {
            throw new NotImplementedException();
        }
    }


}
