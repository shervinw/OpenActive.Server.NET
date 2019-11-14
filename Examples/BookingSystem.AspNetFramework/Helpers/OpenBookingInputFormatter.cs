// using Microsoft.AspNetCore.Mvc.Formatters;
using OpenActive.NET;
using OpenActive.Server.NET.OpenBookingHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BookingSystem.AspNetCore.Helpers
{
    public class OpenBookingInputFormatter : MediaTypeFormatter
    {
        public OpenBookingInputFormatter()
        {
            var mediaType = new MediaTypeHeaderValue(MediaTypeNames.OpenBooking.Version1Name);
            mediaType.Parameters.Add(MediaTypeNames.OpenBooking.Version1Parameter);
            this.SupportedMediaTypes.Add(mediaType);
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override bool CanWriteType(Type type)
        {
            return typeof(string) == type;
        }

    }
}
