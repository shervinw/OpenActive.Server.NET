using OpenActive.DatasetSite.NET;
using OpenActive.NET;
using OpenActive.Server.NET.OpenBookingHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenActive.Server.NET.StoreBooking
{
    public interface IOpportunityStore
    {
        void SetConfiguration(IBookablePairIdTemplate template, SingleIdTemplate<SellerIdComponents> sellerTemplate);
        /*
        OrderItem GetOrderItem(IBookableIdComponents opportunityOfferId, ISingleIdTemplate sellerId);
        */

        void GetOrderItems(List<IOrderItemContext> orderItemContexts, StoreBookingFlowContext flowContext);
        void LeaseOrderItems(List<IOrderItemContext> orderItemContexts, StoreBookingFlowContext flowContext, dynamic databaseTransactionContext);
        void BookOrderItems(List<IOrderItemContext> orderItemContexts, StoreBookingFlowContext flowContext, dynamic databaseTransactionContext);

        void CreateTestDataItem(OpportunityType opportunityType, Event @event);
        void DeleteTestDataItem(OpportunityType opportunityType, string name);
    }


    //TODO: Remove duplication between this and RpdeBase if possible as they are using the same pattern?
    public abstract class OpportunityStore<TComponents, TDatabaseTransaction> : ModelSupport<TComponents>, IOpportunityStore where TComponents : class, IBookableIdComponents, new()
    {
        public void SetConfiguration(IBookablePairIdTemplate template, SingleIdTemplate<SellerIdComponents> sellerTemplate)
        {
            if (template as BookablePairIdTemplate<TComponents> == null)
            {
                throw new NotSupportedException($"{template.GetType().ToString()} does not match {typeof(BookablePairIdTemplate<TComponents>).ToString()}. All types of IBookableIdComponents (T) used for BookablePairIdTemplate<T> assigned to feeds via settings.IdConfiguration must match those used for RPDEFeedGenerator<T> in settings.OpenDataFeeds.");
            }

            base.SetConfiguration((BookablePairIdTemplate<TComponents>)template, sellerTemplate);
        }


        public void GetOrderItems(List<IOrderItemContext> orderItemContexts, StoreBookingFlowContext context)
        {
            // TODO: Include validation on the OrderItem created, to ensure it includes all the required fields
            GetOrderItem(ConvertToSpecificComponents(orderItemContexts), context);
        }

        public void LeaseOrderItems(List<IOrderItemContext> orderItemContexts, StoreBookingFlowContext context, dynamic databaseTransactionContext)
        {
            // TODO: Include validation on the OrderItem created, to ensure it includes all the required fields
            LeaseOrderItem(ConvertToSpecificComponents(orderItemContexts), context, (TDatabaseTransaction)databaseTransactionContext);
        }

        public void BookOrderItems(List<IOrderItemContext> orderItemContexts, StoreBookingFlowContext context, dynamic databaseTransactionContext)
        {
            // TODO: Include validation on the OrderItem created, to ensure it includes all the required fields
            BookOrderItem(ConvertToSpecificComponents(orderItemContexts), context, (TDatabaseTransaction)databaseTransactionContext);
        }

        protected abstract void GetOrderItem(List<OrderItemContext<TComponents>> orderItemContexts, StoreBookingFlowContext flowContext);

        /// <summary>
        /// BookOrderItem will always succeed or throw an error on failure.
        /// Note that responseOrderItems provided by GetOrderItems are supplied for cases where Sales Invoices or other audit records
        /// need to be written that require prices. As GetOrderItems occurs outside of the transaction.
        /// 
        /// </summary>
        protected abstract void BookOrderItem(List<OrderItemContext<TComponents>> orderItemContexts, StoreBookingFlowContext flowContext, TDatabaseTransaction databaseTransactionContext);

        protected abstract void LeaseOrderItem(List<OrderItemContext<TComponents>> orderItemContexts, StoreBookingFlowContext flowContext, TDatabaseTransaction databaseTransactionContext);

        public abstract void CreateTestDataItem(OpportunityType opportunityType, Event @event);
        public abstract void DeleteTestDataItem(OpportunityType opportunityType, string name);


        private List<OrderItemContext<TComponents>> ConvertToSpecificComponents(List<IOrderItemContext> orderItemContexts)
        {
            if (orderItemContexts == null) throw new ArgumentNullException(nameof(orderItemContexts));

            if (!(orderItemContexts.Select(x => x.RequestBookableOpportunityOfferId).ToList().TrueForAll(x => x.GetType() == typeof(TComponents))))
            {
                throw new NotSupportedException($"OpportunityIdComponents does not match {typeof(BookablePairIdTemplate<TComponents>).ToString()}. All types of IBookableIdComponents (T) used for BookablePairIdTemplate<T> assigned to feeds via settings.IdConfiguration must match those used by the stores in storeSettings.OpenBookingStoreRouting.");
            }

            return orderItemContexts.ConvertAll<OrderItemContext<TComponents>>(x => (OrderItemContext<TComponents>)x);
        }

        /*
        private List<OrderItemContext<TComponents>> ConvertToSpecificComponents(List<IOrderItemContext> orderItemContexts)
        {
            if (orderItemContexts == null) throw new ArgumentNullException(nameof(orderItemContexts));

            if (!(orderItemContexts.Select(x => x.OpportunityIdComponents).ToList().TrueForAll(x => x.GetType() == typeof(TComponents))))
            {
                throw new NotSupportedException($"OpportunityIdComponents does not match {typeof(BookablePairIdTemplate<TComponents>).ToString()}. All types of IBookableIdComponents (T) used for BookablePairIdTemplate<T> assigned to feeds via settings.IdConfiguration must match those used by the stores in storeSettings.OpenBookingStoreRouting.");
            }

            return orderItemContexts.ConvertAll<OrderItemContext<TComponents>>(x => new OrderItemContext<TComponents>
            {
                Index = x.Index,
                OpportunityIdComponents = (TComponents)x.OpportunityIdComponents,
                OrderIdComponents = x.OrderIdComponents,
                requestOrderItem = x.requestOrderItem,
                responseOrderItem = x.responseOrderItem
            });
        }

        private List<IOrderItemContext> ConvertToGenericComponents(List<OrderItemContext<TComponents>> orderItemContexts)
        {
            return orderItemContexts.ConvertAll<IOrderItemContext>(x => new IOrderItemContext
            {
                Index = x.Index,
                OpportunityIdComponents = (IBookableIdComponents)x.OpportunityIdComponents,
                OrderIdComponents = x.OrderIdComponents,
                requestOrderItem = x.requestOrderItem,
                responseOrderItem = x.responseOrderItem
            });
        }
        */

    }
}
