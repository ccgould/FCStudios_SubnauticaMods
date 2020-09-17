using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FCS_DeepDriller.Configuration;
using FCSTechFabricator.Configuration;
using Oculus.Newtonsoft.Json;
using UnityEngine;
using UWE;

namespace FCS_DeepDriller.Managers
{
    internal static class BiomeManager
    {

        public static List<TechType> Resources = new List<TechType>()
        {
            TechType.AluminumOxide, 
            TechType.Sulphur, 
            TechType.Diamond,
            TechType.Kyanite,
            TechType.Lead, 
            TechType.Lithium , 
            TechType.Magnetite,
            TechType.Nickel, 
            TechType.Quartz, 
            TechType.Silver, 
            TechType.UraniniteCrystal,
            TechType.Salt, 
            TechType.Titanium, 
            TechType.Copper, 
            TechType.Gold, 
            TechType.LimestoneChunk,
            TechType.ShaleChunk,
            TechType.SandstoneChunk,
            TechType.DrillableSalt,
            TechType.DrillableQuartz,
            TechType.DrillableCopper,
            TechType.DrillableTitanium,
            TechType.DrillableLead,
            TechType.DrillableSilver,
            TechType.DrillableDiamond,
            TechType.DrillableGold,
            TechType.DrillableMagnetite,
            TechType.DrillableLithium,
            TechType.DrillableMercury,
            TechType.DrillableUranium,
            TechType.DrillableAluminiumOxide,
            TechType.DrillableNickel,
            TechType.DrillableSulphur,
            TechType.DrillableKyanite
        };

        private static List<TechType> _allOres = new List<TechType>();


        private static void FindMatchingBiome(string biome, out string biomeType)
        {
            //ILZ

            if (biome.ToLower().EndsWith("Mesa"))
            {
                biomeType = "Mesas";
            }
            else if (biome.ToLower().StartsWith("kelp"))
            {
                biomeType = "Kelp";
            }
            else if (biome.ToLower().StartsWith("bloodkelp"))
            {
                biomeType = "BloodKelp";
            }
            else if (biome.ToLower().StartsWith("lostriver"))
            {
                biomeType = "LostRiver";
            }
            else if (biome.ToLower().StartsWith("ilz"))
            {
                biomeType = "InactiveLavaZone";
            }
            else if (biome.ToLower().StartsWith("alz"))
            {
                biomeType = "ActiveLavaZone";
            }
            else if (biome.ToLower().StartsWith("lava"))
            {
                biomeType = "ActiveLavaZone";
            }
            else
            {
                biomeType = biome;
            }
        }

        internal static string GetBiome()
        {
            if (Player.main != null)
            {
                return Player.main.GetBiomeString();
            }
            return string.Empty;
        }

        internal static string CalculateBiome(Transform tr)
        {
            if (LargeWorld.main)
            {
                return LargeWorld.main.GetBiome(tr.position);
            }
            return "<unknown>";
        }

