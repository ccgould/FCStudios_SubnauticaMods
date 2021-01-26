using System;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Managers.Quests;
using FCS_AlterraHub.Mono.FCSPDA.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using HarmonyLib;
using SMLHelper.V2.Json.ExtensionMethods;
using UnityEngine;


namespace FCS_AlterraHub.Patches
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Awake")]
    internal static class Player_Awaker_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(Player __instance)
        {
            if (QPatch.QuestManagerGM == null)
            {
                //Load GamePlaySettings
                Mod.LoadGamePlaySettings();
                QPatch.QuestManagerGM = new GameObject("QuestManager").AddComponent<QuestManager>();
            }
        }

        
    }

    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Update")]
    internal static class Player_Update_Patch
    {
        internal static Action OnPlayerUpdate;
        public static FCSPDAController FCSPDA;
        private static Player _instance;
        public static bool LoadSavesQuests { get; set; }
        
        
        private static void Postfix(Player __instance)
        {

            _instance = __instance;
            OnPlayerUpdate?.Invoke();

            if (LoadSavesQuests && QPatch.QuestManagerGM != null)
            {
                QuestManager.Instance.Load();
                LoadSavesQuests = false;
            }

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
            var audioSource = __instance.gameObject.AddComponent<AudioSource>();
            controller.AudioSource = audioSource;
            Player_Update_Patch.FCSPDA = controller;
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
