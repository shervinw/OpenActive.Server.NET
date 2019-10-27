using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET
{
    public enum BookableOpportunityClass { Event, ScheduledSession, HeadlineEvent, Slot, CourseInstance }

    public class BookingEngineSettings
    {
        //Dictionary<BookableOpportunityClass, BookablePairIdTemplate<>> SupportedOpportunityTypes { get; set; }

    }
}

// Event, ScheduledSession, HeadlineEvent, Slot or CourseInstance

/// Seller = IdTemplate<SellerIdComponents>("<scheduled_session_url>,</scheduled_session_url>"),

/// List of orderedItems we can handle
/// Dict<nameof(Event)> 
///   - ScheduledSession = IdTemplate<ScheduledSessionIdComponents>("<scheduled_session_url", "offer_url")
///     - 
///
/// If acceptedOffer.type = ScheduledSession
///   ScheduledSessionIdComponents = ScheduledSession.IdTemplate.Bind(acceptedOffer.id, acceptedOffer.id)
///   d
///   .Bind // If the same string exists in both with different values, fail due to incompatibility.

//IOrderStore.getOrderItem(ScheduledSessionIdComponents