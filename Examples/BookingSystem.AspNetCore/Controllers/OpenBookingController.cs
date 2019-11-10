using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingSystem.AspNetCore.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OpenActive.NET;
using OpenActive.NET.Rpde.Version1;
using OpenActive.Server.NET;
using OpenActive.Server.NET.OpenBookingHelper;

namespace BookingSystem.AspNetCore.Controllers
{
    //[Authorize]
    [Route("api/openbooking")]
    [ApiController]
    [Consumes(MediaTypeNames.OpenBooking.Version1)]
    public class OpenBookingController : ControllerBase
    {
        // Open Booking Errors must be handled as thrown exceptions and the ErrorResponseContent of the exception returned.
        // Note that exceptions may be caught and logged in the usual way, and such error handling moved to a filter or middleware as required,
        // provided that the ErrorResponseContent is still returned. 

        /// Note that this interface expects JSON requests to be supplied as strings, and provides JSON responses as strings.
        /// This ensures that deserialisation is always correct, regardless of the configuration of the web framework.
        /// It also removes the need to expose OpenActive (de)serialisation settings and parsers to the implementer, and makes
        /// this interface more maintainble as OpenActive.NET will likely upgrade to use the new System.Text.Json in time.

        /// <summary>
        /// OrderQuote Creation C1
        /// GET api/openbooking/order-quote-templates/ABCD1234
        /// </summary>
        [HttpPut("order-quote-templates/{uuid}")]
        public ContentResult OrderQuoteCreationC1([FromServices] IBookingEngine bookingEngine, string uuid, [FromBody] string orderQuote)
        {
            try
            {
                return bookingEngine.ProcessCheckpoint1(uuid, orderQuote).GetContentResult();
            }
            catch (OpenBookingException obe)
            {
                return obe.ErrorResponseContent.GetContentResult();
            }
        }

        /// <summary>
        /// OrderQuote Creation C2
        /// GET api/openbooking/order-quotes/ABCD1234
        /// </summary>
        [HttpPut("order-quotes/{uuid}")]
        public ContentResult OrderQuoteCreationC2([FromServices] IBookingEngine bookingEngine, string uuid, [FromBody] string orderQuote)
        {
            try
            {
                return bookingEngine.ProcessCheckpoint2(uuid, orderQuote).GetContentResult();
            }
            catch (OpenBookingException obe)
            {
                return obe.ErrorResponseContent.GetContentResult();
            }
        }

        /// <summary>
        /// Order Creation B
        /// GET api/openbooking/orders/ABCD1234
        /// </summary>
        [HttpPut("orders/{uuid}")]
        public ContentResult OrderCreationB([FromServices] IBookingEngine bookingEngine, string uuid, [FromBody] string order)
        {
            try
            {
                return bookingEngine.ProcessOrderCreationB(uuid, order).GetContentResult();
            }
            catch (OpenBookingException obe)
            {
                return obe.ErrorResponseContent.GetContentResult();
            }
        }

        /// <summary>
        /// Order Deletion
        /// DELETE api/openbooking/orders/ABCD1234
        /// </summary>
        [HttpDelete("orders/{uuid}")]
        public IActionResult OrderDeletion([FromServices] IBookingEngine bookingEngine, string uuid)
        {
            try
            {
                bookingEngine.DeleteOrder(uuid);
                return NoContent();
            }
            catch (OpenBookingException obe)
            {
                return obe.ErrorResponseContent.GetContentResult();
            }
        }

        /// <summary>
        /// Order Cancellation
        /// GET api/openbooking/orders/ABCD1234
        /// </summary>
        [HttpPatch("orders/{uuid}")]
        public IActionResult OrderUpdate([FromServices] IBookingEngine bookingEngine, string uuid, [FromBody] string order)
        {
            try
            {
                bookingEngine.ProcessOrderUpdate(uuid, order);
                return NoContent();
            }
            catch (OpenBookingException obe)
            {
                return obe.ErrorResponseContent.GetContentResult();
            }
        }


        // GET api/openbooking/orders-rpde
        [HttpGet("orders-rpde")]
        public IActionResult GetOrdersFeed([FromServices] IBookingEngine bookingEngine, long? afterTimestamp, string afterId, long? afterChangeNumber)
        {
            try
            {
                // Note only a subset of these parameters will be supplied when this endpoints is called
                // They are all provided here for the bookingEngine to choose the correct endpoint
                // The auth token must also be provided from the associated authentication method
                return bookingEngine.GetOrdersRPDEPageForFeed("<insert auth token>", afterTimestamp, afterId, afterChangeNumber).GetContentResult();
            }
            catch (OpenBookingException obe)
            {
                return obe.ErrorResponseContent.GetContentResult();
            }
        }


        // POST api/openbooking/test-interface/scheduled-sessions
        [HttpPost("test-interface/{type}")]
        public IActionResult Post([FromServices] IBookingEngine bookingEngine, string type, [FromBody] string @event)
        {
            try
            {
                bookingEngine.CreateTestData(type, @event);
                return NoContent();
            }
            catch (OpenBookingException obe)
            {
                return obe.ErrorResponseContent.GetContentResult();
            }
        }

        // DELETE api/openbooking/test-interface/scheduled-sessions/{name}
        [HttpDelete("test-interface/{type}/{name}")]
        public IActionResult Delete([FromServices] IBookingEngine bookingEngine, string type, string name)
        {
            try
            {
                bookingEngine.DeleteTestData(type, name);
                return NoContent();
            }
            catch (OpenBookingException obe)
            {
                return obe.ErrorResponseContent.GetContentResult();
            }
        }
    }
}
