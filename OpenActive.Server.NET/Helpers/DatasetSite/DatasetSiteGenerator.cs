using OpenActive.NET;
using Newtonsoft.Json;
using Stubble.Core.Builders;
using Stubble.Extensions.JsonNet;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OpenActive.Server.NET.Helpers
{
    public static partial class DatasetSiteGenerator
    {
        public static string RenderSimpleDatasetSite(DatasetSiteGeneratorSettings settings)
        {
            // Check input settings is not null
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            // Strongly typed JSON generation based on OpenActive.NET
            var dataset = new Dataset
            {
                Id = settings.DatasetSiteUrl.ParseUrlOrNull(),
                Url = settings.DatasetSiteUrl.ParseUrlOrNull(),
                Name = settings.OrganisationName + " Sessions and Facilities",
                Description = "Near real-time availability and rich descriptions relating to the sessions and facilities available from {settings.organisationName}, published using the OpenActive Modelling Specification 2.0.",
                Keywords = new List<string> {
                    "Sessions",
                    "Facilities",
                    "Activities",
                    "Sports",
                    "Physical Activity",
                    "OpenActive"
                },
                License = new Uri("https://creativecommons.org/licenses/by/4.0/"),
                DiscussionUrl = settings.DatasetSiteDocumentationUrl.ParseUrlOrNull(),
                Documentation = settings.DocumentationUrl.ParseUrlOrNull(),
                InLanguage = "en-GB",
                SchemaVersion = new Uri("https://www.openactive.io/modelling-opportunity-data/2.0/"),
                Publisher = new OpenActive.NET.Organization
                {
                    Name = settings.OrganisationName,
                    LegalName = settings.LegalEntity,
                    Description = settings.PlainTextDescription,
                    Email = settings.Email,
                    Url = settings.Url.ParseUrlOrNull(),
                    Logo = new OpenActive.NET.ImageObject
                    {
                        Url = settings.LogoUrl.ParseUrlOrNull()
                    }
                },
                Distribution = new List<DataDownload>
                {
                    new DataDownload
                    {
                        Name = "SessionSeries",
                        AdditionalType = new Uri("https://openactive.io/SessionSeries"),
                        EncodingFormat = OpenActiveDiscovery.MediaTypes.Version1.RealtimePagedDataExchange.ToString(),
                        ContentUrl = (settings.BaseUrl + "feeds/session-series").ParseUrlOrNull()
                    },
                    new DataDownload
                    {
                        Name = "ScheduledSession",
                        AdditionalType = new Uri("https://openactive.io/ScheduledSession"),
                        EncodingFormat = OpenActiveDiscovery.MediaTypes.Version1.RealtimePagedDataExchange.ToString(),
                        ContentUrl = (settings.BaseUrl + "feeds/scheduled-sessions").ParseUrlOrNull()
                    },
                    new DataDownload
                    {
                        Name = "FacilityUse",
                        AdditionalType = new Uri("https://openactive.io/FacilityUse"),
                        EncodingFormat = OpenActiveDiscovery.MediaTypes.Version1.RealtimePagedDataExchange.ToString(),
                        ContentUrl = (settings.BaseUrl + "feeds/facility-uses").ParseUrlOrNull()
                    },
                    new DataDownload
                    {
                        Name = "Slot",
                        AdditionalType = new Uri("https://openactive.io/Slot"),
                        EncodingFormat = OpenActiveDiscovery.MediaTypes.Version1.RealtimePagedDataExchange.ToString(),
                        ContentUrl = (settings.BaseUrl + "feeds/slots").ParseUrlOrNull()
                    }
                },
                DatePublished = settings.DateFirstPublished,
                BackgroundImage = new ImageObject {
                    Url = settings.BackgroundImageUrl.ParseUrlOrNull()
                },
                BookingService = new BookingService
                {
                    Name = settings.PlatformName,
                    Url = settings.PlatformUrl.ParseUrlOrNull(),
                    SoftwareVersion = ApplicationVersion.GetVersion()
                }
            };
            return RenderDatasetSite(dataset);
        }

        public static string RenderDatasetSite(Dataset dataset)
        {
            // Check input dataset is not null
            if (dataset == null) throw new ArgumentNullException(nameof(dataset));

            // OpenActive.NET creates complete JSON from the strongly typed structure, complete with schema.org types.
            var jsonString = dataset.ToOpenActiveHtmlEmbeddableString();

            // Deserialize the completed JSON object to make it compatible with the mustache template
            JObject jsonObj = JObject.Parse(jsonString);

            // Stringify the input JSON, and place the contents of the string
            // within the "json" property at the root of the JSON itself.
            jsonObj.Add("json", jsonObj.ToString(Formatting.Indented));

            //Use the resulting JSON with the mustache template to render the dataset site.
            var stubble = new StubbleBuilder().Configure(s => s.AddJsonNet()).Build();
            return stubble.Render(MustacheContent, jsonObj);
        }

        private static class ApplicationVersion
        {
            public static string GetVersion()
            {
                return typeof(ApplicationVersion).Assembly.GetName().Version.ToString();
            }
        }
    }
}
