using System.Collections.Generic;
using DataStorageSolutions.Model;
using HarmonyLib;

namespace DataStorageSolutions.Patches
{
    /**
     * Patches into StorageContainer::Awake method to alert our Resource monitor about the newly placed storage container
     */
    [HarmonyPatch(typeof(StorageContainer))]
    [HarmonyPatch("Awake")]
    internal class StorageContainerAwakePatcher
    {
        private static readonly List<BaseStorageManager> registeredBaseStorageManagers = new List<BaseStorageManager>();

        [HarmonyPostfix]
        internal static void Postfix(StorageContainer __instance)
        {
            if (__instance == null)
            {
                return;
            }

            foreach (BaseStorageManager baseStorageManager in registeredBaseStorageManagers)
            {
                baseStorageManager.AlertNewStorageContainerPlaced(__instance);
            }
        }

        internal static void RegisterForNewStorageContainerUpdates(BaseStorageManager baseStorageManager)
        {
            registeredBaseStorageManagers.Add(baseStorageManager);
        }

        internal static void ClearRegisteredBaseStorageManager()
        {
            registeredBaseStorageManagers.Clear();
        }
    }
}
