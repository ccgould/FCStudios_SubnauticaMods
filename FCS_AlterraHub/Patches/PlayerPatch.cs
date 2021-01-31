using System;
using System.Collections;
using System.IO;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Managers.Quests;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.FCSPDA.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using HarmonyLib;
using SMLHelper.V2.Json.ExtensionMethods;
using UnityEngine;


namespace FCS_AlterraHub.Patches
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Update")]
    internal static class Player_Update_Patch
    {
        internal static Action OnPlayerUpdate;
        public static FCSPDAController FCSPDA;
        private static Player _instance;
        private static bool _wasPlaying;
        private static bool _firstMissionAdded;
        private static float _time;
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

            FCSPDA.AudioTrack.isPlaying(out bool isPlaying);


            if (isPlaying && Mathf.Approximately(Time.timeScale, 0f))
            {
                FCSPDA.AudioTrack.setPaused(true);
                _wasPlaying = true;
            }

            if (_wasPlaying && Time.timeScale > 0)
            {
                FCSPDA.AudioTrack.setPaused(false);
                FCSPDA.AudioTrack.setVolume(SoundSystem.voiceVolume);
                _wasPlaying = false;
            }

            if (Input.GetKeyDown(QPatch.Configuration.FCSPDAKeyCode) && !__instance.GetPDA().isOpen)
            {
                if (FCSPDA == null) return;

                if (!FCSPDA.IsOpen)
                {
                    FCSPDA.Open();
                }
            }

            if (LargeWorldStreamer.main.IsWorldSettled() && FCSPDA != null && DayNightCycle.main.timePassed >= 600f)
            {
                if (_firstMissionAdded) return;
                FCSPDA.MissionController.UpdateQuest(QuestManager.Instance.GetActiveMission());
                FCSPDA.MessagesController.AddNewMessage("Message From: Jack Winton (Chief Engineer)", "Jack Winton", "AH-Mission01-Pt1");
                _firstMissionAdded = true;
            }

            _time += Time.deltaTime;
            if (_time >= 1)
            {
                BaseManager.RemoveDestroyedBases();
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

            CreateQuestManager();

            var pda = CreateFcsPda(__instance);
            
            var defPDA = __instance.pdaSpawn.spawnedObj;

            MoveFcsPdaIntoPosition(defPDA, pda);
        
            _pdaCreated = true;
        }
        
        private static void MoveFcsPdaIntoPosition(GameObject defPDA, GameObject pda)
        {
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
                QuickLogger.Debug("DEFAULT NOT PDA FOUND");
            }
        }

        private static GameObject CreateFcsPda(Player __instance)
        {
            var pda = GameObject.Instantiate(AlterraHub.FcsPDAPrefab);
            pda.SetActive(false);
            pda.EnsureComponent<Rigidbody>().isKinematic = true;
            var controller = pda.AddComponent<FCSPDAController>();
            controller.CreateMissionController();
            Player_Update_Patch.FCSPDA = controller;
            controller.PDAObj = __instance.pdaSpawn.spawnedObj;

            QuickLogger.Debug("FCS PDA FOUND");
            return pda;
        }

        private static void CreateQuestManager()
        {
            if (QPatch.QuestManagerGM == null)
            {
                //Load GamePlaySettings
                Mod.LoadGamePlaySettings();
                QPatch.QuestManagerGM = new GameObject("QuestManager").AddComponent<QuestManager>();
            }
        }
    }
}
