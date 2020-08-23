using FCS_DeepDriller.Mono.MK2;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;

namespace FCS_DeepDriller.Patchers
{
    [HarmonyPatch(typeof(Builder), "CheckAsSubModule")]
    static class Builder_CheckAsSubModule_Patch
    {
        static bool Prefix(ref bool __result)
        {
            if (!Builder.prefab.GetComponent<FCSDeepDrillerController>())
            {
                return true;
            }
            
            __result = false;

            if (Builder.placePosition.y > 0 && !Player.main.IsUnderwater())
                return false;

            Transform aimTransform = Builder.GetAimTransform();
            Builder.placementTarget = null;

            if (Physics.Raycast(aimTransform.position, aimTransform.forward, out RaycastHit hit, Builder.placeMaxDistance, Builder.placeLayerMask.value, QueryTriggerInteraction.Ignore))
            {
                Builder.SetPlaceOnSurface(hit, ref Builder.placePosition, ref Builder.placeRotation);
                return false;
            }

            __result = Builder.CheckSpace(Builder.placePosition, Builder.placeRotation, Builder.bounds, Builder.placeLayerMask.value, hit.collider);

            return false;
        }
    }
}
