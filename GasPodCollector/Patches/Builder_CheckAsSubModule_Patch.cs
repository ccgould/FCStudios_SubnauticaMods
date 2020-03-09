using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GasPodCollector.Mono;
using Harmony;
using UnityEngine;

namespace GasPodCollector.Patches
{
    [HarmonyPatch(typeof(Builder), "CheckAsSubModule")]
    static class Builder_CheckAsSubModule_Patch
    {
        static bool Prefix(ref bool __result)
        {
            if (!Builder.prefab.GetComponent<GaspodCollectorController>())
                return true;

            __result = false;

            if (Builder.placePosition.y > 0)
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
