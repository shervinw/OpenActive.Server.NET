using System;
using System.Collections.Generic;
using OpenActive.DatasetSite.NET;
using OpenActive.NET;
using OpenActive.NET.Rpde.Version1;

// TODO: Consolidate this logic with RpdeBase.cs to remove duplication (using generics?)
namespace OpenActive.Server.NET.OpenBookingHelper
{
    public abstract class OrdersRPDEFeedGenerator : IRPDEFeedGenerator
    {
        public int RPDEPageSize { get; private set; }
        public Uri JsonLdIdBaseUrl { get; private set; }
        public Uri OrderBaseUrl { get; private set; }
        private OrderIdTemplate IdTemplate { get; set; }
        protected Uri FeedUrl { get; private set; }

        internal void SetConfiguration(Uri jsonLdIdBaseUrl, Uri orderBaseUrl, int rpdePageSize, OrderIdTemplate template,  Uri offersFeedUrl)
        {
            this.IdTemplate = template;

            this.JsonLdIdBaseUrl = jsonLdIdBaseUrl;

            this.OrderBaseUrl = orderBaseUrl;

            this.RPDEPageSize = rpdePageSize;

            // Allow these to be overridden by implementations if customisation is required
            this.FeedUrl = offersFeedUrl;
        }

        protected Uri RenderOrderId(OrderType orderType, string uuid)
        {
            return this.IdTemplate.RenderOrderId(new OrderIdComponents { BaseUrl = this.OrderBaseUrl, OrderType = orderType, uuid = uuid } );
        }

        //TODO reduce duplication of the strings / logic below
        protected Uri RenderOrderItemId(OrderType orderType, string uuid, string orderItemId)
        {
            if (orderType != OrderType.Order) throw new ArgumentOutOfRangeException(nameof(orderType), "The Open Booking API 1.0 specification only permits OrderItem Ids to exist within Orders, not OrderQuotes or OrderProposals.");
            return this.IdTemplate.RenderOrderItemId(new OrderIdComponents { BaseUrl = this.OrderBaseUrl, OrderType = orderType, uuid = uuid, OrderItemIdString = orderItemId });
        }
        protected Uri RenderOrderItemId(OrderType orderType, string uuid, long orderItemId)
        {
            if (orderType != OrderType.Order) throw new ArgumentOutOfRangeException(nameof(orderType), "The Open Booking API 1.0 specification only permits OrderItem Ids to exist within Orders, not OrderQuotes or OrderProposals.");
            return this.IdTemplate.RenderOrderItemId(new OrderIdComponents { BaseUrl = this.OrderBaseUrl, OrderType = orderType, uuid = uuid, OrderItemIdLong = orderItemId });
        }

        /// <summary>
        /// This class is not designed to be used outside of the library, one of its subclasses must be used instead
        /// </summary>
        internal OrdersRPDEFeedGenerator() { }
    }

    public abstract class OrdersRPDEFeedIncrementingUniqueChangeNumber : OrdersRPDEFeedGenerator, IRPDEFeedIncrementingUniqueChangeNumber
    {
        protected abstract List<RpdeItem> GetRPDEItems(long? afterChangeNumber);

        public RpdePage GetRPDEPage(long? afterChangeNumber)
        {
            return new RpdePage(this.FeedUrl, afterChangeNumber, GetRPDEItems(afterChangeNumber));
        }
    }

    public abstract class OrdersRPDEFeedModifiedTimestampAndID : OrdersRPDEFeedGenerator, IRPDEFeedModifiedTimestampAndIDString
    {
        protected abstract List<RpdeItem> GetRPDEItems(long? afterTimestamp, string afterId);

        public RpdePage GetRPDEPage(long? afterTimestamp, string afterId)
        {
            if ((!afterTimestamp.HasValue && !string.IsNullOrWhiteSpace(afterId)) ||
                (afterTimestamp.HasValue && string.IsNullOrWhiteSpace(afterId)))
            {
                throw new ArgumentNullException("afterTimestamp and afterId must both be supplied, or neither supplied");
            }
            else
            {
                return new RpdePage(this.FeedUrl, afterTimestamp, afterId, GetRPDEItems(afterTimestamp, afterId));
            }
        }
    }

}
