using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Linq;
using Bogus;

namespace OpenActive.FakeDatabase.NET
{
    /// <summary>
    /// This class models the database schema within an actual booking system.
    /// It is designed to simulate the database that would be available in a full implementation.
    /// </summary>
    public static class FakeBookingSystem
    {
        /// <summary>
        /// The Database is created as static, to simulate the persistence of a real database
        /// 
        /// TODO: Move this initialisation data into an embedded string to increase portability / ease of installation
        /// </summary>
        public static FakeDatabase Database { get; } = new FakeDatabase();// JsonConvert.DeserializeObject<FakeBookingSystem>(File.ReadAllText($"../../../../fakedata.json"));
    }


    public class FakeDatabase
    {
        private static readonly Faker faker = new Faker("en");

        // A database-wide auto-incrementing id is used for simplicity
        private static int nextId = 100000;

        /// <summary>
        /// TODO: Call this on a schedule from both .NET Core and .NET Framework reference implementations
        /// </summary>
        public void CleanupExpiredLeases()
        {
            foreach (OrderTable order in Orders.Where(x => x.LeaseExpires < DateTimeOffset.Now))
            {
                OrderItems.RemoveAll(x => x.OrderId == order.Id);
                Orders.RemoveAll(x => x.Id == order.Id);
            }
        }

        public void AddLease(string uuid, string brokerName, long sellerId, string customerEmail, DateTimeOffset leaseExpires)
        {
            Orders.Add(new OrderTable
            {
                Id = uuid,
                Deleted = false,
                BrokerName = brokerName,
                SellerId = sellerId,
                CustomerEmail = customerEmail,
                IsLease = true,
                LeaseExpires = leaseExpires
            });
        }

        public void DeleteLease(string uuid)
        {
            if (Orders.Exists(x => x.IsLease && x.Id == uuid))
            {
                OrderItems.RemoveAll(x => x.OrderId == uuid);
                Orders.RemoveAll(x => x.Id == uuid);
            }
        }

        public void AddOrder(string uuid, BrokerRole brokerRole, string brokerName, long sellerId, string customerEmail, string paymentIdentifier, decimal totalOrderPrice)
        {
            Orders.Add(new OrderTable
            {
                Id = uuid,
                Deleted = false,
                BrokerRole = brokerRole,
                BrokerName = brokerName,
                SellerId = sellerId,
                CustomerEmail = customerEmail,
                PaymentIdentifier = paymentIdentifier,
                TotalOrderPrice = totalOrderPrice,
                IsLease = false
            });
        }

        public void DeleteOrder(string uuid)
        {
            // Set the Order to deleted in the feed, and erase all associated personal data
            var order = Orders.FirstOrDefault(x => !x.IsLease && x.Id == uuid && !x.Deleted);
            if (order != null)
            {
                order.Deleted = true;
                order.CustomerEmail = null;
                order.Modified = DateTimeOffset.Now;
                OrderItems.RemoveAll(x => x.OrderId == order.Id);
            }
        }

        public bool LeaseOrderItemsForClassOccurrence(string uuid, long occurrenceId, string opportunityJsonLdId, string offerJsonLdId, long numberOfSpaces)
        {
            var thisOccurrence = Occurrences.FirstOrDefault(x => x.Id == occurrenceId && !x.Deleted);
            var thisClass = Classes.FirstOrDefault(x => x.Id == thisOccurrence.ClassId && !x.Deleted);

            if (thisOccurrence != null && thisClass != null)
            {
                // Only lease if all spaces requested are available
                if (thisOccurrence.RemainingSpaces >= numberOfSpaces)
                {
                    for (int i = 0; i < numberOfSpaces; i++)
                    {
                        OrderItems.Add(new OrderItemsTable
                        {
                            Id = nextId++,
                            Deleted = false,
                            OrderId = uuid,
                            OccurrenceId = occurrenceId
                        });
                    }

                    // Update number of spaces remianing for the opportunity
                    var occurrence = Occurrences.Single(x => x.Id == occurrenceId);
                    var totalSpacesTaken = OrderItems.Where(x => x.OccurrenceId == occurrenceId && (x.Status == BookingStatus.Confirmed || x.Status == BookingStatus.Attended)).Count();
                    occurrence.RemainingSpaces = occurrence.TotalSpaces - totalSpacesTaken;
                    occurrence.Modified = DateTimeOffset.Now;
                    return true;
                }
                else
                {
                    return false;
                }
            } else
            {
                return false;
            }
        }

