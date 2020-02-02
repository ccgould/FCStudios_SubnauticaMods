using FCSCommon.Utilities;
using System;
using System.Collections.Generic;

namespace FCS_DeepDriller.Managers
{
    internal static class BiomeManager
    {
        internal static List<TechType> GetBiomeData(string biome)
        {
            QuickLogger.Debug("Finding Biome Data.");

            foreach (var biomeOre in QPatch.Configuration.BiomeOresTechType)
            {
                QuickLogger.Debug($"Comparing {biomeOre.Key}");
                QuickLogger.Debug($" To {biome.ToLower()}");

                if (biomeOre.Key == biome.ToLower())
                {
                    QuickLogger.Debug("Found Biome Data.");

                    return biomeOre.Value;
                }
            }
            return new List<TechType>();
        }

        internal static string GetBiome()
        {
            var biome = string.Empty;
            var curBiome = Player.main.GetBiomeString().ToLower();

            QuickLogger.Debug($"Current Player Biome: {curBiome}");

            var match = FindMatchingBiome(curBiome);

            if (string.IsNullOrEmpty(match))
            {
                QuickLogger.Debug($"Biome {curBiome} not found! Setting biome to none");
                biome = NoneString;
            }
            else
            {
                biome = match;
            }

            return biome;
        }

        internal static string NoneString { get; } = "none";

        private static string FindMatchingBiome(string biome)
        {

            if (string.IsNullOrEmpty(biome)) return String.Empty;

            var result = string.Empty;
            QuickLogger.Debug("// ============================= IN FindMatchingBiome ============================= //");

            foreach (var biomeItem in QPatch.Configuration.BiomeOresTechType)
            {
                QuickLogger.Debug($"Checking for {biome} || Current biome in iteration = {biomeItem.Key}");

                if (biome.StartsWith(biomeItem.Key))
                {
                    result = biomeItem.Key;
                    QuickLogger.Info($"Find Biome Result = {result}");
                    break;
                }
            }

            QuickLogger.Debug("// ============================= IN FindMatchingBiome ============================= //");

            return result;
        }
    }
}
