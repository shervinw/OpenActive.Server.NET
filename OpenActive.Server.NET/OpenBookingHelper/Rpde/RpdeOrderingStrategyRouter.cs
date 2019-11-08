using OpenActive.NET.Rpde.Version1;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET.OpenBookingHelper
{

    public interface IRPDEFeedIncrementingUniqueChangeNumber : IRPDEFeedGenerator
    {
        RpdePage GetRPDEPage(long? afterChangeNumber);
    }

    public interface IRPDEFeedModifiedTimestampAndIDLong : IRPDEFeedGenerator
    {
        RpdePage GetRPDEPage(long? afterTimestamp, long? afterId);
    }

    public interface IRPDEFeedModifiedTimestampAndIDString : IRPDEFeedGenerator
    {
        RpdePage GetRPDEPage(long? afterTimestamp, string afterId);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "<Pending>")]
    // This interface exists to provide the extension method below for all RPDE feeds
    public interface IRPDEFeedGenerator { }

    public static class RpdeOrderingStrategyRouter
    {
        /// <summary>
        /// This method provides simple routing for the RPDE generator based on the subclasses defined
        /// </summary>
        /// <param name="feedidentifier"></param>
        /// <param name="generator"></param>
        /// <param name="afterTimestamp"></param>
        /// <param name="afterId"></param>
        /// <param name="afterChangeNumber"></param>
        /// <returns></returns>
        public static RpdePage GetRPDEPage(this IRPDEFeedGenerator generator, string feedidentifier, long? afterTimestamp, string afterId, long? afterChangeNumber)
        {
            switch (generator)
            {
                case IRPDEFeedIncrementingUniqueChangeNumber changeNumberGenerator:
                    return changeNumberGenerator.GetRPDEPage(afterChangeNumber);

                case IRPDEFeedModifiedTimestampAndIDLong timestampAndIDGeneratorLong:
                    if (long.TryParse(afterId, out long afterIdLong))
                    {
                        return timestampAndIDGeneratorLong.GetRPDEPage(afterTimestamp, afterIdLong);
                    }
                    else if (string.IsNullOrWhiteSpace(afterId))
                    {
                        return timestampAndIDGeneratorLong.GetRPDEPage(afterTimestamp, null);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(afterId), "afterId must be numeric");
                    }

                case IRPDEFeedModifiedTimestampAndIDString timestampAndIDGeneratorString:
                    return timestampAndIDGeneratorString.GetRPDEPage(afterTimestamp, afterId);

                default:
                    throw new InvalidCastException($"RPDEFeedGenerator for '{feedidentifier}' not recognised - check the generic template for RPDEFeedModifiedTimestampAndID uses either <string> or <long?>");
            }

        }

        public static long? ConvertStringToLongOrThrow(string argumentValue, string argumentName)
        {
            if (long.TryParse(argumentValue, out long result))
            {
                return result;
            }
            else if (!string.IsNullOrWhiteSpace(argumentValue))
            {
                throw new ArgumentOutOfRangeException($"{argumentName}", $"{argumentName} must be numeric");
            }
            else
            {
                return null;
            }
        }
    }
}
