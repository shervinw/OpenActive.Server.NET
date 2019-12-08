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
        [Route("order-quote-templates/{uuid}")]
        public HttpResponseMessage OrderQuoteCreationC1(string uuid, [FromBody] string orderQuote)
        {
            try
            {
                (string clientId, Uri sellerId) = AuthenticationHelper.GetIdsFromAuth(Request, User);
                return _bookingEngine.ProcessCheckpoint1(clientId, sellerId, uuid, orderQuote).GetContentResult();
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
        public HttpResponseMessage OrderQuoteCreationC2(string uuid, [FromBody] string orderQuote)
        {
            try
            {
                (string clientId, Uri sellerId) = AuthenticationHelper.GetIdsFromAuth(Request, User);
                return _bookingEngine.ProcessCheckpoint2(clientId, sellerId, uuid, orderQuote).GetContentResult();
            }
            catch (OpenBookingException obe)
            {
                return obe.ErrorResponseContent.GetContentResult();
            }
        }

        /// <summary>
        /// OrderQuote Deletion
        /// DELETE api/openbooking/orders-quotes/ABCD1234
        /// </summary>
        [HttpDelete]
        [Route("orders-quotes/{uuid}")]
        public HttpResponseMessage OrderQuoteDeletion(string uuid)
        {
            try
            {
                (string clientId, Uri sellerId) = AuthenticationHelper.GetIdsFromAuth(Request, User);
                return _bookingEngine.DeleteOrderQuote(clientId, sellerId, uuid).GetContentResult();
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
        public HttpResponseMessage OrderCreationB(string uuid, [FromBody] string order)
        {
            try
            {
                (string clientId, Uri sellerId) = AuthenticationHelper.GetIdsFromAuth(Request, User);
                return _bookingEngine.ProcessOrderCreationB(clientId, sellerId, uuid, order).GetContentResult();
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
                (string clientId, Uri sellerId) = AuthenticationHelper.GetIdsFromAuth(Request, User);
                return _bookingEngine.DeleteOrder(clientId, sellerId, uuid).GetContentResult();
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
                (string clientId, Uri sellerId) = AuthenticationHelper.GetIdsFromAuth(Request, User);
                return _bookingEngine.ProcessOrderUpdate(clientId, sellerId, uuid, order).GetContentResult();
            }
            catch (OpenBookingException obe)
            {
                return obe.ErrorResponseContent.GetContentResult();
            }
        }

        // GET api/openbooking/orders-rpde
        [HttpGet]
        [Route("orders-rpde")]
        public HttpResponseMessage GetOrdersFeed(long? afterTimestamp = (long?)null, string afterId = null, long? afterChangeNumber = (long?)null)
        {
            try
            {
                // Note only a subset of these parameters will be supplied when this endpoints is called
                // They are all provided here for the bookingEngine to choose the correct endpoint
                // The auth token must also be provided from the associated authentication method
                string clientId = AuthenticationHelper.GetClientIdFromAuth(Request, User);
                return _bookingEngine.GetOrdersRPDEPageForFeed(clientId, afterTimestamp, afterId, afterChangeNumber).GetContentResult();
            }
            catch (OpenBookingException obe)
            {
                return obe.ErrorResponseContent.GetContentResult();
            }
        }

        // POST api/openbooking/test-interface/scheduledsession
        [HttpPost]
        [Route("test-interface/{type}")]
        public HttpResponseMessage Post(string type, [FromBody] string @event)
        {
            try
            {
                string clientId = AuthenticationHelper.GetClientIdFromAuth(Request, User);
                return _bookingEngine.CreateTestData(clientId, type, @event).GetContentResult();
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
                string clientId = AuthenticationHelper.GetClientIdFromAuth(Request, User);
                return _bookingEngine.DeleteTestData(clientId, type, name).GetContentResult();
            }
            catch (OpenBookingException obe)
            {
                return obe.ErrorResponseContent.GetContentResult();
            }
        }

    }
}
