// RDPro/Services/ConnectionFileService.cs (DEFINITIVE VERSION: COMPLETE SAVE AND LOAD)

using System;
using System.IO;
using System.Text.Json;
using RDPro.Models; 
using System.Text; 
using System.Linq; 

namespace RDPro.Services;

public static class ConnectionFileService // <-- It is static
{
    // Uses LocalApplicationData for persistence
    private static string GetBaseDirectory()
    {
        const string appName = "RD Pro"; 
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
    }
    
    private static string GetLegacyFolder() => Path.Combine(GetBaseDirectory(), "Connections", "Legacy");
    private static string GetEnhancedFolder() => Path.Combine(GetBaseDirectory(), "Connections", "Enhanced");
    
    // CRITICAL: Method for the ViewModel to find the folder to load from.
    public static string GetEnhancedPath() => GetEnhancedFolder();

    // Helper method to generate RDP content
    private static string GenerateRdpContent(ConnectionDetails connection)
    {
        var rdp = new StringBuilder();

        rdp.AppendLine($"full address:s:{connection.ServerAddress}");
        rdp.AppendLine("screen mode id:i:2");

        // If a username is provided, add it to the RDP file so mstsc can pre-fill the user name field.
        if (!string.IsNullOrWhiteSpace(connection.Username))
        {
            var usernameRaw = connection.Username.Trim();

            // If the username includes a domain (DOMAIN\user), split and write both domain and username.
            if (usernameRaw.Contains('\\'))
            {
                var parts = usernameRaw.Split(new[] { '\\' }, 2);
                var domain = parts[0];
                var user = parts.Length > 1 ? parts[1] : string.Empty;
                if (!string.IsNullOrWhiteSpace(domain)) rdp.AppendLine($"domain:s:{domain}");
                // Write username as DOMAIN\user which some mstsc clients expect.
                if (!string.IsNullOrWhiteSpace(user)) rdp.AppendLine($"username:s:{domain}\\{user}");
            }
            else
            {
                // Otherwise write username as-is (supports UPN like user@domain)
                rdp.AppendLine($"username:s:{usernameRaw}");
            }

            // Credential-related flags that can influence how mstsc prompts/uses credentials.
            // enablecredsspsupport:i:1 — enable CredSSP support (modern auth)
            // promptcredentialonce:i:1 — prompt for credentials once (can help with pre-fill behavior)
            rdp.AppendLine("enablecredsspsupport:i:1");
            rdp.AppendLine("promptcredentialonce:i:1");
        }
        
        if (!string.IsNullOrWhiteSpace(connection.Gateway) && !string.Equals(connection.Gateway, "No Gateway", StringComparison.OrdinalIgnoreCase))
        {
            // Normalize gateway string: trim, strip quotes, and take the first token if whitespace present.
            var gatewayRaw = connection.Gateway.Trim();
            if (gatewayRaw.StartsWith("\"") && gatewayRaw.EndsWith("\""))
                gatewayRaw = gatewayRaw.Substring(1, gatewayRaw.Length - 2).Trim();
            // If the user accidentally pasted a full RDP-style line like "gatewayhostname:s:host", extract the host.
            if (gatewayRaw.StartsWith("gatewayhostname", StringComparison.OrdinalIgnoreCase))
            {
                var parts = gatewayRaw.Split(':');
                if (parts.Length >= 3)
                {
                    gatewayRaw = parts[2].Trim();
                }
            }
            // If there are spaces, take the first token (hostname)
            if (gatewayRaw.Contains(' '))
            {
                gatewayRaw = gatewayRaw.Split(' ')[0].Trim();
            }

            if (!string.IsNullOrWhiteSpace(gatewayRaw))
            {
                // gatewayhostname is required to point to the gateway server
                rdp.AppendLine($"gatewayhostname:s:{gatewayRaw}");
                // gatewayusagemethod: 1 => Always use a Remote Desktop gateway
                rdp.AppendLine("gatewayusagemethod:i:1");
                // gatewayprofileusagemethod: 1 => Use explicit settings ("Use these RD Gateway server settings")
                rdp.AppendLine("gatewayprofileusagemethod:i:1");
                // credential source: 0 => Ask for password (NTLM). Keep this conservative.
                rdp.AppendLine("gatewaycredentialssource:i:0");

                try { Console.WriteLine($"[RDPGEN] Normalized gateway for '{connection.ConnectionName}' => '{gatewayRaw}'"); } catch { }
            }
        }
        
        var content = rdp.ToString();
        try { Console.WriteLine($"[RDPGEN] Generated RDP content for '{connection.ConnectionName}':\n{content}"); } catch { }
        return content;
    }

