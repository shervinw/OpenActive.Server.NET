using System;
using OpenActive.NET;
using OpenActive.NET.Rpde.Version1;

namespace OpenActive.Server.NET
{
    public interface IBookingEngine
    {
        void DeleteOrder(string uuid);
        RpdePage GetOpenDataRPDEPageForFeed(string feedname, long? afterTimestamp, string afterId, long? afterChangeNumber);
        Order ProcessOrderCreationB(string uuid, Order order);
        OrderQuote ProcessCheckpoint1(string uuid, OrderQuote orderQuote);
        OrderQuote ProcessCheckpoint2(string uuid, OrderQuote orderQuote);
        void ProcessOrderUpdate(string uuid, Order order);
        string RenderDatasetSite();
        void CreateTestData(string opportunityType, Event @event);
        void DeleteTestData(string opportunityType, string name);

        TOrder ProcessFlowRequest<TOrder>(BookingFlowContext<TOrder> request) where TOrder : Order;
    }
}