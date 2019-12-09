using System;
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
                                Seller = thisOrder.Key.seller.IsIndividual ? (ILegalEntity)new Person
                                {
                                    Id = this.RenderSellerId(new SellerIdComponents { SellerIdLong = thisOrder.Key.seller.Id }),
                                    Name = thisOrder.Key.seller.Name,
                                    TaxMode = TaxMode.TaxGross
                                } : (ILegalEntity)new Organization
                                {
                                    Id = this.RenderSellerId(new SellerIdComponents { SellerIdLong = thisOrder.Key.seller.Id }),
                                    Name = thisOrder.Key.seller.Name,
                                    TaxMode = TaxMode.TaxGross
                                },
                                Customer = thisOrder.Key.orders.CustomerIsOrganization ? (ILegalEntity) new Organization
                                {
                                    Email = thisOrder.Key.orders.CustomerEmail
                                } : (ILegalEntity) new Person
                                {
                                    Email = thisOrder.Key.orders.CustomerEmail
                                },
                                BrokerRole = thisOrder.Key.orders.BrokerRole == BrokerRole.AgentBroker ? BrokerType.AgentBroker : thisOrder.Key.orders.BrokerRole == BrokerRole.ResellerBroker ? BrokerType.ResellerBroker : BrokerType.NoBroker,
                                Broker = new Organization { 
                                    Name = thisOrder.Key.orders.BrokerName
                                },
                                Payment = new Payment
                                {
                                    Identifier = thisOrder.Key.orders.PaymentIdentifier
                                },
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
                                        orderItem.Status == BookingStatus.Confirmed ? OrderItemStatus.OrderConfirmed :
                                        orderItem.Status == BookingStatus.CustomerCancelled ? OrderItemStatus.CustomerCancelled :
                                        orderItem.Status == BookingStatus.SellerCancelled ? OrderItemStatus.SellerCancelled :
                                        orderItem.Status == BookingStatus.Attended ? OrderItemStatus.CustomerAttended : (OrderItemStatus?)null

                                }).ToList()
                            }
                        };
            var list = query.Take(this.RPDEPageSize).ToList();
            return list;
        }
    }
}
