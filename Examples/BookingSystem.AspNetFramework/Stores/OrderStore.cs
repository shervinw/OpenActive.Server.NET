using OpenActive.FakeDatabase.NET;
using OpenActive.NET;
using OpenActive.Server.NET.OpenBookingHelper;
using OpenActive.Server.NET.StoreBooking;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BookingSystem
{
    public class AcmeOrderStore : OrderStore<OrderTransaction>
    {
        /// <summary>
        /// Initiate customer cancellation for the specified OrderItems
        /// Note sellerId will always be null in Single Seller mode
        /// </summary>
        /// <returns>True if Order found, False if Order not found</returns>
        public override bool CustomerCancelOrderItems(OrderIdComponents orderId, SellerIdComponents sellerId, OrderIdTemplate orderIdTemplate, List<OrderIdComponents> orderItemIds)
        {
            return FakeBookingSystem.Database.CancelOrderItem(orderId.ClientId, sellerId.SellerIdLong ?? null  /* Hack to allow this to work in Single Seller mode too */, orderId.uuid, orderItemIds.Select(x => x.OrderItemIdLong.Value).ToList(), true);
        }

        public override Lease CreateLease(OrderQuote responseOrderQuote, StoreBookingFlowContext flowContext, OrderTransaction databaseTransaction)
        {
            if (responseOrderQuote.TotalPaymentDue.PriceCurrency != "GBP")
            {
                throw new OpenBookingException(new OpenBookingError(), "Unsupported currency");
            }

            // Note if no lease support, simply return null always here instead

            // In this example leasing is only supported at C2
            if (flowContext.Stage == FlowStage.C2)
            {
                // TODO: Make the lease duration configurable
                var leaseExpires = DateTimeOffset.Now + new TimeSpan(0, 5, 0);

                var result = databaseTransaction.Database.AddLease(
                    flowContext.OrderId.ClientId,
                    flowContext.OrderId.uuid,
                    flowContext.BrokerRole == BrokerType.AgentBroker ? BrokerRole.AgentBroker : flowContext.BrokerRole == BrokerType.ResellerBroker ? BrokerRole.ResellerBroker : BrokerRole.NoBroker,
                    flowContext.Broker.Name,
                    flowContext.SellerId.SellerIdLong ?? null, // Small hack to allow use of FakeDatabase when in Single Seller mode
                    flowContext.Customer.Email,
                    leaseExpires
                    );

                if (!result) throw new OpenBookingException(new OrderAlreadyExistsError());

                return new Lease
                {
                    LeaseExpires = leaseExpires
                };
            }
            else
            {
                return null;
            }
        }

        public override void DeleteLease(OrderIdComponents orderId, SellerIdComponents sellerId)
        {
            // Note if no lease support, simply do nothing here
            FakeBookingSystem.Database.DeleteLease(orderId.ClientId, orderId.uuid, sellerId.SellerIdLong.Value);
        }

        public override void CreateOrder(Order responseOrder, StoreBookingFlowContext flowContext, OrderTransaction databaseTransaction)
        {
            if (responseOrder.TotalPaymentDue.PriceCurrency != "GBP")
            {
                throw new OpenBookingException(new OpenBookingError(), "Unsupported currency");
            }

            var result = databaseTransaction.Database.AddOrder(
                flowContext.OrderId.ClientId,
                flowContext.OrderId.uuid,
                flowContext.BrokerRole == BrokerType.AgentBroker ? BrokerRole.AgentBroker : flowContext.BrokerRole == BrokerType.ResellerBroker ? BrokerRole.ResellerBroker : BrokerRole.NoBroker,
                flowContext.Broker.Name,
                flowContext.SellerId.SellerIdLong ?? null, // Small hack to allow use of FakeDatabase when in Single Seller mode
                flowContext.Customer.Email,
                flowContext.Payment?.Identifier,
                responseOrder.TotalPaymentDue.Price.Value);

            if (!result) throw new OpenBookingException(new OrderAlreadyExistsError());
        }

        public override void DeleteOrder(OrderIdComponents orderId, SellerIdComponents sellerId)
        {
            FakeBookingSystem.Database.DeleteOrder(orderId.ClientId, orderId.uuid, sellerId.SellerIdLong ?? null /* Small hack to allow use of FakeDatabase when in Single Seller mode */);
        }


        protected override OrderTransaction BeginOrderTransaction(FlowStage stage)
        {
            if (stage != FlowStage.C1)
            {
                return new OrderTransaction();
            }
            else
            {
                return null;
            }
        }
    }
}
