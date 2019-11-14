
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BookingSystem.AspNetFramework.Helpers
{
    public static class ResponseContentHelper
    {
        public static HttpResponseMessage GetContentResult(this OpenActive.Server.NET.OpenBookingHelper.ResponseContent response)
        {
            var resp = new HttpResponseMessage();
            resp.Content = new StringContent(response.Content);
            resp.StatusCode = response.StatusCode;
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(response.ContentTypeName);
            if (response.ContentTypeContainsParameters)
                resp.Content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue(response.ContentTypeParameter.Key, response.ContentTypeParameter.Value));

            return resp;
        }
    }
}
