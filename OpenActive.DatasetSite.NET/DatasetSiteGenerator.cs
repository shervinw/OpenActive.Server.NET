using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenActive.NET;
using Stubble.Core.Builders;
using Stubble.Extensions.JsonNet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenActive.DataSetSite.NET
{
    public static class DatasetSiteGenerator
    {
        public static string RenderSimpleDatasetSite(DatasetSiteGeneratorSettings settings, List<FeedType> supportedFeedTypes)
        {
            // Check input is not null
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (supportedFeedTypes == null) throw new ArgumentNullException(nameof(supportedFeedTypes));

            var supportedFeedConfigurations = supportedFeedTypes.Select(x => FeedConfigurations.Configurations[x]);

            var dataDownloads = supportedFeedConfigurations
                .Select(x => new DataDownload
                {
                    Name = x.Name,
                    AdditionalType = x.SameAs,
                    EncodingFormat = OpenActiveDiscovery.MediaTypes.Version1.RealtimePagedDataExchange.ToString(),
                    ContentUrl = new Uri(settings.OpenFeedBaseUrl + x.DefaultFeedPath)
                })
                .ToList();

            var dataFeedDescriptions = supportedFeedConfigurations.Select(x => x.DisplayName).Distinct().ToList();

            return RenderSimpleDatasetSite(settings, dataDownloads, dataFeedDescriptions);
        }

        /// <summary>
        /// Converts a list of nouns into a human readable list
        /// 
        /// ["One", "Two", "Three", "Four"] => "One, Two, Three and Four"
        /// </summary>
        /// <param name="list">List of nouns</param>
        /// <returns>String containing human readable list</returns>
        private static string ToHumanisedList(this List<string> list)
        {
            const string separator = ", ";
            var humanList = String.Join(separator, list);
            int i = humanList.LastIndexOf(separator, StringComparison.InvariantCulture);
            if (i >= 0)
                humanList = humanList.Substring(0, i) + " and " + humanList.Substring(i + separator.Length);
            return humanList;
        }

        public static string RenderSimpleDatasetSite(DatasetSiteGeneratorSettings settings, List<DataDownload> dataDownloads, List<string> dataFeedDescriptions)
        {
            // Check input is not null
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (dataDownloads == null) throw new ArgumentNullException(nameof(dataDownloads));
            if (dataFeedDescriptions == null) throw new ArgumentNullException(nameof(dataFeedDescriptions));

            // Pre-process list of feed descriptions
            var dataFeedHumanisedList = dataFeedDescriptions.ToHumanisedList();
            var keywords = new List<string> {
                    "Activities",
                    "Sports",
                    "Physical Activity",
                    "OpenActive"
                };
            keywords.InsertRange(0, dataFeedDescriptions);

            // Strongly typed JSON generation based on OpenActive.NET
            var dataset = new Dataset
            {
                Id = settings.DatasetSiteUrl,
                Url = settings.DatasetSiteUrl,
                Name = settings.OrganisationName + " " + dataFeedHumanisedList,
                Description = $"Near real-time availability and rich descriptions relating to the {dataFeedHumanisedList.ToLowerInvariant()} available from {settings.OrganisationName}, published using the OpenActive Modelling Specification 2.0.",
                Keywords = keywords,
                License = new Uri("https://creativecommons.org/licenses/by/4.0/"),
                DiscussionUrl = settings.DatasetDiscussionUrl,
                Documentation = settings.DatasetDocumentationUrl,
                //InLanguage = new List<string> { "en-GB" },
                SchemaVersion = new Uri("https://www.openactive.io/modelling-opportunity-data/2.0/"),
                Publisher = new OpenActive.NET.Organization
                {
                    Name = settings.OrganisationName,
                    LegalName = settings.OrganisationLegalEntity,
                    Description = settings.DatasetPlainTextDescription,
                    Email = settings.OrganisationEmail,
                    Url = settings.OrganisationUrl,
                    Logo = new OpenActive.NET.ImageObject
                    {
                        Url = settings.OrganisationLogoUrl
                    }
                },
                Distribution = dataDownloads,
                DatePublished = settings.DateFirstPublished,
                BackgroundImage = new ImageObject {
                    Url = settings.BackgroundImageUrl
                },
                BookingService = new BookingService
                {
                    Name = settings.PlatformName,
                    Url = settings.PlatformUrl,
                    SoftwareVersion = settings.PlatformVersion
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

            // Stringify the input JSON using formatting, and place the contents of the string
            // within the "json" property at the root of the JSON itself.
            jsonObj.Add("json", jsonObj.ToString(Formatting.Indented));

            //Use the resulting JSON with the mustache template to render the dataset site.
            var stubble = new StubbleBuilder().Configure(s => s.AddJsonNet()).Build();
            return stubble.Render(DatasetSiteMustacheTemplate.Content, jsonObj);
        }
    }
}
