using System.Collections.Generic;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.AutoCrafter.Helpers
{
    internal static class CompatibilityMethods
    {
        internal static void AttemptToAddToNetwork(TechType techType, BaseManager manager, List<TechType> _storedItems)
        {
#if SUBNAUTICA_STABLE
            var inventoryItem = techType.ToInventoryItemLegacy();
            var result = BaseManager.AddItemToNetwork(manager, inventoryItem, true);
            if (result)
            {
                _storedItems.Remove(techType);
            }
            else
            {
                GameObject.Destroy(inventoryItem.item.gameObject);
            }

#else
            CoroutineHost.StartCoroutine(AttemptToAddToNetworkAsync(techType, manager, _storedItems));
#endif
        }

#if BELOWZERO
         private  static IEnumerator AttemptToAddToNetworkAsync(TechType techType, BaseManager manager,List<TechType> _storedItems)
        {
            TaskResult<InventoryItem> taskResult = new TaskResult<InventoryItem>();
            yield return AsyncExtensions.ToInventoryItemLegacyAsync(techType, taskResult);
            var inventoryItem = taskResult.Get();
            var result = BaseManager.AddItemToNetwork(manager, inventoryItem, true);
            if (result)
            {
                _storedItems.Remove(techType);
            }
            else
            {
                GameObject.Destroy(inventoryItem.item.gameObject);
            }
            yield break;
        }
#endif
    }
}
