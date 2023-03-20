﻿using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;
using UWE;
using System.Collections;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.ModItems.FCSPDA.Mono;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;

namespace FCS_AlterraHub.ModItems.FCSPDA.Patches
{
    [HarmonyPatch]
    public static class Player_Patches
    {
        internal static bool ForceOpenPDA { get; set; }
        internal static FCSPDAController FCSPDA;
        private static GameObject defPDA;
        private static float _timeSinceUse;

        [HarmonyPatch(typeof(Player), nameof(Player.Update))]
        [HarmonyPostfix]
        private static void Update_Postfix(Player __instance)
        {
            //OnPlayerUpdate?.Invoke();
            //if (FCSPDA != null)
            //{
            //    FCSPDA.AudioTrack.isPlaying(out bool isPlaying);

            //    if (isPlaying && WorldHelpers.CheckIfPaused())
            //    {
            //        FCSPDA.AudioTrack.setPaused(true);
            //        _wasPlaying = true;
            //    }

            //    if (_wasPlaying && !WorldHelpers.CheckIfPaused())
            //    {
            //        FCSPDA.AudioTrack.setPaused(false);
            //        FCSPDA.AudioTrack.setVolume(SoundSystem.voiceVolume);
            //        _wasPlaying = false;
            //    }
            //}

            if ((Input.GetKeyDown(Main.Configuration.FCSPDAKeyCode) || ForceOpenPDA))
            {
                if (!FCSPDA.IsOpen)
                {
                    if (__instance.pda.isOpen)
                    {
                        __instance.pda.Close();
                    }

                    if (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl))
                    {                       
                        FCSPDA.Screen.GoToPage(PDAPages.Teleportation);
                        FCSPDA.Open();
                    }
                    else
                    {
                        FCSPDA.Open();
                    }
                }
                ForceOpenPDA = false;
            }

            _timeSinceUse += Time.deltaTime;
            if (_timeSinceUse >= 2f)
            {


                _timeSinceUse -= 2f;
            }

         
            //_time += Time.deltaTime;

            //if (_time >= 1)
            //{
            //    BaseManager.RemoveDestroyedBases();
            //}
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
        [HarmonyPostfix]
        private static void Awake_Postfix(Player __instance)
        {
            QuickLogger.Debug("Player Awake", true);
            __instance.gameObject.EnsureComponent<AccountService>();
            CoroutineHost.StartCoroutine(CreateFcsPda(__instance));
        }

        private static IEnumerator CreateFcsPda(Player player)
        {
            yield return new WaitUntil(() => player.pda.gameObject != null);

            QuickLogger.Debug("Creating FCS PDA");

            defPDA = player.pda.gameObject;

           
            var pda = GameObject.Instantiate(FCSAssetBundlesService.PublicAPI.GetLocalPrefab("fcsPDA"), default, default, true);
            var canvas = pda.GetComponentInChildren<Canvas>();
            if (canvas != null)
                canvas.sortingLayerID = 1479780821;

            pda.EnsureComponent<Rigidbody>().isKinematic = true;
            var controller = pda.AddComponent<FCSPDAController>();

            FCSPDA = controller;
            controller.PDAObj = defPDA;
            controller.SetInstance();
            controller.LoadFromSave();
            //AddUnlockedEncyclopediaEntries(FCSAlterraHubService.InternalAPI.EncyclopediaEntries);
            pda.SetActive(false);
            QuickLogger.Debug("FCS PDA Created");
            MoveFcsPdaIntoPosition(FCSPDA.gameObject);
        }

        private static void MoveFcsPdaIntoPosition(GameObject pda)
        {
            if (defPDA == null)
            {
                QuickLogger.Info("defPDA is null");
                return;
            }
            if (pda.transform.position == defPDA.transform.position) return;

            QuickLogger.Info("Move the FCS PDA");
            // Move the FCS PDA
            pda.transform.SetParent(defPDA.gameObject.transform.parent, false);
            Utils.ZeroTransform(pda.transform);
            MaterialHelpers.ApplyGlassShaderTemplate(pda, "_glass", "AH");
        }
    }
}
