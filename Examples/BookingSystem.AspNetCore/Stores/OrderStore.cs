using OpenActive.NET;
using OpenActive.Server.NET.OpenBookingHelper;
using OpenActive.Server.NET.StoreBooking;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace BookingSystem.AspNetCore
{
    public class AcmeOrderStore : OrderStore<DbTransaction>
    {
        public override void CancelOrderItemByCustomer(OrderIdTemplate orderIdTemplate, OrderIdComponents orderId, List<OrderIdComponents> orderItemIds)
        {
            throw new NotImplementedException();
        }

        public override Lease CreateLease(OrderQuote responseOrderQuote, StoreBookingFlowContext context, DbTransaction databaseTransaction)
        {
            throw new NotImplementedException();
        }

        public override void DeleteLease(OrderIdComponents orderId)
        {
            throw new NotImplementedException();
        }

        public override void CreateOrder(Order responseOrder, StoreBookingFlowContext context, DbTransaction databaseTransaction)
        {
            throw new NotImplementedException();
        }

        public override void DeleteOrder(OrderIdComponents orderId)
        {
            throw new NotImplementedException();
        }

        protected override DbTransaction BeginOrderTransaction(FlowStage stage)
        {
            throw new NotImplementedException();
        }

        protected override void CompleteOrderTransaction(DbTransaction databaseTransaction)
        {
            throw new NotImplementedException();
        }

        protected override void RollbackOrderTransaction(DbTransaction databaseTransaction)
        {
            throw new NotImplementedException();
        }
    }
}
