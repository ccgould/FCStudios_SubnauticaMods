using System;
using System.Collections;
using UnityEngine;

namespace FCS_AlterraHub.Helpers
{
    public static class TechTypeHelpers
    {
        public static IEnumerator ConvertToPickupable(TechType techType,Action<Pickupable> callback)
        {
            var task = CraftData.GetPrefabForTechTypeAsync(techType, false);
            yield return task;

            var prefab = task.GetResult();
            prefab.SetActive(false);
            var gameObject = GameObject.Instantiate(prefab);
            var pickupable = gameObject.EnsureComponent<Pickupable>();
            callback?.Invoke(pickupable);
        }
    }
}
