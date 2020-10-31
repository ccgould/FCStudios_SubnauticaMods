using System.Collections.Generic;
using FCS_AlterraHub.Enumerators;
using FCS_EnergySolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_EnergySolutions.JetStreamT242.Mono
{
    internal class JetStreamT242PowerManager : HandTarget, IHandTarget
    {
        private PowerRelay _powerRelay;
        private PowerSource _powerSource;
        private JetStreamT242Controller _mono;
        private FCSPowerStates _powerState;
        private const float MaxSpeed = 300f;
        private float _secondsInAMinute = 60f;
        private float _energyPerSec;
        private bool _hasBreakerTripped;
        private string _curBiome;

        internal bool ProducingPower
        {
            get
            {
                var value = _mono != null && _mono.IsConstructed && _powerState != FCSPowerStates.Tripped;
                return value;
            }
        }
        
        private void Start()
        {
            _mono = gameObject.GetComponent<JetStreamT242Controller>();
            _powerRelay = gameObject.GetComponentInParent<PowerRelay>();

            if (_powerRelay == null)
            {
                Debug.LogError("JetStream could not find PowerRelay", this);
            }
            _powerSource = base.GetComponent<PowerSource>();
            if (_powerSource == null)
            {
                Debug.LogError("JetStream could not find PowerSource", this);
            }
        }

        private void Update()
        {
            if (this.ProducingPower)
            {
                ProducePower();
            }
        }

        private void ProducePower()
        {
            float perSecondDelta = 0f;
            if (MaxSpeed > 0)
            {
                var decPercentage = (QPatch.JetStreamT242Configuration.PowerPerSecond / MaxSpeed);
                _energyPerSec = (_mono.GetCurrentSpeed() * decPercentage);
                perSecondDelta = _energyPerSec * DayNightCycle.main.deltaTime;
            }
            else
            {
                perSecondDelta = 0;
            }

            if (_hasBreakerTripped) return;

            float num2 = _powerSource.maxPower - _powerSource.power;

            if (num2 > 0f)
            {
                if (num2 < perSecondDelta)
                {
                    perSecondDelta = num2;
                }

                _powerSource.AddEnergy(perSecondDelta, out var amountStored);
            }
        }

        internal float GetBiomeData(string biome)
        {
            if (QPatch.JetStreamT242Configuration.BiomeSpeeds == null) return 0;

            QuickLogger.Debug($"Finding the speed for the biome {biome}",true);

            foreach (KeyValuePair<string, float> biomeSpeed in QPatch.JetStreamT242Configuration.BiomeSpeeds)
            {
                if (biome.Trim().StartsWith(biomeSpeed.Key))
                {
                    QuickLogger.Debug($"Found Speed: {biomeSpeed.Value}",true);
                    return biomeSpeed.Value;
                }
            }

            QuickLogger.DebugError("Biome not found returning 0.", true);
            return 0;
        }

        internal float GetBiomeData()
        {
           return GetBiomeData(GetBiome());
        }

        internal string GetBiome()
        {
            _curBiome = LargeWorld.main.GetBiome(gameObject.transform.position);
            QuickLogger.Debug($"Current Biome: {_curBiome}",true);
            return _curBiome;
        }
        
        public void LoadFromSave(JetStreamT242DataEntry savedData)
        {
            if(_powerSource == null || savedData?.StoredPower == null) return;
           _powerSource.power = savedData.StoredPower;
        }

        public float GetEnergyProducing()
        {
            return _energyPerSec;
        }

        public float GetStoredPower()
        {
            return _powerSource.power;
        }

        public float GetMaxPower()
        {
            return _powerSource.maxPower;
        }

        public void Save(JetStreamT242DataEntry savedData)
        {
            savedData.PowerState = _powerState;
            savedData.StoredPower = GetStoredPower();
            savedData.CurrentBiome = _curBiome;
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
            main.SetInteractText(Language.main.GetFormat(AuxPatchers.JetStreamOnHover(), _mono.UnitID,Mathf.RoundToInt(this._powerSource.GetPower()), Mathf.RoundToInt(this._powerSource.GetMaxPower()), (_energyPerSec * 60).ToString("N1")));
            main.SetIcon(HandReticle.IconType.Info);
        }

        public void OnHandClick(GUIHand hand)
        {

        }
    }
}
