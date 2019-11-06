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
    public class AcmeScheduledSessionRPDEGenerator : RPDEFeedModifiedTimestampAndIDLong<SessionOpportunity>
    {
        //public override string FeedPath { get; protected set; } = "example path override";

        protected override List<RpdeItem> GetRPDEItems(long? afterTimestamp, long? afterId)
        {
            var query = from occurances in FakeBookingSystem.Database.Occurrences
                        orderby occurances.Modified, occurances.Id
                        where !afterTimestamp.HasValue && !afterId.HasValue ||
                              occurances.Modified.ToUnixTimeMilliseconds() > afterTimestamp ||  
                              (occurances.Modified.ToUnixTimeMilliseconds() == afterTimestamp && occurances.Id > afterId)
                        
                        select new RpdeItem
                        {
                            Kind = RpdeKind.ScheduledSession,
                            Id = occurances.Id,
                            Modified = occurances.Modified.ToUnixTimeMilliseconds(),
                            State = occurances.Deleted ? RpdeState.Deleted : RpdeState.Updated,
                            Data = occurances.Deleted ? null : new ScheduledSession
                            {
                                // QUESTION: Should the this.IdTemplate and this.BaseUrl be passed in each time rather than set on
                                // the parent class? Current thinking is it's more extensible on parent class as function signature remains
                                // constant as power of configuration through underlying class grows (i.e. as new properties are added)
                                Id = this.RenderOpportunityId(OpportunityType.ScheduledSession, new SessionOpportunity
                                {
                                    BaseUrl = this.JsonLdIdBaseUrl,
                                    SessionSeriesId = occurances.ClassId,
                                    ScheduledSessionId = occurances.Id
                                }),
                                SuperEvent = this.RenderOpportunityId(OpportunityType.SessionSeries, new SessionOpportunity
                                {
                                    BaseUrl = this.JsonLdIdBaseUrl,
                                    SessionSeriesId = occurances.ClassId
                                }),
                                StartDate = (DateTimeOffset)occurances.Start,
                                EndDate = (DateTimeOffset)occurances.End
                            }
                        };
            return query.Take(500).ToList();
        }
    }

    public class AcmeSessionSeriesRPDEGenerator : RPDEFeedModifiedTimestampAndIDLong<SessionOpportunity>
    {
        protected override List<RpdeItem> GetRPDEItems(long? afterTimestamp, long? afterId)
        {
            var query = from @class in FakeBookingSystem.Database.Classes
                        orderby @class.Modified, @class.Id
                        where !afterTimestamp.HasValue && !afterId.HasValue ||
                              @class.Modified.ToUnixTimeMilliseconds() > afterTimestamp ||
                              (@class.Modified.ToUnixTimeMilliseconds() == afterTimestamp && @class.Id > afterId)

                        select new RpdeItem
                        {
                            Kind = RpdeKind.SessionSeries,
                            Id = @class.Id,
                            Modified = @class.Modified.ToUnixTimeMilliseconds(),
                            State = @class.Deleted ? RpdeState.Deleted : RpdeState.Updated,
                            Data = @class.Deleted ? null : new ScheduledSession
                            {
                                // QUESTION: Should the this.IdTemplate and this.BaseUrl be passed in each time rather than set on
                                // the parent class? Current thinking is it's more extensible on parent class as function signature remains
                                // constant as power of configuration through underlying class grows (i.e. as new properties are added)
                                Id = this.RenderOpportunityId(OpportunityType.SessionSeries, new SessionOpportunity
                                {
                                    BaseUrl = this.JsonLdIdBaseUrl,
                                    SessionSeriesId = @class.Id
                                }),
                                Name = @class.Title,
                                Offers = new List<Offer> { new Offer
                                    {
                                        Id = this.RenderOfferId(OpportunityType.SessionSeries, new SessionOpportunity
                                        {
                                            BaseUrl = this.JsonLdIdBaseUrl,
                                            SessionSeriesId = @class.Id,
                                            OfferId = 0
                                        }),
                                        Price = @class.Price
                                    } 
                                }
                            }
                        };
            var items = query.Take(500).ToList();
            return items;
        }
    }


    public class AcmeFacilityUseRPDEGenerator : RPDEFeedIncrementingUniqueChangeNumber<SessionOpportunity>
    {
        protected override List<RpdeItem> GetRPDEItems(long? afterChangeNumber)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// These classes must be created by the booking system, the below are some simple examples.
    /// These should be created alongside the IdConfiguration and OpenDataFeeds settings, as the two work together
    /// 
    /// They can be completely customised to match the preferred ID structure of the booking system
    /// 
    /// There is a choice of `string`, `long?` and `Uri` available for each component of the ID
    /// </summary>

    public class SessionOpportunity : IBookableIdComponents
    {
        public Uri BaseUrl { get; set; }
        public OpportunityType? OpportunityType { get; set; }
        public long? SessionSeriesId { get; set; }
        public long? ScheduledSessionId { get; set; }
        public long? OfferId { get; set; }
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
