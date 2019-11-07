using OpenActive.NET;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace OpenActive.Server.NET.OpenBookingHelper
{
    /// <summary>
    /// This is a .NET version agnostic representation of a result from the Booking Engine
    /// It includes a .NET version-specific helper functions that simplify integration with .NET Framework MVC
    /// A .NET Core MVC helper extension is also available at TODO: [Add URL]
    /// </summary>
    public class ResponseContent
    {
        public static ResponseContent HtmlResponse(string content)
        {
            return new ResponseContent
            {
                Content = content,
                ContentType = System.Net.Mime.MediaTypeNames.Text.Html,
                StatusCode = HttpStatusCode.OK
            };
        }

        public static ResponseContent OpenBookingResponse (string content)
        {
            return new ResponseContent
            {
                Content = content,
                ContentType = MediaTypeNames.OpenBooking.Version1,
                StatusCode = HttpStatusCode.OK
            };
        }

        public static ResponseContent OpenBookingErrorResponse(string content, HttpStatusCode statusCode)
        {
            return new ResponseContent
            {
                Content = content,
                ContentType = MediaTypeNames.OpenBooking.Version1,
                StatusCode = statusCode
            };
        }

        public static ResponseContent RpdeResponse(string content)
        {
            return new ResponseContent
            {
                Content = content,
                ContentType = MediaTypeNames.RealtimePagedDataExchange.Version1,
                StatusCode = HttpStatusCode.OK
            };
        }

        //
        // Summary:
        //     A string representing the JSON response
        public string Content { get; internal set; }
        //
        // Summary:
        //     The default Open Booking API content type for the version of Open Booking supported by the SDK
        public string ContentType { get; internal set; } = MediaTypeNames.OpenBooking.Version1;
        //
        // Summary:
        //     The intended HTTP status code of the response
        public HttpStatusCode StatusCode { get; internal set; } = HttpStatusCode.OK;

        /// <summary>
        /// This is provided as a convenience to .NET Framework users, to create a standards compliant JSON output.
        /// </summary>
        /// <example>
        /// var resp = req.CreateResponse(HttpStatusCode.OK);
        /// resp.Content = bookingEngine.{method}.ToStringContent();
        /// </example>
        public System.Net.Http.StringContent StringContent
        {
            get
            {
                return new StringContent(this.Content, Encoding.UTF8, this.ContentType);
            }
        }

        public override string ToString()
        {
            return this.Content;
        }
    }
}
