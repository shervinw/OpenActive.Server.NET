using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenActive.DatasetSite.NET;
using OpenActive.NET;
using OpenActive.NET.Rpde.Version1;

namespace OpenActive.Server.NET
{
    /// <summary>
    /// The StoreBookingEngine provides a more opinionated implementation of the Open Booking API on top of AbstractBookingEngine.
    /// This is designed to be quick to implement, but may not fit the needs of more complex systems.
    /// 
    /// It is not designed to be subclassed (it could be sealed?), but instead the implementer is encouraged
    /// to implement and provide an IOpenBookingStore on instantiation. 
    /// </summary>
    public class StoreBookingEngine : AbstractBookingEngine
    {
        /// <summary>
        /// Simple contructor
        /// </summary>
        /// <param name="settings">Settings are used exclusively by the AbstractBookingEngine</param>
        /// <param name="store">Store used exclusively by the StoreBookingEngine</param>
        public StoreBookingEngine(BookingEngineSettings settings, DatasetSiteGeneratorSettings datasetSettings, Dictionary<IOpenBookingStore, List<OpportunityType>> openBookingStoreRouting) : base(settings, datasetSettings)
        {
            this.stores = openBookingStoreRouting.Keys.ToList();

            // TODO: Add test to ensure there are not two or more at FirstOrDefault step, in case of configuration error 
            storeRouting = openBookingStoreRouting.Select(t => t.Value.Select(y => new
            {
                store = t.Key,
                opportunityType = y
            })).SelectMany(x => x.ToList()).GroupBy(g => g.opportunityType).ToDictionary(k => k.Key, v => v.Select(a => a.store).FirstOrDefault());
        }

        private readonly List<IOpenBookingStore> stores;
        private readonly Dictionary<OpportunityType, IOpenBookingStore> storeRouting;

        public override void CreateTestDataItem(OpportunityType opportunityType, Event @event)
        {
            if (!storeRouting.ContainsKey(opportunityType))
                throw new OpenBookingException(new OpenBookingError(), "Specified opportunity type is not configured as bookable in the StoreBookingEngine constructor.");

            //TODO: This forces the cast into the Store. Perhaps best to move the cast here to simplify the store?
            storeRouting[opportunityType].CreateTestDataItem(opportunityType, @event);
        }

        public override void DeleteTestDataItem(OpportunityType opportunityType, string name)
        {
            if (!storeRouting.ContainsKey(opportunityType))
                throw new OpenBookingException(new OpenBookingError(), "Specified opportunity type is not configured as bookable in the StoreBookingEngine constructor.");

            storeRouting[opportunityType].DeleteTestDataItem(opportunityType, name);
        }

        public override TOrder ProcessFlowRequest<TOrder>(FlowStage stage, OrderIdComponents orderId, TOrder orderQuote,
            TaxPayeeRelationship taxPayeeRelationship, SingleValues<Organization, Person> payer)
        {
            throw new NotImplementedException();


            /*
            // Get seller info (note Id has already been validated in Abstract
            var seller = store.GetSeller(authKey, orderQuote.Seller.Id, data);


            // Get the booking service info (usually static for the booking system)
            var bookingService = getBookingService(data);

            // Reflect back only those customer fields that are supported
            var customer = getCustomerSupportedFields(orderQuote.customer)

            // Reflect back only those broker fields that are supported
            var broker = getBrokerSupportedFields(orderQuote.broker)

                    
            // The below goes into RpdeBase

        // Map all requested OrderedItems to their full details, and validate any details provided if at C2
        var orderedItems = orderQuote.orderedItem.Select(x => 
          // Validate input OrderItem
          {
            if (x.acceptedOffer && x.orderedItem) {
              var opportunityComponents = getComponentsFromId(x.orderedItem.id, getOpportunityUrlTemplate());
              var offerComponents = getComponentsFromId(x.acceptedOffer.id, getOfferUrlTemplate());

              var fullOrderItem = getOrderItem(x, opportunityComponents, offerComponents, seller, taxPayeeRelationship, data);

              var sellerId = fullOrderItem.organizer.id || fullOrderItem.superEvent.organizer.id || fullOrderItem.facilityUse.organizer.id || fullOrderItem.superEvent.superEvent.organizer.id;

              if (seller.id != sellerId) {
                x.error[] += renderError("OpportunitySellerMismatch", notBookableReason);    
                return x;
              }

              // Validate output, and throw on error
              validateOutputOrderItem(fullOrderItem);

              // Check for a 'bookable' Opportunity and Offer pair was returned
              var notBookableReason = validateBookable(fullOrderItem);
              if (notBookableReason) {
                x.error[] += renderError("OpportunityOfferPairNotBookable", notBookableReason);    
                return x;
              } else {
                // Note: only validating details if the output is valid (previous check)
                return validateDetailsCapture(checkpointStage, fullOrderItem);
              }
            } else {
              x.error[] += renderError("IncompleteOrderItemError");  
              return x;
            }
          } 
        );

        if (checkpointStage == "C1" || checkpointStage == "C2") {
          var draftOrderQuote = {
            "@context": "https://openactive.io/",
            "type": "OrderQuote",
            "identifier": orderQuoteId,
            "brokerRole": orderQuote.brokerRole,
            "broker": broker,
            "seller": seller,
            "customer": customer,
            "bookingService": bookingService
          };

          // Attempt to retrieve lease, or at a minimum check the items can be purchased together (are not conflicted)
          // Note leaseOrCheckBookable adds errors to the orderedItems supplied array
          draftOrderQuote.lease = leaseOrCheckBookable(mutable orderedItems, draftOrderQuote, checkpointStage, seller.taxMode, taxPayeeRelationship, payer, authKey, data );

          // TODO: leases need to be set at the beginning, as they'll influence remaining spaces and OpportunityIsFullError etc. 

          // Add orderItems and totals to draftOrderQuote
          draftOrderQuote.orderedItems = orderedItems;
          augmentOrderWithTotals(draftOrderQuote);
          return draftOrderQuote;
        }

        if (checkpointStage == "B") {
          var payment = getPaymentSupportedFields(orderQuote.payment)

          var draftOrder = {
            "@context": "https://openactive.io/",
            "type": "Order",
            "identifier": orderQuoteId,
            "brokerRole": orderQuote.brokerRole,
            "broker": broker,
            "seller": seller,
            "customer": customer,
            "bookingService": bookingService,
            "orderedItems": orderedItems,
            "payment": payment
          };
          augmentOrderWithTotals(draftOrder);

          // Check that draftOrder matches expected totalPaymentDue provided with input order
          if (draftOrder.totalPaymentDue.price != orderQuote.totalPaymentDue.price) {
            return renderError("TotalPaymentDueMismatchError");
          }

          // Add orderItem.id, orderItem.accessCode and orderItem.accessToken for successful booking
          // Note this needs to store enough data to contract an Orders feed entry
          processBooking(orderId, draftOrder, seller.taxMode, taxPayeeRelationship, payer, authKey, data);

          return draftOrder;
        }

   }
   */
        }
    }
}
