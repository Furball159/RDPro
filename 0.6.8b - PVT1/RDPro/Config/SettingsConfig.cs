using System;
using System.IO;
using System.Text.Json;

namespace RDPro.Config
{
    public class SettingsConfig
    {
        public string Theme { get; set; } = "Default";       // "Default" | "Light" | "Dark"
        public bool PreferUserAccentColor { get; set; } = true;
        public string CustomAccentHex { get; set; } = "#FF0078D7"; // Windows blue

        private static string ConfigDir =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RD Pro");

        private static string ConfigPath => Path.Combine(ConfigDir, "settings.json");

        public static SettingsConfig Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    var cfg = JsonSerializer.Deserialize<SettingsConfig>(json);

                    if (cfg != null)
                    {
                        // Defensive defaults
                        cfg.Theme ??= "Default";
                        cfg.CustomAccentHex ??= "#FF0078D7";
                        return cfg;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RDPro] Settings file corrupted: {ex.Message}");
            }

            // ✅ If we got here → return defaults and reset file
            var defaults = new SettingsConfig();
            defaults.Save();
            return defaults;
        }

        public void Save()
        {
            try
            {
                if (!Directory.Exists(ConfigDir))
                    Directory.CreateDirectory(ConfigDir);

                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RDPro] Failed to save settings: {ex.Message}");
            }
        }
    }
}
