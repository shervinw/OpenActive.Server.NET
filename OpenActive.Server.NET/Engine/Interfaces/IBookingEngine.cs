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
        void CreateTestData(Event @event);
        void DeleteTestData(string name);
        TOrder ProcessFlowRequest<TOrder>(FlowStage stage, OrderId orderId, TOrder orderQuote, TaxPayeeRelationship taxPayeeRelationship, SingleValues<Organization, Person> payer) where TOrder : Order;
    }
}