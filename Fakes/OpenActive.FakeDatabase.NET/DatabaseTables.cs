using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.FakeDatabase.NET
{

    public enum BrokerRole { AgentBroker, ResellerBroker, NoBroker }

    public enum BookingStatus { CustomerCancelled, SellerCancelled, Confirmed, Attended }


    public abstract class Table
    {
        public long Id { get; set; }
        public bool Deleted { get; set; } = false;
        public DateTimeOffset Modified { get; set; } = DateTimeOffset.Now;
    }

    public class ClassTable : Table
    {
        public string TestDatasetIdentifier { get; set; }
        public string Title { get; set; }
        public long SellerId { get; set; }
        public decimal? Price { get; set; }
    }



    public class OccurrenceTable : Table
    {
        public string TestDatasetIdentifier { get; set; }
        public long ClassId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public long TotalSpaces { get; set; }
        public long LeasedSpaces { get; set; }
        public long RemainingSpaces { get; set; }
    }


    public class OrderItemsTable : Table
    {
        public string ClientId { get; internal set; }
        public string OpportunityJsonLdType { get; set; }
        public string OpportunityJsonLdId { get; set; }
        public string OfferJsonLdId { get; set; }
        public string OrderId { get; set; }
        public long OccurrenceId { get; set; }
        public BookingStatus Status { get; set; }
        public decimal Price { get; set; }
    }


    public class OrderTable : Table
    {
        public string ClientId { get; set; }
        public new string Id { get; set; }
        public long SellerId { get; set; }
        public bool CustomerIsOrganization { get; set; }
        public BrokerRole BrokerRole { get; set; }
        public string BrokerName { get; set; }
        public string CustomerEmail { get; set; }
        public string PaymentIdentifier { get; set; }
        public decimal TotalOrderPrice { get; set; }
        public bool IsLease { get; set; }
        public DateTimeOffset LeaseExpires { get; set; }
        public bool VisibleInFeed { get; set; }
    }


    public class SellerTable : Table
    {
        public string Name { get; set; }
        public bool IsIndividual { get; set; }

    }
}
