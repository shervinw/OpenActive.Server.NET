using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using OpenActive.DatasetSite.NET;
using OpenActive.NET;
using OpenActive.NET.Rpde.Version1;

namespace OpenActive.Server.NET
{
    public enum FlowStage { C1, C2, B }

    /// <summary>
    /// The AbstractBookingEngine provides a simple, basic and extremely flexible implementation of Open Booking API.
    /// 
    /// It is designed for systems where their needs are not met by StoreBookingEngine to provide a solid foundation for thier implementations.
    /// 
    /// Methods of this class will return OpenActive POCO models that can be rendered using ToOpenActiveString(),
    /// and throw exceptions that subclass OpenActiveException, on which GetHttpStatusCode() and ToOpenActiveString() can
    /// be called to construct a response.
    /// </summary>
    public abstract class AbstractBookingEngine : IBookingEngine
    {
        /// <summary>
        /// In this mode, the Booking Engine also handles generation of open data feeds and the dataset site
        /// 
        /// Note this is also the mode used by the StoreBookingEngine
        /// 
        /// In order to use RenderDatasetSite, DatasetSiteGeneratorSettings must be provided
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="datasetSettings"></param>
        public AbstractBookingEngine(BookingEngineSettings settings, DatasetSiteGeneratorSettings datasetSettings) : this(settings, datasetSettings?.OpenBookingAPIBaseUrl, datasetSettings?.OpenDataFeedBaseUrl)
        {
            if (datasetSettings == null) throw new ArgumentNullException(nameof(datasetSettings));
            this.datasetSettings = datasetSettings;
        }

        /// <summary>
        /// In this mode, the Booking Engine additionally handles generation of open data feeds, but the dataset site is handled manually
        /// 
        /// In order to generate open data RPDE pages, OpenDataFeedBaseUrl must be provided
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="openBookingAPIBaseUrl"></param>
        /// <param name="openDataFeedBaseUrl"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "Exception relates to specific settings property being null")]
        public AbstractBookingEngine(BookingEngineSettings settings, Uri openBookingAPIBaseUrl, Uri openDataFeedBaseUrl) : this (settings, openBookingAPIBaseUrl)
        {
            if (openDataFeedBaseUrl == null) throw new ArgumentNullException(nameof(openDataFeedBaseUrl));
            if (settings.OpenDataFeeds == null) throw new ArgumentNullException("settings.OpenDataFeeds");

            this.openDataFeedBaseUrl = openDataFeedBaseUrl;

            // Create a lookup of each IdTemplate to pass into the appropriate RpdeGenerator
            // TODO: Output better error if there is a feed assigned across two templates
            // (there should never be, as each template represents everyting you need in one feed)
            this.feedAssignedTemplates = settings.IdConfiguration.Select(t => t.IdConfigurations.Select(x => new
            {
                assignedFeed = x.AssignedFeed,
                bookablePairIdTemplate = t
            })).SelectMany(x => x.ToList()).ToDictionary(k => k.assignedFeed, v => v.bookablePairIdTemplate);

            // Setup each RPDEFeedGenerator with the relevant settings, including the relevant IdTemplate inferred from the config
            foreach (var kv in settings.OpenDataFeeds)
            {
                kv.Value.SetConfiguration(OpportunityTypes.Configurations[kv.Key], settings, this.feedAssignedTemplates[kv.Key], openDataFeedBaseUrl);
            }

            // Create a dictionary of RPDEFeedGenerator indexed by FeedPath
            this.feedLookup = settings.OpenDataFeeds.Values.ToDictionary(x => x.FeedPath);

            // Set supportedFeeds locally for use by dataset site
            this.supportedFeeds = settings.OpenDataFeeds.Keys.ToList();

            // Check that OpenDataFeeds match IdConfiguration
            if (supportedFeeds.Except(feedAssignedTemplates.Keys).Any() || feedAssignedTemplates.Keys.Except(supportedFeeds).Any())
            {
                throw new ArgumentException("Feeds configured in OpenDataFeeds must match those in IdConfiguration");
            }

            // Setup array of types for lookup of OrderItem, based on the type string that will be supplied with the opportunity
            this.idConfigurationLookup = settings.IdConfiguration.Select(t => t.IdConfigurations.Select(x => new
            {
                // TODO: Create an extra prop in DatasetSite lib so that we don't need to parse the URL here
                opportunityTypeName = OpportunityTypes.Configurations[x.OpportunityType].SameAs.AbsolutePath.Trim('/'),
                bookablePairIdTemplate = t
            })).SelectMany(x => x.ToList())
            .GroupBy(g => g.opportunityTypeName)
            .ToDictionary(k => k.Key, v => v.Select(y => y.bookablePairIdTemplate).ToList());

        }

