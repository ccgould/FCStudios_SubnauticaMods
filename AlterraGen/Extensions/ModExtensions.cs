using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlterraGen.Extensions
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
