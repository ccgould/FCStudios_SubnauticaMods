using FCS_LifeSupportSolutions.ModItems.Buildables.MiniMedBay.Enumerators;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FCS_LifeSupportSolutions.ModItems.Buildables.MiniMedBay.Mono;
internal class MiniMedBayBedManager : MonoBehaviour
{
    [SerializeField] private MiniMedBayTrigger trigger;
    private FMOD_CustomLoopingEmitter _loopingEmitter;


    private HealingStatus _healingStatus;
    private float _timeCurrDeltaTime;
    private float _nitrogenPartial;
    private NitrogenLevel _nitrogenLevel;
    private float _healthPartial;
    private const float ProcessTime = 3.0f;
    public bool IsHealing => _healingStatus == HealingStatus.Healing;

    private void Awake()
    {
        _loopingEmitter = gameObject.GetComponent<FMOD_CustomLoopingEmitter>();

        if (Player.main.gameObject.GetComponent<NitrogenLevel>() != null)
        {
            _nitrogenLevel = Player.main.gameObject.GetComponent<NitrogenLevel>();
        }
    }


    private void Update()
    {

        if (Player.main == null)
        {
            return;
        }

        if (_healingStatus != HealingStatus.Healing) return;

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
        UpdateIsHealing(HealingStatus.Idle);
    }

    private void UpdateIsHealing(HealingStatus status)
    {
        _healingStatus = status;

        if (status == HealingStatus.Healing)
        {
            PlayScanAudio();
        }
        else
        {
            StopScanAudio();
        }
    }

    internal HealingStatus GetHealingStatus()
    {
        return _healingStatus;
    }

    public void HealPlayer()
    {
        if (!trigger.IsPlayerInTrigger())
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
        UpdateIsHealing(HealingStatus.Healing);
    }

    #region Audio System
    private void PlayScanAudio()
    {
        if (!_loopingEmitter.playing)
        {
            _loopingEmitter.Play();
        }
    }

    private void StopScanAudio()
    {
        if (_loopingEmitter.playing)
        {
            _loopingEmitter.Stop();
        }
    }
    #endregion
}
