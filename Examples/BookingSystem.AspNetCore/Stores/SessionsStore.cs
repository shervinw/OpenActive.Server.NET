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
using OpenActive.DatasetSite.NET;

namespace BookingSystem.AspNetCore
{

    /// <summary>
    /// TODO: Move to BookingSystem.AspNetCore
    /// </summary>
    class SessionsStore : OpenBookingStore<SessionOpportunity>, IOpenBookingStore
    {
        
        public override void CreateTestDataItem(OpportunityType opportunityType, Event @event)
        {
            // Note assume that if it's been routed here, it will be possible to cast it to type Event
            FakeBookingSystem.Database.AddClass(@event.Name, ((Event)@event).Offers?.FirstOrDefault()?.Price);
        }

        public override void DeleteTestDataItem(OpportunityType opportunityType, string name)
        {
            FakeBookingSystem.Database.DeleteClass(name);
        }


        //public void CreateFakeEvent()
        public override OrderItem GetOrderItem(SessionOpportunity opportunityOfferId, SellerIdComponents sellerId)
        {
            // Note switch statement exists here as we need to handle booking for a single Order that contains different types of opportunity
            /*switch (opportunityOfferId)
            {
                case SessionOpportunity sessionOpportunity:

                case FacilityOpportunity facilityOpportunity:

                case null:

                default:
                    break;
            }*/

            return null;
        }
    }

}
