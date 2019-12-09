using BookingSystem.AspNetFramework.Helpers;
using BookingSystem.AspNetFramework.Controllers;
using Microsoft.Extensions.DependencyInjection;
using OpenActive.DatasetSite.NET;
using OpenActive.NET;
using OpenActive.Server.NET;
using OpenActive.Server.NET.OpenBookingHelper;
using OpenActive.Server.NET.StoreBooking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using BookingSystem.AspNetFramework.Utils;

namespace BookingSystem.AspNetFramework
{
    public class ServiceConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Formatters.Add(new OpenBookingInputFormatter());

            const string BaseUrl = "https://localhost:44349/";

            var services = new ServiceCollection();
            services.AddTransient<DatasetSiteController>();
            services.AddTransient<OpenDataController>();
            services.AddTransient<OpenBookingController>();
            services.AddSingleton<IBookingEngine>(sp => EngineConfig.CreateStoreBookingEngine(BaseUrl));

            var resolver = new DependencyResolver(services.BuildServiceProvider(true));
            config.DependencyResolver = resolver;
        }
    }
}