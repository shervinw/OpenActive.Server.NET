using OpenActive.DatasetSite.NET;
using OpenActive.NET;
using OpenActive.Server.NET.OpenBookingHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET.StoreBooking
{
    public interface IOpportunityStore
    {
        void SetConfiguration(IBookablePairIdTemplate template, SingleIdTemplate<SellerIdComponents> sellerTemplate);
        /*
        OrderItem GetOrderItem(IBookableIdComponents opportunityOfferId, ISingleIdTemplate sellerId);
        */

        List<OrderItem> GetOrderItems(List<IBookableIdComponents> opportunityOfferId, StoreBookingFlowContext context);
        List<List<OpenBookingError>> LeaseOrderItems(List<IBookableIdComponents> opportunityOfferId, StoreBookingFlowContext context, dynamic databaseTransactionContext);
        List<OrderIdComponents> BookOrderItems(List<IBookableIdComponents> opportunityOfferId, List<OrderItem> orderItems, StoreBookingFlowContext context, dynamic databaseTransactionContext);

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


        public List<OrderItem> GetOrderItems(List<IBookableIdComponents> opportunityOfferId, StoreBookingFlowContext context)
        {
            CheckOpportunityTypeMatch(opportunityOfferId);

            // TODO: Include validation on the OrderItem created, to ensure it includes all the required fields
            return GetOrderItem(opportunityOfferId.ConvertAll<TComponents>(x => (TComponents)x), context);
        }

        public List<List<OpenBookingError>> LeaseOrderItems(List<IBookableIdComponents> opportunityOfferId, StoreBookingFlowContext context, dynamic databaseTransactionContext)
        {
            CheckOpportunityTypeMatch(opportunityOfferId);

            // TODO: Include validation on the OrderItem created, to ensure it includes all the required fields
            return LeaseOrderItem(opportunityOfferId.ConvertAll<TComponents>(x => (TComponents)x), context, (TDatabaseTransaction)databaseTransactionContext);
        }

        public List<OrderIdComponents> BookOrderItems(List<IBookableIdComponents> opportunityOfferId, List<OrderItem> orderItems, StoreBookingFlowContext context, dynamic databaseTransactionContext)
        {
            CheckOpportunityTypeMatch(opportunityOfferId);

            // TODO: Include validation on the OrderItem created, to ensure it includes all the required fields
            return BookOrderItem(opportunityOfferId.ConvertAll<TComponents>(x => (TComponents)x), orderItems, context, (TDatabaseTransaction)databaseTransactionContext);
        }

        protected abstract List<OrderItem> GetOrderItem(List<TComponents> opportunityOfferId, StoreBookingFlowContext context);

        /// <summary>
        /// BookOrderItem will always succeed or throw an error on failure.
        /// Note that orderItems provided by GetOrderItems are supplied for cases where Sales Invoices or other audit records
        /// need to be written that require prices. As GetOrderItems occurs outside of the transaction.
        /// 
        /// </summary>
        protected abstract List<OrderIdComponents> BookOrderItem(List<TComponents> opportunityOfferId, List<OrderItem> orderItems, StoreBookingFlowContext context, TDatabaseTransaction databaseTransactionContext);
        /// <summary>
        /// Return null if leasing of the item failed
        /// </summary>
        /// <param name="opportunityOfferId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected abstract List<List<OpenBookingError>> LeaseOrderItem(List<TComponents> opportunityOfferId, StoreBookingFlowContext context, TDatabaseTransaction databaseTransactionContext);

        public abstract void CreateTestDataItem(OpportunityType opportunityType, Event @event);
        public abstract void DeleteTestDataItem(OpportunityType opportunityType, string name);

        private void CheckOpportunityTypeMatch(List<IBookableIdComponents> opportunityOfferId)
        {
            if (opportunityOfferId == null) throw new ArgumentNullException(nameof(opportunityOfferId));

            if (!(opportunityOfferId.TrueForAll(x => x.GetType() == typeof(TComponents))))
            {
                throw new NotSupportedException($"{opportunityOfferId.GetType().ToString()} does not match {typeof(BookablePairIdTemplate<TComponents>).ToString()}. All types of IBookableIdComponents (T) used for BookablePairIdTemplate<T> assigned to feeds via settings.IdConfiguration must match those used by the stores in storeSettings.OpenBookingStoreRouting.");
            }
        }

    }
}
