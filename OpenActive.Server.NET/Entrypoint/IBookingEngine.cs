using System;
using Newtonsoft.Json.Linq;
using OpenActive.NET;
using OpenActive.NET.Rpde.Version1;
using OpenActive.Server.NET.OpenBookingHelper;

namespace OpenActive.Server.NET
{
    /// <summary>
    /// This is the interface between the BookingEngine and the Web Framework (e.g. ASP.NET Core).
    /// 
    /// Note that this interface expects JSON requests to be supplied as strings, and provides JSON responses as strings.
    /// This ensures that deserialisation is always correct, regardless of the configuration of the web framework.
    /// It also removes the need to expose OpenActive (de)serialisation settings and parsers to the implementer, and makes
    /// this interface more maintainble as OpenActive.NET will likely upgrade to use the new System.Text.Json in time.
    /// </summary>
    public interface IBookingEngine
    {
        // These endpoints are fully open
        ResponseContent RenderDatasetSite();
        ResponseContent GetOpenDataRPDEPageForFeed(string feedname, long? afterTimestamp, string afterId, long? afterChangeNumber);
        ResponseContent GetOpenDataRPDEPageForFeed(string feedname, string afterTimestamp, string afterId, string afterChangeNumber);

        // These endpoints are authenticated by seller credentials (OAuth Authorization Code Grant)
        ResponseContent ProcessCheckpoint1(string bookingPartnerClientId, Uri sellerId, string uuid, string orderQuoteJson);
        ResponseContent ProcessCheckpoint2(string bookingPartnerClientId, Uri sellerId, string uuid, string orderQuoteJson);
        ResponseContent ProcessOrderCreationB(string bookingPartnerClientId, Uri sellerId, string uuid, string orderJson);
        ResponseContent DeleteOrder(string bookingPartnerClientId, Uri sellerId, string uuid);
        ResponseContent DeleteOrderQuote(string bookingPartnerClientId, Uri sellerId, string uuid);
        ResponseContent ProcessOrderUpdate(string bookingPartnerClientId, Uri sellerId, string uuid, string orderJson);

        // These endpoints are authenticated by client credentials (OAuth Client Credentials Grant)
        ResponseContent CreateTestData(string bookingPartnerClientId, string opportunityType, string eventJson);
        ResponseContent DeleteTestData(string bookingPartnerClientId, string opportunityType, string name);
        ResponseContent GetOrdersRPDEPageForFeed(string bookingPartnerClientId, string afterTimestamp, string afterId, string afterChangeNumber);
        ResponseContent GetOrdersRPDEPageForFeed(string bookingPartnerClientId, long? afterTimestamp, string afterId, long? afterChangeNumber);
    }
}