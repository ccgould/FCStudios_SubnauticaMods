using FCSCommon.Utilities;
using Nautilus.Handlers;
using System;
using System.Collections;
using UnityEngine;


namespace FCS_AlterraHub.Core.Extensions;
public static class AsyncExtensions
{
    public static InventoryItem ToInventoryItem(this Pickupable pickupable)
    {
        InventoryItem item = null;
        if (pickupable != null)
        {
            PickupReplacement(pickupable);
            item = new InventoryItem(pickupable);
        }

        return item;
    }
    public static IEnumerator AddTechTypeToContainerUnSafe(this TechType techType, ItemsContainer container)
    {
        if (techType != TechType.None)
        {
            var itemResult = new TaskResult<InventoryItem>();
            yield return techType.ToInventoryItem(itemResult);
            var item = itemResult.Get();
            container.UnsafeAdd(item);
        }
    }

    public static IEnumerator ToInventoryItem(this TechType techType, TaskResult<InventoryItem> item)
    {
        if (techType != TechType.None)
        {
            var result = CraftData.GetPrefabForTechTypeAsync(techType, false);

            yield return result;

            var go = GameObject.Instantiate(result.GetResult());

            if (!go.TryGetComponent(out Pickupable pickupable))
                pickupable = go.AddComponent<Pickupable>();

            PickupReplacement(pickupable);
            item.Set(new InventoryItem(pickupable));
        }
        else
        {
            QuickLogger.DebugError("ToInventoryItem: TechType is None ");
        }

        yield break;
    }
    
    
    private static void PickupReplacement(Pickupable pickupable)
    {
        pickupable.SendMessage("OnExamine", SendMessageOptions.DontRequireReceiver);
        int num = pickupable.gameObject.GetComponentsInChildren<Rigidbody>(true).Length;
        if (num == 0)
        {
            pickupable.gameObject.AddComponent<Rigidbody>();
        }

        pickupable.Deactivate();
        pickupable.attached = true;
        if (pickupable._isInSub)
        {
            pickupable.Unplace();
            pickupable._isInSub = false;
        }
    }

    public static IEnumerator ToInventoryItemAsync(TechType techType, IOut<InventoryItem> result2)
    {
        TaskResult<GameObject> result = new TaskResult<GameObject>();
        yield return CraftData.InstantiateFromPrefabAsync(techType, result, false);
        GameObject gameObject = result.Get();
        if (gameObject != null)
        {
            CrafterLogic.NotifyCraftEnd(gameObject, techType);
            Pickupable component = gameObject.GetComponent<Pickupable>();
            if (component != null)
            {
                result2.Set(new InventoryItem(component));
            }

            yield break;
        }
    }
}
