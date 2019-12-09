using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET.OpenBookingHelper
{
    public class ClientRegistrationModel
    {
        [JsonProperty(OidcConstants.RegistrationResponse.ClientId)]
        public string ClientId { get; set; }

        [JsonProperty(OidcConstants.ClientMetadata.ClientName)]
        public string ClientName { get; set; }

        [JsonProperty(OidcConstants.ClientMetadata.ClientUri)]
        public string ClientUri { get; set; }

        [JsonProperty(OidcConstants.ClientMetadata.LogoUri)]
        public string LogoUri { get; set; }

        [JsonProperty(OidcConstants.ClientMetadata.GrantTypes)]
        public IEnumerable<string> GrantTypes { get; set; }

        [JsonProperty(OidcConstants.ClientMetadata.RedirectUris)]
        public IEnumerable<string> RedirectUris { get; set; } = new List<string>();

        public string Scope { get; set; } = "openid profile email";
    }

    public class ClientRegistrationResponse : ClientRegistrationModel
    {
        [JsonProperty(OidcConstants.RegistrationResponse.ClientSecret)]
        public string ClientSecret { get; set; }
    }
}
