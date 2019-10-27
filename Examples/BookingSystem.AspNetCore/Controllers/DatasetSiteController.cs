using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenActive.DataSetSite.NET;
using OpenActive.NET;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BookingSystem.AspNetCore.Controllers
{
    [Route("openactive")]
    public class DatasetSiteController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            // Customer-specific settings for dataset JSON (these should come from a database)
            var settings = new DatasetSiteGeneratorSettings
            {
                OrganisationName = "Better",
                OrganisationUrl = "https://www.better.org.uk/".ParseUrlOrNull(),
                OrganisationLogoUrl = "http://data.better.org.uk/images/logo.png".ParseUrlOrNull(),
                OrganisationLegalEntity = "GLL",
                OrganisationEmail = "info@better.org.uk",
                DatasetSiteUrl = "https://halo-odi.legendonlineservices.co.uk/openactive/".ParseUrlOrNull(),
                DatasetDiscussionUrl = "https://github.com/gll-better/opendata".ParseUrlOrNull(),
                DatasetPlainTextDescription = "Established in 1993, GLL is the largest UK-based charitable social enterprise delivering leisure, health and community services. Under the consumer facing brand Better, we operate 258 public Sports and Leisure facilities, 88 libraries, 10 children’s centres and 5 adventure playgrounds in partnership with 50 local councils, public agencies and sporting organisations. Better leisure facilities enjoy 46 million visitors a year and have more than 650,000 members.",
                DatasetDocumentationUrl = "https://docs.acmebooker.example.com/".ParseUrlOrNull(),
                BackgroundImageUrl = "https://data.better.org.uk/images/bg.jpg".ParseUrlOrNull(),
                OpenFeedBaseUrl = "https://customer.example.com/feed/".ParseUrlOrNull(),
                // Note that Booking Base URL is not yet implemented in the template
                BookingBaseUrl = "https://customer.example.com/api/openbooking/".ParseUrlOrNull(),
                PlatformName = "AcmeBooker",
                PlatformUrl = "https://acmebooker.example.com/".ParseUrlOrNull(),
                PlatformVersion = "2.0"
            };

            var supportedFeeds = new List<FeedType> {
                FeedType.SessionSeries,
                FeedType.ScheduledSession,
                FeedType.FacilityUse,
                FeedType.Slot,
                FeedType.CourseInstance
            };

            return Content(DatasetSiteGenerator.RenderSimpleDatasetSite(settings, supportedFeeds), "text/html");
        }
    }
}
