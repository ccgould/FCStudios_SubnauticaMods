using System;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.QuantumTeleporter.Enumerators;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.QuantumTeleporter.Mono
{
    internal static class TeleportManager
    {
        private static QuantumTeleporterController _currentTeleporter;
        private static QuantumTeleporterController _destinationTeleporter;
        private static TeleportScreenFXController _teleportEffects;
        private static bool _teleportComplete;
        private static bool _startTeleportCheck;
        private static int _freezeStatsCount;
        private static float _timeLeft = 3f;
        private static QTTeleportTypes _tab;

        internal static void Update()
        {
            if (!_teleportComplete && _startTeleportCheck)
            {
                _timeLeft -= DayNightCycle.main.deltaTime;
                if (_timeLeft <= 0)
                {
                    CheckTeleportationComplete();
                    _timeLeft = 1f;
                }
                
            }
        }

        internal static void TeleportPlayer(QuantumTeleporterController currentTeleporter, QuantumTeleporterController destinationTeleporter, QTTeleportTypes tab)
        {
            //Check if any of the teleporters return null if so cancel
            QuickLogger.Debug($"In Teleport Player Main",true);

            if (currentTeleporter == null || destinationTeleporter == null || currentTeleporter.Manager == null || destinationTeleporter.Manager == null)
            {
                if (currentTeleporter == null)
                {
                    QuickLogger.Debug("Current Teleporter entry was null", true);
                }

                if (destinationTeleporter == null)
                {
                    QuickLogger.Debug("Destination Teleporter entry was null", true);
                }

                QuickLogger.ModMessage(AuxPatchers.TeleportCanceledMessage());
                return;
            }

            _tab = tab;
            _currentTeleporter = currentTeleporter;
            _destinationTeleporter = destinationTeleporter;

            //Start Teleport Sequence
            TeleportPlayer();

        }

        private static void TeleportPlayer()
        {
            QuickLogger.Debug($"In Teleport Player", true);
            ToggleWarpScreen(true);

            //bool flag2 = Player.main.AddUsedTool(TechType.PrecursorTeleporter);
            SetWarpPosition();
            Player.main.playerController.inputEnabled = false;
            Inventory.main.quickSlots.SetIgnoreHotkeyInput(true);
            Player.main.GetPDA().SetIgnorePDAInput(true);
            Player.main.playerController.SetEnabled(false);
            Player.main.teleportingLoopSound.Play();
            _teleportComplete = false;
            _startTeleportCheck = true;
            FreezeStats();
        }

        private static void SetWarpPosition()
        {
            Quaternion quaternion = Quaternion.Euler(new Vector3(0f, 180f, 0f));
            Player.main.gameObject.transform.position = _destinationTeleporter.GetTarget().position;
            Player.main.gameObject.transform.rotation = quaternion;
        }
        
        private static void CheckTeleportationComplete()
        {
            Player.main.SetCurrentSub(_destinationTeleporter.Manager.Habitat);

            if (LargeWorldStreamer.main.IsWorldSettled())
            {
                ToggleWarpScreen(false);
                _teleportComplete = true;
                _startTeleportCheck = false;
                UnfreezeStats();
                Inventory.main.quickSlots.SetIgnoreHotkeyInput(false);
                Player.main.GetPDA().SetIgnorePDAInput(false);
                Player.main.playerController.inputEnabled = true;
                Player.main.teleportingLoopSound.Stop();
                Player.main.playerController.SetEnabled(true);
                _timeLeft = 1f;
            }
        }

        private static void FreezeStats()
        {
            if (_freezeStatsCount == 0)
            {
                Survival component = Player.main.GetComponent<Survival>();
                if (component != null)
                {
                    component.freezeStats = true;
                }
            }
            _freezeStatsCount++;
        }

        public static void UnfreezeStats()
        {
            _freezeStatsCount--;
            if (_freezeStatsCount == 0)
            {
                Survival component = Player.main.GetComponent<Survival>();
                if (component != null)
                {
                    component.freezeStats = false;
                    return;
                }
            }
            else if (_freezeStatsCount < 0)
            {
                Debug.LogError("UnfreezeStats when not frozen");
                _freezeStatsCount = 0;
            }
        }

        private static void ToggleWarpScreen(bool activate)
        {
            if (_tab != QTTeleportTypes.Global) return;

            if (_teleportEffects == null)
            {
                _teleportEffects = Player.main.camRoot.mainCam.GetComponentInChildren<TeleportScreenFXController>();
            }
            if (activate)
            {
                _teleportEffects.StartTeleport();
            }
            else
            {
                _teleportEffects.StopTeleport();
            }
        }
    }
}