        internal static List<TechType> FindBiomeLoot(Transform tr, string currentBiome)
        {
            Dictionary<BiomeType, List<TechType>> biomeLoot = new Dictionary<BiomeType, List<TechType>>();

            var loot = new List<TechType>();

            foreach (BiomeType biomeType in Enum.GetValues(typeof(BiomeType)))
            {
                if (tr == null)
                {
                    QuickLogger.Debug("FindBiomeLoot: Transform cannot be null");
                    return null;
                }
   

                if (string.IsNullOrEmpty(currentBiome))
                {
                    QuickLogger.Error($"No biome found!");
                    return null;
                }
                
                //QuickLogger.Debug($"Biome Found: {currentBiome}");

                FindMatchingBiome(currentBiome, out var matchingBiome);

                if (biomeType.AsString().StartsWith(matchingBiome, StringComparison.OrdinalIgnoreCase))
                {
                    if (Mod.LootDistributionData == null)
                    {
                        QuickLogger.Debug("LootDistributionData is null");
                        return null;
                    }
                    
                    if (!Mod.LootDistributionData.GetBiomeLoot(biomeType, out LootDistributionData.DstData data))
                    {
                        QuickLogger.Debug("DstData is null");
                        continue;
                    }

                    foreach (LootDistributionData.PrefabData prefabData in data.prefabs)
                    {
                        if (prefabData.classId.ToLower() != "none")
                        {
                            if (WorldEntityDatabase.TryGetInfo(prefabData.classId, out WorldEntityInfo wei))
                            {
                                if (wei == null)
                                {
                                    QuickLogger.Debug("WorldEntityInfo is null");
                                    continue;
                                }

                                if (Resources.Contains(wei.techType))
                                {
                                    if (!biomeLoot.ContainsKey(biomeType))
                                        biomeLoot[biomeType] = new List<TechType>();

                                    biomeLoot[biomeType].Add(wei.techType);

                                    QuickLogger.Debug($"Added Resource: {wei.techType} in biome {biomeType}");
                                }
                            }
                        }
                    }
                }
            }
            
            foreach (KeyValuePair<BiomeType, List<TechType>> pair in biomeLoot)
            {
                foreach (TechType techType in pair.Value)
                {
                    if (techType.ToString().EndsWith("Chunk", StringComparison.OrdinalIgnoreCase) ||
                        techType.ToString().StartsWith("Drillable", StringComparison.OrdinalIgnoreCase))
                    {
                        GetResourceForSpecial(techType, loot);
                    }
                    else
                    {
                        loot.Add(techType);
                    }

                    loot = loot.Distinct().ToList();
                }
            }

            AdditionalLoot(ref loot, currentBiome);

            return loot;
        }

        /// <summary>
        /// Get all player added ores to the list
        /// </summary>
        /// <param name="defaultLoot"></param>
        /// <param name="currentBiome"></param>
        /// <returns></returns>
        private static void AdditionalLoot(ref List<TechType> defaultLoot, string currentBiome)
        {
            QPatch.Configuration.Convert();
#if DEBUG
            QuickLogger.Debug($"Addition Ore: {QPatch.Configuration.BiomeOresTechType.Count}");
#endif

            foreach (KeyValuePair<string, List<TechType>> valuePair in QPatch.Configuration.BiomeOresTechType)
            {
#if DEBUG
                QuickLogger.Debug($"Checking if biomes match {currentBiome} => {valuePair.Key} = {valuePair.Key.Equals(currentBiome,StringComparison.OrdinalIgnoreCase)}");
#endif
                if (valuePair.Key.Equals(currentBiome,StringComparison.OrdinalIgnoreCase))
                {
                    foreach (TechType techType in valuePair.Value)
                    {
                        if (!defaultLoot.Contains(techType) && Resources.Contains(techType))
                        {
                            defaultLoot.Add(techType);
                        }
                    }
                }
            }
        }

        private static void GetResourceForSpecial(TechType techType, List<TechType> items)
        {
            switch (techType)
            {
                case TechType.LimestoneChunk:
                    items.Add(TechType.Titanium);
                    items.Add(TechType.Copper);
                    break;
                case TechType.ShaleChunk:
                    items.Add(TechType.Diamond);
                    items.Add(TechType.Gold);
                    items.Add(TechType.Lithium);
                    break;
                case TechType.SandstoneChunk:
                    items.Add(TechType.Silver);
                    items.Add(TechType.Gold);
                    items.Add(TechType.Lead);
                    break;
                case TechType.DrillableSalt:
                    items.Add(TechType.Salt);
                    break;
                case TechType.DrillableQuartz:
                    items.Add(TechType.Quartz);
                    break;
                case TechType.DrillableCopper:
                    items.Add(TechType.Copper);
                    break;
                case TechType.DrillableTitanium:
                    items.Add(TechType.Titanium);
                    break;
                case TechType.DrillableLead:
                    items.Add(TechType.Lead);
                    break;
                case TechType.DrillableSilver:
                    items.Add(TechType.Silver);
                    break;
                case TechType.DrillableDiamond:
                    items.Add(TechType.Diamond);
                    break;
                case TechType.DrillableGold:
                    items.Add(TechType.Gold);
                    break;
                case TechType.DrillableMagnetite:
                    items.Add(TechType.Magnetite);
                    break;
                case TechType.DrillableLithium:
                    items.Add(TechType.Lithium);
                    break;
                case TechType.DrillableMercury:
                    items.Add(TechType.MercuryOre);
                    break;
                case TechType.DrillableUranium:
                    items.Add(TechType.UraniniteCrystal);
                    break;
                case TechType.DrillableAluminiumOxide:
                    items.Add(TechType.AluminumOxide);
                    break;
                case TechType.DrillableNickel:
                    items.Add(TechType.Nickel);
                    break;
                case TechType.DrillableSulphur:
                    items.Add(TechType.Sulphur);
                    break;
                case TechType.DrillableKyanite:
                    items.Add(TechType.Kyanite);
                    break;
            }
        }

