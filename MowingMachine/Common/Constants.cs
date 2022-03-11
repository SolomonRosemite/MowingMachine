using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using MowingMachine.Models;

namespace MowingMachine.Common
{
    public static class Constants
    {
        private static readonly Dictionary<FieldType, int> _ExpensesDictionary = new()
        {
            { FieldType.Grass, 10 },
            { FieldType.ChargingStation, 10 },
            { FieldType.MowedLawn, 10 },
            
            { FieldType.CobbleStone, 5 },
            { FieldType.Sand, 20 },
        };

        public static double TurnExpense => 4;
        
        public static double TranslateMoveToExpense(FieldType fieldType)
        {
            if (fieldType is FieldType.Water)
            {
                throw new ArgumentException("Water is a field which cant be stepped on");
            }
            return _ExpensesDictionary.First(e => e.Key == fieldType).Value;
        }

        public static int[][] DefaultMapSample
        {
            get
            {
                int[][] sample =
                {
                    new[] {1, 1, 1, 1, 1, 1, 1, 6, 6, 6, 1, 1, 1, 1, 1, 1, 1, 6, 6, 6},
                    new[] {1, 1, 6, 6, 6, 1, 1, 1, 1, 6, 1, 1, 6, 6, 6, 1, 1, 1, 1, 6},
                    new[] {1, 1, 6, 6, 6, 6, 1, 1, 1, 6, 1, 1, 6, 6, 6, 6, 1, 1, 1, 6},
                    new[] {1, 1, 6, 6, 6, 6, 1, 1, 1, 1, 1, 1, 6, 6, 6, 6, 1, 1, 1, 1},
                    new[] {1, 1, 1, 1, 1, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 1, 1, 1, 1},
                    new[] {1, 1, 1, 1, 1, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3, 1, 1, 1, 1},
                    new[] {1, 1, 1, 1, 1, 3, 1, 1, 0, 1, 1, 1, 1, 1, 1, 3, 1, 1, 5, 1},
                    new[] {1, 6, 3, 3, 3, 3, 1, 0, 0, 0, 1, 6, 3, 3, 3, 3, 1, 0, 0, 0},
                    new[] {1, 1, 3, 1, 1, 1, 1, 0, 6, 0, 1, 1, 3, 1, 1, 1, 1, 0, 6, 0},
                    new[] {1, 1, 3, 1, 1, 1, 0, 0, 0, 0, 1, 1, 3, 1, 1, 1, 0, 0, 0, 0},
                    
                    new[] {1, 1, 3, 1, 1, 1, 0, 0, 0, 0, 1, 1, 3, 1, 1, 1, 0, 0, 0, 0},
                    new[] {1, 1, 3, 1, 1, 1, 0, 0, 0, 0, 1, 1, 3, 1, 1, 1, 0, 0, 0, 0},
                    new[] {1, 1, 3, 1, 1, 1, 0, 0, 0, 0, 1, 1, 3, 1, 1, 1, 0, 0, 0, 0},
                    new[] {1, 1, 3, 1, 1, 1, 0, 0, 0, 0, 1, 1, 3, 1, 1, 1, 0, 0, 0, 0},
                    new[] {1, 1, 3, 1, 1, 1, 0, 0, 0, 0, 1, 1, 3, 1, 1, 1, 0, 0, 0, 0},
                    new[] {1, 1, 3, 1, 1, 1, 0, 0, 0, 0, 1, 1, 3, 1, 1, 1, 0, 0, 0, 0},
                    new[] {1, 1, 3, 1, 1, 1, 0, 0, 0, 0, 1, 1, 3, 1, 1, 1, 0, 0, 0, 0},
                    new[] {1, 1, 3, 1, 1, 1, 0, 0, 0, 0, 1, 1, 3, 1, 1, 1, 0, 0, 0, 0},
                    new[] {1, 1, 3, 1, 1, 1, 0, 0, 0, 0, 1, 1, 3, 1, 1, 1, 0, 0, 0, 0},
                    new[] {1, 1, 3, 1, 1, 1, 0, 0, 0, 0, 1, 1, 3, 1, 1, 1, 0, 0, 0, 0},
                };

                return GetMapFromJson() ?? sample;
            }
        }

        private static int[][] GetMapFromJson()
        {
            try
            {
                var json = File.ReadAllText(
                    GetJsonFileName(@"C:\Users\kanu-agha\RiderProjects\MowingMachine\MowingMachine\Maps\", false));

                Console.WriteLine(GetJsonFileName(@"C:\Users\kanu-agha\RiderProjects\MowingMachine\MowingMachine\Maps\", false));
                return JsonSerializer.Deserialize<int[][]>(json);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load existing map. See Exception for more info.");
                Console.WriteLine(e);
            }

            return null;
        }

        public static void SaveMapAsJson(int[][] map)
        {
            try
            {
                var jsonContent = JsonSerializer.Serialize(map);
                var fileName = GetJsonFileName(@"C:\Users\kanu-agha\RiderProjects\MowingMachine\MowingMachine\Maps\");

                File.WriteAllText(fileName, jsonContent);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to save map. See Exception for more info.");
                Console.WriteLine(e);
            }
        }

        private static string GetJsonFileName(string path, bool newFileName = true)
        {
            var x = Directory.GetFiles(path);
            var files = Directory.GetFiles(path).Select(p
                => p.Substring(p.LastIndexOf('-') + 1, p.LastIndexOf('.') - p.LastIndexOf('-') - 1));
            var mapIds = files.Select(int.Parse);

            if (!newFileName)
            {
                if (!mapIds.Any())
                    throw new Exception("No maps found");
                
                return path + $"Map-{mapIds.Max()}.json";
            }
            
            if (!mapIds.Any())
                return path + "Map-1.json";
            
            return path + $"Map-{mapIds.Max() + 1}.json";
        }
    }
}