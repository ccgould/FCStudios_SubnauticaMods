using System;
using System.Collections.Generic;
using FCS_HomeSolutions.Configuration;
using HarmonyLib;
using UnityEngine;

namespace FCS_HomeSolutions.Patches
{
    internal class Builder_Patches
    {
        public static List<TechType> acceptableColliders = new List<TechType>() { TechType.BaseRoom, TechType.BaseMoonpool, TechType.BaseReinforcement, TechType.BaseCorridor, TechType.BaseCorridorGlass };

        [HarmonyPatch(typeof(Builder), nameof(Builder.CheckSpace), new Type[] { typeof(Vector3), typeof(Quaternion), typeof(List<OrientedBounds>), typeof(int), typeof(Collider), })]
        internal class Builder_CheckSpace_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(List<OrientedBounds> localBounds, Collider allowedCollider, ref bool __result)
            {

                if (__result || Builder.constructableTechType != Mod.CurtainTechType || localBounds.Count <= 0f)
                    return;

                var allowedDeconstructable = allowedCollider.gameObject.GetComponentInParent<BaseDeconstructable>();

                if (allowedDeconstructable != null && allowedDeconstructable.recipe == TechType.BaseWindow && Builder.sColliders.Length > 0)
                {
                    List<BaseDeconstructable> deconstructables = new List<BaseDeconstructable>() { };

                    for (int i = 0; i < Builder.sColliders.Length; i++)
                    {
                        Collider collider = Builder.sColliders[i];
                        var deconstructable = collider.gameObject.GetComponentInParent<BaseDeconstructable>();

                        if (deconstructable != null && !deconstructables.Contains(deconstructable))
                            deconstructables.Add(deconstructable);
                    }

                    if (deconstructables.Count == 1 && !deconstructables.Contains(allowedDeconstructable) && acceptableColliders.Contains(deconstructables[0].recipe))
                        __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(Builder), nameof(Builder.CheckTag))]
        internal class Builder_CheckTag_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(Collider c, ref bool __result)
            {
                if (Builder.constructableTechType == Mod.CurtainTechType && (c?.gameObject?.GetComponentInParent<BaseDeconstructable>()?.recipe ?? TechType.None) == TechType.BaseWindow)
                    __result = true;
            }
        }
    }
}
