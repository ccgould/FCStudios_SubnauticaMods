using System.Collections.Generic;

namespace FCS_EnergySolutions.Mods.AlterraGen.Extensions
{
    internal static class ModExtensions
    {
        internal static IEnumerable<TechType> ToList(this Dictionary<TechType, int> dict)
        {
            foreach (KeyValuePair<TechType, int> pair in dict)
            {
                for (int i = 0; i < pair.Value; i++)
                {
                    yield return pair.Key;
                }
            }
        }
    }
}
