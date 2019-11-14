using System;
using System.Collections.Generic;
using System.Linq;
using OpenActive.Server.NET.OpenBookingHelper;
using System.Threading.Tasks;
using OpenActive.NET.Rpde.Version1;
using BookingSystem.FakeDatabase;
using OpenActive.NET;

namespace BookingSystem.AspNetFramework
{
    public class AcmeOrdersFeedRPDEGenerator : OrdersRPDEFeedModifiedTimestampAndID
    {
        //public override string FeedPath { get; protected set; } = "example path override";

        // TODO: Update to use fake orders database
        protected override List<RpdeItem> GetRPDEItems(long? afterTimestamp, string afterId)
        {
            var query = from classes in FakeBookingSystem.Database.Classes
                        join occurances in FakeBookingSystem.Database.Occurrences on classes.Id equals occurances.ClassId
                        where !afterTimestamp.HasValue || classes.Modified.ToUnixTimeMilliseconds() > afterTimestamp ||
                        (classes.Modified.ToUnixTimeMilliseconds() == afterTimestamp && classes.Id > long.Parse(afterId))
                        group occurances by classes into thisClass
                        orderby thisClass.Key.Modified, thisClass.Key.Id
                        select new RpdeItem
                        {
                            Kind = RpdeKind.ScheduledSession,
                            Id = thisClass.Key.Id,
                            Modified = thisClass.Key.Modified.ToUnixTimeMilliseconds(),
                            State = thisClass.Key.Deleted ? RpdeState.Deleted : RpdeState.Updated,
                            Data = thisClass.Key.Deleted ? null : new Order
                            {
                                // QUESTION: Should the this.IdTemplate and this.BaseUrl be passed in each time rather than set on
                                // the parent class? Current thinking is it's more extensible on parent class as function signature remains
                                // constant as power of configuration through underlying class grows (i.e. as new properties are added)
                                Id = this.RenderOrderId(OrderType.Order, thisClass.Key.Id.ToString()),
                                Name = thisClass.Key.Title,
                                OrderedItem = thisClass.Select(orderItem => new OrderItem
                                {
                                    Identifier = orderItem.Start.ToString()

                                }).ToList()
                            }
                        };
            var list = query.Take(this.RPDEPageSize).ToList();
            return list;
        }
    }
}
