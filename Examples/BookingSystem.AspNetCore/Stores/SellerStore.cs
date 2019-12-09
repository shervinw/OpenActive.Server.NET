using OpenActive.FakeDatabase.NET;
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
            // Note both examples are shown below to demonstrate options available. Only one block of the if statement below is required.
            if (sellerIdComponents.SellerIdLong == null && sellerIdComponents.SellerIdString == null)
            {

                // For Single Seller booking systems, no ID will be available from sellerIdComponents, and this data should instead come from your configuration table
                return new Organization
                {
                    Id = this.RenderSingleSellerId(),
                    Name = "Test Seller",
                    TaxMode = TaxMode.TaxGross
                };

            }
            else
            {

                // Otherwise it may be looked up based on supplied sellerIdComponents which are extacted from the sellerId.
                var seller = FakeBookingSystem.Database.Sellers.SingleOrDefault(x => x.Id == sellerIdComponents.SellerIdLong);
                if (seller != null)
                {
                    return seller.IsIndividual ? (ILegalEntity)new Person
                    {
                        Id = this.RenderSellerId(new SellerIdComponents { SellerIdLong = seller.Id }),
                        Name = seller.Name,
                        TaxMode = TaxMode.TaxGross
                    } : (ILegalEntity)new Organization
                    {
                        Id = this.RenderSellerId(new SellerIdComponents { SellerIdLong = seller.Id }),
                        Name = seller.Name,
                        TaxMode = TaxMode.TaxGross
                    };
                }
                else
                {
                    return null;
                }

            }
        }
    }
}
