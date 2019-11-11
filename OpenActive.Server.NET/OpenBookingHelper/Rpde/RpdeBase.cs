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
        void SetConfiguration(OpportunityTypeConfiguration OpportunityTypeConfiguration, Uri jsonLdIdBaseUrl, int rpdePageSize, IBookablePairIdTemplate template, Uri openDataFeedBaseUrl);
    }

    public abstract class OpporunityDataRPDEFeedGenerator<T> : ModelSupport<T>, IOpportunityDataRPDEFeedGenerator where T : class, IBookableIdComponents, new()
    {
        public int RPDEPageSize { get; private set; }
        public virtual Uri FeedUrl { get; protected set; }
        public virtual string FeedPath { get; protected set; }

        public void SetConfiguration(OpportunityTypeConfiguration opportunityTypeConfiguration, Uri jsonLdIdBaseUrl, int rpdePageSize, IBookablePairIdTemplate template, Uri openDataFeedBaseUrl)
        {
            if (template as BookablePairIdTemplate<T> == null)
            {
                throw new NotSupportedException($"{template.GetType().ToString()} does not match {typeof(BookablePairIdTemplate<T>).ToString()}. All types of IBookableIdComponents (T) used for BookablePairIdTemplate<T> assigned to feeds via settings.IdConfiguration must match those used for RPDEFeedGenerator<T> in settings.OpenDataFeeds.");
            }

            SetConfiguration(opportunityTypeConfiguration, jsonLdIdBaseUrl, rpdePageSize, (BookablePairIdTemplate<T>)template, openDataFeedBaseUrl);
        }

        internal void SetConfiguration(OpportunityTypeConfiguration opportunityTypeConfiguration, Uri jsonLdIdBaseUrl, int rpdePageSize, BookablePairIdTemplate<T> template, Uri openDataFeedBaseUrl)
        {
            base.SetConfiguration(template);

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

    public abstract class RPDEFeedIncrementingUniqueChangeNumber<T> : OpporunityDataRPDEFeedGenerator<T>, IRPDEFeedIncrementingUniqueChangeNumber where T : class, IBookableIdComponents, new()
    {
        protected abstract List<RpdeItem> GetRPDEItems(long? afterChangeNumber);

        public RpdePage GetRPDEPage(long? afterChangeNumber)
        {
            return new RpdePage(this.FeedUrl, afterChangeNumber, GetRPDEItems(afterChangeNumber));
        }
    }

    public abstract class RPDEFeedModifiedTimestampAndIDLong<T> : OpporunityDataRPDEFeedGenerator<T>, IRPDEFeedModifiedTimestampAndIDLong where T : class, IBookableIdComponents, new()
    {
        protected abstract List<RpdeItem> GetRPDEItems(long? afterTimestamp, long? afterId);

        public RpdePage GetRPDEPage(long? afterTimestamp, long? afterId)
        {
            if ((!afterTimestamp.HasValue && afterId.HasValue) ||
                (afterTimestamp.HasValue && !afterId.HasValue))
            {
                throw new ArgumentNullException("afterTimestamp and afterId must both be supplied, or neither supplied");
            }
            else
            {
                return new RpdePage(this.FeedUrl, afterTimestamp, afterId, GetRPDEItems(afterTimestamp, afterId));
            }
        }
    }

    public abstract class RPDEFeedModifiedTimestampAndIDString<T> : OpporunityDataRPDEFeedGenerator<T>, IRPDEFeedModifiedTimestampAndIDString where T : class, IBookableIdComponents, new()
    {
        protected abstract List<RpdeItem> GetRPDEItems(long? afterTimestamp, string afterId);

        public RpdePage GetRPDEPage(long? afterTimestamp, string afterId)
        {
            if ((!afterTimestamp.HasValue && !string.IsNullOrWhiteSpace(afterId)) ||
                (afterTimestamp.HasValue && string.IsNullOrWhiteSpace(afterId)))
            {
                throw new ArgumentNullException("afterTimestamp and afterId must both be supplied, or neither supplied");
            }
            else
            {
                return new RpdePage(this.FeedUrl, afterTimestamp, afterId, GetRPDEItems(afterTimestamp, afterId));
            }
        }
    }

}
