using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET
{
    class AbstractBookingEngine
    {
        // TODO: Move to engine settings
        Dictionary<BookableOpportunityClass, IBookablePairIdTemplate> IdConfiguration { get; set; }
            = new Dictionary<BookableOpportunityClass, IBookablePairIdTemplate> {
                {
                    BookableOpportunityClass.ScheduledSession,
                    new BookablePairIdTemplate<ScheduledSessionOpportunity>(
                        "{+BaseUrl}api/scheduled-sessions/{SessionSeriesId}/events/{ScheduledSessionId}",
                        "{+BaseUrl}api/scheduled-sessions/{SessionSeriesId}/events/{ScheduledSessionId}#/offers/{OfferId}"
                        )
                },
                {
                    BookableOpportunityClass.Slot,
                    new BookablePairIdTemplate<SlotOpportunity>(
                        "{+BaseUrl}api/facility-uses/{FacilityUseId}/slots/{SlotId}",
                        "{+BaseUrl}api/facility-uses/{FacilityUseId}/slots/{SlotId}#/offers/{OfferId}"
                        )
                }
            };


        // Note this is not a helper as it relies on engine settings state

        public IBookableIdComponents ResolveOpportunityID(BookableOpportunityClass opportunityClass, Uri opportunityId, Uri offerId)
        {
            return IdConfiguration[opportunityClass].GetOpportunityReference(opportunityId, offerId);
        }
    }
}
