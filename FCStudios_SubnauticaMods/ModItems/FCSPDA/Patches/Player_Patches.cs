using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;
using UWE;
using System.Collections;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.ModItems.FCSPDA.Mono;
using FCS_AlterraHub.API;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;

namespace FCS_AlterraHub.ModItems.FCSPDA.Patches;

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

        if ((Input.GetKeyDown(Plugin.Configuration.FCSPDAKeyCode) || ForceOpenPDA))
        {
            if (!FCSPDA.isOpen)
            {
                if (__instance.pda.isOpen)
                {
                    __instance.pda.Close();
                }

                if (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl))
                {
                    //FCSPDA.Screen.GoToPage(PDAPages.Teleportation);
                    FCSPDA.Open();
                }
                else
                {
                    FCSPDA.Open();
                }
            }
            ForceOpenPDA = false;
        }

        //_timeSinceUse += Time.deltaTime;
        //if (_timeSinceUse >= 2f)
        //{


        //    _timeSinceUse -= 2f;
        //}


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
        CoroutineHost.StartCoroutine(CreateFcsPda(__instance));
    }

    private static IEnumerator CreateFcsPda(Player player)
    {
        yield return new WaitUntil(() => player.pda.gameObject != null);

        QuickLogger.Debug("Creating FCS PDA");

        defPDA = player.pda.gameObject;

       
        var pda = GameObject.Instantiate(FCSAssetBundlesService.PublicAPI.GetLocalPrefab("fcsPDA"));
        QuickLogger.Debug("1");
        var canvas = pda.GetComponentInChildren<Canvas>();
        QuickLogger.Debug("2");

        if (canvas != null)
            canvas.sortingLayerID = 1479780821;
        var controller = pda.GetComponent<FCSPDAController>();
        QuickLogger.Debug("3");

        //controller.CreateScreen();
        FCSPDA = controller;
        //controller.PDAObj = defPDA;
        QuickLogger.Debug("4");

        controller.SetInstance();
        QuickLogger.Debug("5");

        //AddUnlockedEncyclopediaEntries(FCSAlterraHubService.InternalAPI.EncyclopediaEntries);
        pda.SetActive(false);
        QuickLogger.Debug("6");

        QuickLogger.Debug("FCS PDA Created");
        MoveFcsPdaIntoPosition(pda.gameObject);
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

    [HarmonyPatch(typeof(Player), nameof(Player.OnKill))]
    [HarmonyPostfix]
    private static void OnKill_Postfix(Player __instance, DamageType damageType)
    {
        FCSPDAController pda = FCSPDAController.Main;
        if (pda.state == PDA.State.Opening || pda.state == PDA.State.Opened)
        {
            pda.SetIgnorePDAInput(false);
            pda.Close();
        }
        pda.SetIgnorePDAInput(true);
    }
}
