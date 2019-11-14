using System;
using System.Collections.Generic;
using OpenActive.DatasetSite.NET;
using OpenActive.NET;
using OpenActive.NET.Rpde.Version1;


namespace OpenActive.Server.NET.OpenBookingHelper
{
    public interface IOpportunityDataRPDEFeedGenerator : IRPDEFeedGenerator
    {
        string FeedPath { get; }
        void SetConfiguration(OpportunityTypeConfiguration OpportunityTypeConfiguration, Uri jsonLdIdBaseUrl, int rpdePageSize, IBookablePairIdTemplate template, SingleIdTemplate<SellerIdComponents> sellerTemplate, Uri openDataFeedBaseUrl);
    }

    public abstract class OpporunityDataRPDEFeedGenerator<TComponents, TClass> : ModelSupport<TComponents>, IOpportunityDataRPDEFeedGenerator where TComponents : class, IBookableIdComponents, new() where TClass : Schema.NET.Thing
    {
        public int RPDEPageSize { get; private set; }
        public virtual Uri FeedUrl { get; protected set; }
        public virtual string FeedPath { get; protected set; }

        public void SetConfiguration(OpportunityTypeConfiguration opportunityTypeConfiguration, Uri jsonLdIdBaseUrl, int rpdePageSize, IBookablePairIdTemplate template, SingleIdTemplate<SellerIdComponents> sellerTemplate, Uri openDataFeedBaseUrl)
        {
            if (template as BookablePairIdTemplate<TComponents> == null)
            {
                throw new EngineConfigurationException($"{template.GetType().ToString()} does not match {typeof(BookablePairIdTemplate<TComponents>).ToString()}. All types of IBookableIdComponents (T) used for BookablePairIdTemplate<T> assigned to feeds via settings.IdConfiguration must match those used for RPDEFeedGenerator<T> in settings.OpenDataFeeds.");
            }

            if (opportunityTypeConfiguration.SameAs.AbsolutePath.Trim('/') != typeof(TClass).Name)
            {
                throw new EngineConfigurationException($"'{this.GetType().ToString()}' does not have this expected OpenActive model type as generic parameter: '{opportunityTypeConfiguration.SameAs.AbsolutePath.Trim('/')}'");
            }

            base.SetConfiguration((BookablePairIdTemplate<TComponents>)template, sellerTemplate);

            this.RPDEPageSize = rpdePageSize;

            // Allow these to be overridden by implementations if customisation is required
            this.FeedUrl = FeedUrl ?? new Uri(openDataFeedBaseUrl + opportunityTypeConfiguration.DefaultFeedPath);
            this.FeedPath = FeedPath ?? opportunityTypeConfiguration.DefaultFeedPath;
        }

        /// <summary>
        /// This class is not designed to be used outside of the library, one of its subclasses must be used instead
        /// </summary>
        internal OpporunityDataRPDEFeedGenerator() { }
    }

    public abstract class RPDEFeedIncrementingUniqueChangeNumber<TComponents, TClass> : OpporunityDataRPDEFeedGenerator<TComponents, TClass>, IRPDEFeedIncrementingUniqueChangeNumber where TComponents : class, IBookableIdComponents, new() where TClass : Schema.NET.Thing
    {
        protected abstract List<RpdeItem<TClass>> GetRPDEItems(long? afterChangeNumber);

        public RpdePage GetRPDEPage(long? afterChangeNumber)
        {
            return new RpdePage(this.FeedUrl, afterChangeNumber, GetRPDEItems(afterChangeNumber).ConvertAll<RpdeItem>(x => (RpdeItem)x));
        }
    }

    public abstract class RPDEFeedModifiedTimestampAndIDLong<TComponents, TClass> : OpporunityDataRPDEFeedGenerator<TComponents, TClass>, IRPDEFeedModifiedTimestampAndIDLong where TComponents : class, IBookableIdComponents, new() where TClass : Schema.NET.Thing
    {
        protected abstract List<RpdeItem<TClass>> GetRPDEItems(long? afterTimestamp, long? afterId);

        public RpdePage GetRPDEPage(long? afterTimestamp, long? afterId)
        {
            if ((!afterTimestamp.HasValue && afterId.HasValue) ||
                (afterTimestamp.HasValue && !afterId.HasValue))
            {
                throw new ArgumentNullException("afterTimestamp and afterId must both be supplied, or neither supplied");
            }
            else
            {
                return new RpdePage(this.FeedUrl, afterTimestamp, afterId, GetRPDEItems(afterTimestamp, afterId).ConvertAll<RpdeItem>(x => (RpdeItem)x));
            }
        }
    }

    public abstract class RPDEFeedModifiedTimestampAndIDString<TComponents, TClass> : OpporunityDataRPDEFeedGenerator<TComponents, TClass>, IRPDEFeedModifiedTimestampAndIDString where TComponents : class, IBookableIdComponents, new() where TClass : Schema.NET.Thing
    {
        protected abstract List<RpdeItem<TClass>> GetRPDEItems(long? afterTimestamp, string afterId);

        public RpdePage GetRPDEPage(long? afterTimestamp, string afterId)
        {
            if ((!afterTimestamp.HasValue && !string.IsNullOrWhiteSpace(afterId)) ||
                (afterTimestamp.HasValue && string.IsNullOrWhiteSpace(afterId)))
            {
                throw new ArgumentNullException("afterTimestamp and afterId must both be supplied, or neither supplied");
            }
            else
            {
                return new RpdePage(this.FeedUrl, afterTimestamp, afterId, GetRPDEItems(afterTimestamp, afterId).ConvertAll<RpdeItem>(x => (RpdeItem)x));
            }
        }
    }

}
