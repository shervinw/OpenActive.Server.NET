using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenActive.NET;
using OpenActive.NET.Rpde;
using OpenActive.NET.Rpde.Version1;
using OpenActive.Server.NET;

namespace BookingSystem.AspNetCore.Controllers
{
    [Route("feeds")]
    [ApiController]
    public class OpenDataController : ControllerBase
    {
        //TODO: Fix deserialisation of these types (using TypeConverter?)
        //QUESTION: Add middleware or filter to catch errors - or do it in each method as below?

        /// <summary>
        /// Open Data Feeds
        /// GET feeds/{feedname}
        /// </summary>
        [HttpGet("{feedname}")]
        public ActionResult<RpdePage> GetOpenDataFeed([FromServices] IBookingEngine bookingEngine, string feedname, long? afterTimestamp, string afterId, long? afterChangeNumber)
        {
            try
            {
                // Note only a subset of these parameters will be supplied when this endpoints is called
                // They are all provided here for the bookingEngine to choose the correct endpoint
                return bookingEngine.GetOpenDataRPDEPageForFeed(feedname, afterTimestamp, afterId, afterChangeNumber); // .ToStringContent();
            }
            catch (KeyNotFoundException kn)
            {
                return NotFound();
            }
        }

    }
}
