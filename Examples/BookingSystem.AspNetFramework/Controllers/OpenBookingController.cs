using BookingSystem.AspNetFramework.Helpers;
using OpenActive.Server.NET;
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
        private IBookingEngine _bookingEngine = null;

        public OpenBookingController(IBookingEngine bookingEngine)
        {
            _bookingEngine = bookingEngine;
        }

        /// <summary>
        /// OrderQuote Creation C1
        /// GET api/openbooking/order-quote-templates/ABCD1234
        /// </summary>
        [HttpPut]
        [Route("order-quote-template/{uuid}")]
        public async Task<HttpResponseMessage> OrderQuoteCreationC1(string uuid, [FromBody] string orderQuote)
        {
            try
            {
                return _bookingEngine.ProcessCheckpoint1(uuid, orderQuote).GetContentResult();
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
        public async Task<HttpResponseMessage> OrderQuoteCreationC2(string uuid, [FromBody] string orderQuote)
        {
            try
            {
                return _bookingEngine.ProcessCheckpoint2(uuid, orderQuote).GetContentResult();
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
        public async Task<HttpResponseMessage> OrderCreationB(string uuid, [FromBody] string order)
        {
            try
            {
                return _bookingEngine.ProcessOrderCreationB(uuid, order).GetContentResult();
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
        public HttpResponseMessage OrderDeletion(string uuid)
        {
            try
            {
                _bookingEngine.DeleteOrder(uuid);
                return Request.CreateResponse(System.Net.HttpStatusCode.NoContent);
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
        public HttpResponseMessage OrderUpdate(string uuid, [FromBody] string order)
        {
            try
            {
                _bookingEngine.ProcessOrderUpdate(uuid, order);
                return Request.CreateResponse(System.Net.HttpStatusCode.NoContent);
            }
            catch (OpenBookingException obe)
            {
                return obe.ErrorResponseContent.GetContentResult();
            }
        }

        // GET api/openbooking/orders-rpde
        [HttpGet]
        [Route("orders-rpde")]
        public async Task<HttpResponseMessage> Get(int uuid)
        {
            return Request.CreateResponse(System.Net.HttpStatusCode.OK);
        }

        // POST api/openbooking/test-interface/scheduled-sessions
        [HttpPost]
        [Route("test-interface/{type}")]
        public HttpResponseMessage Post(string type, [FromBody] string @event)
        {
            try
            {
                _bookingEngine.CreateTestData(type, @event);
                return Request.CreateResponse(System.Net.HttpStatusCode.NoContent);
            }
            catch (OpenBookingException obe)
            {
                return obe.ErrorResponseContent.GetContentResult();
            }
        }

        // DELETE api/openbooking/test-interface/scheduled-sessions/{name}
        [HttpDelete]
        [Route("test-interface/{type}/{name}")]
        public HttpResponseMessage Delete(string type, string name)
        {
            try
            {
                _bookingEngine.DeleteTestData(type, name);
                return Request.CreateResponse(System.Net.HttpStatusCode.NoContent);
            }
            catch (OpenBookingException obe)
            {
                return obe.ErrorResponseContent.GetContentResult();
            }
        }

    }
}
