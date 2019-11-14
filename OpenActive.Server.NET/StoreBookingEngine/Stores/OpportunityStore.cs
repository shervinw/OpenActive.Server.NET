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

        OrderItem GetOrderItem(IBookableIdComponents opportunityOfferId, StoreBookingFlowContext context);

        void CreateTestDataItem(OpportunityType opportunityType, Event @event);
        void DeleteTestDataItem(OpportunityType opportunityType, string name);
    }


    //TODO: Remove duplication between this and RpdeBase if possible as they are using the same pattern?
    public abstract class OpportunityStore<TComponents> : ModelSupport<TComponents>, IOpportunityStore where TComponents : class, IBookableIdComponents, new()
    {
        public void SetConfiguration(IBookablePairIdTemplate template, SingleIdTemplate<SellerIdComponents> sellerTemplate)
        {
            if (template as BookablePairIdTemplate<TComponents> == null)
            {
                throw new NotSupportedException($"{template.GetType().ToString()} does not match {typeof(BookablePairIdTemplate<TComponents>).ToString()}. All types of IBookableIdComponents (T) used for BookablePairIdTemplate<T> assigned to feeds via settings.IdConfiguration must match those used for RPDEFeedGenerator<T> in settings.OpenDataFeeds.");
            }

            base.SetConfiguration((BookablePairIdTemplate<TComponents>)template, sellerTemplate);
        }


        public OrderItem GetOrderItem(IBookableIdComponents opportunityOfferId, StoreBookingFlowContext context)
        {
            if (!(opportunityOfferId.GetType() == typeof(TComponents)))
            {
                throw new NotSupportedException($"{opportunityOfferId.GetType().ToString()} does not match {typeof(BookablePairIdTemplate<TComponents>).ToString()}. All types of IBookableIdComponents (T) used for BookablePairIdTemplate<T> assigned to feeds via settings.IdConfiguration must match those used by the stores in storeSettings.OpenBookingStoreRouting.");
            }

            // TODO: Include validation on the OrderItem created, to ensure it includes all the required fields
            return GetOrderItem((TComponents)opportunityOfferId, context);
        }

        protected abstract OrderItem GetOrderItem(TComponents opportunityOfferId, StoreBookingFlowContext context);
        public abstract void CreateTestDataItem(OpportunityType opportunityType, Event @event);
        public abstract void DeleteTestDataItem(OpportunityType opportunityType, string name);

    }
}
