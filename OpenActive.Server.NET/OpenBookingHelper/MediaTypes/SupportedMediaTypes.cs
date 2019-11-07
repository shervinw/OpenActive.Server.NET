using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET.OpenBookingHelper
{
    public static class MediaTypeNames
    {
        public static class OpenBooking
        {
            public const string Version1 = "application/vnd.openactive.booking+json; version=1";
        }
        public static class RealtimePagedDataExchange
        {
            public const string Version1 = "application/vnd.openactive.rpde+json; version=1";
        }
    }
}
