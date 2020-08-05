﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenActive.Server.NET.OpenBookingHelper;
using System.Threading.Tasks;
using OpenActive.NET.Rpde.Version1;
using OpenActive.NET;
using OpenActive.FakeDatabase.NET;
using ServiceStack.OrmLite;

namespace BookingSystem
{
    public class AcmeOrdersFeedRPDEGenerator : OrdersRPDEFeedModifiedTimestampAndID
    {
        //public override string FeedPath { get; protected set; } = "example path override";

        protected override List<RpdeItem> GetRPDEItems(string clientId, long? afterTimestamp, string afterId)
        {
            using (var db = FakeBookingSystem.Database.Mem.Database.Open())
            {
                var q = db.From<OrderTable>()
                .Join<SellerTable>()
                .Join<OrderItemsTable>()
                .OrderBy(x => x.Modified)
                .OrderBy(x => x.Id)
                .Where(x => x.VisibleInFeed && x.ClientId == clientId && (!afterTimestamp.HasValue || x.Modified > afterTimestamp ||
                        (x.Modified == afterTimestamp &&  string.Compare(afterId, x.OrderId) <= 0)) && x.Modified < (DateTimeOffset.UtcNow - new TimeSpan(0, 0, 2)).UtcTicks);

                var query = db
                    .SelectMulti<OrderTable, SellerTable, OrderItemsTable>(q)
                    .GroupBy(x => new { x.Item1.Id })
                    .Select((result) => new
                    {
                        OrderTable = result.Select(item => new { item.Item1 }).FirstOrDefault().Item1,
                        Seller = result.Select(item => new { item.Item2 }).FirstOrDefault().Item2,
                        OrderItemsTable = result.Select(item => new { item.Item3 }).ToList()
                    })
                    .Select((result) => new RpdeItem
                    {
                        Kind = RpdeKind.Order,
                        Id = result.OrderTable.OrderId,
                        Modified = result.OrderTable.Modified,
                        State = result.OrderTable.Deleted ? RpdeState.Deleted : RpdeState.Updated,
                        Data = result.OrderTable.Deleted ? null : new Order
                        {
                            Id = this.RenderOrderId(OrderType.Order, result.OrderTable.OrderId),
                            Identifier = result.OrderTable.OrderId,
                            TotalPaymentDue = new PriceSpecification
                            {
                                Price = result.OrderTable.TotalOrderPrice,
                                PriceCurrency = "GBP"
                            },
                            OrderedItem = result.OrderItemsTable.Select((orderItem) => new OrderItem
                            {
                                Id = this.RenderOrderItemId(OrderType.Order, result.OrderTable.OrderId, orderItem.Item3.Id),
                                AcceptedOffer = new Offer
                                {
                                    Id = new Uri(orderItem.Item3.OfferJsonLdId),
                                    Price = orderItem.Item3.Price,
                                    PriceCurrency = "GBP"
                                },
                                OrderedItem = RenderOpportunityWithOnlyId(orderItem.Item3.OpportunityJsonLdType, new Uri(orderItem.Item3.OpportunityJsonLdId)),
                                OrderItemStatus =
                                    orderItem.Item3.Status == BookingStatus.Confirmed ? OrderItemStatus.OrderItemConfirmed :
                                    orderItem.Item3.Status == BookingStatus.CustomerCancelled ? OrderItemStatus.CustomerCancelled :
                                    orderItem.Item3.Status == BookingStatus.SellerCancelled ? OrderItemStatus.SellerCancelled :
                                    orderItem.Item3.Status == BookingStatus.Attended ? OrderItemStatus.CustomerAttended : (OrderItemStatus?)null

                            }).ToList()
                        }
                    });

                // Note there's a race condition in the in-memory database that allows records to be returned from the above query out of order when modified at the same time. The below ensures the correct order is returned.
                var items = query.ToList().Take(this.RPDEPageSize).ToList();

                /*
                // Filter out any that were updated while the query was running
                var lastItemModified = items.LastOrDefault()?.Modified;

                if (lastItemModified != null)
                {
                    items = items.Where(x => x.Modified <= lastItemModified).ToList(); //.OrderBy(x => x.Modified).ThenBy(x => x.Id)
                }
                */
                return items;
            }  
        }
    }
}
