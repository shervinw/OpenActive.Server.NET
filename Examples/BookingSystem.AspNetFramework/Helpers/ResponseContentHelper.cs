using System.Net.Http;
using System.Net.Http.Headers;

namespace BookingSystem.AspNetFramework.Helpers
{
    public static class ResponseContentHelper
    {
        public static HttpResponseMessage GetContentResult(this OpenActive.Server.NET.OpenBookingHelper.ResponseContent response)
        {
            var resp = new HttpResponseMessage
            {
                Content = new StringContent(response.Content ?? ""),
                StatusCode = response.StatusCode
            };
            resp.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(response.ContentType);
            return resp;
        }
    }
}
