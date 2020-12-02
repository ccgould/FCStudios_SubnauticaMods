using System.Collections.Generic;
using UnityEngine;

namespace FCSCommon.Utilities
{
    internal static class TechUtil
    {
        private static class PrefabComponentSearches<T>
        {
            public static readonly Dictionary<TechType, bool> cash = new Dictionary<TechType, bool>();
        }

        internal static bool TechTypePrefabContains<T>(TechType techType) where T : MonoBehaviour
        {
            if (PrefabComponentSearches<T>.cash.TryGetValue(techType, out var result)) { return result; }

            var prefab = CraftData.GetPrefabForTechType(techType, false);
            result = prefab != null && prefab.GetComponent<T>();

            PrefabComponentSearches<T>.cash[techType] = result;
            return result;
        }
    }
}
