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
        Lease CreateLease(OrderQuote orderQuote, StoreBookingFlowContext flowContext, IDatabaseTransaction dbTransaction);
        void CreateOrder(Order order, StoreBookingFlowContext flowContext, IDatabaseTransaction dbTransaction);
        bool CustomerCancelOrderItems(OrderIdComponents orderId, SellerIdComponents sellerId, OrderIdTemplate orderIdTemplate, List<OrderIdComponents> orderItemIds);
        void DeleteOrder(OrderIdComponents orderId, SellerIdComponents sellerId);
        void DeleteLease(OrderIdComponents orderId, SellerIdComponents sellerId);
    }

    public abstract class OrderStore<TDatabaseTransaction> : IOrderStore where TDatabaseTransaction : IDatabaseTransaction
    {
        public abstract Lease CreateLease(OrderQuote orderQuote, StoreBookingFlowContext flowContext, TDatabaseTransaction databaseTransaction);
        public abstract void CreateOrder(Order order, StoreBookingFlowContext flowContext, TDatabaseTransaction databaseTransaction);

        public Lease CreateLease(OrderQuote orderQuote, StoreBookingFlowContext flowContext, IDatabaseTransaction dbTransaction)
        {
            return CreateLease(orderQuote, flowContext, (TDatabaseTransaction)dbTransaction);
        }

        public void CreateOrder(Order order, StoreBookingFlowContext flowContext, IDatabaseTransaction dbTransaction)
        {
            CreateOrder(order, flowContext, (TDatabaseTransaction)dbTransaction);
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
