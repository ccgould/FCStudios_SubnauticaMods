using Harmony;
using AE.DropAllOnDeath.Config;
using System.Collections.Generic;

namespace AE.DropAllOnDeath.Patches
{
    [HarmonyPatch(typeof(Inventory))]
    [HarmonyPatch(nameof(Inventory.LoseItems))]
    internal class Inventory_Patcher
    {
        public static void Postfix(ref Inventory __instance, ref bool __result)
        {
            if (!Mod.Configuration.Enabled) return;

            List<InventoryItem> list = new List<InventoryItem>();

            foreach (InventoryItem inventoryItem in Inventory.main.container)
            {
                list.Add(inventoryItem);
            }
            foreach (InventoryItem inventoryItem2 in ((IItemsContainer)Inventory.main.equipment))
            {
                list.Add(inventoryItem2);
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (__instance.InternalDropItem(list[i].item))
                {
                    __result = true;
                }
            }
        }
    }
}
