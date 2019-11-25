using OpenActive.DatasetSite.NET;
using OpenActive.FakeDatabase.NET;
using OpenActive.NET;
using OpenActive.NET.Rpde.Version1;
using OpenActive.Server.NET.OpenBookingHelper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BookingSystem.AspNetCore
{
    public class AcmeScheduledSessionRPDEGenerator : RPDEFeedModifiedTimestampAndIDLong<SessionOpportunity, ScheduledSession>
    {
        //public override string FeedPath { get; protected set; } = "example path override";

        protected override List<RpdeItem<ScheduledSession>> GetRPDEItems(long? afterTimestamp, long? afterId)
        {
            var query = from occurances in FakeBookingSystem.Database.Occurrences
                        orderby occurances.Modified, occurances.Id
                        where !afterTimestamp.HasValue && !afterId.HasValue ||
                              occurances.Modified.ToUnixTimeMilliseconds() > afterTimestamp ||  
                              (occurances.Modified.ToUnixTimeMilliseconds() == afterTimestamp && occurances.Id > afterId)
                        
                        select new RpdeItem<ScheduledSession>
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
                                    SessionSeriesId = occurances.ClassId,
                                    ScheduledSessionId = occurances.Id
                                }),
                                SuperEvent = this.RenderOpportunityId(new SessionOpportunity
                                {
                                    OpportunityType = OpportunityType.SessionSeries,
                                    SessionSeriesId = occurances.ClassId
                                }),
                                StartDate = (DateTimeOffset)occurances.Start,
                                EndDate = (DateTimeOffset)occurances.End
                            }
                        };
            return query.Take(this.RPDEPageSize).ToList();
        }
    }

    public class AcmeSessionSeriesRPDEGenerator : RPDEFeedModifiedTimestampAndIDLong<SessionOpportunity, SessionSeries>
    {
        protected override List<RpdeItem<SessionSeries>> GetRPDEItems(long? afterTimestamp, long? afterId)
        {
            var query = from @class in FakeBookingSystem.Database.Classes
                        join seller in FakeBookingSystem.Database.Sellers on @class.SellerId equals seller.Id
                        orderby @class.Modified, @class.Id
                        where !afterTimestamp.HasValue && !afterId.HasValue ||
                              @class.Modified.ToUnixTimeMilliseconds() > afterTimestamp ||
                              (@class.Modified.ToUnixTimeMilliseconds() == afterTimestamp && @class.Id > afterId)

                        select new RpdeItem<SessionSeries>
                        {
                            Kind = RpdeKind.SessionSeries,
                            Id = @class.Id,
                            Modified = @class.Modified.ToUnixTimeMilliseconds(),
                            State = @class.Deleted ? RpdeState.Deleted : RpdeState.Updated,
                            Data = @class.Deleted ? null : new SessionSeries
                            {
                                // QUESTION: Should the this.IdTemplate and this.BaseUrl be passed in each time rather than set on
                                // the parent class? Current thinking is it's more extensible on parent class as function signature remains
                                // constant as power of configuration through underlying class grows (i.e. as new properties are added)
                                Id = this.RenderOpportunityId( new SessionOpportunity
                                {
                                    OpportunityType = OpportunityType.SessionSeries,
                                    SessionSeriesId = @class.Id
                                }),
                                Name = @class.Title,
                                Organizer = seller.IsIndividual ? (ILegalEntity)new Person
                                {
                                    Id = this.RenderSellerId(new SellerIdComponents { SellerIdLong = seller.Id }),
                                    Name = seller.Name,
                                    TaxMode = TaxMode.TaxGross
                                } : (ILegalEntity)new Organization
                                {
                                    Id = this.RenderSellerId(new SellerIdComponents { SellerIdLong = seller.Id }),
                                    Name = seller.Name,
                                    TaxMode = TaxMode.TaxGross
                                },
                                Offers = new List<Offer> { new Offer
                                    {
                                        Id = this.RenderOfferId(new SessionOpportunity
                                        {
                                            OfferOpportunityType = OpportunityType.SessionSeries,
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
