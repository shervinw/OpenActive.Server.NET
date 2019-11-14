using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text;

namespace OpenActive.Server.NET.OpenBookingHelper
{
    public static class MediaTypeNames
    {
        public static class OpenBooking
        {
            public const string Version1 = "application/vnd.openactive.booking+json; version=1";
            public const string Version1Name = "application/vnd.openactive.booking+json";
            public static readonly NameValueHeaderValue Version1Parameter = new NameValueHeaderValue("version", "1");
        }
        public static class RealtimePagedDataExchange
        {
            public const string Version1 = "application/vnd.openactive.rpde+json; version=1";
        }
    }

}