        // TODO this should reuse code of LeaseOrderItemsForClassOccurrence
        public List<long> BookOrderItemsForClassOccurrence(string uuid, long occurrenceId, string opportunityJsonLdId, string offerJsonLdId, long numberOfSpaces)
        {
            var thisOccurrence = Occurrences.FirstOrDefault(x => x.Id == occurrenceId && !x.Deleted);
            var thisClass = Classes.FirstOrDefault(x => x.Id == thisOccurrence.ClassId && !x.Deleted);

            if (thisOccurrence != null && thisClass != null)
            {
                // Only lease if all spaces requested are available
                if (thisOccurrence.RemainingSpaces >= numberOfSpaces)
                {
                    var OrderItemIds = new List<long>();
                    for (int i = 0; i < numberOfSpaces; i++)
                    {
                        var orderItemId = nextId++;
                        OrderItemIds.Add(orderItemId);
                        OrderItems.Add(new OrderItemsTable
                        {
                            Id = orderItemId,
                            Deleted = false,
                            OrderId = uuid,
                            Status = BookingStatus.Confirmed,
                            OccurrenceId = occurrenceId
                        });
                    }

                    // Update number of spaces remaining for the opportunity
                    var occurrence = Occurrences.Single(x => x.Id == occurrenceId);
                    var totalSpacesTaken = OrderItems.Where(x => x.OccurrenceId == occurrenceId && (x.Status == BookingStatus.Confirmed || x.Status == BookingStatus.Attended)).Count();
                    occurrence.RemainingSpaces = occurrence.TotalSpaces - totalSpacesTaken;
                    occurrence.Modified = DateTimeOffset.Now;

                    return OrderItemIds;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public bool CancelOrderItem(string uuid, List<long> orderItemIds, bool customerCancelled)
        {
            var order = Orders.FirstOrDefault(x => !x.IsLease && x.Id == uuid && !x.Deleted);
            if (order != null)
            {
                List<OrderItemsTable> updatedOrderItems = new List<OrderItemsTable>();
                foreach (OrderItemsTable orderItem in updatedOrderItems) {
                    if (orderItem.Status == BookingStatus.Confirmed || orderItem.Status == BookingStatus.Attended)
                    {
                        updatedOrderItems.Add(orderItem);
                        orderItem.Status = customerCancelled ? BookingStatus.CustomerCancelled : BookingStatus.SellerCancelled;
                    }
                }
                // Update the total price and modified date on the Order to update the feed, if something has changed
                if (updatedOrderItems.Count > 0)
                {
                    var totalPrice = OrderItems.Where(x => x.OrderId == order.Id && (x.Status == BookingStatus.Confirmed || x.Status == BookingStatus.Attended)).Sum(x => x.Price);
                    order.TotalOrderPrice = totalPrice;
                    order.Modified = DateTimeOffset.Now;

                    // Note an actual implementation would need to handle different opportunity types here
                    // Update the number of spaces available as a result of cancellation
                    foreach (long occurrenceId in updatedOrderItems.Select(x => x.OccurrenceId).Distinct())
                    {
                        var occurrence = Occurrences.Single(x => x.Id == occurrenceId);
                        var totalSpacesTaken = OrderItems.Where(x => x.OccurrenceId == occurrenceId && (x.Status == BookingStatus.Confirmed || x.Status == BookingStatus.Attended)).Count();
                        occurrence.RemainingSpaces = occurrence.TotalSpaces - totalSpacesTaken;
                        occurrence.Modified = DateTimeOffset.Now;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }

        }


        public List<ClassTable> Classes { get; set; } = new List<ClassTable>();
        public List<OccurrenceTable> Occurrences { get; set; } = new List<OccurrenceTable>();
        public List<OrderItemsTable> OrderItems { get; set; } = new List<OrderItemsTable>();
        public List<OrderTable> Orders { get; set; } = new List<OrderTable>();
        public List<SellerTable> Sellers { get; set; } = new List<SellerTable>();

        public void CreateFakeClasses()
        {
            Occurrences = Enumerable.Range(1, 10000)
            .Select(n => new {
                id = n,
                startDate = faker.Date.Soon()
            })
            .Select(x => new OccurrenceTable
            {
                ClassId = Decimal.ToInt32(x.id / 10),
                Id = x.id,
                Deleted = false,
                Start = x.startDate,
                End = x.startDate + TimeSpan.FromMinutes(faker.Random.Int(0, 360))
            })
            .ToList();

            Classes = Enumerable.Range(1, 1000)
            .Select(id => new ClassTable
            {
                Id = id,
                Deleted = false,
                Title = faker.Commerce.ProductMaterial() + " " + faker.PickRandomParam("Yoga", "Zumba", "Walking", "Cycling", "Running", "Jumping"),
                Price = Decimal.Parse(faker.Commerce.Price(0, 20)),
                SellerId = faker.Random.Long(0, 1)
            })
            .ToList();

            Sellers.AddRange(new List<SellerTable> {
                new SellerTable { Id = 0, Name = "Acme Fitness Ltd", IsIndividual = false },
                new SellerTable { Id = 1, Name = "Jane Smith", IsIndividual = true }
            });
        }

        public void AddClass(string title, decimal? price, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var classId = nextId++;

            Classes.Add(new ClassTable
            {
                Id = classId,
                Deleted = false,
                Title = title,
                Price = price
            });

            Occurrences.Add(new OccurrenceTable
            {
                Id = nextId++,
                Deleted = false,
                ClassId = classId,
                Start = startTime.DateTime,
                End = endTime.DateTime
            });
        }

        public void DeleteClass(string name)
        {
            foreach (ClassTable @class in Classes.Where(x => x.Title == name))
            {
                if (!@class.Deleted)
                {
                    @class.Modified = DateTimeOffset.Now;
                    @class.Deleted = true;
                }
            }
        }
    }
}
