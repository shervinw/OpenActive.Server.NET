using System;
using System.Collections.Generic;
using OpenActive.DatasetSite.NET;
using OpenActive.NET;
using OpenActive.NET.Rpde.Version1;


namespace OpenActive.Server.NET
{
    public interface IRPDEFeedGenerator
    {
        string FeedPath { get; }
        void SetConfiguration(OpportunityTypeConfiguration OpportunityTypeConfiguration, BookingEngineSettings settings, IBookablePairIdTemplate template, Uri openDataFeedBaseUrl);
    }

    public abstract class RPDEFeedGenerator<T> : IRPDEFeedGenerator where T : IBookableIdComponents, new()
    {
        public Uri JsonLdIdBaseUrl { get; private set; }
        public int RPDEPageSize { get; private set; }
        public virtual Uri FeedUrl { get; protected set; }
        public virtual string FeedPath { get; protected set; }
        private BookablePairIdTemplate<T> IdTemplate { get; set; }
        private OpportunityTypeConfiguration OpportunityTypeConfiguration { get; set; }
        private BookingEngineSettings BookingEngineSettings { get; set; }

        public void SetConfiguration(OpportunityTypeConfiguration opportunityTypeConfiguration, BookingEngineSettings settings, IBookablePairIdTemplate template, Uri openDataFeedBaseUrl)
        {
            if ( !(template.GetType() == typeof(BookablePairIdTemplate<T>) || template.GetType() == typeof(BookablePairIdTemplateWithOfferInheritance<T>) ) ) {
                throw new NotSupportedException($"{template.GetType().ToString()} does not match {typeof(BookablePairIdTemplate<T>).ToString()}. All types of IBookableIdComponents (T) used for BookablePairIdTemplate<T> assigned to feeds via settings.IdConfiguration must match those used for RPDEFeedGenerator<T> in settings.OpenDataFeeds.");
            }

            SetConfiguration(opportunityTypeConfiguration, settings, (BookablePairIdTemplate<T>)template, openDataFeedBaseUrl);
        }

        internal void SetConfiguration(OpportunityTypeConfiguration opportunityTypeConfiguration, BookingEngineSettings settings, BookablePairIdTemplate<T> template, Uri openDataFeedBaseUrl)
        {
            this.OpportunityTypeConfiguration = opportunityTypeConfiguration;
            this.BookingEngineSettings = settings;
            this.IdTemplate = template;

            this.RPDEPageSize = settings.RPDEPageSize;
            this.JsonLdIdBaseUrl = settings.JsonLdIdBaseUrl;

            // Allow these to be overridden by implementations if customisation is required
            this.FeedUrl = FeedUrl ?? new Uri(openDataFeedBaseUrl + opportunityTypeConfiguration.DefaultFeedPath);
            this.FeedPath = FeedPath ?? opportunityTypeConfiguration.DefaultFeedPath;
        }

        /// <summary>
        /// This class is not designed to be used outside of the library, one of its subclasses must be used instead
        /// </summary>
        internal RPDEFeedGenerator() { }

        protected Uri RenderOpportunityId(OpportunityType opportunityType, T components)
        {
            return IdTemplate.RenderOpportunityId(opportunityType, components);
        }

        protected Uri RenderOfferId(OpportunityType opportunityType, T components)
        {
            return IdTemplate.RenderOfferId(opportunityType, components);
        }
    }

    public interface IRPDEFeedIncrementingUniqueChangeNumber
    {
        RpdePage GetRPDEPage(long? afterChangeNumber);
    }

    public abstract class RPDEFeedIncrementingUniqueChangeNumber<T> : RPDEFeedGenerator<T>, IRPDEFeedIncrementingUniqueChangeNumber where T : IBookableIdComponents, new()
    {
        protected abstract List<RpdeItem> GetRPDEItems(long? afterChangeNumber);

        public RpdePage GetRPDEPage(long? afterChangeNumber)
        {
            return new RpdePage(this.FeedUrl, afterChangeNumber, GetRPDEItems(afterChangeNumber));
        }
    }

    public interface IRPDEFeedModifiedTimestampAndIDLong
    {
        RpdePage GetRPDEPage(long? afterTimestamp, long? afterId);
    }

    public abstract class RPDEFeedModifiedTimestampAndIDLong<T> : RPDEFeedGenerator<T>, IRPDEFeedModifiedTimestampAndIDLong where T : IBookableIdComponents, new()
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

    public interface IRPDEFeedModifiedTimestampAndIDString
    {
        RpdePage GetRPDEPage(long? afterTimestamp, string afterId);
    }

    public abstract class RPDEFeedModifiedTimestampAndIDString<T> : RPDEFeedGenerator<T>, IRPDEFeedModifiedTimestampAndIDString where T : IBookableIdComponents, new()
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
