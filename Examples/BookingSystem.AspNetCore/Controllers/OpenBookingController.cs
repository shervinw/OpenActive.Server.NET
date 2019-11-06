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
    //[Authorize]
    [Route("api/openbooking")]
    [ApiController]
    public class OpenBookingController : ControllerBase
    {
        //TODO: Fix deserialisation of these types (using TypeConverter?)
        //QUESTION: Add middleware or filter to catch errors, and revert return types to Order, OrderQuote, etc?

        /// <summary>
        /// OrderQuote Creation C1
        /// GET api/openbooking/order-quote-templates/ABCD1234
        /// </summary>
        [HttpPut("order-quote-template/{uuid}")]
        public ActionResult<Schema.NET.Thing> OrderQuoteCreationC1([FromServices] IBookingEngine bookingEngine, string uuid, [FromBody] OrderQuote orderQuote)
        {
            try
            {
                return bookingEngine.ProcessCheckpoint1(uuid, orderQuote);
            }
            catch (OpenBookingException obe)
            {
                return StatusCode((int)obe.GetHttpStatusCode(), obe);
            }
        }

        /// <summary>
        /// OrderQuote Creation C2
        /// GET api/openbooking/order-quotes/ABCD1234
        /// </summary>
        [HttpPut("order-quotes/{uuid}")]
        public ActionResult<Schema.NET.Thing> OrderQuoteCreationC2([FromServices] IBookingEngine bookingEngine, string uuid, [FromBody] OrderQuote orderQuote)
        {
            try
            {
                return bookingEngine.ProcessCheckpoint2(uuid, orderQuote);
            }
            catch (OpenBookingException obe)
            {
                return StatusCode((int)obe.GetHttpStatusCode(), obe);
            }
        }

        /// <summary>
        /// Order Creation B
        /// GET api/openbooking/orders/ABCD1234
        /// </summary>
        [HttpPut("orders/{uuid}")]
        public IActionResult OrderCreationB([FromServices] IBookingEngine bookingEngine, string uuid, [FromBody] Order order)
        {
            try
            {
                return StatusCode((int)404, new OpenBookingError().ToOpenActiveString());
                //return Content(bookingEngine.ProcessOrderCreationB(uuid, order).ToOpenActiveString(), "application/ld+json");
            }
            catch (OpenBookingException obe)
            {
                return StatusCode((int)obe.GetHttpStatusCode(), Content(obe.ToOpenActiveString(), "application/ld+json"));
            }
        }

        /// <summary>
        /// Order Deletion
        /// DELETE api/openbooking/orders/ABCD1234
        /// </summary>
        [HttpDelete("orders/{uuid}")]
        public ActionResult<Schema.NET.Thing> OrderDeletion([FromServices] IBookingEngine bookingEngine, string uuid)
        {
            try
            {
                bookingEngine.DeleteOrder(uuid);
                return NoContent();
            }
            catch (OpenBookingException obe)
            {
                return StatusCode((int)obe.GetHttpStatusCode(), obe);
            }
        }

        /// <summary>
        /// Order Cancellation
        /// GET api/openbooking/orders/ABCD1234
        /// </summary>
        [HttpPatch("orders/{uuid}")]
        public ActionResult<Schema.NET.Thing> OrderUpdate([FromServices] IBookingEngine bookingEngine, string uuid, [FromBody] Order order)
        {
            try
            {
                bookingEngine.ProcessOrderUpdate(uuid, order);
                return NoContent();
            }
            catch (OpenBookingException obe)
            {
                return StatusCode((int)obe.GetHttpStatusCode(), obe);
            }
        }

        // GET api/openbooking/orders-rpde
        [HttpGet("{id}")]
        public ActionResult<RpdePage> Get([FromServices] IBookingEngine bookingEngine, int uuid)
        {
            throw new NotImplementedException();
        }

        // POST api/openbooking/test-interface/create
        [HttpPost("test-interface/create")]
        public ActionResult<Schema.NET.Thing> Post([FromServices] IBookingEngine bookingEngine, [FromBody] Event @event)
        {
            try
            {
                bookingEngine.CreateTestData(@event);
                return NoContent();
            }
            catch (OpenBookingException obe)
            {
                return StatusCode((int)obe.GetHttpStatusCode(), obe);
            }
        }

        // POST api/openbooking/test-interface
        [HttpPost("test-interface/delete")]
        public ActionResult<Schema.NET.Thing> Delete([FromServices] IBookingEngine bookingEngine, Uri @id)
        {
            try
            {
                bookingEngine.DeleteTestData(@id);
                return NoContent();
            }
            catch (OpenBookingException obe)
            {
                return StatusCode((int)obe.GetHttpStatusCode(), obe);
            }
        }
    }
}
