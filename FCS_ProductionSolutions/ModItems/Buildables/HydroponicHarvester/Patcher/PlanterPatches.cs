using FCS_AlterraHub.Models.Abstract;
using FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Mono;
using FCSCommon.Utilities;
using HarmonyLib;
namespace FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Patcher;


[HarmonyPatch(typeof(Planter))]
[HarmonyPatch("Subscribe")]
internal class PlanterPatches
{
    [HarmonyPrefix]
    public static bool Prefix(ref Planter __instance, bool state)
    {
        if (__instance.gameObject.GetComponent<FCSDevice>())
        {
            __instance.storageContainer.enabled = false;
        }

        if (__instance.gameObject.name.Contains("HydroponicHarvester"))
        {
            var controller = __instance.gameObject.GetComponent<HydroponicHarvesterController>();
            if (__instance.subscribed == state)
            {
                return false;
            }
            if (__instance.storageContainer.container == null)
            {
                QuickLogger.Warning("Planter.Subscribe(): container null; will retry next frame");
                return false;
            }
            if (__instance.subscribed)
            {
                //__instance.storageContainer.container.onAddItem -= controller.AddItem;
                __instance.storageContainer.container.onRemoveItem -= __instance.RemoveItem;
                __instance.storageContainer.container.isAllowedToAdd = null;
                __instance.storageContainer.container.isAllowedToRemove = null;
            }
            else
            {
                //__instance.storageContainer.container.onAddItem += controller.AddItem;
                __instance.storageContainer.container.onRemoveItem += __instance.RemoveItem;
                __instance.storageContainer.container.isAllowedToAdd = new IsAllowedToAdd(controller.IsAllowedToAdd);
                __instance.storageContainer.container.isAllowedToRemove = new IsAllowedToRemove(__instance.IsAllowedToRemove);
            }
            __instance.subscribed = state;

            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Planter))]
[HarmonyPatch("OnEnable")]
internal class PlanterPatches_OnEnable
{
    [HarmonyPostfix]
    public static void Postfix(ref Planter __instance)
    {
        if(__instance.gameObject.GetComponent<FCSDevice>())
        {
            __instance.storageContainer.enabled = false;
        }
    }
}