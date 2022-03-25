using System;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Enumerators;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Interface;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.QuantumTeleporter.Mono
{
    internal static class TeleportManager
    {
        private static IQuantumTeleporter _currentTeleporter;
        private static IQuantumTeleporter _destinationTeleporter;
        private static TeleportScreenFXController _teleportEffects;
        private static bool _teleportComplete;
        private static bool _startTeleportCheck;
        private static int _freezeStatsCount;
        private static float _timeLeft;
        private static QTTeleportTypes _tab;

        internal static void Update()
        {
            if (!_teleportComplete && _startTeleportCheck)
            {
                _timeLeft += DayNightCycle.main.deltaTime;
                if (_timeLeft >= 3f)
                {
                    SetWarpPosition();
                    CheckTeleportationComplete();
                    _timeLeft = 0f;
                }
            }
        }

        internal static void TeleportPlayer(IQuantumTeleporter currentTeleporter,
            IQuantumTeleporter destinationTeleporter, QTTeleportTypes tab)
        {
            //Check if any of the teleporters return null if so cancel
            QuickLogger.Debug($"In Teleport Player Main", true);

            if (currentTeleporter == null || destinationTeleporter == null /*|| currentTeleporter.Manager == null */ ||
                destinationTeleporter.Manager == null)
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
            _currentTeleporter.PowerManager.TakePower(_tab);
            _destinationTeleporter.PowerManager.TakePower(_tab == QTTeleportTypes.Vehicle
                ? QTTeleportTypes.Global
                : _tab);
            ToggleWarpScreen(true);
            _teleportComplete = false;
            _startTeleportCheck = true;
            Player.main.teleportingLoopSound.Play();
            FreezeStats();

            if (_tab != QTTeleportTypes.Vehicle)
            {
                QuickLogger.Debug($"In Teleport Player", true);
                //bool flag2 = Player.main.AddUsedTool(TechType.PrecursorTeleporter);
                Player.main.playerController.inputEnabled = false;
                Inventory.main.quickSlots.SetIgnoreHotkeyInput(true);
                Player.main.GetPDA().SetIgnorePDAInput(true);
                Player.main.playerController.SetEnabled(false);
            }
        }

        private static void SetWarpPosition()
        {
            if (_tab == QTTeleportTypes.Vehicle)
            {
                var vehicle = Player.main.GetVehicle();

                int num3 = UWE.Utils.OverlapSphereIntoSharedBuffer(vehicle.transform.position, 30);

                for (int i = 0; i < num3; i++)
                {
                    Collider collider = UWE.Utils.sharedColliderBuffer[i];
                    GameObject gameObject = UWE.Utils.GetEntityRoot(collider.gameObject);
                    if (gameObject == null)
                    {
                        gameObject = collider.gameObject;
                    }

                    Creature component = gameObject.GetComponent<Creature>();
                    LiveMixin component2 = gameObject.GetComponent<LiveMixin>();
                    if (component != null && component2 != null)
                    {
                        component2.TakeDamage(3, vehicle.transform.position, DamageType.Electrical, vehicle.gameObject);
                    }
                }

                vehicle.TeleportVehicle(
                    _destinationTeleporter
                        .GetTarget(Player.main.inSeamoth ? TeleportItemType.Seamoth : TeleportItemType.Exosuit,
                            vehicle.gameObject.GetComponent<PrefabIdentifier>()?.Id).position,
                    Quaternion.Euler(new Vector3(0f, 0f, 0f)));
            }
            else
            {
                Quaternion quaternion = Quaternion.Euler(new Vector3(0f, 180f, 0f));
                Player.main.SetPosition(_destinationTeleporter.GetTarget().position);
                Player.main.gameObject.transform.rotation = quaternion;
                Player.main.OnPlayerPositionCheat();
                //Player.main.gameObject.transform.position = _destinationTeleporter.GetTarget().position;
            }
        }

        private static void CheckTeleportationComplete()
        {
            if (LargeWorldStreamer.main.IsWorldSettled())
            {
                if (_tab != QTTeleportTypes.Vehicle)
                {
                    Player.main.SetCurrentSub(_destinationTeleporter.IsInside()
                        ? _destinationTeleporter.Manager.Habitat
                        : null);
                    Inventory.main.quickSlots.SetIgnoreHotkeyInput(false);
                    Player.main.GetPDA().SetIgnorePDAInput(false);
                    Player.main.playerController.inputEnabled = true;
                    Player.main.playerController.SetEnabled(true);
                }
                else
                {
                    Player.main.GetVehicle().gameObject.GetComponentInChildren<Rigidbody>().isKinematic = false;
                }

                Player.main.teleportingLoopSound.Stop();
                ToggleWarpScreen(false);
                _teleportComplete = true;
                _startTeleportCheck = false;
                UnfreezeStats();
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
            if (_tab == QTTeleportTypes.Intra) return;

            if (_teleportEffects == null)
            {
                _teleportEffects = Player.main.camRoot.mainCam.GetComponentInChildren<TeleportScreenFXController>();
            }

            if (activate)
            {
                _teleportEffects.StartTeleport(
#if BELOWZERO
                    String.Empty
#endif
                );
            }
            else
            {
                _teleportEffects.StopTeleport();
            }
        }
    }
}