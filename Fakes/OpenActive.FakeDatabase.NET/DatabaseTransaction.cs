using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenActive.FakeDatabase.NET
{
    public class DatabaseTransaction : IDisposable
    {
        public DatabaseTransaction(FakeDatabase database)
        {
            sourceDatabase = database;
            Database = new FakeDatabase
            {
                Classes = database.Classes.Select(x => new ClassTable
                {
                    Id = x.Id,
                    Deleted = x.Deleted,
                    Modified = x.Modified,
                    Title = x.Title,
                    Price = x.Price,
                    SellerId = x.SellerId
                }).ToList(),

                Occurrences = database.Occurrences.Select(x => new OccurrenceTable
                {
                    Id = x.Id,
                    Deleted = x.Deleted,
                    Modified = x.Modified,
                    ClassId = x.ClassId,
                    Start = x.Start,
                    End = x.End,
                    LeasedSpaces = x.LeasedSpaces,
                    RemainingSpaces = x.RemainingSpaces,
                    TotalSpaces = x.TotalSpaces
                }).ToList(),

                OrderItems = database.OrderItems.Select(x => new OrderItemsTable
                {
                    ClientId = x.ClientId,
                    Id = x.Id,
                    Deleted = x.Deleted,
                    Modified = x.Modified,
                    Status = x.Status,
                    Price = x.Price,
                    OccurrenceId = x.OccurrenceId,
                    OfferJsonLdId = x.OfferJsonLdId,
                    OpportunityJsonLdId = x.OpportunityJsonLdId,
                    OpportunityJsonLdType = x.OpportunityJsonLdType,
                    OrderId = x.OrderId
                }).ToList(),

                Orders = database.Orders.Select(x => new OrderTable
                {
                    ClientId = x.ClientId,
                    Id = x.Id,
                    Deleted = x.Deleted,
                    Modified = x.Modified,
                    BrokerName = x.BrokerName,
                    BrokerRole = x.BrokerRole,
                    CustomerEmail = x.CustomerEmail,
                    CustomerIsOrganization = x.CustomerIsOrganization,
                    LeaseExpires = x.LeaseExpires,
                    IsLease = x.IsLease,
                    PaymentIdentifier = x.PaymentIdentifier,
                    SellerId = x.SellerId,
                    TotalOrderPrice = x.TotalOrderPrice,
                    VisibleInFeed = x.VisibleInFeed
                }).ToList(),

                Sellers = database.Sellers.Select(x => new SellerTable
                {
                    Id = x.Id,
                    Deleted = x.Deleted,
                    Modified = x.Modified,
                    Name = x.Name, 
                    IsIndividual = x.IsIndividual
                }).ToList(),
            };
        }

        public void CommitTransaction()
        {
            sourceDatabase.Classes = this.Database.Classes;
            sourceDatabase.Occurrences = this.Database.Occurrences;
            sourceDatabase.OrderItems = this.Database.OrderItems;
            sourceDatabase.Orders = this.Database.Orders;
            sourceDatabase.Sellers = this.Database.Sellers;
        }

        public void Dispose()
        {
            // No implementation required if not disposable
        }

        private FakeDatabase sourceDatabase;
        public FakeDatabase Database { get; set; }
    }

}
