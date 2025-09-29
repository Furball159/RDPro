using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using RDPro.Models;

namespace RDPro.Services;

public static class ConnectionManager
{
    private static readonly string ConnFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                     "RD Pro", "Connections");

    public static ObservableCollection<RdpConnection> LoadConnections()
    {
        var list = new ObservableCollection<RdpConnection>();

        if (!Directory.Exists(ConnFolder))
            Directory.CreateDirectory(ConnFolder);

        foreach (var file in Directory.EnumerateFiles(ConnFolder, "*.rdpx"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var conn = JsonSerializer.Deserialize<RdpConnection>(json);
                if (conn != null)
                    list.Add(conn);
            }
            catch
            {
                // TODO: add logging for invalid files
            }
        }

        return list;
    }

    public static void SaveConnection(RdpConnection conn)
    {
        if (!Directory.Exists(ConnFolder))
            Directory.CreateDirectory(ConnFolder);

        // Sanitize filename (replace invalid chars)
        var safeName = string.Join("_", conn.Name.Split(Path.GetInvalidFileNameChars()));
        var file = Path.Combine(ConnFolder, $"{safeName}.rdpx");

        var json = JsonSerializer.Serialize(conn, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(file, json);
    }
}
