using System.Collections.Generic;
using System.IO;

namespace RDPro.Utils
{
    public static class RdpParser
    {
        public static Dictionary<string, string> Parse(string filePath)
        {
            var result = new Dictionary<string, string>();
            foreach (var line in File.ReadAllLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(':', 3);
                if (parts.Length == 3)
                {
                    var key = parts[0].Trim();
                    var value = parts[2].Trim();
                    result[key] = value;
                }
            }
            return result;
        }
    }
}
