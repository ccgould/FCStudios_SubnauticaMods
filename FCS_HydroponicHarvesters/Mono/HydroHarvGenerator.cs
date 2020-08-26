using System.Collections.Generic;
using FCS_HydroponicHarvesters.Enumerators;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HydroponicHarvesters.Mono
{
    internal class HydroHarvGenerator : MonoBehaviour
    {
        internal const float CooldownComplete = 19f;
        internal const float StartUpComplete = 4f;
        private readonly IList<float> _progress = new List<float>(new[] { -1f, -1f, -1f });
        public bool NotAllowToGenerate => PauseUpdates || !_mono.IsConstructed || _mono.CurrentSpeedMode == SpeedModes.Off;

        public bool PauseUpdates { get; set; }

        private HydroHarvController _mono;

        internal float StartUpProgress
        {
            get => _progress[(int)ClonePhases.StartUp];
            set => _progress[(int)ClonePhases.StartUp] = value;
        }
        internal float GenerationProgress
        {
            get => _progress[(int)ClonePhases.Generating];
            set => _progress[(int)ClonePhases.Generating] = value;
        }

        internal float CoolDownProgress
        {
            get => _progress[(int)ClonePhases.CoolDown];
            set => _progress[(int)ClonePhases.CoolDown] = value;
        }

        internal void Initialize(HydroHarvController mono)
        {
            _mono = mono;
        }

        private void Update()
        {
            //QuickLogger.Debug($"PauseUpdates: {PauseUpdates} || IsConstructed: {_mono.IsConstructed} || CurrentSpeedMode: {_mono.CurrentSpeedMode}",true);

            if (NotAllowToGenerate)
                return;
            
            if (!_mono.PowerManager.HasPowerToConsume())
                return;

            if (CoolDownProgress >= CooldownComplete)
            {
                QuickLogger.Debug("[Hydroponic Harvester] Finished Cooldown", true);

                PauseUpdates = true;
                _mono.HydroHarvContainer.SpawnClone();
                // Finished cool down - See if the next clone can be started                
                TryStartingNextClone(); //TODO check here

                this.PauseUpdates = false;
            }
            else if (this.CoolDownProgress >= 0f)
            {
                // Is currently cooling down
                this.CoolDownProgress = Mathf.Min(CooldownComplete, this.CoolDownProgress + DayNightCycle.main.deltaTime);
            }
            else if (this.GenerationProgress >= _mono.EnergyCost)
            {
                QuickLogger.Debug("[Hydroponic Harvester] Cooldown", true);

                // Finished generating clone - Start cool down
                this.CoolDownProgress = 0f;
            }
            else if (this.GenerationProgress >= 0f)
            {
                _mono.PowerManager.ConsumePower();

                // Is currently generating clone
                this.GenerationProgress = Mathf.Min(_mono.EnergyCost, this.GenerationProgress + _mono.PowerManager.GetEnergyToConsume());
            }
            else if (this.StartUpProgress >= StartUpComplete)
            {
                QuickLogger.Debug("[Hydroponic Harvester] Generating", true);
                // Finished start up - Start generating clone
                this.GenerationProgress = 0f;
            }
            else if (this.StartUpProgress >= 0f)
            {
                // Is currently in start up routine
                this.StartUpProgress = Mathf.Min(StartUpComplete, this.StartUpProgress + DayNightCycle.main.deltaTime);
            }
        }

        internal void TryStartingNextClone()
        {
            QuickLogger.Debug("Trying to start another clone", true);

            if (_mono.CurrentSpeedMode == SpeedModes.Off || _mono.HydroHarvCleanerManager.GetIsDirty())
                return;// Powered off, can't start a new clone

            if (this.StartUpProgress < 0f || // Has not started a clone yet
                this.CoolDownProgress == CooldownComplete) // Has finished a clone
            {
                if (!_mono.HydroHarvContainer.IsFull)
                {
                    QuickLogger.Debug("[Hydroponic Harvester] Start up", true);
                    this.CoolDownProgress = -1f;
                    this.GenerationProgress = -1f;
                    this.StartUpProgress = 0f;
                }
                else
                {
                    QuickLogger.Debug("Cannot start another clone, container is full", true);
                }
            }
            else
            {
                QuickLogger.Debug("Cannot start another clone, another clone is currently in progress", true);
            }
        }
    }
}
