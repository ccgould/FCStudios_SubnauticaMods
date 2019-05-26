using FCS_AIJetStreamT242.Buildable;
using FCSCommon.Utilities;
using System;

namespace FCS_AIJetStreamT242.Model
{
    internal static class BiomeManager
    {
        internal static AISolutionsData.BiomeItem GetBiomeData(string biome)
        {
            return AIJetStreamT242Buildable.JetStreamT242Config.BiomeSpeeds.GetOrDefault(biome.ToLower(), new AISolutionsData.BiomeItem { Speed = 90 });
        }
        internal static string GetBiome()
        {
            var biome = string.Empty;
            var curBiome = Player.main.GetBiomeString().ToLower();

            QuickLogger.Debug($"Current Player Biome: {curBiome}");

            var match = GetBiomeSpeed(curBiome);

            if (string.IsNullOrEmpty(match))
            {
                QuickLogger.Debug($"Biome {curBiome} not found! Setting biome to none");
                biome = "none";
            }
            else
            {
                biome = match;
            }

            return biome;
        }
        private static string GetBiomeSpeed(string biome)
        {
            if (string.IsNullOrEmpty(biome)) return String.Empty;

            var result = string.Empty;

            foreach (var biomeItem in AIJetStreamT242Buildable.JetStreamT242Config.BiomeSpeeds)
            {
                QuickLogger.Debug($"Checking for {biome} || Current biome in iteration = {biomeItem.Key}");

                if (biome.StartsWith(biomeItem.Key))
                {
                    result = biomeItem.Key;
                    QuickLogger.Debug($"Find Biome Result = {result}");
                    break;
                }
            }
            return result;
        }
    }
}
