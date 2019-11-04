using System;
using Xunit;
using OpenActive.Server.NET;
using System.Data;
using System.Linq;
using Xunit.Abstractions;
using BookingSystem.FakeDatabase;

namespace OpenActive.Server.NET.Tests
{
    public class FakeBookingSystemTest
    {
        private readonly ITestOutputHelper output;

        public FakeBookingSystemTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void FakeDatabase_Exists()
        {
            var query = from classes in FakeBookingSystem.Database.Classes
                        join occurances in FakeBookingSystem.Database.Occurrences
                        on classes.Id equals occurances.ClassId
                        select new
                        {
                            title = classes.Title,
                            startDate = occurances.Start,
                        };

            

            foreach (var result in query.ToList())
            {
                output.WriteLine(result.title + " " + result.startDate.ToString());
            }

            //var components = template.GetIdComponents(new Uri("https://example.com/api/session-series/asdf/events/123"));

            Assert.Equal("https://example.com/", "false");
            //Assert.Equal("session-series", components.EventType);
            //Assert.Equal("asdf", components.SessionSeriesId);
            //Assert.Equal(123, components.ScheduledSessionId);
        }

    }
}
