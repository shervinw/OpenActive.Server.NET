﻿using OpenActive.DatasetSite.NET;
using OpenActive.NET;
using OpenActive.Server.NET.OpenBookingHelper;
using OpenActive.Server.NET.StoreBooking;
using System;
using System.Collections.Generic;

namespace BookingSystem
{
    public static class EngineConfig
    {
        public static StoreBookingEngine CreateStoreBookingEngine(string baseUrl)
        {
            return new StoreBookingEngine(
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
                                OpportunityIdTemplate = "{+BaseUrl}scheduled-sessions/{SessionSeriesId}/events/{ScheduledSessionId}",
                                OfferIdTemplate =       "{+BaseUrl}scheduled-sessions/{SessionSeriesId}/events/{ScheduledSessionId}#/offers/{OfferId}",
                                Bookable = true
                            },
                            // Parent
                            new OpportunityIdConfiguration
                            {
                                OpportunityType = OpportunityType.SessionSeries,
                                AssignedFeed = OpportunityType.SessionSeries,
                                OpportunityIdTemplate = "{+BaseUrl}session-series/{SessionSeriesId}",
                                OfferIdTemplate =       "{+BaseUrl}session-series/{SessionSeriesId}#/offers/{OfferId}",
                                Bookable = false
                            }), 

                        new BookablePairIdTemplate<FacilityOpportunity> (
                            // Opportunity
                            new OpportunityIdConfiguration
                            {
                                OpportunityType = OpportunityType.FacilityUseSlot,
                                AssignedFeed = OpportunityType.FacilityUseSlot,
                                OpportunityIdTemplate = "{+BaseUrl}facility-uses/{FacilityUseId}/facility-use-slots/{SlotId}",
                                OfferIdTemplate =       "{+BaseUrl}facility-uses/{FacilityUseId}/facility-use-slots/{SlotId}#/offers/{OfferId}",
                                Bookable = true
                            },
                            // Parent
                            new OpportunityIdConfiguration
                            {
                                OpportunityType = OpportunityType.FacilityUse,
                                AssignedFeed = OpportunityType.FacilityUse,
                                OpportunityIdTemplate = "{+BaseUrl}facility-uses/{FacilityUseId}"
                            })/*,,

                        new BookablePairIdTemplate<ScheduledSessionOpportunity>(
                            // Opportunity
                            new OpportunityIdConfiguration
                            {
                                OpportunityType = OpportunityType.HeadlineEventSubEvent,
                                AssignedFeed = OpportunityType.HeadlineEvent,
                                OpportunityUriTemplate = "{+BaseUrl}headline-events/{HeadlineEventId}/events/{EventId}",
                                OfferUriTemplate =       "{+BaseUrl}headline-events/{HeadlineEventId}/events/{EventId}#/offers/{OfferId}",
                                Bookable = true
                            },
                            // Parent
                            new OpportunityIdConfiguration
                            {
                                OpportunityType = OpportunityType.HeadlineEvent,
                                AssignedFeed = OpportunityType.HeadlineEvent,
                                OpportunityUriTemplate = "{+BaseUrl}headline-events/{HeadlineEventId}",
                                OfferUriTemplate =       "{+BaseUrl}headline-events/{HeadlineEventId}#/offers/{OfferId}"
                            }),

                            new BookablePairIdTemplate<ScheduledSessionOpportunity>(
                            // Opportunity
                            new OpportunityIdConfiguration
                            {
                                OpportunityType = OpportunityType.CourseInstanceSubEvent,
                                AssignedFeed = OpportunityType.CourseInstance,
                                OpportunityUriTemplate = "{+BaseUrl}courses/{CourseId}/events/{EventId}",
                                OfferUriTemplate =       "{+BaseUrl}courses/{CourseId}/events/{EventId}#/offers/{OfferId}"
                            },
                            // Parent
                            new OpportunityIdConfiguration
                            {
                                OpportunityType = OpportunityType.CourseInstance,
                                AssignedFeed = OpportunityType.CourseInstance,
                                OpportunityUriTemplate = "{+BaseUrl}courses/{CourseId}",
                                OfferUriTemplate =       "{+BaseUrl}courses/{CourseId}#/offers/{OfferId}",
                                Bookable = true
                            }),

                        new BookablePairIdTemplate<ScheduledSessionOpportunity>(
                            // Opportunity
                            new OpportunityIdConfiguration
                            {
                                OpportunityType = OpportunityType.Event,
                                AssignedFeed = OpportunityType.Event,
                                OpportunityUriTemplate = "{+BaseUrl}events/{EventId}",
                                OfferUriTemplate =       "{+BaseUrl}events/{EventId}#/offers/{OfferId}",
                                Bookable = true
                            })*/
                    
                    },

