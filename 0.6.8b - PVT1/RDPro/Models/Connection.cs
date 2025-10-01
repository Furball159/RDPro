namespace RDPro.Models
{
    public class Connection
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;

        // Path to the source file (useful for legacy .rdp tracking)
        public string SourceFile { get; set; } = string.Empty;
    }
}
