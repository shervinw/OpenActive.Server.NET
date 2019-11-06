using Newtonsoft.Json;
using OpenActive.NET;
using OpenActive.Server.NET;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Linq;
using BookingSystem.FakeDatabase;
using static BookingSystem.FakeDatabase.FakeDatabase;

namespace BookingSystem.AspNetCore
{

    /// <summary>
    /// TODO: Move to BookingSystem.AspNetCore
    /// </summary>
    class AcmeStore : IOpenBookingStore
    {
        public void CreateTestDataItem(Event @event)
        {
            FakeBookingSystem.Database.AddClass(@event.Name, @event.Offers?.FirstOrDefault()?.Price);
        }

        public void DeleteTestDataItem(Uri id)
        {
            //FakeBookingSystem.Database.DeleteClass(get id from idTemplate);
        }


        //public void CreateFakeEvent()
        public OrderItem GetOrderItem(IBookableIdComponents opportunityOfferId, DefaultSellerIdComponents sellerId)
        {
            // Note switch statement exists here as we need to handle booking for a single Order that contains different types of opportunity
            switch (opportunityOfferId)
            {
                case SessionOpportunity sessionOpportunity:

                case FacilityOpportunity facilityOpportunity:

                case null:

                default:
                    break;
            }

            return null;
        }
    }

    
}
