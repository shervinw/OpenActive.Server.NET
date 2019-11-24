using OpenActive.NET;
using OpenActive.Server.NET.OpenBookingHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET.StoreBooking
{
    public interface IOrderStore
    {
        dynamic BeginOrderTransaction(FlowStage stage);
        void CompleteOrderTransaction(dynamic databaseTransaction);
        void RollbackOrderTransaction(dynamic databaseTransaction);
        Lease CreateLease(FlowStage flowStage, OrderQuote orderQuote, StoreBookingFlowContext context, dynamic dbTransaction);
        void CreateOrder(Order order, StoreBookingFlowContext context, dynamic dbTransaction);
        bool CustomerCancelOrderItems(OrderIdTemplate orderIdTemplate, OrderIdComponents orderId, List<OrderIdComponents> orderItemIds);
        void DeleteOrder(OrderIdComponents orderId);
        void DeleteLease(OrderIdComponents orderId);
    }

    public abstract class OrderStore<TDatabaseTransaction> : IOrderStore
    {
        public abstract Lease CreateLease(FlowStage flowStage, OrderQuote orderQuote, StoreBookingFlowContext context, TDatabaseTransaction databaseTransaction);
        public abstract void CreateOrder(Order order, StoreBookingFlowContext context, TDatabaseTransaction databaseTransaction);

        public Lease CreateLease(FlowStage flowStage, OrderQuote orderQuote, StoreBookingFlowContext context, dynamic dbTransaction)
        {
            return CreateLease(flowStage, orderQuote, context, (TDatabaseTransaction)dbTransaction);
        }

        public void CreateOrder(Order order, StoreBookingFlowContext context, dynamic dbTransaction)
        {
            CreateOrder(order, context, (TDatabaseTransaction)dbTransaction);
        }



        /// <summary>
        /// Stage is provided as it depending on the implementation (e.g. what level of leasing is applied)
        /// it might not be appropriate to create transactions for all stages.
        /// Null can be returned in the case that a transaction has not been created.
        /// </summary>
        /// <param name="stage"></param>
        /// <returns></returns>
        protected abstract TDatabaseTransaction BeginOrderTransaction(FlowStage stage);
        protected abstract void CompleteOrderTransaction(TDatabaseTransaction databaseTransaction);
        protected abstract void RollbackOrderTransaction(TDatabaseTransaction databaseTransaction);

        dynamic IOrderStore.BeginOrderTransaction(FlowStage stage)
        {
            return BeginOrderTransaction(stage);
        }

        void IOrderStore.CompleteOrderTransaction(dynamic databaseTransaction)
        {
            CompleteOrderTransaction((TDatabaseTransaction)databaseTransaction);
        }

        void IOrderStore.RollbackOrderTransaction(dynamic databaseTransaction)
        {
            RollbackOrderTransaction((TDatabaseTransaction)databaseTransaction);
        }

        public abstract bool CustomerCancelOrderItems(OrderIdTemplate orderIdTemplate, OrderIdComponents orderId, List<OrderIdComponents> orderItemIds);
        public abstract void DeleteOrder(OrderIdComponents orderId);
        public abstract void DeleteLease(OrderIdComponents orderId);

        /*
        public OrderItem CreateOrder<TOrder>(IBookableIdComponents opportunityOfferId, StoreBookingFlowContext<TOrder> context)
        {
            if (!(opportunityOfferId.GetType() == typeof(TComponents)))
            {
                throw new NotSupportedException($"{opportunityOfferId.GetType().ToString()} does not match {typeof(BookablePairIdTemplate<TComponents>).ToString()}. All types of IBookableIdComponents (T) used for BookablePairIdTemplate<T> assigned to feeds via settings.IdConfiguration must match those used by the stores in storeSettings.OpenBookingStoreRouting.");
            }

            // TODO: Include validation on the OrderItem created, to ensure it includes all the required fields
            return GetOrderItem<TOrder>((TComponents)opportunityOfferId, context);
        }

        protected abstract OrderItem CreateOrder<TOrder>(OrderIdComponents opportunityOfferId, StoreBookingFlowContext<TOrder> context);


        public OrderItem GetOrderItem<TOrder>(IBookableIdComponents opportunityOfferId, StoreBookingFlowContext<TOrder> context)
        {
            if (!(opportunityOfferId.GetType() == typeof(TComponents)))
            {
                throw new NotSupportedException($"{opportunityOfferId.GetType().ToString()} does not match {typeof(BookablePairIdTemplate<TComponents>).ToString()}. All types of IBookableIdComponents (T) used for BookablePairIdTemplate<T> assigned to feeds via settings.IdConfiguration must match those used by the stores in storeSettings.OpenBookingStoreRouting.");
            }

            // TODO: Include validation on the OrderItem created, to ensure it includes all the required fields
            return GetOrderItem<TOrder>((TComponents)opportunityOfferId, context);
        }

        protected abstract OrderItem GetOrder<TOrder>(OrderIdComponents opportunityOfferId, StoreBookingFlowContext<TOrder> context);

     */
    }


}
