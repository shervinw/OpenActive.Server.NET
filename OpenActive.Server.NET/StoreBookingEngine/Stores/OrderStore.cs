using OpenActive.NET;
using OpenActive.Server.NET.OpenBookingHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET.StoreBooking
{
    
    public abstract class OrderStore
    {
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
