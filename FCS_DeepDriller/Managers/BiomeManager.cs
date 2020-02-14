﻿using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using FCS_DeepDriller.Configuration;
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

        private static readonly SortedDictionary<BiomeType, List<TechType>> _biomeLoot = new SortedDictionary<BiomeType, List<TechType>>();
        
        internal static string CalculateBiome(Transform tr)
        {
            if (LargeWorld.main)
            {
                return LargeWorld.main.GetBiome(tr.position);
            }
            return "<unknown>";
        }

        internal static List<TechType> FindBiomeLoot(Transform tr)
        {
            _biomeLoot.Clear();

            var loot = new List<TechType>();

            foreach (BiomeType biomeType in Enum.GetValues(typeof(BiomeType)))
            {
                if (tr == null)
                {
                    QuickLogger.Error("FindBiomeLoot: Transform cannot be null");
                    return null;
                }
                var currentBiome = CalculateBiome(tr);

                if (string.IsNullOrEmpty(currentBiome))
                {
                    QuickLogger.Error($"No biome found!");
                    return null;
                }

                if (biomeType.AsString().StartsWith(currentBiome, StringComparison.OrdinalIgnoreCase))
                {
                    if (Mod.LootDistributionData == null)
                    {
                        QuickLogger.Error("LootDistributionData is null");
                        return null;
                    }
                    
                    if (!Mod.LootDistributionData.GetBiomeLoot(biomeType, out LootDistributionData.DstData data))
                    {
                        QuickLogger.Error("DstData is null");
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
                                    QuickLogger.Error("WorldEntityInfo is null");
                                    continue;
                                }

                                if (Resources.Contains(wei.techType))
                                {
                                    if (!_biomeLoot.ContainsKey(biomeType))
                                        _biomeLoot[biomeType] = new List<TechType>();

                                    _biomeLoot[biomeType].Add(wei.techType);

                                    QuickLogger.Debug($"Added Resource: {wei.techType} in biome {biomeType}");
                                }
                            }
                        }
                    }
                }
            }
            
            foreach (KeyValuePair<BiomeType, List<TechType>> pair in _biomeLoot)
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

            return loot;
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
                    items.Add(TechType.Uranium);
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
    }
}
