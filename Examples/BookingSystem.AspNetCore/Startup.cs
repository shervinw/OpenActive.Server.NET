using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters.Json;
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
using OpenActive.NET;
using Newtonsoft.Json.Converters;
using OpenActive.Server.NET.StoreBooking;
using OpenActive.Server.NET.OpenBookingHelper;
using BookingSystem.AspNetCore.Helpers;

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
            services
                .AddMvc()
                .AddMvcOptions(options => options.InputFormatters.Insert(0, new OpenBookingInputFormatter()))
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);


            //QUESTION: Should all these be configured here? Are we using the pattern correctly?
            //https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/dependency-injection?view=aspnetcore-3.0

            services.AddSingleton<IBookingEngine>(sp => new StoreBookingEngine(
            new BookingEngineSettings
            {
                // This assigns the ID pattern used for each ID
                IdConfiguration = new List<IBookablePairIdTemplate> {
                    // Note that ScheduledSession is the only opportunity type that allows offer inheritance  
                    new BookablePairIdTemplateWithOfferInheritance<SessionOpportunity> (
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
                            Bookable = false
                        }), 

                    new BookablePairIdTemplate<FacilityOpportunity> (
                        // Opportunity
                        new OpportunityIdConfiguration
                        {
                            OpportunityType = OpportunityType.FacilityUseSlot,
                            AssignedFeed = OpportunityType.FacilityUseSlot,
                            OpportunityIdTemplate = "{+BaseUrl}api/facility-uses/{FacilityUseId}/facility-use-slots/{SlotId}",
                            OfferIdTemplate =       "{+BaseUrl}api/facility-uses/{FacilityUseId}/facility-use-slots/{SlotId}#/offers/{OfferId}",
                            Bookable = true
                        },
                        // Parent
                        new OpportunityIdConfiguration
                        {
                            OpportunityType = OpportunityType.FacilityUse,
                            AssignedFeed = OpportunityType.FacilityUse,
                            OpportunityIdTemplate = "{+BaseUrl}api/facility-uses/{FacilityUseId}"
                        })/*,,

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

                // QUESTION: Would it be useful to have the Base URL auto-populated from the controller here?

                // Note unlike IDs this one needs to match URL of the feed, from whatever is in the controller
                OrdersFeedUrl = new Uri("https://localhost:44307/api/openbooking/orders-rpde"),

                // Note unlike other IDs this one needs to be resolvable
                OrderBaseUrl = new Uri("https://localhost:44307/api/openbooking/orders/"),
                OrderIdTemplate = new OrderIdTemplate(
                    "{+BaseUrl}api/{OrderType}/{uuid}",
                    "{+BaseUrl}api/{OrderType}/{uuid}#/orderedItems/{OrderItemIdString}"
                    ),

                OrderFeedGenerator = new AcmeOrdersFeedRPDEGenerator(),

                SellerIdTemplate = new SingleIdTemplate<SellerIdComponents>(
                    "{+BaseUrl}api/sellers/{SellerIdString}"
                    ),

                OpenDataFeeds = new Dictionary<OpportunityType, IOpportunityDataRPDEFeedGenerator> {
                    {
                        OpportunityType.ScheduledSession, new AcmeScheduledSessionRPDEGenerator()
                    },
                    {
                        OpportunityType.SessionSeries, new AcmeSessionSeriesRPDEGenerator()
                    },
                    {
                        OpportunityType.FacilityUse, new AcmeFacilityUseRPDEGenerator()
                    }
                    ,
                    {
                        OpportunityType.FacilityUseSlot, new AcmeFacilityUseSlotRPDEGenerator()
                    }
                }
            },
            new DatasetSiteGeneratorSettings
            {
                // QUESTION: Do the Base URLs need to come from config, or should they be detected from the request?
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
            new StoreBookingEngineSettings
            {
                // A list of the supported fields that are accepted by your system for guest checkout bookings
                // These are reflected back to the broker
                // Note that only E-mail address is required, as per Open Booking API spec
                CustomerPersonSupportedFields = new List<string>()
                {
                    nameof(Person.Email),
                    nameof(Person.GivenName),
                    nameof(Person.FamilyName),
                    nameof(Person.Telephone)
                },
                // A list of the supported fields that are accepted by your system for guest checkout bookings
                // These are reflected back to the broker
                // Note that only E-mail address is required, as per Open Booking API spec
                CustomerOrganizationSupportedFields = new List<string>()
                {
                    nameof(Organization.Email),
                    nameof(Organization.Name),
                    nameof(Organization.Telephone)
                },
                // A list of the supported fields that are accepted by your system for broker details
                // These are reflected back to the broker
                // Note that storage of these details is entirely optional
                BrokerSupportedFields = new List<string>()
                {
                    nameof(Organization.Name),
                    nameof(Organization.Url),
                    nameof(Organization.Telephone)
                },
                // Details of your booking system, complete with an customer-facing terms and conditions
                BookingServiceDetails = new BookingService
                {
                    Name = "Acme booking system",
                    Url = new Uri("https://example.com"),
                    TermsOfService = new List<Terms>
                    {
                        new PrivacyPolicy
                        {
                            Url = new Uri("https://example.com/privacy.html")
                        }
                    }
                },
                // List of _bookable_ opportunity types and which store to route to for each
                OpenBookingStoreRouting = new Dictionary<IOpportunityStore, List<OpportunityType>> {
                    {
                        new SessionStore(), new List<OpportunityType> { OpportunityType.ScheduledSession }
                    }
                }
            }));
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