        public static Dictionary<BiomeType, List<TechType>> GetAllBiomesData()
        {
            Dictionary<BiomeType, List<TechType>> biomeLoot = new Dictionary<BiomeType, List<TechType>>();

            foreach (BiomeType biomeType in Enum.GetValues(typeof(BiomeType)))
            {
                if (Mod.LootDistributionData == null)
                {
                    QuickLogger.Debug("LootDistributionData is null");
                    return null; 
                }

                if (!Mod.LootDistributionData.GetBiomeLoot(biomeType, out LootDistributionData.DstData data))
                {
                    QuickLogger.Debug("DstData is null");
                    continue;
                }

                foreach (LootDistributionData.PrefabData prefabData in data.prefabs)
                {
                    if (prefabData.classId.ToLower() != "none")
                    {
                        if (WorldEntityDatabase.TryGetInfo(prefabData.classId, out WorldEntityInfo wei))
                        {
                            if (wei == null)
                            {
                                QuickLogger.Debug("WorldEntityInfo is null");
                                continue;
                            }

                            if (Resources.Contains(wei.techType))
                            {
                                if (!biomeLoot.ContainsKey(biomeType))
                                    biomeLoot[biomeType] = new List<TechType>();

                                biomeLoot[biomeType].Add(wei.techType);

                                QuickLogger.Debug($"Added Resource: {wei.techType} in biome {biomeType}");
                            }
                        }
                    }
                }

                
                var tempDictionary = new Dictionary<BiomeType, List<TechType>>();

                foreach (KeyValuePair<BiomeType, List<TechType>> pair in biomeLoot)
                {
                    var loot = new List<TechType>();
                    foreach (TechType techType in pair.Value)
                    {
                        if (techType.ToString().EndsWith("Chunk", StringComparison.OrdinalIgnoreCase) ||
                            techType.ToString().StartsWith("Drillable", StringComparison.OrdinalIgnoreCase))
                        {
                            GetResourceForSpecial(techType, loot);
                        }
                        else
                        {
                            loot.Add(techType);
                        }
                    }

                    tempDictionary.Add(pair.Key, loot.Distinct().ToList());
                }
                
                var newList = new Dictionary<string, List<string>>();

                foreach (KeyValuePair<BiomeType, List<TechType>> biomeData in tempDictionary)
                {
                    var name = biomeData.Key.AsString(true).Split('_')[0];
                    if (!newList.ContainsKey(name))
                    {
                        var g = new List<string>();

                        foreach (var techType in biomeData.Value)
                        {
                            g.Add(techType.AsString());
                        }
                        newList.Add(name,g);
                    }
                    else
                    {
                        foreach (TechType techType in biomeData.Value)
                        {
                            if (!newList[name].Contains(techType.AsString()))
                            {
                                newList[name].Add(techType.AsString());
                            }

                            if (!_allOres.Contains(techType))
                            {
                                _allOres.Add(techType);
                            }
                        }
                    }
                }

                //string json = JsonConvert.SerializeObject(newList, Formatting.Indented);
                //File.WriteAllText("Output_Formatted_TechTypeAsString.json", json);

                //string json = JsonConvert.SerializeObject(biomeLoot, Formatting.Indented);
                //File.WriteAllText("Output.json",json);
            }

            return biomeLoot;
        }

        public static bool IsApproved(TechType techType)
        {
            if (_allOres == null || _allOres.Count == 0)
            {
                GetAllBiomesData();
            }

            return _allOres != null && _allOres.Contains(techType);
        }
    }
}
