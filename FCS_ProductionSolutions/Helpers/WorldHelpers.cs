using FCS_AlterraHub.Core.Services;
using FCS_ProductionSolutions.ModItems.Spawnables;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_ProductionSolutions.Helpers;
internal class WorldHelpers
{
    public static List<TechType> GetBiomeData(ref List<TechType> bioData, string biome = null, Transform tr = null)
    {
        if (bioData?.Count <= 0 && tr != null || biome != null)
        {
            var data = BiomeManager.FindBiomeLoot(tr, biome);

            if (data != null)
            {
                bioData = data;
                bioData.Add(SandBagSpawnable.PatchedTechType);
            }
        }

        return bioData;
    }

    internal static List<TechType> GetBiomeData(ref object bioData, string currentBiome, Transform transform)
    {
        throw new NotImplementedException();
    }
}
