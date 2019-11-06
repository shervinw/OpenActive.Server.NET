using OpenActive.DatasetSite.NET;
using OpenActive.NET;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET
{
    public class StoreBookingEngineSettings
    {
        // TODO: Add check to ensure at least e-mail is always included in both of these (the only required field)
        public List<string> CustomerPersonSupportedFields { get; set; } = new List<string> { nameof(Person.Email) };
        public List<string> CustomerOrganizationSupportedFields { get; set; } = new List<string> { nameof(Person.Email) };
        public List<string> BrokerSupportedFields { get; set; } = new List<string>();
        public BookingService BookingServiceDetails { get; set; }
        public Dictionary<IOpportunityStore, List<OpportunityType>> OpenBookingStoreRouting { get; set; }
            = new Dictionary<IOpportunityStore, List<OpportunityType>>();
    }
}
