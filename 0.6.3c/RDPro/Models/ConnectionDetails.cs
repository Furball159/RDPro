// RDPro/Models/ConnectionDetails.cs
namespace RDPro.Models
{
    public class ConnectionDetails
    {
        public string ConnectionName { get; set; } = string.Empty;
        public string ServerAddress { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Gateway { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        // Indicates whether the connection is in the user's favourites
        public bool IsFavourite { get; set; } = false;
        public string ConnectionID { get; set; } = string.Empty;
    }
}