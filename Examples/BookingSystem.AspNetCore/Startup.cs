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
            services.AddAuthentication(AzureADB2CDefaults.BearerAuthenticationScheme)
                .AddAzureADB2CBearer(options => Configuration.Bind("AzureAdB2C", options));
            services.AddMvc().AddJsonOptions(options => {
                options.SerializerSettings.Converters = new List<JsonConverter>()
                {
                    // This enables every relevant response to be rendered to JSON-LD by OpenActive.NET
                    // TODO: Document use of this
                    new OpenActiveThingConverter()
                };
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);


            //QUESTION: Should all these be configured here? Are we using the pattern correctly?
            //https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/dependency-injection?view=aspnetcore-3.0

            services.AddSingleton<IBookingEngine>(sp => new StoreBookingEngine(new BookingEngineSettings
            {
                // This assigns the ID pattern used for each ID
                IdConfiguration = new Dictionary<BookableOpportunityClass, IBookablePairIdTemplate> {
                    {
                        // Note that ScheduledSession is the only opportunity type that allows offer inheritance  
                        BookableOpportunityClass.ScheduledSession,
                        new BookablePairIdTemplateWithOfferInheritance<ScheduledSessionOpportunity>(
                            "{+BaseUrl}api/scheduled-sessions/{SessionSeriesId}/events/{ScheduledSessionId}",
                            "{+BaseUrl}api/scheduled-sessions/{SessionSeriesId}/events/{ScheduledSessionId}#/offers/{OfferId}",
                            "{+BaseUrl}api/scheduled-sessions/{SessionSeriesId}",
                            "{+BaseUrl}api/scheduled-sessions/{SessionSeriesId}#/offers/{OfferId}"
                            )
                    },
                    {
                        BookableOpportunityClass.Slot,
                        new BookablePairIdTemplate<SlotOpportunity>(
                            "{+BaseUrl}api/facility-uses/{FacilityUseId}/slots/{SlotId}",
                            "{+BaseUrl}api/facility-uses/{FacilityUseId}/slots/{SlotId}#/offers/{OfferId}",
                            "{+BaseUrl}api/facility-uses/{FacilityUseId}"
                            )
                    },
                    {
                        BookableOpportunityClass.Event,
                        new BookablePairIdTemplate<SlotOpportunity>(
                            "{+BaseUrl}api/facility-uses/{FacilityUseId}/slots/{SlotId}",
                            "{+BaseUrl}api/facility-uses/{FacilityUseId}/slots/{SlotId}#/offers/{OfferId}",
                            "{+BaseUrl}api/facility-uses/{FacilityUseId}"
                            )
                    }
                },
        
                OrderBaseUrl = new Uri("https://example.com/api/orders/"),

                OrderIdTemplate = new SingleIdTemplate<OrderId>(
                    "{+BaseUrl}api/scheduled-sessions/{SessionSeriesId}/events/{ScheduledSessionId}"
                    ),

                OpenDataFeeds = new Dictionary<FeedType, RPDEFeedGenerator> {
                    {
                        FeedType.ScheduledSession, new AcmeScheduledSessionRPDEGenerator() // ID, ParentID
                    }
                }
            },
            new DatasetSiteGeneratorSettings
            {
                OpenDataFeedBaseUrl = "https://customer.example.com/feed/".ParseUrlOrNull(),
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
                DateFirstPublished = new DateTimeOffset(new DateTime(2019, 01, 14)),
                OpenBookingAPIBaseUrl = "https://customer.example.com/api/openbooking/".ParseUrlOrNull(),
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
