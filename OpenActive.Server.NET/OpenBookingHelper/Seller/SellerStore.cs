using OpenActive.NET;
using OpenActive.Server.NET.OpenBookingHelper;
using System;
using System.Collections.Generic;
using System.Text;


namespace OpenActive.Server.NET.OpenBookingHelper
{
    public abstract class SellerStore
    {
        private SingleIdTemplate<SellerIdComponents> IdTemplate { get; set; }

        internal void SetConfiguration(SingleIdTemplate<SellerIdComponents> template)
        {
            this.IdTemplate = template;
        }

        protected Uri RenderSellerId(SellerIdComponents sellerIdComponents)
        {
            return this.IdTemplate.RenderId(sellerIdComponents);
        }

        internal ILegalEntity GetSellerById(SellerIdComponents sellerIdComponents)
        {
            // TODO: Include validation on the OrderItem created, to ensure it includes all the required fields
            return GetSeller(sellerIdComponents);
        }

        protected abstract ILegalEntity GetSeller(SellerIdComponents sellerId);

    }
}
