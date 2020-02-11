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
         * TODO: Implement GetOrderItem
        OrderItem GetOrderItem(IBookableIdComponents opportunityOfferId, ISingleIdTemplate sellerId);
        */

        void GetOrderItems(List<IOrderItemContext> orderItemContexts, StoreBookingFlowContext flowContext, IStateContext stateContext);
        void LeaseOrderItems(Lease lease, List<IOrderItemContext> orderItemContexts, StoreBookingFlowContext flowContext, IStateContext stateContext, IDatabaseTransaction databaseTransactionContext);
        void BookOrderItems(List<IOrderItemContext> orderItemContexts, StoreBookingFlowContext flowContext, IStateContext stateContext, IDatabaseTransaction databaseTransactionContext);

        Event CreateTestDataItemEvent(OpportunityType opportunityType, Event @event);
        void DeleteTestDataItemEvent(OpportunityType opportunityType, Uri id);
    }


    //TODO: Remove duplication between this and RpdeBase if possible as they are using the same pattern?
    public abstract class OpportunityStore<TComponents, TDatabaseTransaction, TStateContext> : ModelSupport<TComponents>, IOpportunityStore where TComponents : class, IBookableIdComponents, new() where TDatabaseTransaction : IDatabaseTransaction where TStateContext : IStateContext
    {
        public void SetConfiguration(IBookablePairIdTemplate template, SingleIdTemplate<SellerIdComponents> sellerTemplate)
        {
            if (template as BookablePairIdTemplate<TComponents> == null)
            {
                throw new NotSupportedException($"{template.GetType().ToString()} does not match {typeof(BookablePairIdTemplate<TComponents>).ToString()}. All types of IBookableIdComponents (T) used for BookablePairIdTemplate<T> assigned to feeds via settings.IdConfiguration must match those used for RPDEFeedGenerator<T> in settings.OpenDataFeeds.");
            }

            base.SetConfiguration((BookablePairIdTemplate<TComponents>)template, sellerTemplate);
        }


        public void GetOrderItems(List<IOrderItemContext> orderItemContexts, StoreBookingFlowContext flowContext, IStateContext stateContext)
        {
            // TODO: Include validation on the OrderItem created, to ensure it includes all the required fields
            GetOrderItem(ConvertToSpecificComponents(orderItemContexts), flowContext, stateContext);
        }

        public void LeaseOrderItems(Lease lease, List<IOrderItemContext> orderItemContexts, StoreBookingFlowContext flowContext, IStateContext stateContext, IDatabaseTransaction databaseTransactionContext)
        {
            // TODO: Include validation on the OrderItem created, to ensure it includes all the required fields
            LeaseOrderItem(lease, ConvertToSpecificComponents(orderItemContexts), flowContext, stateContext, (TDatabaseTransaction)databaseTransactionContext);
        }

        public void BookOrderItems(List<IOrderItemContext> orderItemContexts, StoreBookingFlowContext flowContext, IStateContext stateContext, IDatabaseTransaction databaseTransactionContext)
        {
            // TODO: Include validation on the OrderItem created, to ensure it includes all the required fields
            BookOrderItem(ConvertToSpecificComponents(orderItemContexts), flowContext, stateContext, (TDatabaseTransaction)databaseTransactionContext);
        }

        protected abstract void GetOrderItem(List<OrderItemContext<TComponents>> orderItemContexts, StoreBookingFlowContext flowContext, IStateContext stateContext);

        /// <summary>
        /// BookOrderItem will always succeed or throw an error on failure.
        /// Note that responseOrderItems provided by GetOrderItems are supplied for cases where Sales Invoices or other audit records
        /// need to be written that require prices. As GetOrderItems occurs outside of the transaction.
        /// 
        /// </summary>
        protected abstract void BookOrderItem(List<OrderItemContext<TComponents>> orderItemContexts, StoreBookingFlowContext flowContext, IStateContext stateContext, TDatabaseTransaction databaseTransactionContext);

        protected abstract void LeaseOrderItem(Lease lease, List<OrderItemContext<TComponents>> orderItemContexts, StoreBookingFlowContext flowContext, IStateContext stateContext, TDatabaseTransaction databaseTransactionContext);


        protected abstract TComponents CreateTestDataItem(OpportunityType opportunityType, Event @event);
        protected abstract void DeleteTestDataItem(OpportunityType opportunityType, TComponents components);


        public Event CreateTestDataItemEvent(OpportunityType opportunityType, Event @event)
        {
            var components = CreateTestDataItem(opportunityType, @event);
            return OrderCalculations.RenderOpportunityWithOnlyId(RenderOpportunityJsonLdType(components), RenderOpportunityId(components));
        }
        public void DeleteTestDataItemEvent(OpportunityType opportunityType, Uri id)
        {
            var components = GetBookableOpportunityReference(opportunityType, id);
            DeleteTestDataItem(opportunityType, components);
        }


        private List<OrderItemContext<TComponents>> ConvertToSpecificComponents(List<IOrderItemContext> orderItemContexts)
        {
            if (orderItemContexts == null) throw new ArgumentNullException(nameof(orderItemContexts));

            if (!(orderItemContexts.Select(x => x.RequestBookableOpportunityOfferId).ToList().TrueForAll(x => x.GetType() == typeof(TComponents))))
            {
                throw new NotSupportedException($"OpportunityIdComponents does not match {typeof(BookablePairIdTemplate<TComponents>).ToString()}. All types of IBookableIdComponents (T) used for BookablePairIdTemplate<T> assigned to feeds via settings.IdConfiguration must match those used by the stores in storeSettings.OpenBookingStoreRouting.");
            }

            return orderItemContexts.ConvertAll<OrderItemContext<TComponents>>(x => (OrderItemContext<TComponents>)x);
        }

    }
}
