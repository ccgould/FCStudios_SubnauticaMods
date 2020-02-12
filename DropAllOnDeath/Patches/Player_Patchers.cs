using System.Collections;
using Harmony;
using AE.DropAllOnDeath.Config;
using UnityEngine;
using UWE;

namespace AE.DropAllOnDeath.Patches
{

    //[HarmonyPatch(typeof(Player))]
    //[HarmonyPatch("OnKill")]
    //internal class Player_Patchers
    //{
    //    private static Player _player;

    //    [HarmonyTranspiler]
    //    public static bool Prefix(ref Player __instance, IEnumerable __result, ref DamageType damageType)
    //    {
    //        _player = __instance;


    //        if (Mod.Configuration.EscapePodRespawn && Mod.Configuration.Enabled)
    //        {

    //            yield return new WaitForSeconds(5f);
    //            UWE.Utils.EnterPhysicsSyncSection();
    //            _player.playerDeathEvent.Trigger(_player);
    //            _player.gameObject.SendMessage("DisableHeadCameraController", null, SendMessageOptions.RequireReceiver);
    //            uGUI.main.respawning.Show();
    //            _player.ToNormalMode(true);
    //            bool lostStuff = Inventory.main.LoseItems();
    //            if (AtmosphereDirector.main)
    //            {
    //                AtmosphereDirector.main.ResetDirector();
    //            }
    //            EscapePod escapePod = _player.lastEscapePod ?? EscapePod.main;
    //            if (escapePod)
    //            {
    //                escapePod.RespawnPlayer();
    //                _player.SetCurrentSub(null);
    //                _player.currentEscapePod = escapePod;
    //            }


    //            yield return new WaitForSeconds(1f);
    //            LargeWorldStreamer streamer = LargeWorldStreamer.main;
    //            while (!streamer.IsWorldSettled())
    //            {
    //                yield return CoroutineUtils.waitForNextFrame;
    //            }
    //            uGUI.main.respawning.Hide();
    //            if (_player.liveMixin)
    //            {
    //                _player.liveMixin.ResetHealth();
    //            }
    //            _player.oxygenMgr.AddOxygen(1000f);
    //            _player.timeSpawned = Time.time;
    //            _player.playerRespawnEvent.Trigger(_player);
    //            DamageFX.main.ClearHudDamage();
    //            _player.SuffocationReset();
    //            yield return null;
    //            _player.precursorOutOfWater = false;
    //            _player.SetDisplaySurfaceWater(true);
    //            _player.UnfreezeStats();
    //            Inventory.main.quickSlots.SetIgnoreHotkeyInput(false);
    //            _player.GetPDA().SetIgnorePDAInput(false);
    //            _player.playerController.inputEnabled = true;
    //            _player.playerController.SetEnabled(true);
    //            yield return new WaitForSeconds(1f);
    //            UWE.Utils.ExitPhysicsSyncSection();
    //            ErrorMessage.AddWarning((!lostStuff) ? Language.main.Get("YouDied") : Language.main.Get("YouDiedLostStuff"));
    //            yield break;

    //            return false;
    //        }
    //        else
    //        {

    //        }
    //        return false;
    //    }
    //}
}

