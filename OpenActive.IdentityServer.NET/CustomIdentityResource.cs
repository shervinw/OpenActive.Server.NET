using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer
{
    public class CustomIdentityResource
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            // claims configured here will be included in the id_token???
            var customProfile = new IdentityResource(
                name: "openactive.profile",
                displayName: "Openactive profile",
                claimTypes: new[] { "https://openactive.io/sellerId", "https://openactive.io/clientId" });

            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                customProfile
            };
        }
    }
}
