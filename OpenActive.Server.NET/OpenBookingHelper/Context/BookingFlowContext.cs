using OpenActive.NET;
using OpenActive.Server.NET.OpenBookingHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET.OpenBookingHelper
{
    public class BookingFlowContext
    {
        public FlowStage Stage { get; set; }
        public OrderIdTemplate OrderIdTemplate { get; set; }
        public OrderIdComponents OrderIdComponents { get; set; }
        public TaxPayeeRelationship TaxPayeeRelationship { get; set; }
        public IValue Payer { get; set; }
    }
}
