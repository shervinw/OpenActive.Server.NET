using OpenActive.Server.NET.OpenBookingHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace BookingSystem.AspNetFramework.Controllers
{
    [RoutePrefix("api/openbooking")]
    public class OpenBookingController : ApiController
    {
        /*
        /// <summary>
        /// OrderQuote Creation C1
        /// GET api/openbooking/order-quote-templates/ABCD1234
        /// </summary>
        [HttpPut]
        [Route("order-quote-template/{uuid}")]
        public async Task<IHttpActionResult> OrderQuoteCreationC1([FromServices] IBookingEngine bookingEngine, string uuid, [FromBody] string orderQuote)
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
        [HttpPut]
        [Route("order-quotes/{uuid}")]
        public async Task<IHttpActionResult> OrderQuoteCreationC2([FromServices] IBookingEngine bookingEngine, string uuid, [FromBody] string orderQuote)
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
        [HttpPut]
        [Route("orders/{uuid}")]
        public async Task<IHttpActionResult> OrderCreationB([FromServices] IBookingEngine bookingEngine, string uuid, [FromBody] string order)
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
        [HttpDelete]
        [Route("orders/{uuid}")]
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
        [HttpPatch]
        [Route("orders/{uuid}")]
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
        [HttpGet]
        [Route("orders-rpde")]
        public async Task<IHttpActionResult> Get([FromServices] IBookingEngine bookingEngine, int uuid)
        {
            return Ok();
        }

        // POST api/openbooking/test-interface/scheduled-sessions
        [HttpPost]
        [Route("test-interface/{type}")]
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
        [HttpDelete]
        [Route("test-interface/{type}/{name}")]
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
        */
    }
}
