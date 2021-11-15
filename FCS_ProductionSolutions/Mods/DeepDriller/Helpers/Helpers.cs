using System;
using System.Collections.Generic;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.DeepDriller.Managers;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.DeepDriller.Helpers
{
    internal static class Helpers
    {
        public static string GetUntilOrEmpty(this string text, string stopAt = "(")
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
            }

            return String.Empty;
        }

        public static List<TechType> GetBiomeData(ref List<TechType> bioData, string biome = null, Transform tr = null)
        {
            if (bioData?.Count <= 0 && tr != null || biome != null)
            {
                var data = BiomeManager.FindBiomeLoot(tr, biome);

                if (data != null)
                {
                    bioData = data;
                    bioData.Add(Mod.GetSandBagTechType());
                }
            }

            return bioData;
        }
    }
}
