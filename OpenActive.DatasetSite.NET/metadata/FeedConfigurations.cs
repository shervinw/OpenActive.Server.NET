
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.DataSetSite.NET
{
    public enum FeedType
    {
        SessionSeries,
        ScheduledSession,
        FacilityUse,
        IndividualFacilityUse,
        Slot,
        Course,
        CourseInstance,
        HeadlineEvent,
        Event,
        EventSeries
    }

    public static class FeedConfigurations
    {
        public readonly static Dictionary<FeedType, FeedConfiguration> Configurations = new Dictionary<FeedType, FeedConfiguration>
        {
            {
                FeedType.SessionSeries,
                new FeedConfiguration {
                    Name = "SessionSeries",
                    SameAs = new Uri("https://openactive.io/SessionSeries"),
                    DefaultFeedPath = "session-series",
                    PossibleKinds = new List<string> { "SessionSeries", "SessionSeries.ScheduledSession" },
                    DisplayName = "Sessions"
                }
            },
            {
                FeedType.ScheduledSession,
                new FeedConfiguration {
                    Name = "ScheduledSession",
                    SameAs = new Uri("https://openactive.io/ScheduledSession"),
                    DefaultFeedPath = "scheduled-sessions",
                    PossibleKinds = new List<string> { "ScheduledSession", "ScheduledSession.SessionSeries" },
                    DisplayName = "Sessions"
                }
            },
            {
                FeedType.FacilityUse,
                new FeedConfiguration {
                    Name = "FacilityUse",
                    SameAs = new Uri("https://openactive.io/FacilityUse"),
                    DefaultFeedPath = "facility-uses",
                    PossibleKinds = new List<string> { "FacilityUse" },
                    DisplayName = "Facilities"
                }
            },
            {
                FeedType.IndividualFacilityUse,
                new FeedConfiguration {
                    Name = "IndividualFacilityUse",
                    SameAs = new Uri("https://openactive.io/IndividualFacilityUse"),
                    DefaultFeedPath = "individual-facility-uses",
                    PossibleKinds = new List<string> { "IndividualFacilityUse" },
                    DisplayName = "Facilities"
                }
            },
            {
                FeedType.Slot,
                new FeedConfiguration {
                    Name = "Slot",
                    SameAs = new Uri("https://openactive.io/Slot"),
                    DefaultFeedPath = "slots",
                    PossibleKinds = new List<string> { "FacilityUse/Slot", "IndividualFacilityUse/Slot" },
                    DisplayName = "Facilities"
                }
            },
            {
                FeedType.Course,
                new FeedConfiguration {
                    Name = "Course",
                    SameAs = new Uri("https://openactive.io/Course"),
                    DefaultFeedPath = "courses",
                    PossibleKinds = new List<string> { "Course" },
                    DisplayName = "Courses"
                }
            },
            {
                FeedType.CourseInstance,
                new FeedConfiguration {
                    Name = "CourseInstance",
                    SameAs = new Uri("https://openactive.io/CourseInstance"),
                    DefaultFeedPath = "course-instances",
                    PossibleKinds = new List<string> { "CourseInstance", "CourseInstance.Event" },
                    DisplayName = "Courses"
                }
            },
            {
                FeedType.HeadlineEvent,
                new FeedConfiguration {
                    Name = "HeadlineEvent",
                    SameAs = new Uri("https://openactive.io/HeadlineEvent"),
                    DefaultFeedPath = "headline-events",
                    PossibleKinds = new List<string> { "HeadlineEvent" },
                    DisplayName = "Events"
                }
            },
            {
                FeedType.Event,
                new FeedConfiguration {
                    Name = "Event",
                    SameAs = new Uri("https://schema.org/Event"),
                    DefaultFeedPath = "events",
                    PossibleKinds = new List<string> { "Event" },
                    DisplayName = "Events"
                }
            },
            {
                FeedType.EventSeries,
                new FeedConfiguration {
                    Name = "EventSeries",
                    SameAs = new Uri("https://schema.org/EventSeries"),
                    DefaultFeedPath = "event-series",
                    PossibleKinds = new List<string> { "EventSeries" },
                    DisplayName = "undefined"
                }
            }
        };
    }
}
