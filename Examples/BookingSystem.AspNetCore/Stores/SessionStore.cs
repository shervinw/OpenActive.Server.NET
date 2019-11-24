using Newtonsoft.Json;
using OpenActive.NET;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Linq;
using OpenActive.DatasetSite.NET;
using OpenActive.Server.NET.StoreBooking;
using OpenActive.Server.NET.OpenBookingHelper;
using System.Data.Common;
using OpenActive.FakeDatabase.NET;

namespace BookingSystem.AspNetCore
{

    /// <summary>
    /// TODO: Move to BookingSystem.AspNetCore
    /// </summary>
    class SessionStore : OpportunityStore<SessionOpportunity, DatabaseTransaction>
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


        // Similar to the RPDE logic, this needs to render and return an new hypothetical OrderItem from the database based on the supplied opportunity IDs
        protected override List<OrderItem> GetOrderItem(List<SessionOpportunity> opportunityOfferIds, /* Person attendeeDetails, */ StoreBookingFlowContext context)
        {

            // Note the implementation of this method must also check that this OrderItem is from the Seller specified by context.SellerIdComponents

            // Additionall this method must check that there are enough spaces in each entry

            // Order must be returned in the same order as supplied opportunityOfferIds (including duplicates for multi-party booking)

            var query = from opportunityOfferId in opportunityOfferIds
                        join occurances in FakeBookingSystem.Database.Occurrences on opportunityOfferId.ScheduledSessionId equals occurances.Id
                        join classes in FakeBookingSystem.Database.Classes on occurances.ClassId equals classes.Id
                        // and offers.id = opportunityOfferId.OfferId
                        select new OrderItem
                        {
                            AllowCustomerCancellationFullRefund = true,
                            // TODO: The static example below should come from the database (which doesn't currently support tax)
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

        protected override List<OpenBookingError> LeaseOrderItem(List<SessionOpportunity> opportunityOfferId, StoreBookingFlowContext context, DatabaseTransaction databaseTransaction)
        {
            // Check that there are no conflicts between the supplied opportunities
            // Also take into account spaces requested across OrderItems against total spaces in each opportunity

            // The order of the errors returned MUST match the order of the opportunityOfferId provided,
            // as the order of OrderItems are respected throughout processing 
            return opportunityOfferId.Select((x, i) => new
            {
                Index = i,
                OpportunityOfferId = x
            }).GroupBy(x => x.OpportunityOfferId).Select(x =>
            {
                // Check that the Opportunity ID and type are as expected for the store 
                if (x.Key.OpportunityType != OpportunityType.ScheduledSession || !x.Key.ScheduledSessionId.HasValue)
                {
                    return x.Select(y => new { Index = y.Index, error = new OpenBookingError { Description = "OpportunityNotBookableError" } });
                }

                // Attempt to lease for those with the same IDs, which is atomic
                bool result = databaseTransaction.Database.LeaseOrderItemsForClassOccurrence(context.OrderId.uuid, x.Key.SessionSeriesId.Value, this.RenderOpportunityId(x.Key).ToString(), this.RenderOfferId(x.Key).ToString(), x.Count());
                return x.Select(y => new { Index = y.Index, error = result ? null : new OpenBookingError { Description = "Item could not be leased" } });
            })
            .SelectMany(x => x)
            // Maintain the order of the items
            .OrderBy(x => x.Index)
            .Select(x => x.error)
            .ToList();
        }

        //TODO: This should reuse code of LeaseOrderItem
        protected override List<OrderIdComponents> BookOrderItem(List<SessionOpportunity> opportunityOfferId, List<OrderItem> orderItems, StoreBookingFlowContext context, DatabaseTransaction databaseTransaction)
        {
            // Check that there are no conflicts between the supplied opportunities
            // Also take into account spaces requested across OrderItems against total spaces in each opportunity

            // The order of the errors returned MUST match the order of the opportunityOfferId provided,
            // as the order of OrderItems are respected throughout processing 
            return opportunityOfferId.Select((x, i) => new
            {
                Index = i,
                OpportunityOfferId = x
            }).GroupBy(x => x.OpportunityOfferId).Select(x =>
            {
                // Check that the Opportunity ID and type are as expected for the store 
                if (x.Key.OpportunityType != OpportunityType.ScheduledSession || !x.Key.ScheduledSessionId.HasValue)
                {
                    throw new OpenBookingException(new OpenBookingError(), "OpportunityNotBookableError");
                }

                // Attempt to lease for those with the same IDs, which is atomic
                List<long> orderItemIds = databaseTransaction.Database.BookOrderItemsForClassOccurrence(context.OrderId.uuid, x.Key.SessionSeriesId.Value, this.RenderOpportunityId(x.Key).ToString(), this.RenderOfferId(x.Key).ToString(), x.Count());
                if (orderItemIds == null)
                {
                    throw new OpenBookingException(new OpenBookingError(), "BookingFailedError");
                }
                return x.Zip(orderItemIds, (y, id) => new
                {
                    Index = y.Index,
                    Id = new OrderIdComponents
                    {
                        uuid = context.OrderId.uuid,
                        OrderType = context.OrderId.OrderType,
                        OrderItemIdLong = id
                    }
                });
            })
            .SelectMany(x => x)
            // Maintain the order of the items
            .OrderBy(x => x.Index)
            .Select(x => x.Id)
            .ToList();
        }
    }

}
