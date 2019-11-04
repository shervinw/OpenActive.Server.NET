using System;
using System.Collections.Generic;
using OpenActive.DatasetSite.NET;
using OpenActive.NET;
using OpenActive.NET.Rpde.Version1;


namespace OpenActive.Server.NET
{
    public abstract class RPDEFeedGenerator {
        public Uri JsonLdIdBaseUrl { get; private set; }
        public virtual Uri FeedUrl { get; protected set; }
        public virtual string FeedPath { get; protected set; }
        public IBookablePairIdTemplate IdTemplate { get; set; }
        public IBookablePairIdTemplate ParentIdTemplate { get; set; }
        private FeedConfiguration FeedConfiguration { get; set; }
        private BookingEngineSettings BookingEngineSettings { get; set; }
        internal void SetConfiguration(FeedConfiguration feedConfiguration, BookingEngineSettings settings, Uri openDataFeedBaseUrl)
        {
            this.FeedConfiguration = feedConfiguration;
            this.BookingEngineSettings = settings;

            this.JsonLdIdBaseUrl = settings.JsonLdIdBaseUrl;

            // Allow these to be overridden by implementations if customisation is required
            this.FeedUrl = FeedUrl ?? new Uri(openDataFeedBaseUrl + FeedConfiguration.DefaultFeedPath);
            this.FeedPath = FeedPath ?? feedConfiguration.DefaultFeedPath;
        }

        /// <summary>
        /// This class is not designed to be used outside of the library, one of its subclasses must be used instead
        /// </summary>
        internal RPDEFeedGenerator() { }
    }

    public abstract class RPDEFeedIncrementingUniqueChangeNumber : RPDEFeedGenerator
    {
        protected abstract List<RpdeItem> GetRPDEItems(long? afterChangeNumber);

        public RpdePage GetRPDEPage(long? afterChangeNumber)
        {
            return new RpdePage(this.FeedUrl, afterChangeNumber, GetRPDEItems(afterChangeNumber));
        }
    }

    public abstract class RPDEFeedModifiedTimestampAndIDLong : RPDEFeedGenerator
    {
        protected abstract List<RpdeItem> GetRPDEItems(long? afterTimestamp, long? afterId);

        public RpdePage GetRPDEPage(long? afterTimestamp, long? afterId)
        {
            return new RpdePage(this.FeedUrl, afterTimestamp, afterId, GetRPDEItems(afterTimestamp, afterId));
        }
    }
    public abstract class RPDEFeedModifiedTimestampAndIDString : RPDEFeedGenerator
    {
        protected abstract List<RpdeItem> GetRPDEItems(long? afterTimestamp, string afterId);

        public RpdePage GetRPDEPage(long? afterTimestamp, string afterId)
        {
            return new RpdePage(this.FeedUrl, afterTimestamp, afterId, GetRPDEItems(afterTimestamp, afterId));
        }
    }

}