        private DatasetSiteGeneratorSettings datasetSettings = null;
        private readonly BookingEngineSettings settings;
        private Dictionary<string, IRPDEFeedGenerator> feedLookup;
        private List<OpportunityType> supportedFeeds;
        private Uri openDataFeedBaseUrl;
        private Uri openBookingAPIBaseUrl;
        private Dictionary<string, List<IBookablePairIdTemplate>> idConfigurationLookup;
        private Dictionary<OpportunityType, IBookablePairIdTemplate> feedAssignedTemplates;

        /// <summary>
        /// In this mode, the Booking Engine does not handle open data feeds or dataset site rendering, and these must both be handled manually
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="openBookingAPIBaseUrl"></param>
        public AbstractBookingEngine(BookingEngineSettings settings, Uri openBookingAPIBaseUrl)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (openBookingAPIBaseUrl == null) throw new ArgumentNullException(nameof(openBookingAPIBaseUrl));

            this.settings = settings;
        }

        /// <summary>
        /// Handler for Dataset Site endpoint
        /// </summary>
        /// <returns></returns>
        public string RenderDatasetSite()
        {
            if (datasetSettings == null || supportedFeeds == null) throw new NotSupportedException("RenderDatasetSite is only supported if DatasetSiteGeneratorSettings are supplied to the IBookingEngine");
            // TODO add caching layer in front of dataset site rendering
            return DatasetSiteGenerator.RenderSimpleDatasetSite(datasetSettings, supportedFeeds);
        }

        /// <summary>
        /// Handler for an RPDE endpoint - string only version
        /// Designed to be used on a single controller method with a "feedname" parameter,
        /// for uses in situations where the framework does not automatically validate numeric values
        /// </summary>
        /// <param name="feedname">The final component of the path of the feed, i.e. https://example.com/feeds/{feedname} </param>
        /// <param name="afterTimestamp">The "afterTimestamp" parameter from the URL</param>
        /// <param name="afterId">The "afterId" parameter from the URL</param>
        /// <param name="afterChangeNumber">The "afterChangeNumber" parameter from the URL</param>
        /// <returns></returns>
        public RpdePage GetOpenDataRPDEPageForFeed(string feedname, string afterTimestamp, string afterId, string afterChangeNumber)
        {
            long? afterTimestampLong = null;
            long? afterChangeNumberLong = null;

            if (long.TryParse(afterTimestamp, out long timestampValue))
            {
                afterTimestampLong = timestampValue;
            }
            else if (!string.IsNullOrWhiteSpace(afterTimestamp))
            {
                throw new ArgumentOutOfRangeException(nameof(afterTimestamp), "afterTimestamp must be numeric");
            }

            if (long.TryParse(afterChangeNumber, out long changeNumberValue))
            {
                afterChangeNumberLong = changeNumberValue;
            }
            else if (!string.IsNullOrWhiteSpace(afterChangeNumber))
            {
                throw new ArgumentOutOfRangeException(nameof(afterChangeNumber), "afterChangeNumber must be numeric");
            }

            return GetOpenDataRPDEPageForFeed(feedname, afterTimestampLong, afterId, afterChangeNumberLong);
        }

