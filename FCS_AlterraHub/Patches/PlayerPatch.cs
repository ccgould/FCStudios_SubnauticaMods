using System;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.FCSPDA.Mono;
using FCS_AlterraHub.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;
using UWE;


namespace FCS_AlterraHub.Patches
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Update")]
    public static class Player_Update_Patch
    {
        internal static Action OnPlayerUpdate; 
        public static bool ForceOpenPDA { get; set; }
        public static FCSPDAController FCSPDA;
        private static Player _instance;
        private static bool _wasPlaying;
        private static bool _firstMissionAdded;
        private static float _time;


        private static void Postfix(Player __instance)
        {

            _instance = __instance;
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
    }

    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Awake")]
    public static class Player_Awake_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(Player __instance)
        {
            if (Player_Update_Patch.FCSPDA == null)
            {
                QuickLogger.DebugError("FCSPDA IS NULL: Attempting to force creation", true);
                PlayerGetPDA_Patch.ForceFCSPDACreation();
            }

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

            CoroutineHost.StartCoroutine(Mod.SpawnAlterraFabStation());

        }
        public static Transform SunTarget { get; set; }
    }


    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("GetPDA")]
    internal static class PlayerGetPDA_Patch
    {

        [HarmonyPostfix]
        private static void Postfix(Player __instance)
        {
            if (Player_Update_Patch.FCSPDA != null) return;

            var pda = CreateFcsPda(__instance);

            var defPDA = __instance.pdaSpawn.spawnedObj;

            MoveFcsPdaIntoPosition(defPDA, pda);
        }

        internal static void ForceFCSPDACreation()
        {
            Postfix(Player.main);
            QuickLogger.Info($"Forced FCS PDA Creation: {Player_Update_Patch.FCSPDA}");
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
            if (canvas != null)
                canvas.sortingLayerID = 1479780821;

            pda.EnsureComponent<Rigidbody>().isKinematic = true;
            var controller = pda.AddComponent<FCSPDAController>();
            Player_Update_Patch.FCSPDA = controller;
            FCSPDAController.SetInstance(controller);
            controller.PDAObj = __instance.pdaSpawn.spawnedObj;

            QuickLogger.Debug("FCS PDA FOUND");
            return pda;
        }

        [HarmonyPatch(typeof(Player))]
        [HarmonyPatch("OnProtoSerialize")]
        internal static class PlayerOnProtoSerialize_Patch
        {

            [HarmonyPostfix]
            private static void Postfix(Player __instance)
            {
                Mod.SaveGamePlaySettings();
                Mod.Save();
            }
        }
    }
}
