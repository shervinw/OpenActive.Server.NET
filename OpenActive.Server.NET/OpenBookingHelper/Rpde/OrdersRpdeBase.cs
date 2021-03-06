﻿using System;
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
        private OrderIdTemplate OrderIdTemplate { get; set; }
        private SingleIdTemplate<SellerIdComponents> SellerIdTemplate { get; set; }
        protected Uri FeedUrl { get; private set; }

        internal void SetConfiguration(int rpdePageSize, OrderIdTemplate orderIdTemplate, SingleIdTemplate<SellerIdComponents> sellerIdTemplate, Uri offersFeedUrl)
        {
            this.OrderIdTemplate = orderIdTemplate;

            this.SellerIdTemplate = sellerIdTemplate;

            this.RPDEPageSize = rpdePageSize;

            // Allow these to be overridden by implementations if customisation is required
            this.FeedUrl = offersFeedUrl;
        }

        protected Uri RenderOrderId(OrderType orderType, string uuid)
        {
            return this.OrderIdTemplate.RenderOrderId(orderType, uuid);
        }

        //TODO reduce duplication of the strings / logic below
        protected Uri RenderOrderItemId(OrderType orderType, string uuid, string orderItemId)
        {
            return this.OrderIdTemplate.RenderOrderItemId(orderType, uuid, orderItemId);
        }
        protected Uri RenderOrderItemId(OrderType orderType, string uuid, long orderItemId)
        {
            return this.OrderIdTemplate.RenderOrderItemId(orderType, uuid, orderItemId);
        }

        protected Uri RenderSellerId(SellerIdComponents sellerIdComponents)
        {
            return this.SellerIdTemplate.RenderId(sellerIdComponents);
        }

        protected Uri RenderSingleSellerId()
        {
            return this.SellerIdTemplate.RenderId(new SellerIdComponents());
        }

        protected static Event RenderOpportunityWithOnlyId(string jsonLdType, Uri id)
        {
            return OrderCalculations.RenderOpportunityWithOnlyId(jsonLdType, id);
        }

        /// <summary>
        /// This class is not designed to be used outside of the library, one of its subclasses must be used instead
        /// </summary>
        internal OrdersRPDEFeedGenerator() { }
    }

    public abstract class OrdersRPDEFeedIncrementingUniqueChangeNumber : OrdersRPDEFeedGenerator, IRPDEOrdersFeedIncrementingUniqueChangeNumber
    {
        protected abstract List<RpdeItem> GetRPDEItems(string clientId, long? afterChangeNumber);

        public RpdePage GetOrdersRPDEPage(string clientId, long? afterChangeNumber)
        {
            return new RpdePage(this.FeedUrl, afterChangeNumber, GetRPDEItems(clientId, afterChangeNumber));
        }
    }

    public abstract class OrdersRPDEFeedModifiedTimestampAndID : OrdersRPDEFeedGenerator, IRPDEOrdersFeedModifiedTimestampAndIDString
    {
        protected abstract List<RpdeItem> GetRPDEItems(string clientId, long? afterTimestamp, string afterId);

        public RpdePage GetOrdersRPDEPage(string clientId, long? afterTimestamp, string afterId)
        {
            if ((!afterTimestamp.HasValue && !string.IsNullOrWhiteSpace(afterId)) ||
                (afterTimestamp.HasValue && string.IsNullOrWhiteSpace(afterId)))
            {
                throw new ArgumentNullException("afterTimestamp and afterId must both be supplied, or neither supplied");
            }
            else
            {
                return new RpdePage(this.FeedUrl, afterTimestamp, afterId, GetRPDEItems(clientId, afterTimestamp, afterId));
            }
        }
    }

}