        /// <summary>
        /// Handler for an RPDE endpoint
        /// Designed to be used on a single controller method with a "feedname" parameter
        /// </summary>
        /// <param name="feedname">The final component of the path of the feed, i.e. https://example.com/feeds/{feedname} </param>
        /// <param name="afterTimestamp">The "afterTimestamp" parameter from the URL</param>
        /// <param name="afterId">The "afterId" parameter from the URL</param>
        /// <param name="afterChangeNumber">The "afterChangeNumber" parameter from the URL</param>
        /// <returns></returns>
        public RpdePage GetOpenDataRPDEPageForFeed(string feedname, long? afterTimestamp, string afterId, long? afterChangeNumber)
        {
            if (openDataFeedBaseUrl == null) throw new NotSupportedException("GetOpenDataRPDEPageForFeed is only supported if an OpenDataFeedBaseUrl and BookingEngineSettings.OpenDataFeed is supplied to the IBookingEngine");

            if (feedLookup.TryGetValue(feedname, out IRPDEFeedGenerator generator))
            {
                switch (generator) {
                    case IRPDEFeedIncrementingUniqueChangeNumber changeNumberGenerator:
                        return changeNumberGenerator.GetRPDEPage(afterChangeNumber);

                    case IRPDEFeedModifiedTimestampAndIDLong timestampAndIDGeneratorLong:
                        if (long.TryParse(afterId, out long afterIdLong))
                        {
                            return timestampAndIDGeneratorLong.GetRPDEPage(afterTimestamp, afterIdLong);
                        }
                        else if (string.IsNullOrWhiteSpace(afterId))
                        {
                            return timestampAndIDGeneratorLong.GetRPDEPage(afterTimestamp, null);
                        }
                        else     
                        {
                            throw new ArgumentOutOfRangeException(nameof(afterId), "afterId must be numeric");
                        }

                    case IRPDEFeedModifiedTimestampAndIDString timestampAndIDGeneratorString:
                        return timestampAndIDGeneratorString.GetRPDEPage(afterTimestamp, afterId);

                    default:
                        throw new InvalidCastException($"RPDEFeedGenerator for '{feedname}' not recognised - check the generic template for RPDEFeedModifiedTimestampAndID uses either <string> or <long?>");
                }
            } else
            {
                throw new KeyNotFoundException($"OpportunityTypeConfiguration for '{feedname}' not found.");
            }
        }

        // Note this is not a helper as it relies on engine settings state
        private IBookableIdComponents ResolveOpportunityID(string opportunityTypeString, Uri opportunityId, Uri offerId)
        {
            // Return the first matching ID combination for the opportunityId and offerId provided.
            // TODO: Make this more efficient?
            return this.idConfigurationLookup[opportunityTypeString]
                .Select(x => x.GetOpportunityReference(opportunityId, offerId))
                .Where(x => x != null)
                .FirstOrDefault();
        }

        public OrderQuote ProcessCheckpoint1(string uuid, OrderQuote orderQuote)
        {
            return ProcessFlowRequest<OrderQuote>(FlowStage.C1, uuid, orderQuote);
        }
        public OrderQuote ProcessCheckpoint2(string uuid, OrderQuote orderQuote)
        {
            return ProcessFlowRequest<OrderQuote>(FlowStage.C2, uuid, orderQuote);
        }
        public Order ProcessOrderCreationB(string uuid, Order order)
        {
            return ProcessFlowRequest<Order>(FlowStage.B, uuid, order);
        }
        public void DeleteOrder(string uuid)
        {
            throw new NotImplementedException();
        }

        public void ProcessOrderUpdate(string uuid, OpenActive.NET.Order order)
        {
            throw new NotImplementedException();
        }

        private O ProcessFlowRequest<O>(FlowStage stage, string uuid, O orderQuote) where O : Order
        {
            return orderQuote;
        }

        public void CreateTestData(Event @event)
        {
            this.CreateTestDataItem(@event);
        }

        public abstract void CreateTestDataItem(Event @event);

        public void DeleteTestData(Uri id)
        {
            this.DeleteTestDataItem(id);
        }

        public abstract void DeleteTestDataItem(Uri id);



        /*
       private O ProcessFlowRequest<O>(FlowStage stage, string uuid, O orderQuote) where O : Order
       {
           var orderId = new OrderId
           {
               uuid = uuid,
               BaseUrl = settings.OrderBaseUrl
           };




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
            
    }
    */
    }
}
