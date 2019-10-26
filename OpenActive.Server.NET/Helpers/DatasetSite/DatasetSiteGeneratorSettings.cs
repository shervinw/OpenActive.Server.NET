
using System;

namespace OpenActive.Server.NET.Helpers
{
    public class DatasetSiteGeneratorSettings
    {
        public string OrganisationName { get; set; }
        public string DatasetSiteUrl { get; set; }
        public string DatasetSiteDocumentationUrl { get; set; }
        public string DocumentationUrl { get; set; }
        public string LegalEntity { get; set; }
        public string PlainTextDescription { get; set; }
        public string Email { get; set; }
        public string Url { get; set; }
        public string LogoUrl { get; set; }
        public string BackgroundImageUrl { get; set; }
        public string BaseUrl { get; set; }
        public string PlatformName { get; set; }
        public string PlatformUrl { get; set; }

        public DateTimeOffset DateFirstPublished { get; set; }
    }
}