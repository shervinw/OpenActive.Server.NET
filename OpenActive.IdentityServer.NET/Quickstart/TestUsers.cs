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
                    new Claim(JwtClaimTypes.Name, "Example Booking Partner"),
                    //new Claim("https://openactive.io/sellerName", "Test"),
                    //new Claim("https://openactive.io/sellerLogo", "Test"),
                    //new Claim("https://openactive.io/sellerUrl", "Test"),
                    //new Claim("https://openactive.io/sellerId", "Test"),
                    //new Claim("https://openactive.io/clientId", "Test"),
                    //new Claim("https://openactive.io/bookingServiceName", "Test"),
                    //new Claim("https://openactive.io/bookingServiceUrl", "Test")
                }
            }
        };
    }
}