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
using System.Data.Common;

namespace BookingSystem.AspNetCore
{

    /// <summary>
    /// TODO: Move to BookingSystem.AspNetCore
    /// </summary>
    class SessionStore : OpportunityStore<SessionOpportunity, DbTransaction>
    {
        
        public override void CreateTestDataItem(OpportunityType opportunityType, Event @event)
        {
            // Note assume that if it's been routed here, it will be possible to cast it to type Event
            switch (opportunityType)
            {
                case OpportunityType.ScheduledSession:
                    var session = (ScheduledSession)@event;
                    var superEvent = (SessionSeries)session.SuperEvent.GetClass<Event>();
                    // Note temporary hack while waiting for OpenActive.NET accessors to work as expected
                    FakeBookingSystem.Database.AddClass(superEvent.Name, superEvent.Offers?.FirstOrDefault()?.Price, (DateTimeOffset?)session.StartDate.Value ?? default, (DateTimeOffset?)session.EndDate.Value ?? default);
                    break;
            }
            
        }

        public override void DeleteTestDataItem(OpportunityType opportunityType, string name)
        {
            FakeBookingSystem.Database.DeleteClass(name);
        }


        // Similar to the RPDE logic, this needs to render and return an OrderItem from the database
        protected override List<OrderItem> GetOrderItem(List<SessionOpportunity> opportunityOfferIds, /* Person attendeeDetails, */ StoreBookingFlowContext context)
        {

            // Note the implementation of this method must also check that this OrderItem is from the Seller specified by context.SellerIdComponents
            
            var query = from occurances in FakeBookingSystem.Database.Occurrences
                        join classes in FakeBookingSystem.Database.Classes on occurances.ClassId equals classes.Id
                        join opportunityOfferId in opportunityOfferIds on occurances.Id equals opportunityOfferId.ScheduledSessionId
                        // and offers.id = opportunityOfferId.OfferId
                        select new OrderItem
                        {
                            AllowCustomerCancellationFullRefund = true,
                            UnitTaxSpecification = context.TaxPayeeRelationship == TaxPayeeRelationship.BusinessToConsumer ?
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
                                // Note this should always use RenderOfferId with the supplied SessionOpportunity, to take into account inheritance and OfferType
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
                                    SessionSeriesId = occurances.ClassId,
                                    ScheduledSessionId = occurances.Id
                                }),
                                SuperEvent = new SessionSeries
                                {
                                    Id = this.RenderOpportunityId(new SessionOpportunity
                                    {
                                        OpportunityType = OpportunityType.SessionSeries,
                                        SessionSeriesId = occurances.ClassId
                                    }),
                                    Name = classes.Title
                                },
                                StartDate = (DateTimeOffset)occurances.Start,
                                EndDate = (DateTimeOffset)occurances.End
                            }
                        };
            return query.ToList();
        }

        protected override List<List<OpenBookingError>> LeaseOrderItem(List<SessionOpportunity> opportunityOfferId, StoreBookingFlowContext context, DbTransaction dbTransaction)
        {
            throw new NotImplementedException();
        }

        protected override List<OrderIdComponents> BookOrderItem(List<SessionOpportunity> opportunityOfferId, List<OrderItem> orderItems, StoreBookingFlowContext context, DbTransaction dbTransaction)
        {
            throw new NotImplementedException();
        }
    }

}
