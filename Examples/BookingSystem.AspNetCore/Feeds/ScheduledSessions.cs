using BookingSystem.FakeDatabase;
using OpenActive.NET;
using OpenActive.NET.Rpde.Version1;
using OpenActive.Server.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingSystem.AspNetCore.Feeds
{
    public class AcmeScheduledSessionRPDEGenerator : RPDEFeedModifiedTimestampAndIDLong
    {
        //public override string FeedPath { get; protected set; } = "example path override";

        protected override List<RpdeItem> GetRPDEItems(long? modified, long? id)
        {
            var query = from occurances in FakeBookingSystem.Database.Occurrences
                        orderby occurances.Modified, occurances.Id
                        where occurances.Modified.ToUnixTimeMilliseconds() > modified || 
                              (occurances.Modified.ToUnixTimeMilliseconds() == modified && occurances.Id > id)
                        select new RpdeItem
                        {
                            Kind = RpdeKind.ScheduledSession,
                            Id = occurances.Id,
                            Modified = occurances.Modified.ToUnixTimeMilliseconds(),
                            State = occurances.Deleted ? RpdeState.Deleted : RpdeState.Updated,
                            Data = occurances.Deleted ? null : new ScheduledSession
                            {
                                // QUESTION: Should the this.IdTemplate, this.ParentIdTemplate and this.BaseUrl be passed in each time rather than set on
                                // the parent class? Current thinking is it's more extensible on parent class as function signature remains
                                // constant as power of configuration through underlying class grows (i.e. as new properties are added)
                                Id = this.IdTemplate.RenderOpportunityId(new ScheduledSessionOpportunity
                                {
                                    BaseUrl = this.JsonLdIdBaseUrl,
                                    SessionSeriesId = occurances.ClassId,
                                    ScheduledSessionId = occurances.Id
                                }),
                                SuperEvent = this.ParentIdTemplate.RenderOpportunityId(new SessionSeriesOpportunity
                                {
                                    BaseUrl = this.JsonLdIdBaseUrl,
                                    SessionSeriesId = occurances.ClassId
                                }),
                                StartDate = (DateTimeOffset)occurances.Start,
                                EndDate = (DateTimeOffset)occurances.End
                            }
                        };
            return query.ToList();
        }
    }

    public class AcmeFacilityUseRPDEGenerator : RPDEFeedIncrementingUniqueChangeNumber
    {
        protected override List<RpdeItem> GetRPDEItems(long? afterChangeNumber)
        {
            throw new NotImplementedException();
        }
    }
}
