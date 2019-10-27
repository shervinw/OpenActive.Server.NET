
using System;

namespace OpenActive.DataSetSite.NET
{
    public class DatasetSiteGeneratorSettings
    {
        public string OrganisationName { get; set; }
        public Uri DatasetSiteUrl { get; set; }
        public Uri DatasetDiscussionUrl { get; set; }
        public Uri DatasetDocumentationUrl { get; set; }
        public string OrganisationLegalEntity { get; set; }
        public string DatasetPlainTextDescription { get; set; }
        public string OrganisationEmail { get; set; }
        public Uri OrganisationUrl { get; set; }
        public Uri OrganisationLogoUrl { get; set; }
        public Uri BackgroundImageUrl { get; set; }
        public Uri OpenFeedBaseUrl { get; set; }
        public string PlatformName { get; set; }
        public Uri PlatformUrl { get; set; }

        public DateTimeOffset DateFirstPublished { get; set; }
        public string PlatformVersion { get; set; }
        public Uri BookingBaseUrl { get; set; }
    }
}