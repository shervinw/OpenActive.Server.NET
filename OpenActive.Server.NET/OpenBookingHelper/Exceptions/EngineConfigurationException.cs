using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET.OpenBookingHelper
{
    public class EngineConfigurationException : Exception
    {
        public EngineConfigurationException(string message) : base(message)
        {
        }

        public EngineConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public EngineConfigurationException()
        {
        }
    }
}
