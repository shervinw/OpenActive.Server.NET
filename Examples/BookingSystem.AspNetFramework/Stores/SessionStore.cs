using OpenActive.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenActive.DatasetSite.NET;
using OpenActive.Server.NET.StoreBooking;
using OpenActive.Server.NET.OpenBookingHelper;
using OpenActive.FakeDatabase.NET;

namespace BookingSystem
{
    class SessionStore : OpportunityStore<SessionOpportunity, OrderTransaction>
    {

        protected override SessionOpportunity CreateTestDataItem(OpportunityType opportunityType, Event @event)
        {
            // Note assume that if it's been routed here, it will be possible to cast it to type Event
            switch (opportunityType)
            {
                case OpportunityType.ScheduledSession:
                    var session = (ScheduledSession)@event;
                    var superEvent = (SessionSeries)session.SuperEvent.GetClass<Event>();
                    var (classId, occurrenceId) = FakeBookingSystem.Database.AddClass(superEvent.Name, superEvent.Offers?.FirstOrDefault()?.Price, session.StartDate.GetPrimative<DateTimeOffset>() ?? default, session.EndDate.GetPrimative<DateTimeOffset>() ?? default, session.MaximumAttendeeCapacity.Value);
                    return new SessionOpportunity { 
                        OpportunityType = opportunityType,
                        SessionSeriesId = classId,
                        ScheduledSessionId = occurrenceId
                    };
                default:
                    throw new OpenBookingException(new OpenBookingError(), "Opportunity Type not supported");
            }
            
        }

        protected override void DeleteTestDataItem(OpportunityType opportunityType, SessionOpportunity sessionOpportunity)
        {
            if (!sessionOpportunity.SessionSeriesId.HasValue || !sessionOpportunity.ScheduledSessionId.HasValue)
                throw new OpenBookingException(new OpenBookingError(), "Invalid Session Id");

            // Note assume that if it's been routed here, it will be possible to cast it to type Event
            switch (opportunityType)
                {
                    case OpportunityType.ScheduledSession:
                        FakeBookingSystem.Database.DeleteClass(sessionOpportunity.SessionSeriesId.Value, sessionOpportunity.ScheduledSessionId.Value);
                        break;
                }
            
        }


        // Similar to the RPDE logic, this needs to render and return an new hypothetical OrderItem from the database based on the supplied opportunity IDs
        protected override void GetOrderItem(List<OrderItemContext<SessionOpportunity>> orderItemContexts, StoreBookingFlowContext flowContext)
        {

            // Note the implementation of this method must also check that this OrderItem is from the Seller specified by context.SellerIdComponents (this is not required if using a Single Seller)

            // Additionally this method must check that there are enough spaces in each entry

            // Response OrderItems must be updated into supplied orderItemContexts (including duplicates for multi-party booking)

            var query = (from orderItemContext in orderItemContexts
                         join occurances in FakeBookingSystem.Database.Occurrences on orderItemContext.RequestBookableOpportunityOfferId.ScheduledSessionId equals occurances.Id
                         join classes in FakeBookingSystem.Database.Classes on occurances.ClassId equals classes.Id
                         // and offers.id = opportunityOfferId.OfferId
                         select occurances == null ? null : new OrderItem
                         {
                             AllowCustomerCancellationFullRefund = true,
                             // TODO: The static example below should come from the database (which doesn't currently support tax)
                             UnitTaxSpecification = flowContext.TaxPayeeRelationship == TaxPayeeRelationship.BusinessToConsumer ?
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
                                 Id = this.RenderOfferId(orderItemContext.RequestBookableOpportunityOfferId),
                                 Price = classes.Price,
                                 PriceCurrency = "GBP",
                                 AvailableChannel = new List<AvailableChannelType> { AvailableChannelType.OpenBookingPrepayment }
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
                                 EndDate = (DateTimeOffset)occurances.End,
                                 MaximumAttendeeCapacity = occurances.TotalSpaces,
                                 RemainingAttendeeCapacity = occurances.RemainingSpaces
                             }
                         });

