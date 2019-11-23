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
        ResponseContent DeleteOrder(string uuid);
        ResponseContent DeleteOrderQuote(string uuid);
        ResponseContent GetOpenDataRPDEPageForFeed(string feedname, long? afterTimestamp, string afterId, long? afterChangeNumber);
        ResponseContent GetOpenDataRPDEPageForFeed(string feedname, string afterTimestamp, string afterId, string afterChangeNumber);
        ResponseContent ProcessOrderCreationB(string uuid, string orderJson);
        ResponseContent ProcessCheckpoint1(string uuid, string orderQuoteJson);
        ResponseContent ProcessCheckpoint2(string uuid, string orderQuoteJson);
        ResponseContent ProcessOrderUpdate(string uuid, string orderJson);
        ResponseContent RenderDatasetSite();
        ResponseContent CreateTestData(string opportunityType, string eventJson);
        ResponseContent DeleteTestData(string opportunityType, string name);
        ResponseContent GetOrdersRPDEPageForFeed(string authtoken, string afterTimestamp, string afterId, string afterChangeNumber);
        ResponseContent GetOrdersRPDEPageForFeed(string authtoken, long? afterTimestamp, string afterId, long? afterChangeNumber);
    }
}