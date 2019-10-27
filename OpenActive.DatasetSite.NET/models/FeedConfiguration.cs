using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.DataSetSite.NET
{
    public class FeedConfiguration
    {
        public string Name { get; set; }
        public Uri SameAs { get; set; }
        public string DefaultFeedPath { get; set; }
        public List<string> PossibleKinds { get; set; }
        public string DisplayName { get; set; }
    }
}