            // Add the response OrderItems to the relevant contexts (note that the context must be updated within this method)
            foreach (var (item, ctx) in query.Zip(orderItemContexts, (item, ctx) => (item, ctx)))
            {
                if (item == null)
                {
                    ctx.SetResponseOrderItemAsSkeleton();
                    ctx.AddError(new UnknownOpportunityDetailsError());
                }
                else
                {
                    ctx.SetResponseOrderItem(item);

                    if (item.OrderedItem.RemainingAttendeeCapacity == 0)
                    {
                        ctx.AddError(new OpportunityIsFullError());
                    }
                }
                
            }

            // Add errors to the response according to the attendee details specified as required in the ResponseOrderItem,
            // and those provided in the requestOrderItem
            orderItemContexts.ForEach(ctx => ctx.ValidateAttendeeDetails());

            // Additional attendee detail validation logic goes here
            // ...

        }

        protected override void LeaseOrderItem(Lease lease, List<OrderItemContext<SessionOpportunity>> orderItemContexts, StoreBookingFlowContext flowContext, OrderTransaction databaseTransaction)
        {
            // Check that there are no conflicts between the supplied opportunities
            // Also take into account spaces requested across OrderItems against total spaces in each opportunity

            foreach (var ctxGroup in orderItemContexts.GroupBy(x => x.RequestBookableOpportunityOfferId))
            {
                // Check that the Opportunity ID and type are as expected for the store 
                if (ctxGroup.Key.OpportunityType != OpportunityType.ScheduledSession || !ctxGroup.Key.ScheduledSessionId.HasValue)
                {
                    foreach (var ctx in ctxGroup)
                    {
                        ctx.AddError(new OpenBookingError { Description = "OpportunityNotBookableError" });
                    }
                }
                else
                {
                    // Attempt to lease for those with the same IDs, which is atomic
                    bool result = databaseTransaction.Database.LeaseOrderItemsForClassOccurrence(flowContext.OrderId.ClientId, flowContext.SellerId.SellerIdLong ?? null /* Hack to allow this to work in Single Seller mode too */, flowContext.OrderId.uuid, ctxGroup.Key.ScheduledSessionId.Value, ctxGroup.Count());
                   
                    if (!result)
                    {
                        foreach (var ctx in ctxGroup)
                        {
                            ctx.AddError(new OpenBookingError { Description = "Item could not be leased" });
                        }
                    }
                }
            }
        }

        //TODO: This should reuse code of LeaseOrderItem
        protected override void BookOrderItem(List<OrderItemContext<SessionOpportunity>> orderItemContexts, StoreBookingFlowContext flowContext, OrderTransaction databaseTransaction)
        {
            // Check that there are no conflicts between the supplied opportunities
            // Also take into account spaces requested across OrderItems against total spaces in each opportunity

            foreach (var ctxGroup in orderItemContexts.GroupBy(x => x.RequestBookableOpportunityOfferId))
            {
                // Check that the Opportunity ID and type are as expected for the store 
                if (ctxGroup.Key.OpportunityType != OpportunityType.ScheduledSession || !ctxGroup.Key.ScheduledSessionId.HasValue)
                {
                    throw new OpenBookingException(new OpenBookingError(), "OpportunityNotBookableError");
                }

                // Attempt to lease for those with the same IDs, which is atomic
                List<long> orderItemIds = databaseTransaction.Database.BookOrderItemsForClassOccurrence(flowContext.OrderId.ClientId, flowContext.SellerId.SellerIdLong ?? null  /* Hack to allow this to work in Single Seller mode too */, flowContext.OrderId.uuid, ctxGroup.Key.ScheduledSessionId.Value, this.RenderOpportunityJsonLdType(ctxGroup.Key), this.RenderOpportunityId(ctxGroup.Key).ToString(), this.RenderOfferId(ctxGroup.Key).ToString(), ctxGroup.Count());

                if (orderItemIds != null)
                {
                    // Set OrderItemId for each orderItemContext
                    foreach (var (ctx, id) in ctxGroup.Zip(orderItemIds, (ctx, id) => (ctx, id)))
                    {
                        ctx.SetOrderItemId(flowContext, id);
                    }
                }
                else
                {
                    throw new OpenBookingException(new OpenBookingError(), "BookingFailedError");
                }
            }
        }
    }

}
