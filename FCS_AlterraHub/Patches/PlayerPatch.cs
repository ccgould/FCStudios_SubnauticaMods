using System;
using System.Collections;
using System.IO;
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
    [HarmonyPatch("Start")]
    internal static class Player_Awaker_Patch
    {
        private static bool _audioLoaded;

        [HarmonyPostfix]
        private static void Postfix(Player __instance)
        {
            LoadAudioFiles(__instance);
        }

        public static void LoadAudioFiles(Player instance)
        {
            if (_audioLoaded) return;
            instance.StartCoroutine(AddAudioTrack(Path.Combine(Mod.GetAssetPath(), "Audio", "AH-Mission01-Pt1.wav")));
            instance.StartCoroutine(AddAudioTrack(Path.Combine(Mod.GetAssetPath(), "Audio", "AH-Mission01-Pt2.wav")));
            instance.StartCoroutine(AddAudioTrack(Path.Combine(Mod.GetAssetPath(), "Audio", "AH-Mission01-Pt3.wav")));
            _audioLoaded = true;
        }


        public static IEnumerator AddAudioTrack(string audioSource)
        {
            WWW request = GetAudioFromFile(audioSource);
            yield return request;

            var clip = request.GetAudioClip();
            var name = Path.GetFileNameWithoutExtension(audioSource);
            clip.name = name;
            Mod.AudioClips.Add(name, clip);
        }

        private static WWW GetAudioFromFile(string source)
        {
            WWW request = new WWW(source);
            return request;
        }
    }

    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Update")]
    internal static class Player_Update_Patch
    {
        internal static Action OnPlayerUpdate;
        public static FCSPDAController FCSPDA;
        private static Player _instance;
        private static bool _firstMissionAdded;
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

            if (LargeWorldStreamer.main.IsWorldSettled() && FCSPDA != null)
            {
                if(_firstMissionAdded) return;
                FCSPDA.MissionController.UpdateQuest(QuestManager.Instance.GetActiveMission());
                FCSPDA.MessagesController.AddNewMessage("Message From: Jack Winton (Chief Engineer)", "Jack Winton", "AH-Mission01-Pt1");
                _firstMissionAdded = true;
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
            var audioSource = __instance.gameObject.AddComponent<AudioSource>();
            //ADD FCSPDA
            var pda = GameObject.Instantiate(AlterraHub.FcsPDAPrefab);
            pda.SetActive(false);
            pda.EnsureComponent<Rigidbody>().isKinematic = true;
            var controller = pda.AddComponent<FCSPDAController>();
            controller.AudioSource = audioSource;
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
