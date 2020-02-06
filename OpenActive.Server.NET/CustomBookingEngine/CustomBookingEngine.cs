﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using OpenActive.DatasetSite.NET;
using OpenActive.NET;
using OpenActive.NET.Rpde.Version1;
using OpenActive.Server.NET.OpenBookingHelper;

namespace OpenActive.Server.NET.CustomBooking
{
    /// <summary>
    /// The AbstractBookingEngine provides a simple, basic and extremely flexible implementation of Open Booking API.
    /// 
    /// It is designed for systems where their needs are not met by StoreBookingEngine to provide a solid foundation for thier implementations.
    /// 
    /// Methods of this class will return OpenActive POCO models that can be rendered using ToOpenActiveString(),
    /// and throw exceptions that subclass OpenActiveException, on which GetHttpStatusCode() and ToOpenActiveString() can
    /// be called to construct a response.
    /// </summary>
    public abstract class CustomBookingEngine : IBookingEngine
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
        public CustomBookingEngine(BookingEngineSettings settings, DatasetSiteGeneratorSettings datasetSettings) : this(settings, datasetSettings?.OpenBookingAPIBaseUrl, datasetSettings?.OpenDataFeedBaseUrl)
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
        public CustomBookingEngine(BookingEngineSettings settings, Uri openBookingAPIBaseUrl, Uri openDataFeedBaseUrl) : this (settings, openBookingAPIBaseUrl)
        {
            // Check constructor configuration is correct
            if (openDataFeedBaseUrl == null) throw new ArgumentNullException(nameof(openDataFeedBaseUrl));
            if (settings.OpenDataFeeds == null) throw new ArgumentNullException("settings.OpenDataFeeds");


            // Check Seller configuration is provided
            if (settings.SellerStore == null || settings.SellerIdTemplate == null)
            {
                throw new EngineConfigurationException("SellerStore and SellerIdTemplate must be specified in BookingEngineSettings");
            }

            this.openDataFeedBaseUrl = openDataFeedBaseUrl;

            foreach (var idConfiguration in settings.IdConfiguration) {
                idConfiguration.RequiredBaseUrl = settings.JsonLdIdBaseUrl;
            }
            settings.OrderIdTemplate.RequiredBaseUrl = settings.OrderBaseUrl;
            settings.SellerIdTemplate.RequiredBaseUrl = settings.JsonLdIdBaseUrl;

            // Create a lookup of each IdTemplate to pass into the appropriate RpdeGenerator
            // TODO: Output better error if there is a feed assigned across two templates
            // (there should never be, as each template represents everyting you need in one feed)
            this.feedAssignedTemplates = settings.IdConfiguration.Select(t => t.IdConfigurations.Select(x => new
            {
                assignedFeed = x.AssignedFeed,
                bookablePairIdTemplate = t
            })).SelectMany(x => x.ToList()).ToDictionary(k => k.assignedFeed, v => v.bookablePairIdTemplate);

            // Create a lookup for the purposes of finding arbitary IdConfigurations, for use in the store
            // TODO: Pull this and the above into a function?
            this.OpportunityTemplateLookup = settings.IdConfiguration.Select(t => t.IdConfigurations.Select(x => new
            {
                opportunityType = x.OpportunityType,
                bookablePairIdTemplate = t
            })).SelectMany(x => x.ToList()).ToDictionary(k => k.opportunityType, v => v.bookablePairIdTemplate);

            // Setup each RPDEFeedGenerator with the relevant settings, including the relevant IdTemplate inferred from the config
            foreach (var kv in settings.OpenDataFeeds)
            {
                kv.Value.SetConfiguration(OpportunityTypes.Configurations[kv.Key], settings.JsonLdIdBaseUrl, settings.RPDEPageSize, this.feedAssignedTemplates[kv.Key], settings.SellerIdTemplate, openDataFeedBaseUrl);
            }

            settings.OrderFeedGenerator.SetConfiguration(settings.RPDEPageSize, settings.OrderIdTemplate, settings.SellerIdTemplate, settings.OrdersFeedUrl);

            settings.SellerStore.SetConfiguration(settings.SellerIdTemplate);

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
        private Dictionary<string, IOpportunityDataRPDEFeedGenerator> feedLookup;
        private List<OpportunityType> supportedFeeds;
        private Uri openDataFeedBaseUrl;
        private Uri openBookingAPIBaseUrl;
        private Dictionary<string, List<IBookablePairIdTemplate>> idConfigurationLookup;
        private Dictionary<OpportunityType, IBookablePairIdTemplate> feedAssignedTemplates;

        protected Dictionary<OpportunityType, IBookablePairIdTemplate> OpportunityTemplateLookup { get; }

        /// <summary>
        /// In this mode, the Booking Engine does not handle open data feeds or dataset site rendering, and these must both be handled manually
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="openBookingAPIBaseUrl"></param>
        public CustomBookingEngine(BookingEngineSettings settings, Uri openBookingAPIBaseUrl)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (openBookingAPIBaseUrl == null) throw new ArgumentNullException(nameof(openBookingAPIBaseUrl));

            this.settings = settings;
        }

