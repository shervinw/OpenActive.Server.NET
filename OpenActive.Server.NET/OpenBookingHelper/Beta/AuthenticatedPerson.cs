using OpenActive.NET;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET.OpenBookingHelper.Beta
{
    /// <summary>
    /// In line with the outstanding W3C discussion, this models an authenticated person, to be used
    /// </summary>
    public class AuthenticatedPerson : Person
    {
        public string authToken { get; set; }
    }
}
