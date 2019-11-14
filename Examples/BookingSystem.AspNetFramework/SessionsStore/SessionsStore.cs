using Newtonsoft.Json;
using OpenActive.NET;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Linq;
using BookingSystem.FakeDatabase;
using static BookingSystem.FakeDatabase.FakeDatabase;
using OpenActive.DatasetSite.NET;
using OpenActive.Server.NET.StoreBooking;
using OpenActive.Server.NET.OpenBookingHelper;

namespace BookingSystem.AspNetFramework
{

    /// <summary>
    /// TODO: Move to BookingSystem.AspNetCore
    /// </summary>
    class SessionsStore : OpportunityStore<SessionOpportunity>, IOpportunityStore
    {
        
        public override void CreateTestDataItem(OpportunityType opportunityType, Event @event)
        {
            // Note assume that if it's been routed here, it will be possible to cast it to type Event
            FakeBookingSystem.Database.AddClass(@event.Name, ((Event)@event).Offers?.FirstOrDefault()?.Price);
        }

        public override void DeleteTestDataItem(OpportunityType opportunityType, string name)
        {
            FakeBookingSystem.Database.DeleteClass(name);
        }

        // Similar to the RPDE logic, this needs to render and return an OrderItem from the database
        protected override OrderItem GetOrderItem<TOrder>(SessionOpportunity opportunityOfferId, StoreBookingFlowContext<TOrder> context)
        {
            var query = from occurances in FakeBookingSystem.Database.Occurrences
                        join classes in FakeBookingSystem.Database.Classes on occurances.ClassId equals classes.Id
                        where occurances.Id == opportunityOfferId.ScheduledSessionId
                        // and offers.id = opportunityOfferId.OfferId
                        select new OrderItem
                        {
                            AllowCustomerCancellationFullRefund = true,
                            UnitTaxSpecification = context.FlowContext.TaxPayeeRelationship == TaxPayeeRelationship.BusinessToConsumer ?
                                new List<TaxChargeSpecification>
                                {
                                    new TaxChargeSpecification
                                    {
                                        Name = "VAT at 20%",
                                        Price = classes.Price * (decimal?)0.2,
                                        PriceCurrency = "GBP",
                                        Rate = (decimal?)0.2
                                    }
                                } : null,
                            AcceptedOffer = new Offer
                            {
                                // Note this should always use RenderOfferId with the supplied SessionOpportunity, to take into account inheritance
                                Id = this.RenderOfferId(opportunityOfferId),
                                Price = classes.Price,
                                PriceCurrency = "GBP"
                            },
                            OrderedItem = new ScheduledSession
                            {
                                // Note this should always be driven from the database, with new SessionOpportunity's instantiated
                                Id = this.RenderOpportunityId(new SessionOpportunity
                                {
                                    OpportunityType = OpportunityType.ScheduledSession,
                                    BaseUrl = this.JsonLdIdBaseUrl,
                                    SessionSeriesId = occurances.ClassId,
                                    ScheduledSessionId = occurances.Id
                                }),
                                SuperEvent = new SessionSeries
                                {
                                    Id = this.RenderOpportunityId(new SessionOpportunity
                                    {
                                        OpportunityType = OpportunityType.SessionSeries,
                                        BaseUrl = this.JsonLdIdBaseUrl,
                                        SessionSeriesId = occurances.ClassId
                                    }),
                                    Name = classes.Title
                                },
                                StartDate = (DateTimeOffset)occurances.Start,
                                EndDate = (DateTimeOffset)occurances.End
                            }
                        };
            return query.First();
        }
    }

}
