using OpenActive.FakeDatabase.NET;
using OpenActive.NET;
using OpenActive.Server.NET.OpenBookingHelper;
using OpenActive.Server.NET.StoreBooking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingSystem.AspNetCore
{
    public class AcmeOrderStore : OrderStore<DatabaseTransaction>
    {
        /// <summary>
        /// Initiate customer cancellation for the specified OrderItems
        /// </summary>
        /// <returns>True if Order found, False if Order not found</returns>
        public override bool CustomerCancelOrderItems(OrderIdTemplate orderIdTemplate, OrderIdComponents orderId, List<OrderIdComponents> orderItemIds)
        {
            return FakeBookingSystem.Database.CancelOrderItem(orderId.uuid, orderItemIds.Select(x => x.OrderItemIdLong.Value).ToList(), true);
        }

        public override Lease CreateLease(OrderQuote orderQuote, StoreBookingFlowContext flowContext, DatabaseTransaction databaseTransaction)
        {
            if (orderQuote.TotalPaymentDue.PriceCurrency != "GBP")
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
                    flowContext.OrderId.uuid,
                    flowContext.BrokerRole == BrokerType.AgentBroker ? BrokerRole.AgentBroker : flowContext.BrokerRole == BrokerType.ResellerBroker ? BrokerRole.ResellerBroker : BrokerRole.NoBroker,
                    flowContext.Broker.Name,
                    flowContext.SellerId.SellerIdLong.Value,
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

        public override void DeleteLease(OrderIdComponents orderId)
        {
            // Note if no lease support, simply do nothing here
            FakeBookingSystem.Database.DeleteLease(orderId.uuid);
        }

        public override void CreateOrder(Order order, StoreBookingFlowContext flowContext, DatabaseTransaction databaseTransaction)
        {
            if (order.TotalPaymentDue.PriceCurrency != "GBP")
            {
                throw new OpenBookingException(new OpenBookingError(), "Unsupported currency");
            }

            var result = databaseTransaction.Database.AddOrder(
                flowContext.OrderId.uuid,
                flowContext.BrokerRole == BrokerType.AgentBroker ? BrokerRole.AgentBroker : flowContext.BrokerRole == BrokerType.ResellerBroker ? BrokerRole.ResellerBroker : BrokerRole.NoBroker,
                flowContext.Broker.Name,
                flowContext.SellerId.SellerIdLong.Value,
                flowContext.Customer.Email,
                flowContext.Payment?.Identifier,
                order.TotalPaymentDue.Price.Value);

            if (!result) throw new OpenBookingException(new OrderAlreadyExistsError());
        }

        public override void DeleteOrder(OrderIdComponents orderId)
        {
            FakeBookingSystem.Database.DeleteOrder(orderId.uuid);
        }


        protected override DatabaseTransaction BeginOrderTransaction(FlowStage stage)
        {
            if (stage != FlowStage.C1)
            {
                return new DatabaseTransaction(FakeBookingSystem.Database);
            }
            else
            {
                return null;
            }
        }

        protected override void CompleteOrderTransaction(DatabaseTransaction databaseTransaction)
        {
            databaseTransaction.CommitTransaction();
        }

        protected override void RollbackOrderTransaction(DatabaseTransaction databaseTransaction)
        {
            databaseTransaction.Database = null;
        }
    }
}
