using System;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Mono.FCSPDA.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;


namespace FCS_AlterraHub.Patches
{


    //[HarmonyPatch(typeof(PDA))]
    //[HarmonyPatch("Update")]
    //internal static class PDA_Patch
    //{
    //    [HarmonyPrefix]
    //    private static bool Prefix(PDA __instance)
    //    {

    //        __instance.sequence.Update();
    //        if (__instance.sequence.active)
    //        {
    //            float b = (SNCameraRoot.main.mainCamera.aspect > 1.5f) ? __instance.cameraFieldOfView : __instance.cameraFieldOfViewAtFourThree;
    //            SNCameraRoot.main.SetFov(Mathf.Lerp(MiscSettings.fieldOfView, b, __instance.sequence.t));
    //        }
    //        Player main = Player.main;
    //        if (__instance.isInUse && __instance.isFocused && GameInput.GetButtonDown(GameInput.Button.PDA) && !__instance.ui.introActive)
    //        {
    //            __instance.Close();
    //            return true;
    //        }
    //        if (__instance.targetWasSet && (__instance.target == null || (__instance.target.transform.position - main.transform.position).sqrMagnitude >= __instance.activeSqrDistance))
    //        {
    //            __instance.Close();
    //        }

    //        return true;
    //    }
    //}


    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Update")]
    internal static class Player_Patch
    {
        internal static Action OnPlayerUpdate;
        public static FCSPDAController FCSPDA;
        private static Player _instance;

        private static void Postfix(Player __instance)
        {
            _instance = __instance;
            OnPlayerUpdate?.Invoke();

            if (Input.GetKeyDown(QPatch.Configuration.FCSPDAKeyCode) && !__instance.GetPDA().isOpen)
            {
                if (FCSPDA == null) return;

                if (!FCSPDA.IsOpen)
                {
                    FCSPDA.Open();
                }
            }

        }
    }

    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("GetPDA")]
    internal static class PlayerGetPDA_Patch
    {
        private static bool _pdaCreated;

        [HarmonyPostfix]
        private static void Postfix(Player __instance)
        {
            if(_pdaCreated) return;
            _pdaCreated = true;
            //ADD FCSPDA
            var pda = GameObject.Instantiate(AlterraHub.FcsPDAPrefab);
            pda.SetActive(false);
            pda.EnsureComponent<Rigidbody>().isKinematic = true;
            var controller = pda.AddComponent<FCSPDAController>();
            Player_Patch.FCSPDA = controller;
            controller.PDAObj =  __instance.pdaSpawn.spawnedObj;
            if (pda != null)
            {
                QuickLogger.Debug("FCS PDA FOUND");
                var defPDA = __instance.pdaSpawn.spawnedObj;
                if (defPDA != null)
                {
                    QuickLogger.Debug("DEFAULT PDA FOUND");
                    // Move the FCS PDA
                    pda.transform.SetParent(defPDA.gameObject.transform.parent, false);
                    Utils.ZeroTransform(pda.transform);
                    MaterialHelpers.ApplyGlassShaderTemplate(pda, "_glass", Mod.ModName);
                    
                }
                else
                {
                    QuickLogger.Debug("DEFAULT PDA FOUND");
                }
            }
            else
            {
                QuickLogger.Debug("FCS PDA NOT FOUND");
            }
        }
    }
}
