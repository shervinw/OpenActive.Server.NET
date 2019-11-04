using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Linq;
using Bogus;

namespace BookingSystem.FakeDatabase
{
    /// <summary>
    /// This class models the database schema within an actual booking system.
    /// It is designed to simulate the database that would be available in a full implementation.
    /// </summary>
    public class FakeBookingSystem
    {
        /// <summary>
        /// The Database is created as static, to simulate the persistence of a real database
        /// 
        /// TODO: Move this initialisation data into an embedded string to increase portability / ease of installation
        /// </summary>
        public static FakeBookingSystem Database { get; } = new FakeBookingSystem();// JsonConvert.DeserializeObject<FakeBookingSystem>(File.ReadAllText($"../../../../fakedata.json"));

        private static readonly Faker faker = new Faker("en");

        public abstract class Table
        {
            // A database-wide auto-incrementing id is used for simplicity
            private static int nextId = 100000;
            public int Id { get; set; } = nextId++;
            public bool Deleted { get; set; } = false;
            public DateTimeOffset Modified { get; set; } = DateTimeOffset.Now;
        }

        public List<ClassTable> Classes { get; set; } = Enumerable.Range(1, 1000)
            .Select(id => new ClassTable
            {
                Id = id,
                Deleted = false,
                Title = faker.Commerce.ProductMaterial() + " " + faker.PickRandomParam("Yoga", "Zumba", "Walking", "Cycling", "Running", "Jumping"),
                Price = Decimal.Parse(faker.Commerce.Price(0,20))
            })
            .ToList();

        public class ClassTable : Table
        {
            public string Title { get; set; }
            public decimal Price { get; set; }
        }

        public List<OccurrenceTable> Occurrences { get; set; } = Enumerable.Range(1, 10000)
            .Select(id => new {
                id = id,
                startDate = faker.Date.Soon()
            })
            .Select(x => new OccurrenceTable
            {
                Id = Decimal.ToInt32(x.id / 10),
                Deleted = false,
                Start = x.startDate,
                End = x.startDate + TimeSpan.FromMinutes(faker.Random.Int(0, 360))
            })
            .ToList();

        public class OccurrenceTable : Table
        {
            public int ClassId { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }

        }

        public List<BookingTable> Bookings { get; set; } = new List<BookingTable>();
        public class BookingTable : Table
        {
            public string Firstname { get; set; }
            public string Surname { get; set; }
            public string Email { get; set; }
        }

        public List<OrderItemsTable> OrderItems { get; set; } = new List<OrderItemsTable>();
        public class OrderItemsTable : Table
        {

        }

        public List<OrderTable> Orders { get; set; } = new List<OrderTable>();
        public class OrderTable : Table
        {


        }

        public List<SellerTable> Sellers { get; set; } = new List<SellerTable>();
        public class SellerTable : Table
        {
            public string Name { get; set; }

        }
    }
}
