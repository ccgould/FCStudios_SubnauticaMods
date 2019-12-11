using System;
using System.Collections;
using FCSCommon.Utilities;
using QuantumTeleporter.Buildable;
using QuantumTeleporter.Enumerators;
using QuantumTeleporter.Mono;
using UnityEngine;

namespace QuantumTeleporter.Managers
{
    internal static class TeleportManager
    {
        private static float _spoolTime = 3.0f;
        private static float _delayTime = 3.0f;
        private static Transform _target;
        private static QuantumTeleporterController _toUnit;
        private static bool _initialized;
        private static bool _teleportPlayer;
        private static float _timer;
        private static int _animationRunning;
        private static QuantumTeleporterController _fromUnit;
        private static int _isPanelVisible;
        private static Coroutine _coroutine;
        private static bool _hasArrived;
        
        internal static void Update()
        {
            if (!_initialized || _toUnit == null ||!_toUnit.IsConstructed || _fromUnit == null || !_fromUnit.IsConstructed) return;

            if (_teleportPlayer)
            {
                _timer += DayNightCycle.main.deltaTime;

                if (_timer >= _spoolTime && !_hasArrived)
                {
                    if (_target == null)
                    {
                        _teleportPlayer = _hasArrived = false;
                        _timer = 0f;
                        QuickLogger.Debug($"Target Position is null",true);
                        return;
                    }

                    Player.main.SetPosition(new Vector3(_target.transform.position.x, _target.transform.position.y, _target.transform.position.z));
                    _hasArrived = true;

                    if (_fromUnit != null)
                    {
                        _fromUnit.DisplayManager.ShowTeleportPage(false);
                        ResetTeleport(_fromUnit);
                    }

                    QuickLogger.Debug($"// =======  From Unit ====== //");

                }

                if (_hasArrived && _timer >= _spoolTime + _delayTime)
                {
                    Player.main.SetCurrentSub(_toUnit.SubRoot);

                    if (LargeWorldStreamer.main.IsWorldSettled())
                    {
                        ResetTeleport(_toUnit, true);
                        SetIsTeleporting(false);
                        _toUnit.DisplayManager.ShowTeleportPage(false);
                        Player.main.playerController.SetEnabled(true);
                        _hasArrived = false;
                        _timer = 0f;
                        QuickLogger.Debug($"// =======  Mono Unit ====== //");
                    }
                }
            }
        }
        
        private static void ResetTeleport(QuantumTeleporterController mono, bool openDoor = false)
        {
            try
            {
                QuickLogger.Debug($"// =======  Reset Teleport {mono.NameController.GetCurrentName()} ====== //");
                mono.AnimationManager.SetBoolHash(_animationRunning, false);
                mono.AudioManager.StopAudio();
                mono.AnimationManager.SetBoolHash(_isPanelVisible, true);
                mono.DisplayManager.RefreshTabs();
                
                if (openDoor)
                {
                    mono.QTDoorManager.OpenDoor();
                }
                QuickLogger.Debug($"// =======  Reset Teleport {mono.NameController.GetCurrentName()} ====== //");
            }
            catch (Exception e)
            {
                QuickLogger.Error<TeleporterManager>($"{e.Message} Stack: {e.StackTrace}");
            }
        }

        private static void InitializeTeleport(QuantumTeleporterController mono, QTTeleportTypes type)
        {
            mono.AudioManager.PlayAudio();
            mono.AnimationManager.SetBoolHash(_animationRunning, true);
            mono.AnimationManager.SetBoolHash(_isPanelVisible, false);
            mono.DisplayManager.ShowTeleportPage(true);
            mono.QTDoorManager.CloseDoor();
            mono.PowerManager.TakePower(type);
            SetIsTeleporting(true);
        }

        internal static void Initialize()
        {
            _animationRunning = Animator.StringToHash("AnimationRunning");
            _isPanelVisible = Animator.StringToHash("IsPanelVisible");
            _initialized = true;

        }

        internal static void TeleportPlayer(QuantumTeleporterController fromUnit, QuantumTeleporterController toUnit, QTTeleportTypes type)
        {
            if (_initialized)
            {
                _fromUnit = fromUnit;
                _toUnit = toUnit;
                
                if (!fromUnit.PowerManager.HasEnoughPower(type))
                {
                    QuickLogger.Message(QuantumTeleporterBuildable.BaseNotEnoughPower(),true);
                    return;
                }

                if (!_toUnit.PowerManager.HasEnoughPower(type))
                {
                    QuickLogger.Message(QuantumTeleporterBuildable.ToBaseNotEnoughPower(), true);
                    return;
                }
                
                InitializeTeleport(_toUnit, type);
                
                InitializeTeleport(_fromUnit, type);
                
                Player.main.playerController.SetEnabled(false);

                var target = _toUnit.GetTarget();
                
                if (target != null)
                {
                    _target = target;
                    SetIsTeleporting(true);
                }
            }
        }

        internal static void SetIsTeleporting(bool value)
        {
            _teleportPlayer = value;
        }
        internal static bool  IsTeleporting()
        {
            return _teleportPlayer;
        }
    }
}
