using System.Collections.Generic;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Extensions;
using HarmonyLib;
using UnityEngine;

namespace FCS_HomeSolutions.Patches
{
    internal class Builder_Pathes
    {
        [HarmonyPatch(typeof(Builder), nameof(Builder.UpdateAllowed))]
        internal class Builder_UpdateAllowed_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(ref bool __result)
            {
                Constructable component = Builder.prefab.GetComponent<Constructable>();
                if (component.techType != Mod.CurtainTechType || !Player.main.IsInBase()) return;

                Builder.SetDefaultPlaceTransform(ref Builder.placePosition, ref Builder.placeRotation);
                bool flag = false;
                ConstructableBase componentInParent = Builder.ghostModel.GetComponentInParent<ConstructableBase>();
                
                bool flag2;
                if (componentInParent != null)
                {
                    Transform transform = componentInParent.transform;
                    transform.position = Builder.placePosition;
                    transform.rotation = Builder.placeRotation;
                    flag2 = componentInParent.UpdateGhostModel(Builder.GetAimTransform(), Builder.ghostModel, default(RaycastHit), out flag, componentInParent);
                    Builder.placePosition = transform.position;
                    Builder.placeRotation = transform.rotation;
                    if (flag)
                    {
                        Builder.renderers = MaterialExtensions.AssignMaterial(Builder.ghostModel, Builder.ghostStructureMaterial);
                        Builder.InitBounds(Builder.ghostModel);
                    }
                }
                else
                {

                    Builder.CheckAsSubModule();
                    flag2 = true;
                }
                if (flag2)
                {
                    List<GameObject> list = new List<GameObject>();
                    Builder.GetObstacles(Builder.placePosition, Builder.placeRotation, Builder.bounds, list);
                    flag2 = (list.Count == 0);
                    list.Clear();
                }
                __result = flag2;
			}
        }
    }
}
