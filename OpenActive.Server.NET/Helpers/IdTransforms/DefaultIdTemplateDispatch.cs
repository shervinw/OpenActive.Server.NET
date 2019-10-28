using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace OpenActive.Server.NET
{
    public class DefaultIdTemplates
    {
        public static Dictionary<BookableOpportunityClass, BookablePairIdTemplate<DefaultIdComponents>> DefaultIdTemplateDispatch(List<BookableOpportunityClass> supportedBookableOpportunityTypes)
        {
            return new Dictionary<BookableOpportunityClass, BookablePairIdTemplate<DefaultIdComponents>> {
                {
                    BookableOpportunityClass.ScheduledSession,
                    new BookablePairIdTemplate<DefaultIdComponents>(
                        "{+BaseUrl}api/scheduled-sessions/{SessionSeriesId}/events/{ScheduledSessionId}",
                        "{+BaseUrl}api/scheduled-sessions/{SessionSeriesId}/events/{ScheduledSessionId}#/offers/{OfferId}"
                        )
                },
                {
                    BookableOpportunityClass.Slot,
                    new BookablePairIdTemplate<DefaultIdComponents>(
                        "{+BaseUrl}api/facility-uses/{FacilityUseId}/slots/{SlotId}",
                        "{+BaseUrl}api/facility-uses/{FacilityUseId}/slots/{SlotId}#/offers/{OfferId}"
                        )
                }
                // TODO: Add other BookableOpportunityClass with default
            }.Where(x => supportedBookableOpportunityTypes.Contains(x.Key)).ToDictionary(p => p.Key, p => p.Value);
        }

        public class DefaultIdComponents
        {
            public string BaseUrl { get; set; }
            public string OpportunityTypePath { get; set; }
            public string SessionSeriesId { get; set; }
            public long? ScheduledSessionId { get; set; }
            public string FacilityUseId { get; set; }
            public long? SlotId { get; set; }
            public long? OfferId { get; set; }
        }
    }


}
