using Newtonsoft.Json;
using OpenActive.NET;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Linq;

namespace OpenActive.Server.NET.FakeBookingSystem
{
    /// <summary>
    /// This class models the database schema within an actual booking system.
    /// It is designed to simulate the database that would be available in a full implementation.
    /// 
    /// TODO: Move this into its own package that can used in the reference impl, rather than as part of 
    /// </summary>
    public class FakeBookingSystem
    {
        /// <summary>
        /// The Database is created as static, to simulate the persistence of a real database
        /// 
        /// TODO: Move this initialisation data into an embedded string to increase portability / ease of installation
        /// </summary>
        public static FakeBookingSystem Database { get; } = JsonConvert.DeserializeObject<FakeBookingSystem>(File.ReadAllText($"../../../../fakedata.json"));

        public abstract class Table
        {
            // A database-wide auto-incrementing id is used for simplicity
            private static int nextId = 0;
            public int Id { get; set; } = nextId++;
        }

        public List<ClassTable> Classes { get; set; } = new List<ClassTable>();
        public class ClassTable : Table
        {
            public string Title { get; set; }
            public int Price { get; set; }
        }

        public List<OccurrenceTable> Occurrences { get; set; } = new List<OccurrenceTable>();
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
