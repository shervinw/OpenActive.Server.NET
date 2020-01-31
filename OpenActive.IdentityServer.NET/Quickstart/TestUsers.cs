// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Security.Claims;

namespace src
{
    public class TestUsers
    {
        public static List<TestUser> Users = new List<TestUser>
        {
            new TestUser{SubjectId = "818727", Username = "alice", Password = "alice", 
                Claims = 
                {
                    new Claim("https://openactive.io/sellerName", "Example Seller"),
                    new Claim("https://openactive.io/sellerId", "Example Seller Id_asdfiosjudg"),
                    new Claim("https://openactive.io/sellerUrl", "http://abc.com"),
                    new Claim("https://openactive.io/sellerLogo", "http://abc.com/logo.jpg"),
                    new Claim("https://openactive.io/bookingServiceName", "Example Sellers Booking Service"),
                    new Claim("https://openactive.io/bookingServiceUrl", "http://abc.com/booking-service")
                }
            }
        };
    }
}