    public static void SaveConnection(ConnectionDetails connection)
    {
        // 1. Ensure directories exist
        Directory.CreateDirectory(GetLegacyFolder());
        Directory.CreateDirectory(GetEnhancedFolder());
        
        // 2. CRITICAL FILENAME FIX: Use ONLY ConnectionID for the filename for simplicity and reliability.
        // This MUST be simple to match the loading logic.
        string fileName = connection.ConnectionID;
        
        string legacyPath = Path.Combine(GetLegacyFolder(), $"{fileName}.rdp");
        string enhancedPath = Path.Combine(GetEnhancedFolder(), $"{fileName}.rdpx");

    // 3. Write Legacy RDP File
    string rdpContent = GenerateRdpContent(connection);
    try { File.WriteAllText(legacyPath, rdpContent); } catch (Exception ex) { Console.WriteLine($"Error writing .rdp file: {ex.Message}"); }

        // 4. Write Enhanced RDPX File (JSON)
        try
        {
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            string jsonContent = JsonSerializer.Serialize(connection, jsonOptions);
            File.WriteAllText(enhancedPath, jsonContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FATAL ERROR writing RDPX file: {ex.Message}");
        }
    }
    
    // CRITICAL: The Load method is added to read the file back into an object.
    public static ConnectionDetails? Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            string jsonContent = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<ConnectionDetails>(jsonContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading or deserializing RDPX file at {filePath}: {ex.Message}");
            return null;
        }
    }

    // Create a temporary .rdp file containing the connection info but WITHOUT any gateway settings.
    // Returns the full path to the temporary .rdp file.
    public static string CreateTemporaryRdpWithoutGateway(ConnectionDetails connection)
    {
        // Generate RDP content but explicitly ignore any gateway setting
        var rdp = new StringBuilder();
        rdp.AppendLine($"full address:s:{connection.ServerAddress}");
        rdp.AppendLine("screen mode id:i:2");

        // If a username is provided, include it so the temporary .rdp pre-fills the username.
        if (!string.IsNullOrWhiteSpace(connection.Username))
        {
            var usernameRaw = connection.Username.Trim();
            if (usernameRaw.Contains('\\'))
            {
                var parts = usernameRaw.Split(new[] { '\\' }, 2);
                var domain = parts[0];
                var user = parts.Length > 1 ? parts[1] : string.Empty;
                if (!string.IsNullOrWhiteSpace(domain)) rdp.AppendLine($"domain:s:{domain}");
                if (!string.IsNullOrWhiteSpace(user)) rdp.AppendLine($"username:s:{domain}\\{user}");
            }
            else
            {
                rdp.AppendLine($"username:s:{usernameRaw}");
            }

            rdp.AppendLine("enablecredsspsupport:i:1");
            rdp.AppendLine("promptcredentialonce:i:1");
        }

        // Do not include any gateway fields here

        // Write to a temp file with .rdp extension
        string tempPath = Path.Combine(Path.GetTempPath(), $"rdpro_temp_{Guid.NewGuid()}.rdp");
        File.WriteAllText(tempPath, rdp.ToString());
        return tempPath;
    }
}
