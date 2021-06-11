using System;
using System.Collections;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.FCSPDA.Mono;
using FCS_AlterraHub.Mono;
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

            CoroutineHost.StartCoroutine(CreateFcsPda(__instance));
            CoroutineHost.StartCoroutine(Mod.SpawnAlterraFabStation());
        }
        
        
        [HarmonyPatch(typeof(Player), nameof(Player.Update))]
        [HarmonyPostfix]
        private static void Update_Postfix(Player __instance)
        {
            OnPlayerUpdate?.Invoke();
            if (FCSPDA != null)
            {
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
            }

            if (Input.GetKeyDown(QPatch.Configuration.FCSPDAKeyCode) || ForceOpenPDA && !__instance.GetPDA().isOpen)
            {
                if (Mod.GamePlaySettings.IsPDAUnlocked)
                {
                    if (!FCSPDA.IsOpen)
                    {
                        FCSPDA.Open();
                    }
                }
                else
                {
                    QuickLogger.ModMessage("Please complete the Alterra Hub Station Mission.");
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
            MoveFcsPdaIntoPosition(FCSPDA.gameObject);
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
            if (defPDA != null)
            {
                QuickLogger.Debug("DEFAULT PDA FOUND");
                if (pda.transform.position == defPDA.transform.position) return;
                // Move the FCS PDA
                pda.transform.SetParent(defPDA.gameObject.transform.parent, false);
                Utils.ZeroTransform(pda.transform);
                MaterialHelpers.ApplyGlassShaderTemplate(pda, "_glass", Mod.ModPackID);
            }
            else
            {
                QuickLogger.Error("DEFAULT PDA NOT FOUND!! THIS SHOULD NOT BE POSSIBLE!");
            }
        }

        private static IEnumerator CreateFcsPda(Player player)
        {
            yield return new WaitUntil(() => player.pdaSpawn.spawnedObj != null);

            defPDA = player.pdaSpawn.spawnedObj;
            
            var pda = GameObject.Instantiate(AlterraHub.FcsPDAPrefab, default, default, false);
            var canvas = pda.GetComponentInChildren<Canvas>();
            if (canvas != null)
                canvas.sortingLayerID = 1479780821;

            pda.EnsureComponent<Rigidbody>().isKinematic = true;
            var controller = pda.AddComponent<FCSPDAController>();
            FCSPDA = controller;
            controller.PDAObj = player.pdaSpawn.spawnedObj;
            controller.SetInstance();

            QuickLogger.Debug("FCS PDA Created");
            MoveFcsPdaIntoPosition(FCSPDA.gameObject);
        }
    }
}
