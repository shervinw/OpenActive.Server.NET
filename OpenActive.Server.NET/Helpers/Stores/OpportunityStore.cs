using OpenActive.DatasetSite.NET;
using OpenActive.NET;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET
{
    public interface IOpportunityStore
    {
        void SetConfiguration(BookingEngineSettings settings, IBookablePairIdTemplate template);
        /*
        OrderItem GetOrderItem(IBookableIdComponents opportunityOfferId, ISingleIdTemplate sellerId);
        */

        OrderItem GetOrderItem<TOrder>(IBookableIdComponents opportunityOfferId, StoreBookingFlowContext<TOrder> context);

        void CreateTestDataItem(OpportunityType opportunityType, Event @event);
        void DeleteTestDataItem(OpportunityType opportunityType, string name);
    }


    //TODO: Remove duplication between this and RpdeBase if possible as they are using the same pattern?
    public abstract class OpportunityStore<TComponents> : ModelSupport<TComponents>, IOpportunityStore where TComponents : class, IBookableIdComponents, new()
    {
        public void SetConfiguration(BookingEngineSettings settings, IBookablePairIdTemplate template)
        {
            if (template as BookablePairIdTemplate<TComponents> == null)
            {
                throw new NotSupportedException($"{template.GetType().ToString()} does not match {typeof(BookablePairIdTemplate<TComponents>).ToString()}. All types of IBookableIdComponents (T) used for BookablePairIdTemplate<T> assigned to feeds via settings.IdConfiguration must match those used for RPDEFeedGenerator<T> in settings.OpenDataFeeds.");
            }

            SetConfiguration(settings, (BookablePairIdTemplate<TComponents>)template);
        }


        public OrderItem GetOrderItem<TOrder>(IBookableIdComponents opportunityOfferId, StoreBookingFlowContext<TOrder> context)
        {
            if (!(opportunityOfferId.GetType() == typeof(TComponents)))
            {
                throw new NotSupportedException($"{opportunityOfferId.GetType().ToString()} does not match {typeof(BookablePairIdTemplate<TComponents>).ToString()}. All types of IBookableIdComponents (T) used for BookablePairIdTemplate<T> assigned to feeds via settings.IdConfiguration must match those used by the stores in storeSettings.OpenBookingStoreRouting.");
            }

            // TODO: Include validation on the OrderItem created, to ensure it includes all the required fields
            return GetOrderItem<TOrder>((TComponents)opportunityOfferId, context);
        }

        protected abstract OrderItem GetOrderItem<TOrder>(TComponents opportunityOfferId, StoreBookingFlowContext<TOrder> context);
        public abstract void CreateTestDataItem(OpportunityType opportunityType, Event @event);
        public abstract void DeleteTestDataItem(OpportunityType opportunityType, string name);

    }
}
