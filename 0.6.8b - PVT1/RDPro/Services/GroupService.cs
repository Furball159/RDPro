using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RDPro.Models;

namespace RDPro.Services
{
    public static class GroupService
    {
        private static string GetGroupsFilePath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string rdProFolder = Path.Combine(appDataPath, "RD Pro");
            return Path.Combine(rdProFolder, "groups.json");
        }

        public static List<ConnectionGroup> LoadGroups()
        {
            string filePath = GetGroupsFilePath();
            if (!File.Exists(filePath))
            {
                return new List<ConnectionGroup>();
            }

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<ConnectionGroup>>(json) ?? new List<ConnectionGroup>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading groups: {ex.Message}");
                return new List<ConnectionGroup>();
            }
        }

        public static void SaveGroups(List<ConnectionGroup> groups)
        {
            string filePath = GetGroupsFilePath();
            try
            {
                string json = JsonConvert.SerializeObject(groups, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving groups: {ex.Message}");
            }
        }
    }
}