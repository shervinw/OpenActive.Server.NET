﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenActive.Server.NET.OpenBookingHelper;
using System.Threading.Tasks;
using OpenActive.NET.Rpde.Version1;
using OpenActive.NET;
using OpenActive.FakeDatabase.NET;

namespace BookingSystem
{
    public class AcmeOrdersFeedRPDEGenerator : OrdersRPDEFeedModifiedTimestampAndID
    {
        //public override string FeedPath { get; protected set; } = "example path override";

        protected override List<RpdeItem> GetRPDEItems(string clientId, long? afterTimestamp, string afterId)
        {
            var query = from orders in FakeBookingSystem.Database.Orders
                        join seller in FakeBookingSystem.Database.Sellers on orders.SellerId equals seller.Id
                        join orderItems in FakeBookingSystem.Database.OrderItems on orders.Id equals orderItems.OrderId
                        where orders.VisibleInFeed && orders.ClientId == clientId && (!afterTimestamp.HasValue || orders.Modified.ToUnixTimeMilliseconds() > afterTimestamp ||
                        (orders.Modified.ToUnixTimeMilliseconds() == afterTimestamp && orders.Id.CompareTo(afterId) > 0))
                        // Ensure the RPDE endpoint filters out all items with a "modified" date after 2 seconds in the past, to delay items appearing in the feed
                        // https://app.gitbook.com/@openactive/s/openactive-developer/publishing-data/data-feeds/implementing-rpde-feeds
                        && orders.Modified < DateTimeOffset.UtcNow - new TimeSpan(0, 0, 2)
                        group orderItems by new { orders, seller } into thisOrder
                        orderby thisOrder.Key.orders.Modified, thisOrder.Key.orders.Id
                        select new RpdeItem
                        {
                            Kind = RpdeKind.Order,
                            Id = thisOrder.Key.orders.Id,
                            Modified = thisOrder.Key.orders.Modified.ToUnixTimeMilliseconds(),
                            State = thisOrder.Key.orders.Deleted ? RpdeState.Deleted : RpdeState.Updated,
                            Data = thisOrder.Key.orders.Deleted ? null : new Order
                            {
                                Id = this.RenderOrderId(OrderType.Order, thisOrder.Key.orders.Id),
                                Identifier = thisOrder.Key.orders.Id,
                                TotalPaymentDue = new PriceSpecification
                                {
                                    Price = thisOrder.Key.orders.TotalOrderPrice,
                                    PriceCurrency = "GBP"
                                },
                                OrderedItem = thisOrder.Select(orderItem => new OrderItem
                                {
                                    Id = this.RenderOrderItemId(OrderType.Order, thisOrder.Key.orders.Id, orderItem.Id),
                                    AcceptedOffer = new Offer { 
                                        Id = new Uri(orderItem.OfferJsonLdId),
                                        Price = orderItem.Price,
                                        PriceCurrency = "GBP"
                                    },
                                    OrderedItem = RenderOpportunityWithOnlyId(orderItem.OpportunityJsonLdType, new Uri(orderItem.OpportunityJsonLdId)),
                                    OrderItemStatus =
                                        orderItem.Status == BookingStatus.Confirmed ? OrderItemStatus.OrderItemConfirmed :
                                        orderItem.Status == BookingStatus.CustomerCancelled ? OrderItemStatus.CustomerCancelled :
                                        orderItem.Status == BookingStatus.SellerCancelled ? OrderItemStatus.SellerCancelled :
                                        orderItem.Status == BookingStatus.Attended ? OrderItemStatus.CustomerAttended : (OrderItemStatus?)null

                                }).ToList()
                            }
                        };
            // Note there's a race condition in the in-memory database that allows records to be returned from the above query out of order when modified at the same time. The below ensures the correct order is returned.
            var list = query.Take(this.RPDEPageSize).ToArray().OrderBy(x => x.Modified).ThenBy(x => x.Id).ToList();
            return list;
        }
    }
}
