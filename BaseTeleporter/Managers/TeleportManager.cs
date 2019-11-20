using System.Collections;
using AE.BaseTeleporter.Buildables;
using AE.BaseTeleporter.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace AE.BaseTeleporter.Managers
{
    internal class TeleportManager : MonoBehaviour
    {
        private float _spoolTime = 3.0f;
        private Transform _target;
        private BaseTeleporterController _mono;
        private bool _initialized;
        private bool _teleportPlayer;
        private float _timer;
        private int _animationRunning;
        private BaseTeleporterController _fromUnit;
        private float _delayTime = 3.0f;


        private void Update()
        {
            if (_mono == null || !_mono.IsConstructed || !_initialized) return;

            if (_teleportPlayer)
            {
                _timer += DayNightCycle.main.deltaTime;

                if (_timer > _spoolTime + _delayTime)
                {
                    _mono.AnimationManager.SetBoolHash(_animationRunning, false);
                    _mono.AudioManager.StopAudio();
                    Player.main.playerController.SetEnabled(true);
                    _teleportPlayer = false;
                    _timer = 0f;
                }
                else if (_timer > _spoolTime)
                {
                    Player.main.SetPosition(new Vector3(_target.transform.position.x, _target.transform.position.y, _target.transform.position.z));
                    _fromUnit.AnimationManager.SetBoolHash(_animationRunning, false);
                    _fromUnit.AudioManager.StopAudio();
                }
            }
        }

        internal void Initialize(BaseTeleporterController mono)
        {
            _mono = mono;

            _animationRunning = Animator.StringToHash("AnimationRunning");

            if (FindComponents())
            {
                _initialized = true;
            }
        }

        private bool FindComponents()
        {
            var target = _mono.gameObject.FindChild("targetPos");

            if (target == null)
            {
                QuickLogger.Error("Cant find trigger targetPos");
                return false;
            }

            _target = target.transform;

            return true;
        }

        internal Transform Target()
        {
            return _target;
        }

        internal float GetSpoolTime() => _spoolTime;

        internal void SetSpoolTime(float dataSpoolTime) {_spoolTime = dataSpoolTime; }

        internal void TeleportPlayer(BaseTeleporterController mono)
        {
            if (_initialized)
            {
                if (_mono.PowerManager.TakePower())
                {
                    _fromUnit = mono;
                    _fromUnit.AnimationManager.SetBoolHash(_animationRunning, true);
                    _fromUnit.AudioManager.PlayAudio();
                    _mono.AudioManager.PlayAudio();
                    _mono.AnimationManager.SetBoolHash(_animationRunning, true);
                    Player.main.playerController.SetEnabled(false);
                    _teleportPlayer = true;
                }
                else
                {
                    QuickLogger.Message($"{BaseTeleporterBuildable.NotEnoughPower()}: Power Needed: {_mono.PowerManager.NeededPower()} units",true);
                }

            }
        }

    }
}
