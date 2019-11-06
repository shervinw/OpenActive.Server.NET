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

            // Create a lookup for the purposes of finding arbitary IdConfigurations, for use in the store
            // TODO: Pull this and the above into a function?
            this.opportunityTemplateLookup = settings.IdConfiguration.Select(t => t.IdConfigurations.Select(x => new
            {
                assignedFeed = x.OpportunityType,
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
        private Dictionary<OpportunityType, IBookablePairIdTemplate> opportunityTemplateLookup;

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
        protected IBookableIdComponents ResolveOpportunityID(string opportunityTypeString, Uri opportunityId, Uri offerId)
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

        // Note opportunityType is required here to facilitate routing to the correct store to handle the request
        public void CreateTestData(string opportunityType, Event @event)
        {
            this.CreateTestDataItem((OpportunityType)Enum.Parse(typeof(OpportunityType), opportunityType, true), @event);
        }

        public abstract void CreateTestDataItem(OpportunityType opportunityType, Event @event);

        // Note opportunityType is required here to facilitate routing to the correct store to handle the request
        public void DeleteTestData(string opportunityType, string name)
        {
            this.DeleteTestDataItem((OpportunityType)Enum.Parse(typeof(OpportunityType), opportunityType, true), name);
        }

        public abstract void DeleteTestDataItem(OpportunityType opportunityType, string name);


        //TODO: Should we move Seller into the Abstract level? Perhaps too much complexity
        private O ValidateFlowRequest<O>(FlowStage stage, string uuid, O orderQuote) where O : Order
        {
            var orderId = new OrderIdComponents
            {
                uuid = uuid,
                BaseUrl = settings.OrderBaseUrl
            };

            // TODO: Add more request validation rules here

            // Check that taxMode is set in Seller
            if (orderQuote?.Seller?.Id == null)
            {
                // TODO: Update data model to throw actual error for all occurances of OpenBookingError
                throw new OpenBookingException(new OpenBookingError(), "SellerNotSpecified");
            }

            var sellerID = settings.SellerIdTemplate.GetIdComponents(orderQuote.Seller.Id);

            // Check that taxMode is set in Seller
            if (orderQuote?.Seller?.Id == null)
            {
                // TODO: Update data model to throw actual error for all occurances of OpenBookingError
                throw new OpenBookingException(new OpenBookingError(), "SellerNotSpecified");
            }

            // Check that taxMode is set in Seller
            if (!(orderQuote?.Seller?.TaxMode == TaxMode.TaxGross || orderQuote?.Seller?.TaxMode == TaxMode.TaxNet))
            {
                throw new OpenBookingException(new OpenBookingError(), "taxMode must always be set in the Seller");
            }

            TaxPayeeRelationship taxPayeeRelationship = orderQuote.BrokerRole == BrokerType.ResellerBroker
                // TODO: Add HasValue to SingleValues to make the below check more robust
                || orderQuote.Customer.Value1.Type == "Organisation" ? TaxPayeeRelationship.BusinessToBusiness : TaxPayeeRelationship.BusinessToConsumer;

            var payer = orderQuote.BrokerRole == BrokerType.ResellerBroker ? orderQuote.Broker : orderQuote.Customer;

            return ProcessFlowRequest<O>(new BookingFlowContext<O> {
                Stage = stage,
                OrderIdComponents = orderId,
                Order = orderQuote,
                TaxPayeeRelationship = taxPayeeRelationship,
                Payer = payer }
            );
        }

        public abstract TOrder ProcessFlowRequest<TOrder>(BookingFlowContext<TOrder> request) where TOrder : Order;

    }
}
