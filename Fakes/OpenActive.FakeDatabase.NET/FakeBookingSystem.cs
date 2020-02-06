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
        public static FakeDatabase Database { get; } = FakeDatabase.GetPrepopulatedFakeDatabase();// JsonConvert.DeserializeObject<FakeBookingSystem>(File.ReadAllText($"../../../../fakedata.json"));
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
            var occurrenceIds = new List<long>();

            foreach (OrderTable order in Orders.Where(x => x.LeaseExpires < DateTimeOffset.Now))
            {
                occurrenceIds.AddRange(OrderItems.Where(x => x.OrderId == order.Id).Select(x => x.OccurrenceId));
                OrderItems.RemoveAll(x => x.OrderId == order.Id);
                Orders.RemoveAll(x => x.Id == order.Id);
            }

            RecalculateSpaces(occurrenceIds.Distinct());
        }

        public bool AddLease(string clientId, string uuid, BrokerRole brokerRole, string brokerName, long? sellerId, string customerEmail, DateTimeOffset leaseExpires, FakeDatabaseTransaction transaction)
        {
            if (transaction != null) transaction.OrdersIds.Add(uuid);

            var existingOrder = Orders.SingleOrDefault(x => x.ClientId == clientId && x.Id == uuid);
            if (existingOrder == null)
            {
                Orders.Add(new OrderTable
                {
                    ClientId = clientId,
                    Id = uuid,
                    Deleted = false,
                    BrokerRole = brokerRole,
                    BrokerName = brokerName,
                    SellerId = sellerId ?? default,
                    CustomerEmail = customerEmail,
                    IsLease = true,
                    LeaseExpires = leaseExpires,
                    VisibleInFeed = false
                });
                return true;
            }
            // Return false if there's a clash
            else if (!existingOrder.IsLease || existingOrder.Deleted)
            {
                return false;
            }
            // Reuse existing lease if it exists
            else
            {
                existingOrder.BrokerRole = brokerRole;
                existingOrder.BrokerName = brokerName;
                existingOrder.SellerId = sellerId ?? default;
                existingOrder.CustomerEmail = customerEmail;
                existingOrder.IsLease = true;
                existingOrder.LeaseExpires = leaseExpires;

                return true;
            }

        }

        public void DeleteLease(string clientId, string uuid, long? sellerId)
        {
            // TODO: Note this should throw an error if the Seller ID does not match, same as DeleteOrder
            if (Orders.Exists(x => x.ClientId == clientId && x.IsLease && x.Id == uuid && (!sellerId.HasValue || x.SellerId == sellerId)))
            {
                var occurrenceIds = OrderItems.Where(x => x.ClientId == clientId && x.OrderId == uuid).Select(x => x.OccurrenceId).Distinct();

                OrderItems.RemoveAll(x => x.ClientId == clientId && x.OrderId == uuid);
                Orders.RemoveAll(x => x.ClientId == clientId && x.Id == uuid);

                RecalculateSpaces(occurrenceIds);
            }
        }

        public bool AddOrder(string clientId, string uuid, BrokerRole brokerRole, string brokerName, long? sellerId, string customerEmail, string paymentIdentifier, decimal totalOrderPrice, FakeDatabaseTransaction transaction)
        {
            transaction.OrdersIds.Add(uuid);

            var existingOrder = Orders.SingleOrDefault(x => x.ClientId == clientId && x.Id == uuid);
            if (existingOrder == null)
            {
                Orders.Add(new OrderTable
                {
                    ClientId = clientId,
                    Id = uuid,
                    Deleted = false,
                    BrokerRole = brokerRole,
                    BrokerName = brokerName,
                    SellerId = sellerId ?? default,
                    CustomerEmail = customerEmail,
                    PaymentIdentifier = paymentIdentifier,
                    TotalOrderPrice = totalOrderPrice,
                    IsLease = false,
                    VisibleInFeed = false
                });
                return true;
            }
            // Return false if there's a clash
            else if (!existingOrder.IsLease || existingOrder.Deleted)
            {
                return false;
            }
            // Reuse existing lease if it exists
            else
            {
                existingOrder.BrokerRole = brokerRole;
                existingOrder.BrokerName = brokerName;
                existingOrder.SellerId = sellerId ?? default;
                existingOrder.CustomerEmail = customerEmail;
                existingOrder.PaymentIdentifier = paymentIdentifier;
                existingOrder.TotalOrderPrice = totalOrderPrice;
                existingOrder.IsLease = false;

                return true;
            }
        }

        public void DeleteOrder(string clientId, string uuid, long? sellerId)
        {
            // Set the Order to deleted in the feed, and erase all associated personal data
            var order = Orders.FirstOrDefault(x => x.ClientId == clientId && x.IsLease && x.Id == uuid && !x.Deleted);
            if (order != null)
            {
                if (sellerId.HasValue && order.SellerId != sellerId)
                {
                    throw new ArgumentException("SellerId does not match Order");
                }
                order.Deleted = true;
                order.CustomerEmail = null;
                order.Modified = DateTimeOffset.Now;

                var occurrenceIds = OrderItems.Where(x => x.ClientId == clientId && x.OrderId == order.Id).Select(x => x.OccurrenceId).Distinct();
                OrderItems.RemoveAll(x => x.ClientId == clientId && x.OrderId == order.Id);
                RecalculateSpaces(occurrenceIds);
            }
        }

        public void RollbackOrder(string uuid)
        {
            // Set the Order to deleted in the feed, and erase all associated personal data
            var occurrenceIds = OrderItems.Where(x => x.OrderId == uuid).Select(x => x.OccurrenceId).Distinct();
            Orders.RemoveAll(x => x.Id == uuid);
            OrderItems.RemoveAll(x => x.OrderId == uuid);
            RecalculateSpaces(occurrenceIds);
        }

        public bool LeaseOrderItemsForClassOccurrence(string clientId, long? sellerId, string uuid, long occurrenceId, long numberOfSpaces)
        {
            var thisOccurrence = Occurrences.FirstOrDefault(x => x.Id == occurrenceId && !x.Deleted);
            var thisClass = Classes.FirstOrDefault(x => x.Id == thisOccurrence?.ClassId && !x.Deleted);

            if (thisOccurrence != null && thisClass != null)
            {
                if (sellerId.HasValue && thisClass.SellerId != sellerId)
                {
                    throw new ArgumentException("SellerId does not match Order");
                }

                // Remove existing leases
                // Note a real implementation would likely maintain existing leases instead of removing and recreating them
                OrderItems.RemoveAll(x => x.ClientId == clientId && x.OrderId == uuid && x.OccurrenceId == occurrenceId);
                RecalculateSpaces(occurrenceId);

                // Only lease if all spaces requested are available
                if (thisOccurrence.RemainingSpaces - thisOccurrence.LeasedSpaces >= numberOfSpaces)
                {
                    for (int i = 0; i < numberOfSpaces; i++)
                    {
                        OrderItems.Add(new OrderItemsTable
                        {
                            ClientId = clientId,
                            Id = nextId++,
                            Deleted = false,
                            OrderId = uuid,
                            OccurrenceId = occurrenceId
                        });
                    }

                    // Update number of spaces remaining for the opportunity
                    RecalculateSpaces(occurrenceId);

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
        public List<long> BookOrderItemsForClassOccurrence(string clientId, long? sellerId, string uuid, long occurrenceId, string opportunityJsonLdType, string opportunityJsonLdId, string offerJsonLdId, long numberOfSpaces)
        {
            var thisOccurrence = Occurrences.FirstOrDefault(x => x.Id == occurrenceId && !x.Deleted);
            var thisClass = Classes.FirstOrDefault(x => x.Id == thisOccurrence.ClassId && !x.Deleted);

            if (thisOccurrence != null && thisClass != null)
            {
                if (sellerId.HasValue && thisClass.SellerId != sellerId)
                {
                    throw new ArgumentException("SellerId does not match Order");
                }

                // Remove existing leases
                // Note a real implementation would likely maintain existing leases instead of removing and recreating them
                OrderItems.RemoveAll(x => x.ClientId == clientId && x.OrderId == uuid && x.OccurrenceId == occurrenceId);
                RecalculateSpaces(occurrenceId);

                // Only lease if all spaces requested are available
                if (thisOccurrence.RemainingSpaces - thisOccurrence.LeasedSpaces >= numberOfSpaces)
                {
                    var OrderItemIds = new List<long>();
                    for (int i = 0; i < numberOfSpaces; i++)
                    {
                        var orderItemId = nextId++;
                        OrderItemIds.Add(orderItemId);
                        OrderItems.Add(new OrderItemsTable
                        {
                            ClientId = clientId,
                            Id = orderItemId,
                            Deleted = false,
                            OrderId = uuid,
                            Status = BookingStatus.Confirmed,
                            OccurrenceId = occurrenceId,
                            OpportunityJsonLdType = opportunityJsonLdType,
                            OpportunityJsonLdId = opportunityJsonLdId,
                            OfferJsonLdId = offerJsonLdId,
                            // Include the price locked into the OrderItem as the opportunity price may change
                            Price = thisClass.Price.Value
                        });
                    }

                    RecalculateSpaces(occurrenceId);

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

        public bool CancelOrderItem(string clientId, long? sellerId, string uuid, List<long> orderItemIds, bool customerCancelled)
        {
            var order = Orders.FirstOrDefault(x => x.ClientId == clientId && !x.IsLease && x.Id == uuid && !x.Deleted);
            if (sellerId.HasValue && order.SellerId != sellerId)
            {
                throw new ArgumentException("SellerId does not match Order");
            }
            if (order != null)
            {
                List<OrderItemsTable> updatedOrderItems = new List<OrderItemsTable>();
                foreach (OrderItemsTable orderItem in OrderItems.Where(x => x.ClientId == clientId && x.OrderId == order.Id && orderItemIds.Contains(x.Id))) {
                    if (orderItem.Status == BookingStatus.Confirmed || orderItem.Status == BookingStatus.Attended)
                    {
                        updatedOrderItems.Add(orderItem);
                        orderItem.Status = customerCancelled ? BookingStatus.CustomerCancelled : BookingStatus.SellerCancelled;
                    }
                }
                // Update the total price and modified date on the Order to update the feed, if something has changed
                if (updatedOrderItems.Count > 0)
                {
                    var totalPrice = OrderItems.Where(x => x.ClientId == clientId && x.OrderId == order.Id && (x.Status == BookingStatus.Confirmed || x.Status == BookingStatus.Attended)).Sum(x => x.Price);
                    order.TotalOrderPrice = totalPrice;
                    order.VisibleInFeed = true;
                    order.Modified = DateTimeOffset.Now;

                    // Note an actual implementation would need to handle different opportunity types here
                    // Update the number of spaces available as a result of cancellation
                    RecalculateSpaces(updatedOrderItems.Select(x => x.OccurrenceId).Distinct());
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RecalculateSpaces(long occurrenceId)
        {
            RecalculateSpaces(new List<long> { occurrenceId });
        }

        public void RecalculateSpaces(IEnumerable<long> occurrenceIds)
        {
            foreach (var occurrenceId in occurrenceIds)
            {
                var thisOccurrence = Occurrences.FirstOrDefault(x => x.Id == occurrenceId && !x.Deleted);

                // Update number of leased spaces remaining for the opportunity
                var leasedSpaces = OrderItems.Where(x => Orders.Single(o => o.Id == x.OrderId).IsLease && x.OccurrenceId == occurrenceId).Count();
                thisOccurrence.LeasedSpaces = leasedSpaces;

                // Update number of actual spaces remaining for the opportunity
                var totalSpacesTaken = OrderItems.Where(x => !Orders.Single(o => o.Id == x.OrderId).IsLease && x.OccurrenceId == occurrenceId && (x.Status == BookingStatus.Confirmed || x.Status == BookingStatus.Attended)).Count();
                thisOccurrence.RemainingSpaces = thisOccurrence.TotalSpaces - totalSpacesTaken;

                // Push the change into the future to avoid it getting lost in the feed (see race condition transaction challenges https://developer.openactive.io/publishing-data/data-feeds/implementing-rpde-feeds#preventing-the-race-condition) // TODO: Document this!
                thisOccurrence.Modified = DateTimeOffset.Now;
            }
        }

        public List<ClassTable> Classes { get; set; } = new List<ClassTable>();
        public List<OccurrenceTable> Occurrences { get; set; } = new List<OccurrenceTable>();
        public List<OrderItemsTable> OrderItems { get; set; } = new List<OrderItemsTable>();
        public List<OrderTable> Orders { get; set; } = new List<OrderTable>();
        public List<SellerTable> Sellers { get; set; } = new List<SellerTable>();

        public List<BookingPartnerTable> BookingPartners { get; set; } = new List<BookingPartnerTable>();

        public static FakeDatabase GetPrepopulatedFakeDatabase()
        {
            var db = new FakeDatabase();
            db.CreateFakeClasses();
            return db;
        }

        public void CreateFakeClasses()
        {
            Occurrences = Enumerable.Range(1, 1000)
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

            Classes = Enumerable.Range(1, 100)
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
                new SellerTable { Id = 0, SellerId = "abcd", Name = "Acme Fitness Ltd", IsIndividual = false },
                new SellerTable { Id = 1, SellerId = "efgh", Name = "Jane Smith", IsIndividual = true }
            });
            
            BookingPartners.AddRange(new List<BookingPartnerTable>
            {
                new BookingPartnerTable { ClientId = "clientid_123", SellerId = "abcd", ClientSecret = "secret",
                    ClientJson = new ClientRegistrationModel {
                        ClientId = "clientid_123",
                        ClientName = "Example Booking Partner 1",
                        Scope = "openid profile openactive-openbooking openactive-ordersfeed oauth-dymamic-client-update openactive-identity",
                        RedirectUris = new[] { "http://localhost:5002/signin-oidc" },
                        GrantTypes = new[] { "authorization_code", "refresh_token" }
                    } },
                new BookingPartnerTable { ClientId = "clientid_456", SellerId = "abcd", ClientSecret = "secret",
                    ClientJson = new ClientRegistrationModel {
                        ClientId = "clientid_456",
                        ClientName = "Example Booking Partner 2",
                        Scope = "openid profile openactive-openbooking openactive-ordersfeed oauth-dymamic-client-update openactive-identity",
                        RedirectUris = new[] { "http://localhost:5002/signin-oidc" },
                        GrantTypes = new[] { "authorization_code", "refresh_token" }
                    } },
                new BookingPartnerTable { ClientId = "clientid_789", SellerId = "abcd", ClientSecret = "secret",
                    ClientJson = new ClientRegistrationModel {
                        ClientId = "clientid_789",
                        ClientName = "Example Booking Partner 3",
                        Scope = "openid profile openactive-openbooking openactive-ordersfeed oauth-dymamic-client-update openactive-identity",
                        GrantTypes = new[] { "client_credentials" }
                    } }
            });
        }

        public ( int, int ) AddClass(string title, decimal? price, DateTimeOffset startTime, DateTimeOffset endTime, long totalSpaces)
        {
            var classId = nextId++;
            var occurrenceId = nextId++;

            Classes.Add(new ClassTable
            {
                Id = classId,
                Deleted = false,
                Title = title,
                Price = price,
                SellerId = faker.Random.Long(0, 1)
            });

            Occurrences.Add(new OccurrenceTable
            {
                Id = occurrenceId,
                Deleted = false,
                ClassId = classId,
                Start = startTime.DateTime,
                End = endTime.DateTime,
                TotalSpaces = totalSpaces,
                RemainingSpaces = totalSpaces
            });

            return ( classId, occurrenceId );
        }

        public void DeleteClass(long classId, long occurrenceId)
        {
            foreach (ClassTable @class in Classes.Where(x => x.Id == classId))
            {
                if (!@class.Deleted)
                {
                    @class.Modified = DateTimeOffset.Now;
                    @class.Deleted = true;
                }
            }

            foreach (OccurrenceTable occurrence in Occurrences.Where(x => x.Id == occurrenceId))
            {
                if (!occurrence.Deleted)
                {
                    occurrence.Modified = DateTimeOffset.Now;
                    occurrence.Deleted = true;
                }
            }
        }
    }
}
