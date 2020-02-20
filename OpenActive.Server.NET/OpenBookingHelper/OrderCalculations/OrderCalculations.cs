﻿using OpenActive.NET;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace OpenActive.Server.NET.OpenBookingHelper
{
    public static class OrderCalculations
    {
        public static void ValidateAttendeeDetails(OrderItem requestOrderItem, OrderItem responseOrderItem)
        {
            // TODO: Add attendee errors
        }

        public static Event RenderOpportunityWithOnlyId(string jsonLdType, Uri id)
        {
            switch (jsonLdType)
            {
                case nameof(Event):
                    return new Event { Id = id };
                case nameof(ScheduledSession):
                    return new ScheduledSession { Id = id };
                case nameof(HeadlineEvent):
                    return new HeadlineEvent { Id = id };
                case nameof(Slot):
                    return new Slot { Id = id };
                case nameof(CourseInstance):
                    return new CourseInstance { Id = id };
                default:
                    return null;
            }
        }

        public static TaxChargeSpecification AddTaxes(TaxChargeSpecification x, TaxChargeSpecification y)
        {
            // If one is null, return the other. If both are null, return null.
            if (x == null || y == null) return x ?? y;

            // Check that taxes are compatible
            if (x.Name != y.Name)
            {
                throw new ArgumentException("Different types of taxes cannot be added together");
            }
            if (x.Rate != y.Rate)
            {
                throw new ArgumentException("Taxes with the same name must have the same rate");
            }
            if (x.Identifier != y.Identifier)
            {
                throw new ArgumentException("Taxes with the same name must have the same identifier");
            }
            if (x.PriceCurrency != y.PriceCurrency)
            {
                throw new ArgumentException("Taxes with the same name must have the same currency");
            }

            // If compatible, return the sum
            return new TaxChargeSpecification {
                Name = x.Name,
                Price = x.Price + y.Price,
                PriceCurrency = x.PriceCurrency,
                Rate = x.Rate,
                Identifier = x.Identifier
            };
        }

        public static void AugmentOrderWithTotals<TOrder>(TOrder order) where TOrder : Order
        {
            if (order == null) throw new ArgumentNullException(nameof(order));

            // Calculate total payment due
            decimal totalPaymentDuePrice = 0;
            string totalPaymentDueCurrency = null;
            var totalPaymentTaxMap = new Dictionary<string, TaxChargeSpecification>();

            foreach (OrderItem orderedItem in order.OrderedItem) {
                // Only items with no errors associated are included in the total price
                if (!(orderedItem.Error?.Count > 0))
                {
                    // Keep track of total price
                    totalPaymentDuePrice += orderedItem.AcceptedOffer.Price ?? 0;

                    // Set currency based on first item
                    if (totalPaymentDueCurrency == null)
                    {
                        totalPaymentDueCurrency = orderedItem.AcceptedOffer.PriceCurrency;
                    }
                    else if (totalPaymentDueCurrency != orderedItem.AcceptedOffer.PriceCurrency)
                    {
                        throw new EngineConfigurationException("All currencies in an Order must match");
                    }

                    // Add the taxes to the map
                    if (orderedItem.UnitTaxSpecification != null)
                    {
                        foreach (TaxChargeSpecification taxChargeSpecification in orderedItem.UnitTaxSpecification)
                        {
                            if (totalPaymentTaxMap.TryGetValue(taxChargeSpecification.Name, out TaxChargeSpecification currentTaxValue))
                            {
                                totalPaymentTaxMap[taxChargeSpecification.Name] = AddTaxes(currentTaxValue, taxChargeSpecification);
                            }
                            else
                            {
                                totalPaymentTaxMap[taxChargeSpecification.Name] = taxChargeSpecification;
                            }
                        }
                    }
                }
            }

            order.TotalPaymentTax = totalPaymentTaxMap.Values.ToListOrNullIfEmpty();

            // If we're in Net taxMode, tax must be added to get the total price
            if (order.Seller.TaxMode == TaxMode.TaxNet)
            {
                totalPaymentDuePrice += order.TotalPaymentTax.Sum(x => x.Price ?? 0);
            }

            order.TotalPaymentDue = new PriceSpecification
            {
                Price = totalPaymentDuePrice,
                PriceCurrency = totalPaymentDueCurrency
            };
        }
    }
}
