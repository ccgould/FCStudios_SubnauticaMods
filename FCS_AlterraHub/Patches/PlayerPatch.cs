using System;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Managers.Mission;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.FCSPDA.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using HarmonyLib;
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

            if (LoadSavesQuests && QPatch.MissionManagerGM != null)
            {
                //QuestManager.Instance.Load();
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
                if (FCSPDA == null)
                {
                    QuickLogger.DebugError("FCSPDA IS NULL: Attempting to force creation", true);
                    PlayerGetPDA_Patch.ForceFCSPDACreation();
                    if (FCSPDA == null)
                    {
                        QuickLogger.DebugError("Forcing PDA creation failed returning", true);
                        QuickLogger.ModMessage(AlterraHub.ErrorHasOccured());
                        return;
                    }
                }

                if (!FCSPDA.IsOpen)
                {
                    FCSPDA.Open();
                }
            }

            if (LargeWorldStreamer.main.IsWorldSettled() && FCSPDA != null && DayNightCycle.main.timePassed >= 600f)
            {
                if (_firstMissionAdded) return;
                //FCSPDA.MissionController.UpdateMission(QuestManager.Instance.GetActiveMission());
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

        [HarmonyPostfix]
        private static void Postfix(Player __instance)
        {
            if(Player_Update_Patch.FCSPDA != null) return;

            CreateQuestManager();

            var pda = CreateFcsPda(__instance);
            
            var defPDA = __instance.pdaSpawn.spawnedObj;

            MoveFcsPdaIntoPosition(defPDA, pda);
        }
        
        internal static void ForceFCSPDACreation()
        {
            Postfix(Player.main);
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
            var canvas = pda.GetComponentInChildren<Canvas>();
            if(canvas != null)
                canvas.sortingLayerID = 1479780821;
            
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
            if (QPatch.MissionManagerGM == null)
            {
                QuickLogger.Debug("Load Gameplay Settings");
                //Load GamePlaySettings
                Mod.LoadGamePlaySettings();
                QPatch.MissionManagerGM = new GameObject("MissionManager").AddComponent<MissionManager>();
            }
        }
    }
}
