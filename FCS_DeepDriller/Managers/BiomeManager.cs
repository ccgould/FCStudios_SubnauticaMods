using FCS_DeepDriller.Buildable;
using FCSCommon.Utilities;
using System;

namespace FCS_DeepDriller.Managers
{
    internal static class BiomeManager
    {
        //internal static AISolutionsData.BiomeItem GetBiomeData(string biome)
        //{
        //    return AIJetStreamT242Buildable.JetStreamT242Config.BiomeSpeeds.GetOrDefault(biome.ToLower(), new AISolutionsData.BiomeItem { Speed = 0 });
        //}

        internal static string GetBiome()
        {
            var biome = string.Empty;
            var curBiome = Player.main.GetBiomeString().ToLower();

            QuickLogger.Debug($"Current Player Biome: {curBiome}");

            var match = FindMatchingBiome(curBiome);

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
        private static string FindMatchingBiome(string biome)
        {

            if (string.IsNullOrEmpty(biome)) return String.Empty;

            var result = string.Empty;
            QuickLogger.Info("// ============================= IN FindMatchingBiome ============================= //");

            foreach (var biomeItem in FCSDeepDrillerBuildable.DeepDrillConfig.BiomeOres)
            {
                QuickLogger.Info($"Checking for {biome} || Current biome in iteration = {biomeItem.Key}");

                if (biome.StartsWith(biomeItem.Key))
                {
                    result = biomeItem.Key;
                    QuickLogger.Info($"Find Biome Result = {result}");
                    break;
                }
            }

            QuickLogger.Info("// ============================= IN FindMatchingBiome ============================= //");

            return result;
        }
    }
}
