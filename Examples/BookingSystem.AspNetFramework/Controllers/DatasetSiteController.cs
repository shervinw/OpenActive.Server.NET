using OpenActive.Server.NET.Helpers;
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
                OrganisationName = "Better",
                DatasetSiteUrl = "https://halo-odi.legendonlineservices.co.uk/openactive/",
                DatasetSiteDocumentationUrl = "https://github.com/gll-better/opendata",
                DocumentationUrl = "https://docs.acmebooker.example.com/",
                LegalEntity = "GLL",
                PlainTextDescription = "Established in 1993, GLL is the largest UK-based charitable social enterprise delivering leisure, health and community services. Under the consumer facing brand Better, we operate 258 public Sports and Leisure facilities, 88 libraries, 10 children’s centres and 5 adventure playgrounds in partnership with 50 local councils, public agencies and sporting organisations. Better leisure facilities enjoy 46 million visitors a year and have more than 650,000 members.",
                Email = "info@better.org.uk",
                Url = "https://www.better.org.uk/",
                LogoUrl = "http://data.better.org.uk/images/logo.png",
                BackgroundImageUrl = "https://data.better.org.uk/images/bg.jpg",
                BaseUrl = "https://customer.example.com/feed/",
                PlatformName = "AcmeBooker",
                PlatformUrl = "https://acmebooker.example.com/"
            };

            return Content(DatasetSiteGenerator.RenderSimpleDatasetSite(settings), "text/html");
        }
    }
}
