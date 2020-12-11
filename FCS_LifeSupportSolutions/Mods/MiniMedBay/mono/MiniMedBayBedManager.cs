using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCS_LifeSupportSolutions.Mods.MiniMedBay.Enumerators;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Mods.MiniMedBay.mono
{
    internal class MiniMedBayBedManager : MonoBehaviour
    {
        private MiniMedBayController _mono;
        private bool _isHealing;
        private HealingStatus _healingStatus;
        private float _timeCurrDeltaTime;
        private float _nitrogenPartial;
        private NitrogenLevel _nitrogenLevel;
        private float _healthPartial;
        private const float ProcessTime = 3.0f;

        internal void Initialize(MiniMedBayController mono)
        {
            _mono = mono;

            if (Player.main.gameObject.GetComponent<NitrogenLevel>() != null)
            {
                _nitrogenLevel = Player.main.gameObject.GetComponent<NitrogenLevel>();
            }
        }

        private void Update()
        {
            
            if (_mono != null && _mono.DisplayManager != null && Player.main != null)
            {
                _mono.DisplayManager.UpdatePlayerHealthPercent(_mono.Trigger.InPlayerInTrigger ? Mathf.CeilToInt(Player.main.liveMixin.health) : 0);
            }
            else
            {
                return;
            }

            if (!_isHealing) return;

            _healingStatus = HealingStatus.Healing;

            QuickLogger.Debug("Healing Player", true);

            _timeCurrDeltaTime += DayNightCycle.main.deltaTime;

            QuickLogger.Debug($"Delta Time: {_timeCurrDeltaTime}");
            if (!(_timeCurrDeltaTime >= 1)) return;

            QuickLogger.Debug("Delta Passed", true);

            _timeCurrDeltaTime = 0.0f;

            var playerHealth = Player.main.liveMixin.health;
            var playerMaxHealth = Player.main.liveMixin.maxHealth;

            if (_nitrogenLevel != null)
            {
                _nitrogenLevel.safeNitrogenDepth =
                    Mathf.Clamp(_nitrogenLevel.safeNitrogenDepth -= _nitrogenPartial, 0, float.MaxValue);
            }


            if (!Player.main.liveMixin.IsFullHealth())
            {
                QuickLogger.Debug("Added Health", true);
                Player.main.liveMixin.health = Mathf.Clamp(playerHealth + _healthPartial, 0, playerMaxHealth);
                QuickLogger.Debug($"Player Health = {playerHealth}", true);
            }
            else
            {
                ResetMachine();
            }
        }

        private void ResetMachine()
        {
            Player.main?.playerController.SetEnabled(true);
            QuickLogger.Debug("Resetting", true);
            _healingStatus = HealingStatus.Idle;
            UpdateIsHealing(false);
        }

        private void UpdateIsHealing(bool value)
        {
            _isHealing = value;

            if (_isHealing)
            {
                _mono.AudioHandler.PlayScanAudio();
            }
            else
            {
                _mono.AudioHandler.StopScanAudio();
            }
        }

        internal HealingStatus GetHealingStatus()
        {
            return _healingStatus;
        }

        internal void HealPlayer()
        {
            if (!_mono.Trigger.InPlayerInTrigger)
            {
                //TODO Add Message To Screen
                return;
            }

            if (_nitrogenLevel.GetNitrogenEnabled())
            {
                _nitrogenPartial = _nitrogenLevel.safeNitrogenDepth / ProcessTime;
            }

            if (Player.main.liveMixin.IsFullHealth()) return;

            Player.main.playerController.SetEnabled(false);
            var remainder = Player.main.liveMixin.maxHealth - Player.main.liveMixin.health;
            _healthPartial = remainder / ProcessTime;
            UpdateIsHealing(true);
        }

    }
}
