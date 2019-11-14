using OpenActive.NET;
using OpenActive.Server.NET.OpenBookingHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingSystem.AspNetCore
{
    public class AcmeSellerStore : SellerStore
    {
        // If the Seller is not found, simply return null to generate the correct Open Booking error
        protected override ILegalEntity GetSeller(SellerIdComponents sellerIdComponents)
        {
            // For single-organization sellers, this may be hardcoded.
            // Otherwise it may be looked up based on supplied sellerIdComponents which are extacted from the sellerId.
            return new Organization
            {
                Name = "Acme Fitness Ltd",
                TaxMode = TaxMode.TaxGross,
                Id = this.RenderSellerId(sellerIdComponents)
            };
        }
    }
}
