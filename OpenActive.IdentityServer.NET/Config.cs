﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> Ids =>
            CustomIdentityResource.GetIdentityResources();

        public static IEnumerable<ApiResource> Apis =>
           new List<ApiResource>
           {
                new ApiResource
                {
                    Name = "openbooking",

                    // secret for using introspection endpoint
                    ApiSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // this API defines three scopes
                    Scopes =
                    {
                        new Scope()
                        {
                            Name = "openactive-openbooking",
                            DisplayName = "Access to C1, C2, B Endpoints",
                            UserClaims = { JwtClaimTypes.Name, "https://openactive.io/sellerId", "https://openactive.io/clientId" }
                        },
                        new Scope
                        {
                            Name = "openactive-ordersfeed",
                            DisplayName = "Access to Orders RPDE Feeds",
                            UserClaims = { JwtClaimTypes.Name, "https://openactive.io/clientId" }
                        },
                        new Scope
                        {
                            Name = "oauth-dymamic-client-update",
                            DisplayName = "Access to perform a Dynamic Client Update",
                            UserClaims = { JwtClaimTypes.Name, "https://openactive.io/clientId" }
                        }
                    }
                }
           };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                // clients list needs to be obtained from booking system administration page
                // client credentials
                new Client
                {
                    ClientId = "oijsadgfoijasg",
                    ClientName = "Example Booking Partner",
                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // scopes that client has access to
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "openactive-openbooking",
                        "openactive-ordersfeed",
                        "oauth-dymamic-client-update",
                        "openactive-identity"
                    },
                    // this claim should come from the booking administration database
                    Claims = new List<System.Security.Claims.Claim>()
                    {
                        new System.Security.Claims.Claim("https://openactive.io/clientId", "abc")
                    },
                    ClientClaimsPrefix = "",
                    AlwaysSendClientClaims = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowOfflineAccess = true, //enables sending refresh tokens
                    UpdateAccessTokenClaimsOnRefresh = true //ensures claims are updated on refresh
                },
                // 
                new Client
                {
                    ClientId = "dfjlosdkgsdgsdfh",
                    ClientName = "Example Booking Partner",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,
                    RequireConsent = true,
                    RequirePkce = true,
                    

                    // where to redirect to after login
                    RedirectUris = { "http://localhost:5002/signin-oidc" },

                    // where to redirect to after logout
                    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "openactive-openbooking",
                        "oauth-dymamic-client-update",
                        "openactive-identity"
                    }, 
                    // this claim should come from the booking administration database
                    Claims = new List<System.Security.Claims.Claim>()
                    {
                        new System.Security.Claims.Claim("https://openactive.io/clientId", "dfjlosdkgsdgsdfh")
                    },
                    ClientClaimsPrefix = "",
                    AlwaysSendClientClaims = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowOfflineAccess = true, //enables sending refresh tokens
                    UpdateAccessTokenClaimsOnRefresh = true //ensures claims are updated on refresh

                    
                }
            };

    }
}