        /// <summary>
        /// Handler for Dataset Site endpoint
        /// </summary>
        /// <returns></returns>
        public ResponseContent RenderDatasetSite()
        {
            if (datasetSettings == null || supportedFeeds == null) throw new NotSupportedException("RenderDatasetSite is only supported if DatasetSiteGeneratorSettings are supplied to the IBookingEngine");
            // TODO add caching layer in front of dataset site rendering
            return ResponseContent.HtmlResponse(DatasetSiteGenerator.RenderSimpleDatasetSite(datasetSettings, supportedFeeds));
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
        public ResponseContent GetOpenDataRPDEPageForFeed(string feedname, string afterTimestamp, string afterId, string afterChangeNumber)
        {
            return ResponseContent.RpdeResponse(
                RouteOpenDataRPDEPageForFeed(
                    feedname,
                    RpdeOrderingStrategyRouter.ConvertStringToLongOrThrow(afterTimestamp, nameof(afterTimestamp)),
                    afterId,
                    RpdeOrderingStrategyRouter.ConvertStringToLongOrThrow(afterChangeNumber, nameof(afterChangeNumber))
                    ).ToString());
        }


        /// <summary>
        /// Handler for an RPDE endpoint
        /// Designed to be used on a single controller method with a "feedname" parameter,
        /// for uses in situations where the framework does not automatically validate numeric values
        /// </summary>
        /// <param name="feedname">The final component of the path of the feed, i.e. https://example.com/feeds/{feedname} </param>
        /// <param name="afterTimestamp">The "afterTimestamp" parameter from the URL</param>
        /// <param name="afterId">The "afterId" parameter from the URL</param>
        /// <param name="afterChangeNumber">The "afterChangeNumber" parameter from the URL</param>
        /// <returns></returns>
        public ResponseContent GetOpenDataRPDEPageForFeed(string feedname, long? afterTimestamp, string afterId, long? afterChangeNumber)
        {
            return ResponseContent.RpdeResponse(RouteOpenDataRPDEPageForFeed(feedname, afterTimestamp, afterId, afterChangeNumber).ToString());
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
        private RpdePage RouteOpenDataRPDEPageForFeed(string feedname, long? afterTimestamp, string afterId, long? afterChangeNumber)
        {
            if (openDataFeedBaseUrl == null) throw new NotSupportedException("GetOpenDataRPDEPageForFeed is only supported if an OpenDataFeedBaseUrl and BookingEngineSettings.OpenDataFeed is supplied to the IBookingEngine");

            if (feedLookup.TryGetValue(feedname, out IOpportunityDataRPDEFeedGenerator generator))
            {
                return generator.GetRPDEPage(feedname, afterTimestamp, afterId, afterChangeNumber);
            } else
            {
                throw new OpenBookingException(new NotFoundError(), $"OpportunityTypeConfiguration for '{feedname}' not found.");
            }
        }

        /// <summary>
        /// Handler for an Orders RPDE endpoint (separate to the open data endpoint for security) - string only version
        /// Designed to be used on a single controller method with a "feedname" parameter,
        /// for uses in situations where the framework does not automatically validate numeric values
        /// </summary>
        /// <param name="authtoken">Token designating the specific authenticated party for which the feed is intended</param>
        /// <param name="afterTimestamp">The "afterTimestamp" parameter from the URL</param>
        /// <param name="afterId">The "afterId" parameter from the URL</param>
        /// <param name="afterChangeNumber">The "afterChangeNumber" parameter from the URL</param>
        /// <returns></returns>
        public ResponseContent GetOrdersRPDEPageForFeed(string clientId, string afterTimestamp, string afterId, string afterChangeNumber)
        {
            return ResponseContent.RpdeResponse(
                RenderOrdersRPDEPageForFeed(
                    clientId,
                    RpdeOrderingStrategyRouter.ConvertStringToLongOrThrow(afterTimestamp, nameof(afterTimestamp)),
                    afterId,
                    RpdeOrderingStrategyRouter.ConvertStringToLongOrThrow(afterChangeNumber, nameof(afterChangeNumber))
                    ).ToString());
        }

        /// <summary>
        /// Handler for an Orders RPDE endpoint (separate to the open data endpoint for security)
        /// For uses in situations where the framework does not automatically validate numeric values
        /// </summary>
        /// <param name="authtoken">Token designating the specific authenticated party for which the feed is intended</param>
        /// <param name="afterTimestamp">The "afterTimestamp" parameter from the URL</param>
        /// <param name="afterId">The "afterId" parameter from the URL</param>
        /// <param name="afterChangeNumber">The "afterChangeNumber" parameter from the URL</param>
        /// <returns></returns>
        public ResponseContent GetOrdersRPDEPageForFeed(string clientId, long? afterTimestamp, string afterId, long? afterChangeNumber)
        {
            return ResponseContent.RpdeResponse(RenderOrdersRPDEPageForFeed(clientId, afterTimestamp, afterId, afterChangeNumber).ToString());
        }

        /// <summary>
        /// Handler for Orders RPDE endpoint
        /// </summary>
        /// <param name="authtoken">Token designating the specific authenticated party for which the feed is intended</param>
        /// <param name="afterTimestamp">The "afterTimestamp" parameter from the URL</param>
        /// <param name="afterId">The "afterId" parameter from the URL</param>
        /// <param name="afterChangeNumber">The "afterChangeNumber" parameter from the URL</param>
        /// <returns></returns>
        private RpdePage RenderOrdersRPDEPageForFeed(string clientId, long? afterTimestamp, string afterId, long? afterChangeNumber)
        {
            if (settings.OrderFeedGenerator != null)
            {
                // Add lookup against authtoken and pass this into generator?
                return settings.OrderFeedGenerator.GetRPDEPage(clientId, afterTimestamp, afterId, afterChangeNumber);
            }
            else
            {
                // TODO: Change to Not Authorised Error
                throw new OpenBookingException(new NotFoundError(), $"Access to this endpoint is not authorised.");
            }
        }

        protected bool IsOpportunityTypeRecognised(string opportunityTypeString)
        {
            return this.idConfigurationLookup.ContainsKey(opportunityTypeString);
        }

        // Note this is not a helper as it relies on engine settings state
        protected IBookableIdComponents ResolveOpportunityID(string opportunityTypeString, Uri opportunityId, Uri offerId)
        {
            // Return the first matching ID combination for the opportunityId and offerId provided.
            // TODO: Make this more efficient?
            return this.idConfigurationLookup[opportunityTypeString]
                .Select(x => x.GetOpportunityReference(opportunityId, offerId))
                .Where(x => x != null)
                .FirstOrDefault();
        }

        public ResponseContent ProcessCheckpoint1(string clientId, Uri sellerId, string uuid, string orderQuoteJson)
        {
            return ProcessCheckpoint(clientId, sellerId, uuid, orderQuoteJson, FlowStage.C1, OrderType.OrderQuote);
        }
        public ResponseContent ProcessCheckpoint2(string clientId, Uri sellerId, string uuid, string orderQuoteJson)
        {
            return ProcessCheckpoint(clientId, sellerId, uuid, orderQuoteJson, FlowStage.C2, OrderType.OrderQuote);
        }
        private ResponseContent ProcessCheckpoint(string clientId, Uri sellerId, string uuid, string orderQuoteJson, FlowStage flowStage, OrderType orderType)
        {
            OrderQuote orderQuote = OpenActiveSerializer.Deserialize<OrderQuote>(orderQuoteJson);
            if (orderQuote == null || orderQuote.GetType() != typeof(OrderQuote))
            {
                throw new OpenBookingException(new OpenBookingError(), "OrderQuote is required for C1 and C2");
            }
            var orderResponse = ValidateFlowRequest<OrderQuote>(clientId, sellerId, flowStage, uuid, orderType, orderQuote);
            // Return a 409 status code if any OrderItem level errors exist
            return ResponseContent.OpenBookingResponse(OpenActiveSerializer.Serialize(orderResponse),
                orderResponse.OrderedItem.Exists(x => x.Error?.Count > 0) ? HttpStatusCode.Conflict : HttpStatusCode.OK);
        }
        public ResponseContent ProcessOrderCreationB(string clientId, Uri sellerId, string uuid, string orderJson)
        {
            // Note B will never contain OrderItem level errors, and any issues that occur will be thrown as exceptions.
            // If C1 and C2 are used correctly, B should not fail except in very exceptional cases.
            Order order = OpenActiveSerializer.Deserialize<Order>(orderJson);
            if (order == null || order.GetType() != typeof(Order))
            {
                throw new OpenBookingException(new OpenBookingError(), "Order is required for B");
            }
            return ResponseContent.OpenBookingResponse(OpenActiveSerializer.Serialize(ValidateFlowRequest<Order>(clientId, sellerId, FlowStage.B, uuid, OrderType.Order, order)), HttpStatusCode.OK);
        }

        private SellerIdComponents GetSellerIdComponentsFromApiKey(Uri sellerId)
        {
            // Return empty SellerIdComponents in Single Seller mode, as it is not required in the API Key
            if (settings.HasSingleSeller == true) return new SellerIdComponents();

            var sellerIdComponents = settings.SellerIdTemplate.GetIdComponents(sellerId);
            if (sellerIdComponents == null) throw new OpenBookingException(new InvalidAPITokenError());
            return sellerIdComponents;
        }

        public ResponseContent DeleteOrder(string clientId, Uri sellerId, string uuid)
        {
            ProcessOrderDeletion(new OrderIdComponents { ClientId = clientId, OrderType = OrderType.Order, uuid = uuid }, GetSellerIdComponentsFromApiKey(sellerId));
            return ResponseContent.OpenBookingNoContentResponse();
        }

        protected abstract void ProcessOrderDeletion(OrderIdComponents orderId, SellerIdComponents sellerId);

        public ResponseContent DeleteOrderQuote(string clientId, Uri sellerId, string uuid)
        {
            ProcessOrderQuoteDeletion(new OrderIdComponents { ClientId = clientId, OrderType = OrderType.OrderQuote, uuid = uuid }, GetSellerIdComponentsFromApiKey(sellerId));
            return ResponseContent.OpenBookingNoContentResponse();
        }

        protected abstract void ProcessOrderQuoteDeletion(OrderIdComponents orderId, SellerIdComponents sellerId);

        public ResponseContent ProcessOrderUpdate(string clientId, Uri sellerId, string uuid, string orderJson)
        {
            Order order = OpenActiveSerializer.Deserialize<Order>(orderJson);
            SellerIdComponents sellerIdComponents = GetSellerIdComponentsFromApiKey(sellerId);

            // Check for PatchContainsExcessiveProperties
            Order orderWithOnlyAllowedProperties = new Order
            {
                OrderedItem = order.OrderedItem.Select(x => new OrderItem { Id = x.Id, OrderItemStatus = x.OrderItemStatus }).ToList()
            };
            if (OpenActiveSerializer.Serialize<Order>(order) != OpenActiveSerializer.Serialize<Order>(orderWithOnlyAllowedProperties)) {
                throw new OpenBookingException(new PatchContainsExcessiveProperties());
            }

            // Check for PatchNotAllowedOnProperty
            if (!order.OrderedItem.TrueForAll(x => x.OrderItemStatus == OrderItemStatus.CustomerCancelled))
            {
                throw new OpenBookingException(new PatchNotAllowedOnProperty(), "Only 'https://openactive.io/CustomerCancelled' is permitted for this property.");
            }

            var orderItemIds = order.OrderedItem.Select(x => settings.OrderIdTemplate.GetOrderItemIdComponents(clientId, x.Id)).ToList();

            // Check for mismatching UUIDs
            if (!orderItemIds.TrueForAll(x => x != null))
            {
                throw new OpenBookingException(new OpenBookingError(), "Invalid OrderItem Id supplied.");
            }

            // Check for mismatching UUIDs
            if (!orderItemIds.TrueForAll(x => x.OrderType == OrderType.Order && x.uuid == uuid))
            {
                throw new OpenBookingException(new OpenBookingError(), "The UUID for each OrderItem specified must match the UUID of the Order being PATCHed.");
            }

            ProcessCustomerCancellation(new OrderIdComponents { ClientId = clientId, OrderType = OrderType.Order, uuid = uuid }, sellerIdComponents, settings.OrderIdTemplate, orderItemIds);

            return ResponseContent.OpenBookingNoContentResponse();
        }

        public abstract void ProcessCustomerCancellation(OrderIdComponents orderId, SellerIdComponents sellerId, OrderIdTemplate orderIdTemplate, List<OrderIdComponents> orderItemIds);

        // Note opportunityType is required here to facilitate routing to the correct store to handle the request
        public ResponseContent CreateTestData(string opportunityType, string eventJson)
        {
            Event @event = OpenActiveSerializer.Deserialize<ScheduledSession>(eventJson);

            // Returns a matching Event subclass that will only include "@type" and "@id" properties
            var createdEvent = this.CreateTestDataItem((OpportunityType)Enum.Parse(typeof(OpportunityType), opportunityType, true), @event);

            if (createdEvent.Type != @event.Type)
            {
                throw new OpenBookingException(new OpenBookingError(), "Type of created Event does not match type of requested Event");
            }

            return ResponseContent.OpenBookingResponse(OpenActiveSerializer.Serialize(createdEvent), HttpStatusCode.OK);
        }

        protected abstract Event CreateTestDataItem(OpportunityType opportunityType, Event @event);

        // Note opportunityType is required here to facilitate routing to the correct store to handle the request
        public ResponseContent DeleteTestData(string opportunityType, string id)
        {
            if (Uri.TryCreate(id, UriKind.Absolute, out Uri jsonLdId))
            {
                this.DeleteTestDataItem((OpportunityType)Enum.Parse(typeof(OpportunityType), opportunityType, true), jsonLdId);
            }
            else
            {
                throw new OpenBookingException(new OpenBookingError(), "Invalid ID provided");
            }   
            
            return ResponseContent.OpenBookingNoContentResponse();
        }

        protected abstract void DeleteTestDataItem(OpportunityType opportunityType, Uri id);


        //TODO: Should we move Seller into the Abstract level? Perhaps too much complexity
        private O ValidateFlowRequest<O>(string clientId, Uri authenticationSellerId, FlowStage stage, string uuid, OrderType orderType, O orderQuote) where O : Order, new()
        {
            var orderId = new OrderIdComponents
            {
                ClientId = clientId,
                uuid = uuid,
                OrderType = orderType
            };

            // TODO: Add more request validation rules here

            SellerIdComponents sellerIdComponents = GetSellerIdComponentsFromApiKey(authenticationSellerId);

            ILegalEntity seller = settings.SellerStore.GetSellerById(sellerIdComponents);

            if (seller == null)
            {
                // TODO: Update data model to throw actual error for all occurances of OpenBookingError
                throw new OpenBookingException(new OpenBookingError(), "SellerNotFound");
            }
            
            if (orderQuote?.Seller?.Id != null && seller?.Id != orderQuote?.Seller?.Id)
            {
                // TODO: Update data model to throw actual error for all occurances of OpenBookingError
                throw new OpenBookingException(new OpenBookingError(), "SellerIdMismatch");
            }

            // Check that taxMode is set in Seller
            if (!(seller?.TaxMode == TaxMode.TaxGross || seller?.TaxMode == TaxMode.TaxNet))
            {
                throw new OpenBookingException(new OpenBookingError(), "taxMode must always be set in the Seller");
            }

            // Default to BusinessToConsumer if no customer provided
            TaxPayeeRelationship taxPayeeRelationship =
                orderQuote.Customer == null ? 
                    TaxPayeeRelationship.BusinessToConsumer :
                    orderQuote.BrokerRole == BrokerType.ResellerBroker || orderQuote.Customer.IsOrganization
                        ? TaxPayeeRelationship.BusinessToBusiness : TaxPayeeRelationship.BusinessToConsumer;

            var payer = orderQuote.BrokerRole == BrokerType.ResellerBroker ? orderQuote.Broker : orderQuote.Customer;

            return ProcessFlowRequest<O>(new BookingFlowContext {
                Stage = stage,
                OrderId = orderId,
                OrderIdTemplate = settings.OrderIdTemplate,
                Seller = seller,
                SellerId = sellerIdComponents,
                TaxPayeeRelationship = taxPayeeRelationship,
                Payer = payer 
            }, orderQuote);
        }

        public abstract TOrder ProcessFlowRequest<TOrder>(BookingFlowContext request, TOrder order) where TOrder : Order, new();

    }
}
