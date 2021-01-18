using FCS_LifeSupportSolutions.Mods.OxygenTank.Mono;
using HarmonyLib;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Patches
{
    [HarmonyPatch(typeof(OxygenPipe), nameof(OxygenPipe.UpdatePipe))]
    public static class OxygenPipe_Patches
    {
        [HarmonyPostfix]
        public static void Postfix(OxygenPipe __instance)
        {
            if (!string.IsNullOrEmpty(__instance.parentPipeUID) && __instance.isGhost)
            {
                OxygenTankAttachPoint oxygenTank = null;
                int num2 = UWE.Utils.OverlapSphereIntoSharedBuffer(__instance.transform.position, 1f, -1, QueryTriggerInteraction.UseGlobal);
                for (int i = 0; i < num2; i++)
                {
                    GameObject entityRoot = UWE.Utils.GetEntityRoot(UWE.Utils.sharedColliderBuffer[i].gameObject);
                    OxygenTankAttachPoint component = entityRoot?.GetComponent<OxygenTankAttachPoint>();
                    if (component != null && component.parentPipeUID is null && component.allowConnection)
                    {
                        oxygenTank = component;
                        break;
                    }
                }

                if (oxygenTank != null)
                {
                    Vector3 attachpoint = oxygenTank.GetAttachPoint();
                    Vector3 vector = Vector3.Normalize(__instance.parentPosition - attachpoint);
                    float magnitude = (__instance.parentPosition - attachpoint).magnitude;
                    __instance.transform.position = attachpoint;
                    __instance.topSection.rotation = Quaternion.LookRotation(vector, Vector3.up);
                    __instance.endCap.rotation = __instance.topSection.rotation;
                    __instance.bottomSection.rotation = Quaternion.LookRotation(vector, Vector3.up);
                    __instance.bottomSection.position = __instance.parentPosition;
                    __instance.stretchedPart.position = __instance.topSection.position + vector;
                    Vector3 localScale = __instance.stretchedPart.localScale;
                    localScale.z = magnitude - 2f;
                    __instance.stretchedPart.localScale = localScale;
                    __instance.stretchedPart.rotation = __instance.topSection.rotation;
                }
            }

        }
    }
}
