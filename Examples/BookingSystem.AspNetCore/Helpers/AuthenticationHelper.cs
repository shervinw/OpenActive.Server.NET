using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using OpenActive.NET;
using OpenActive.Server.NET.OpenBookingHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookingSystem.AspNetCore.Helpers
{
    public static class AuthenticationHelper
    {
        public static (string clientId, Uri sellerId) GetIdsFromAuth(HttpRequest request, ClaimsPrincipal principal, bool requireSellerId)
        {
            // NOT FOR PRODUCTION USE: Please remove this block in production
            if (request.Headers.TryGetValue(AuthenticationTestHeaders.ClientId, out StringValues testClientId)
                && testClientId.Count == 1
                && (!requireSellerId || (request.Headers.TryGetValue(AuthenticationTestHeaders.SellerId, out StringValues testSellerId) && testSellerId.FirstOrDefault().ParseUrlOrNull() != null))
                )
            {
                return (testClientId.FirstOrDefault(), testSellerId.FirstOrDefault().ParseUrlOrNull());
            }

            // For production use: Get Ids from JWT
            var clientId = principal.GetClientId();
            var sellerId = principal.GetSellerId().ParseUrlOrNull();
            if (clientId != null && (sellerId != null || !requireSellerId))
            {
                return (clientId, sellerId);
            }
            else
            {
                throw new OpenBookingException(new InvalidAPITokenError());
            }
        }

        public static (string clientId, Uri sellerId) GetIdsFromAuth(HttpRequest request, ClaimsPrincipal principal)
        {
            return GetIdsFromAuth(request, principal, true);
        }

        public static string GetClientIdFromAuth(HttpRequest request, ClaimsPrincipal principal)
        {
            return GetIdsFromAuth(request, principal, false).clientId;
        }
    }
}
