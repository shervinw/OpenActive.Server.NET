using System;
using System.Collections.Generic;
using System.Text;
using OpenActive.NET;

namespace OpenActive.Server.NET
{
    public enum FlowStage { C1, C2, B }

    /// <summary>
    /// The AbstractBookingEngine provides a simple, basic and extremely flexible implementation of Open Booking API.
    /// 
    /// It is designed for systems where their needs are not met by StoreBookingEngine to provide a solid foundation for thier implementations.
    /// </summary>
    public class AbstractBookingEngine
    {
        public AbstractBookingEngine(BookingEngineSettings settings)
        {
            this.settings = settings;
        }
        private readonly BookingEngineSettings settings;

        // Note this is not a helper as it relies on engine settings state
        public IBookableIdComponents ResolveOpportunityID(BookableOpportunityClass opportunityClass, Uri opportunityId, Uri offerId)
        {
            return settings.IdConfiguration[opportunityClass].GetOpportunityReference(opportunityId, offerId);
        }

        private OrderQuote ProcessCheckpoint(OrderQuote orderQuote, string orderQuoteUUID)
        {

        }

        private Order processFlowRequest(FlowStage stage, OrderQuote orderQuote, string orderQuoteUUID)
        {
            var checkpointStage


            /*
             * 
function processCheckpoint (orderQuote, orderQuoteId, authKey) {

  var checkpointStage = orderQuote.customer ? "C2" : "C1";

  // Get authkey being used the access the Open Booking API 
  var authKey = getAuthKey();

  // Get data for checkpoint (optional optimisation to load the data in one place)
  var data = fetchCheckpointData(orderId,  authKey, orderQuote);

  // Check that taxMode is set in Seller
  if (!seller.id)
  {
    return renderError("SellerNotSpecified");
  }

  // Get seller info
  var seller = getSeller(authKey, orderQuote.seller.id, data);

  // Check that taxMode is set in Seller
  if (!(seller.taxMode == "https://openactive/TaxGross" || seller.taxMode == "https://openactive/TaxNet"))
  {
    throw "taxMode must always be set in the Seller";
  }

  // Get the booking service info (usually static for the booking system)
  var bookingService = getBookingService(data);

  // Reflect back only those customer fields that are supported
  var customer = getCustomerSupportedFields(orderQuote.customer)

  // Reflect back only those broker fields that are supported
  var broker = getBrokerSupportedFields(orderQuote.broker)

  var taxPayeeRelationship = orderQuote.brokerRole == "https://openactive.io/ResellerBroker" 
    || customer.type == "Organisation" ? "B2B" : "B2C";
  
  var payer = orderQuote.brokerRole == "https://openactive.io/ResellerBroker" ? orderQuote.broker : orderQuote.customer;

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
             */
        }
    }
}
