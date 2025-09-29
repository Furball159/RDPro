using System;

namespace RDPro.Models
{
    public class Gateway
    {
        public string Hostname { get; set; } = string.Empty;
        // Optional friendly display name
        public string FriendlyName { get; set; } = string.Empty;

        // If true, the gateway will use the user's PC credentials; otherwise use explicit gateway credentials
        public bool UsePcCredentials { get; set; } = true;

        // Optional gateway-specific credentials (only used if UsePcCredentials == false)
        public string GatewayUsername { get; set; } = string.Empty;
        public string GatewayPassword { get; set; } = string.Empty;
    }
}
