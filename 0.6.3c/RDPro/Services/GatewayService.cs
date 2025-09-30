using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using RDPro.Models;

namespace RDPro.Services
{
    public static class GatewayService
    {
        public static event Action? GatewaysChanged;
        private static string GetGatewaysFolder()
        {
            var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RD Pro");
            return Path.Combine(baseDir, "Gateways");
        }

        private static string GetGatewaysFile()
        {
            return Path.Combine(GetGatewaysFolder(), "gateways.json");
        }

        public static List<Gateway> LoadGateways()
        {
            try
            {
                Directory.CreateDirectory(GetGatewaysFolder());
                var file = GetGatewaysFile();
                if (!File.Exists(file)) return new List<Gateway>();

                var json = File.ReadAllText(file);
                var list = JsonSerializer.Deserialize<List<Gateway>>(json);
                if (list == null) return new List<Gateway>();
                return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading gateways: {ex.Message}");
                return new List<Gateway>();
            }
        }

        public static void SaveGateways(IEnumerable<Gateway> gateways)
        {
            try
            {
                Directory.CreateDirectory(GetGatewaysFolder());
                var file = GetGatewaysFile();
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(gateways, options);
                File.WriteAllText(file, json);
                // Notify listeners that gateways were updated
                try { GatewaysChanged?.Invoke(); } catch { }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving gateways: {ex.Message}");
            }
        }
    }
}
