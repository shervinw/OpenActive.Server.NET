using OpenActive.NET;
using OpenActive.Server.NET.OpenBookingHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET.StoreBooking
{
    public interface IOrderStore
    {
        IDatabaseTransaction BeginOrderTransaction(FlowStage stage);
        Lease CreateLease(OrderQuote responseOrderQuote, StoreBookingFlowContext flowContext, IDatabaseTransaction dbTransaction);
        void CreateOrder(Order responseOrder, StoreBookingFlowContext flowContext, IDatabaseTransaction dbTransaction);
        bool CustomerCancelOrderItems(OrderIdComponents orderId, SellerIdComponents sellerId, OrderIdTemplate orderIdTemplate, List<OrderIdComponents> orderItemIds);
        void DeleteOrder(OrderIdComponents orderId, SellerIdComponents sellerId);
        void DeleteLease(OrderIdComponents orderId, SellerIdComponents sellerId);
    }

    public abstract class OrderStore<TDatabaseTransaction> : IOrderStore where TDatabaseTransaction : IDatabaseTransaction
    {
        public abstract Lease CreateLease(OrderQuote responseOrderQuote, StoreBookingFlowContext flowContext, TDatabaseTransaction databaseTransaction);
        public abstract void CreateOrder(Order responseOrder, StoreBookingFlowContext flowContext, TDatabaseTransaction databaseTransaction);

        public Lease CreateLease(OrderQuote responseOrderQuote, StoreBookingFlowContext flowContext, IDatabaseTransaction dbTransaction)
        {
            return CreateLease(responseOrderQuote, flowContext, (TDatabaseTransaction)dbTransaction);
        }

        public void CreateOrder(Order responseOrder, StoreBookingFlowContext flowContext, IDatabaseTransaction dbTransaction)
        {
            CreateOrder(responseOrder, flowContext, (TDatabaseTransaction)dbTransaction);
        }

        /// <summary>
        /// Stage is provided as it depending on the implementation (e.g. what level of leasing is applied)
        /// it might not be appropriate to create transactions for all stages.
        /// Null can be returned in the case that a transaction has not been created.
        /// </summary>
        /// <param name="stage"></param>
        /// <returns></returns>
        protected abstract TDatabaseTransaction BeginOrderTransaction(FlowStage stage);

        IDatabaseTransaction IOrderStore.BeginOrderTransaction(FlowStage stage)
        {
            return BeginOrderTransaction(stage);
        }

        public abstract bool CustomerCancelOrderItems(OrderIdComponents orderId, SellerIdComponents sellerId, OrderIdTemplate orderIdTemplate, List<OrderIdComponents> orderItemIds);
        public abstract void DeleteOrder(OrderIdComponents orderId, SellerIdComponents sellerId);
        public abstract void DeleteLease(OrderIdComponents orderId, SellerIdComponents sellerId);
    }


}
