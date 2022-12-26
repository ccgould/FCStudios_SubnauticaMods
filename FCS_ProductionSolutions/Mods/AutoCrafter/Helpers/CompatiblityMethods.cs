﻿using System.Collections;
using System.Collections.Generic;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using UnityEngine;
using UWE;

namespace FCS_ProductionSolutions.Mods.AutoCrafter.Helpers
{
    internal static class CompatibilityMethods
    {
        internal static void AttemptToAddToNetwork(TechType techType, BaseManager manager, List<TechType> _storedItems)
        {
            CoroutineHost.StartCoroutine(AttemptToAddToNetworkAsync(techType, manager, _storedItems));
        }

        private static IEnumerator AttemptToAddToNetworkAsync(TechType techType, BaseManager manager, List<TechType> _storedItems)
        {
            TaskResult<InventoryItem> taskResult = new TaskResult<InventoryItem>();
            yield return AsyncExtensions.ToInventoryItemAsync(techType, taskResult);
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
    }
}
