using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenActive.NET.Rpde.Version1;
using Newtonsoft.Json;
using OpenActive.Server.NET;
using OpenActive.DatasetSite.NET;
using BookingSystem.AspNetCore.Feeds;
using OpenActive.NET;
using Newtonsoft.Json.Converters;

namespace BookingSystem.AspNetCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // TODO: Authentication disabled for now
            //services.AddAuthentication(AzureADB2CDefaults.BearerAuthenticationScheme)
            //    .AddAzureADB2CBearer(options => Configuration.Bind("AzureAdB2C", options));
            services.AddMvc().AddJsonOptions(options => {
                options.SerializerSettings.Converters = new List<JsonConverter>()
                {
                    // This enables every relevant response to be rendered to JSON-LD by OpenActive.NET
                    // TODO: Document use of this
                    // TODO: Is there a way we can not require this? Just output a string from the library direct? Less chances of rendering issues and misuse?
                    new OpenActiveThingConverter(),
                    new ValuesConverter(),
                    new StringEnumConverter()
                };
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);


            //QUESTION: Should all these be configured here? Are we using the pattern correctly?
            //https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/dependency-injection?view=aspnetcore-3.0

            services.AddSingleton<IBookingEngine>(sp => new StoreBookingEngine(new BookingEngineSettings
            {
                // This assigns the ID pattern used for each ID
                IdConfiguration = new List<IBookablePairIdTemplate> {
                    // Note that ScheduledSession is the only opportunity type that allows offer inheritance  
                    new BookablePairIdTemplateWithOfferInheritance<ScheduledSessionOpportunity>(
                        // Opportunity
                        new OpportunityIdConfiguration
                        {
                            OpportunityType = OpportunityType.ScheduledSession,
                            AssignedFeed = OpportunityType.ScheduledSession,
                            OpportunityIdTemplate = "{+BaseUrl}api/scheduled-sessions/{SessionSeriesId}/events/{ScheduledSessionId}",
                            OfferIdTemplate =       "{+BaseUrl}api/scheduled-sessions/{SessionSeriesId}/events/{ScheduledSessionId}#/offers/{OfferId}",
                            Bookable = true
                        },
                        // Parent
                        new OpportunityIdConfiguration
                        {
                            OpportunityType = OpportunityType.SessionSeries,
                            AssignedFeed = OpportunityType.SessionSeries,
                            OpportunityIdTemplate = "{+BaseUrl}api/session-series/{SessionSeriesId}",
                            OfferIdTemplate =       "{+BaseUrl}api/session-series/{SessionSeriesId}#/offers/{OfferId}",
                            Bookable = true
                        }) /*,

                    new BookablePairIdTemplate<ScheduledSessionOpportunity>(
                        // Opportunity
                        new OpportunityIdConfiguration
                        {
                            OpportunityType = OpportunityType.FacilityUseSlot,
                            AssignedFeed = OpportunityType.FacilityUseSlot,
                            OpportunityUriTemplate = "{+BaseUrl}api/facility-uses/{FacilityUseId}/facility-use-slots/{SlotId}",
                            OfferUriTemplate =       "{+BaseUrl}api/facility-uses/{FacilityUseId}/facility-use-slots/{SlotId}#/offers/{OfferId}",
                            Bookable = true
                        },
                        // Parent
                        new OpportunityIdConfiguration
                        {
                            OpportunityType = OpportunityType.FacilityUse,
                            AssignedFeed = OpportunityType.FacilityUse,
                            OpportunityUriTemplate = "{+BaseUrl}api/facility-uses/{FacilityUseId}"
                        }),

                    new BookablePairIdTemplate<ScheduledSessionOpportunity>(
                        // Opportunity
                        new OpportunityIdConfiguration
                        {
                            OpportunityType = OpportunityType.HeadlineEventSubEvent,
                            AssignedFeed = OpportunityType.HeadlineEvent,
                            OpportunityUriTemplate = "{+BaseUrl}api/headline-events/{HeadlineEventId}/events/{EventId}",
                            OfferUriTemplate =       "{+BaseUrl}api/headline-events/{HeadlineEventId}/events/{EventId}#/offers/{OfferId}",
                            Bookable = true
                        },
                        // Parent
                        new OpportunityIdConfiguration
                        {
                            OpportunityType = OpportunityType.HeadlineEvent,
                            AssignedFeed = OpportunityType.HeadlineEvent,
                            OpportunityUriTemplate = "{+BaseUrl}api/headline-events/{HeadlineEventId}",
                            OfferUriTemplate =       "{+BaseUrl}api/headline-events/{HeadlineEventId}#/offers/{OfferId}"
                        }),

                     new BookablePairIdTemplate<ScheduledSessionOpportunity>(
                        // Opportunity
                        new OpportunityIdConfiguration
                        {
                            OpportunityType = OpportunityType.CourseInstanceSubEvent,
                            AssignedFeed = OpportunityType.CourseInstance,
                            OpportunityUriTemplate = "{+BaseUrl}api/courses/{CourseId}/events/{EventId}",
                            OfferUriTemplate =       "{+BaseUrl}api/courses/{CourseId}/events/{EventId}#/offers/{OfferId}"
                        },
                        // Parent
                        new OpportunityIdConfiguration
                        {
                            OpportunityType = OpportunityType.CourseInstance,
                            AssignedFeed = OpportunityType.CourseInstance,
                            OpportunityUriTemplate = "{+BaseUrl}api/courses/{CourseId}",
                            OfferUriTemplate =       "{+BaseUrl}api/courses/{CourseId}#/offers/{OfferId}",
                            Bookable = true
                        }),

                    new BookablePairIdTemplate<ScheduledSessionOpportunity>(
                        // Opportunity
                        new OpportunityIdConfiguration
                        {
                            OpportunityType = OpportunityType.Event,
                            AssignedFeed = OpportunityType.Event,
                            OpportunityUriTemplate = "{+BaseUrl}api/events/{EventId}",
                            OfferUriTemplate =       "{+BaseUrl}api/events/{EventId}#/offers/{OfferId}",
                            Bookable = true
                        })*/
                    
                },

                JsonLdIdBaseUrl = new Uri("https://example.com/api/identifiers/"),
                OrderBaseUrl = new Uri("https://example.com/api/orders/"),

                OrderIdTemplate = new SingleIdTemplate<OrderId>(
                    "{+BaseUrl}api/{Mode}/{OrderId}"
                    ),

                OpenDataFeeds = new Dictionary<OpportunityType, RPDEFeedGenerator> {
                    {
                        OpportunityType.ScheduledSession, new AcmeScheduledSessionRPDEGenerator()
                    },
                    {
                        OpportunityType.SessionSeries, new AcmeSessionSeriesRPDEGenerator()
                    }
                }
            },
            new DatasetSiteGeneratorSettings
            {
                OpenDataFeedBaseUrl = "https://localhost:44307/feeds/".ParseUrlOrNull(),
                DatasetSiteUrl = "https://localhost:44307/openactive/".ParseUrlOrNull(),
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
                DateFirstPublished = new DateTimeOffset(new DateTime(2019, 01, 14)),
                OpenBookingAPIBaseUrl = "https://localhost:44307/api/openbooking/".ParseUrlOrNull(),
            },
            new AcmeStore()
            ));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