                    JsonLdIdBaseUrl = new Uri(baseUrl + "api/identifiers/"),

                
                    // Multiple Seller Mode
                    SellerStore = new AcmeSellerStore(),
                    SellerIdTemplate = new SingleIdTemplate<SellerIdComponents>(
                        "{+BaseUrl}sellers/{SellerIdLong}"
                        ),

                    /*
                    // Single Seller Mode
                    SellerStore = new AcmeSellerStore(),
                    SellerIdTemplate = new SingleIdTemplate<SellerIdComponents>(
                        "{+BaseUrl}seller"
                        ),
                    HasSingleSeller = true,
                    */

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
                    },


                    // QUESTION: Would it be useful to have the Base URL auto-populated from the controller here?

                    // Note unlike IDs this one needs to match URL of the feed, from whatever is in the controller
                    OrdersFeedUrl = new Uri(baseUrl + "api/openbooking/orders-rpde"),

                    // Note unlike other IDs this one needs to be resolvable
                    // and must match the controller configuration
                    OrderBaseUrl = new Uri(baseUrl + "api/openbooking/"),
                    OrderIdTemplate = new OrderIdTemplate(
                        "{+BaseUrl}{OrderType}/{uuid}",
                        "{+BaseUrl}{OrderType}/{uuid}#/orderedItems/{OrderItemIdLong}"
                        ),

                    OrderFeedGenerator = new AcmeOrdersFeedRPDEGenerator()
                },
                new DatasetSiteGeneratorSettings
                {
                    // QUESTION: Do the Base URLs need to come from config, or should they be detected from the request?
                    OpenDataFeedBaseUrl = (baseUrl + "feeds/").ParseUrlOrNull(),
                    DatasetSiteUrl = (baseUrl + "openactive/").ParseUrlOrNull(),
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
                    OpenBookingAPIBaseUrl = (baseUrl + "api/openbooking/").ParseUrlOrNull(),
                },
                new StoreBookingEngineSettings
                {
                    // A list of the supported fields that are accepted by your system for guest checkout bookings
                    // These are reflected back to the broker
                    // Note that only E-mail address is required, as per Open Booking API spec
                    CustomerPersonSupportedFields = p => new Person {
                        Email = p.Email,
                        GivenName = p.GivenName,
                        FamilyName = p.FamilyName,
                        Telephone = p.Telephone
                    },
                    // A list of the supported fields that are accepted by your system for guest checkout bookings
                    // These are reflected back to the broker
                    // Note that only E-mail address is required, as per Open Booking API spec
                    CustomerOrganizationSupportedFields = o => new Organization {
                        Email = o.Email,
                        Name = o.Name,
                        Telephone = o.Telephone
                    },
                    // A list of the supported fields that are accepted by your system for broker details
                    // These are reflected back to the broker
                    // Note that storage of these details is entirely optional
                    BrokerSupportedFields = o => new Organization {
                        Name = o.Name,
                        Url = o.Url,
                        Telephone = o.Telephone
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
                                Name = "Privacy Policy",
                                Url = new Uri("https://example.com/privacy.html"),
                                RequiresExplicitConsent = false
                            }
                        }
                    },
                    // A list of the supported fields that are accepted by your system for payment details
                    // These are reflected back to the broker
                    PaymentSupportedFields = o => new Payment
                    {
                        Name = o.Name,
                        Identifier = o.Identifier,
                        AccountId = o.AccountId,
                        PaymentProviderId = o.PaymentProviderId
                    },
                    // List of _bookable_ opportunity types and which store to route to for each
                    OpportunityStoreRouting = new Dictionary<IOpportunityStore, List<OpportunityType>> {
                        {
                            new SessionStore(), new List<OpportunityType> { OpportunityType.ScheduledSession }
                        }
                    },
                    OrderStore = new AcmeOrderStore(),
                });
        }
    }
}
