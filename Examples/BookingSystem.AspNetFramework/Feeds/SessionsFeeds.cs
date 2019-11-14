using BookingSystem.FakeDatabase;
using OpenActive.DatasetSite.NET;
using OpenActive.NET;
using OpenActive.NET.Rpde.Version1;
using OpenActive.Server.NET.OpenBookingHelper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BookingSystem.AspNetFramework
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
                                Id = this.RenderOpportunityId(new SessionOpportunity
                                {
                                    OpportunityType = OpportunityType.ScheduledSession,
                                    BaseUrl = this.JsonLdIdBaseUrl,
                                    SessionSeriesId = occurances.ClassId,
                                    ScheduledSessionId = occurances.Id
                                }),
                                SuperEvent = this.RenderOpportunityId(new SessionOpportunity
                                {
                                    OpportunityType = OpportunityType.SessionSeries,
                                    BaseUrl = this.JsonLdIdBaseUrl,
                                    SessionSeriesId = occurances.ClassId
                                }),
                                StartDate = (DateTimeOffset)occurances.Start,
                                EndDate = (DateTimeOffset)occurances.End
                            }
                        };
            return query.Take(this.RPDEPageSize).ToList();
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
                                Id = this.RenderOpportunityId( new SessionOpportunity
                                {
                                    OpportunityType = OpportunityType.SessionSeries,
                                    BaseUrl = this.JsonLdIdBaseUrl,
                                    SessionSeriesId = @class.Id
                                }),
                                Name = @class.Title,
                                Offers = new List<Offer> { new Offer
                                    {
                                        Id = this.RenderOfferId(new SessionOpportunity
                                        {
                                            OfferOpportunityType = OpportunityType.SessionSeries,
                                            BaseUrl = this.JsonLdIdBaseUrl,
                                            SessionSeriesId = @class.Id,
                                            OfferId = 0
                                        }),
                                        Price = @class.Price
                                    } 
                                }
                            }
                        };
            var items = query.Take(this.RPDEPageSize).ToList();
            return items;
        }
    }

}
