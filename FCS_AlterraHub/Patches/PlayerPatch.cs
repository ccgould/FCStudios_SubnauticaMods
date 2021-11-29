using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.DataCollectors;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono;
using FCS_AlterraHub.Mods.FCSPDA.Enums;
using FCS_AlterraHub.Mods.FCSPDA.Mono;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Structs;
using FCS_AlterraHub.Systems;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;
using UWE;


namespace FCS_AlterraHub.Patches
{
    [HarmonyPatch]
    public static class Player_Patches
    {
        private static readonly FieldInfo _shouldPlayIntro = typeof(bool).GetField("shouldPlayIntro", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static Action OnPlayerUpdate;
        internal static bool ForceOpenPDA { get; set; }
        internal static FCSPDAController FCSPDA;
        private static GameObject defPDA;
        private static bool _wasPlaying;
        private static bool _firstMissionAdded;
        private static float _time;
        public static Transform SunTarget { get; private set; }

        [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
        [HarmonyPostfix]
        private static void Awake_Postfix(Player __instance)
        {
            
            __instance.gameObject.EnsureComponent<VoiceNotificationSystem>();
            var f = uSkyManager.main.SunLight.transform;
            if (f != null)
            {
                QuickLogger.Debug("Found Directional Light");
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.SetParent(f.transform, false);
                Utils.ZeroTransform(go.transform);
                go.transform.localPosition = new Vector3(0, 0, -50000);
                SunTarget = go.transform;
            }

            QuickLogger.Debug("Player Awake",true);


            CoroutineHost.StartCoroutine(CreateFcsPda(__instance));
            PatreonCollector.GetData();
            //var shouldPlay = (bool)_shouldPlayIntro.GetValue(__instance.GetPDA());



            //Mod.OnGamePlaySettingsLoaded += settings =>
            //{
            //    CoroutineHost.StartCoroutine(Mod.SpawnAlterraFabStation(settings));
            //};
            Mod.LoadGamePlaySettings();
        }
        
        
        [HarmonyPatch(typeof(Player), nameof(Player.Update))]
        [HarmonyPostfix]
        private static void Update_Postfix(Player __instance)
        {
            OnPlayerUpdate?.Invoke();
            if (FCSPDA != null)
            {
                FCSPDA.AudioTrack.isPlaying(out bool isPlaying);
                
                if (isPlaying && WorldHelpers.CheckIfPaused())
                {
                    FCSPDA.AudioTrack.setPaused(true);
                    _wasPlaying = true;
                }

                if (_wasPlaying && !WorldHelpers.CheckIfPaused())
                {
                    FCSPDA.AudioTrack.setPaused(false);
                    FCSPDA.AudioTrack.setVolume(SoundSystem.voiceVolume);
                    _wasPlaying = false;
                }
            }


            if (Input.GetKeyDown(QPatch.Configuration.FCSPDAKeyCode) || ForceOpenPDA && !__instance.GetPDA().isOpen)
            {
                if (!FCSPDA.IsOpen)
                {
                    if(Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl))
                    {
                        FCSPDA.GoToPage(PDAPages.Teleportation);
                        FCSPDA.Open();
                    }
                    else
                    { 
                        FCSPDA.Open();
                    }
                }
                ForceOpenPDA = false;
            }
            
            _time += Time.deltaTime;

            if (_time >= 1)
            {
                BaseManager.RemoveDestroyedBases();
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.GetPDA))]
        [HarmonyPostfix]
        private static void GetPDA_Postfix(Player __instance)
        {
            if(FCSPDA != null) MoveFcsPdaIntoPosition(FCSPDA.gameObject);
        }

        [HarmonyPatch(typeof(Player), nameof(Player.OnProtoSerialize))]
        [HarmonyPostfix]
        private static void OnProtoSerialize_Postfix()
        {
            Mod.SaveGamePlaySettings();
            Mod.Save();
        }
        
        private static void MoveFcsPdaIntoPosition(GameObject pda)
        {
            if (defPDA == null) return;
            if (pda.transform.position == defPDA.transform.position) return;
            // Move the FCS PDA
            pda.transform.SetParent(defPDA.gameObject.transform.parent, false);
            Utils.ZeroTransform(pda.transform);
            MaterialHelpers.ApplyGlassShaderTemplate(pda, "_glass", Mod.ModPackID);
        }

        private static IEnumerator CreateFcsPda(Player player)
        {
            yield return new WaitUntil(() => player.pdaSpawn.spawnedObj != null);

            QuickLogger.Debug("Creating FCS PDA");

            defPDA = player.pdaSpawn.spawnedObj;
            
            var pda = GameObject.Instantiate(AlterraHub.FcsPDAPrefab, default, default, true);
            var canvas = pda.GetComponentInChildren<Canvas>();
            if (canvas != null)
                canvas.sortingLayerID = 1479780821;

            pda.EnsureComponent<Rigidbody>().isKinematic = true;
            var controller = pda.AddComponent<FCSPDAController>();

            FCSPDA = controller;
            controller.PDAObj = player.pdaSpawn.spawnedObj;
            controller.SetInstance();
            controller.LoadCart(Mod.GetAlterraHubSaveData());
            AddUnlockedEncyclopediaEntries(FCSAlterraHubService.InternalAPI.EncyclopediaEntries);
            pda.SetActive(false);
            QuickLogger.Debug("FCS PDA Created");
            MoveFcsPdaIntoPosition(FCSPDA.gameObject);
        }

        private static void AddUnlockedEncyclopediaEntries(List<Dictionary<string, List<EncyclopediaEntryData>>> encyclopediaEntries)
        {
            foreach (var data in from entry in encyclopediaEntries from data in entry from entryData in data.Value where entryData.Unlocked select data)
            {
                PDAEncyclopedia.Add(data.Key, false);
            }
        }
    }
}
