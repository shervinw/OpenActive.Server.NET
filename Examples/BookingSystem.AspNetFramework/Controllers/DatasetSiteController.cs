
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BookingSystem.AspNetFramework.Controllers
{
    public class DatasetSiteController : Controller
    {
        public ActionResult Index()
        {
            // Customer-specific settings for dataset JSON (these should come from a database)
            var settings = new DatasetSiteGeneratorSettings
            {
                OpenDataFeedBaseUrl = "https://customer.example.com/feed/".ParseUrlOrNull(),
                OpenBookingAPIBaseUrl = "https://customer.example.com/api/openbooking/".ParseUrlOrNull(),
                DatasetSiteUrl = "https://halo-odi.legendonlineservices.co.uk/openactive/".ParseUrlOrNull(),
                DatasetDiscussionUrl = "https://github.com/gll-better/opendata".ParseUrlOrNull(),
                DatasetDocumentationUrl = "https://docs.acmebooker.example.com/".ParseUrlOrNull(),
                DatasetLanguages = new List<string> { "en-GB" },
                OrganisationName = "Better",
                OrganisationUrl = "https://www.better.org.uk/".ParseUrlOrNull(),
                OrganisationLegalEntity = "GLL",
                OrganisationPlainTextDescription = "Established in 1993, GLL is the largest UK-based charitable social enterprise delivering leisure, health and community services. Under the consumer facing brand Better, we operate 258 public Sports and Leisure facilities, 88 libraries, 10 children’s centres and 5 adventure playgrounds in partnership with 50 local councils, public agencies and sporting organisations. Better leisure facilities enjoy 46 million visitors a year and have more than 650,000 members.",
                OrganisationLogoUrl = "http://data.better.org.uk/images/logo.png".ParseUrlOrNull(),
                OrganisationEmail = "info@better.org.uk",
                PlatformName = "AcmeBooker",
                PlatformUrl = "https://acmebooker.example.com/".ParseUrlOrNull(),
                PlatformVersion = "2.0",
                BackgroundImageUrl = "https://data.better.org.uk/images/bg.jpg".ParseUrlOrNull(),
                DateFirstPublished = new DateTimeOffset(new DateTime(2019, 01, 14))
            };

            var supportedFeeds = new List<FeedType> {
                FeedType.SessionSeries,
                FeedType.ScheduledSession,
                FeedType.FacilityUse,
                FeedType.Slot,
                FeedType.CourseInstance
            };

            return Content(DatasetSiteGenerator.RenderSimpleDatasetSite(settings), "text/html");
        }
    }
}
