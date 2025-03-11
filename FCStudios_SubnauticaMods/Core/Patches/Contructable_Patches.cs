using FCS_AlterraHub.Models.Mono;
using FCSCommon.Utilities;
using FMODUnity;
using HarmonyLib;
using UnityEngine;

namespace FCS_AlterraHub.Core.Patches;

//[HarmonyPatch(typeof(SubRoot))]
//[HarmonyPatch("Awake")]
//internal class SubRoot_Awake
//{
//    [HarmonyPostfix]
//    public static void Postfix(ref SubRoot __instance)
//    {
//        QuickLogger.Debug($"Awake Called on {__instance.gameObject.GetComponent<PrefabIdentifier>()?.id}");
//        QuickLogger.Debug($"Awake Called on {__instance.gameObject.activeSelf} | {__instance.gameObject.transform.position != Vector3.zero}");
//        if (__instance.gameObject.activeSelf && __instance.gameObject.transform.position != Vector3.zero)
//        {
//            var habitatManager = __instance.gameObject.EnsureComponent<HabitatManager>();
//            var portManager = __instance.gameObject.EnsureComponent<PortManager>();
//            portManager.Manager = habitatManager;
//            habitatManager.SetPortManager(portManager);

//        }
//    }
//}



//TODO figure out reason and issue with error
//[HarmonyPatch(typeof(Builder))]
//[HarmonyPatch("TryPlace")]
//internal class SubRoot_OnKill
//{
//    [HarmonyPrefix]
//    public static void Prefix(bool __result)
//    {
//        if (Builder.prefab == null || !Builder.canPlace)
//        {
//            __result = false;
//        }
//        RuntimeManager.PlayOneShot("event:/tools/builder/place", Builder.ghostModel.transform.position);
//        ConstructableBase componentInParent = Builder.ghostModel.GetComponentInParent<ConstructableBase>();
//        if (componentInParent != null)
//        {
//            BaseGhost component = Builder.ghostModel.GetComponent<BaseGhost>();
//            component.Place();
//            if (component.TargetBase != null)
//            {
//                componentInParent.transform.SetParent(component.TargetBase.transform, true);
//            }
//            componentInParent.SetState(false, true);
//        }
//        else
//        {
//            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Builder.prefab);
//            bool flag = false;
//            bool flag2 = false;
//            SubRoot currentSub = Player.main.GetCurrentSub();
//            if (currentSub != null)
//            {
//                flag = currentSub.isBase;
//                flag2 = currentSub.isCyclops;
//                gameObject.transform.parent = currentSub.GetModulesRoot();
//            }
//            else if (Builder.placementTarget != null && Builder.allowedOutside)
//            {
//                SubRoot componentInParent2 = Builder.placementTarget.GetComponentInParent<SubRoot>();
//                if (componentInParent2 != null)
//                {
//                    gameObject.transform.parent = componentInParent2.GetModulesRoot();
//                }
//            }
//            Transform transform = gameObject.transform;
//            transform.position = Builder.placePosition;
//            transform.rotation = Builder.placeRotation;
//            Constructable componentInParent3 = gameObject.GetComponentInParent<Constructable>();
//            componentInParent3.SetState(false, true);
//            if (Builder.ghostModel != null)
//            {
//                UnityEngine.Object.Destroy(Builder.ghostModel);
//            }
//            componentInParent3.SetIsInside(flag || flag2);
//            SkyEnvironmentChanged.Send(gameObject, currentSub);
//            if (flag2)
//            {
//                componentInParent3.ExcludeFromSubParentRigidbody();
//            }
//        }
//        Builder.ghostModel = null;
//        Builder.prefab = null;
//        Builder.canPlace = false;
//        __result =  true;
//    }
//}