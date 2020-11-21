using System;
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
        private float _energyPerSec;
        private string _curBiome;
        private float temperature;
        private float _inverseLerp;
        private bool _isInitialized;
        private const float GenerationAmount = 1.6500001f;

        internal bool ProducingPower
        {
            get
            {
                var value = _mono != null && _mono.IsConstructed && _powerState == FCSPowerStates.Powered;
                return value;
            }
        }
        
        internal void Initialize(JetStreamT242Controller mono)
        {
            if (_isInitialized) return;
            _mono = mono;
            _powerRelay = gameObject.GetComponentInParent<PowerRelay>();

            if (_powerRelay == null)
            {
                QuickLogger.Error("JetStream could not find PowerRelay", this);
                return;
            }
            _powerSource = GetComponent<PowerSource>();
            if (_powerSource == null)
            {
                Debug.LogError("JetStream could not find PowerSource", this);
                return;
            }
            InvokeRepeating("QueryTemperature", UnityEngine.Random.value, 10f);
            _isInitialized = true;
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
            //float perSecondDelta = 0f;
            //if (MaxSpeed > 0)
            //{
            //    var decPercentage = (QPatch.JetStreamT242Configuration.PowerPerSecond / MaxSpeed);
            //    _energyPerSec = (_mono.GetCurrentSpeed() * decPercentage);
            //    perSecondDelta = _energyPerSec * DayNightCycle.main.deltaTime;
            //}
            //else
            //{
            //    perSecondDelta = 0;
            //}

            //if (_hasBreakerTripped) return;

            //float num2 = _powerSource.maxPower - _powerSource.power;

            //if (num2 > 0f)
            //{
            //    if (num2 < perSecondDelta)
            //    {
            //        perSecondDelta = num2;
            //    }
            //    _powerSource.AddEnergy(perSecondDelta, out var amountStored);
            //}

            var energryGen = GenerationAmount * DayNightCycle.main.deltaTime;
            _inverseLerp = Mathf.Clamp01(Mathf.InverseLerp(25f, 100f, this.temperature));
            _energyPerSec = energryGen + _inverseLerp;
            
            //float amount = 4.1666665f * DayNightCycle.main.deltaTime;

            _powerSource.AddEnergy(_energyPerSec, out float num2);
            
            //float amount = this.GetRechargeScalar() * DayNightCycle.main.deltaTime * 40f * WindyMultiplier();
            //float num;
            //_powerSource.AddEnergy(amount / 4f, out num);

            //QuickLogger.Debug($" Amount: {amount} || Per Minute: {amount * 60}",true);

        }

        private void QueryTemperature()
        {
            WaterTemperatureSimulation main = WaterTemperatureSimulation.main;
            if (main)
            {
                this.temperature = Mathf.Max(this.temperature, main.GetTemperature(base.transform.position));
            }
        }
        
        float WaterMultiplier()
        {
            return (1f + (Mathf.PerlinNoise(0f, Time.time * 0.05f) * 1.5f));
        }
        
        internal float GetBiomeData(string biome)
        {
            if (QPatch.JetStreamT242Configuration.BiomeSpeeds == null) return 0;

            QuickLogger.Debug($"Finding the speed for the biome {biome}",true);

            foreach (KeyValuePair<string, float> biomeSpeed in QPatch.JetStreamT242Configuration.BiomeSpeeds)
            {
                if (biome.Trim().StartsWith(biomeSpeed.Key,StringComparison.OrdinalIgnoreCase))
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
            QuickLogger.Debug($"Trying to load from save. Is null check = SP{savedData == null} || PS{_powerSource == null}");
            if (_powerSource == null || savedData == null) return;
           _powerSource.power = savedData.StoredPower;
           ChangePowerState(savedData.PowerState);
           QuickLogger.Debug($"Loaded Save: {savedData.PowerState} Current: {_powerState}");
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
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
            main.SetInteractText(
                Language.main.GetFormat(AuxPatchers.JetStreamOnHover(), _mono.UnitID,
                    Mathf.RoundToInt(this._powerSource.GetPower()), Mathf.RoundToInt(this._powerSource.GetMaxPower()),
                    ((GenerationAmount + _inverseLerp) * 60).ToString("N1")),
                AuxPatchers.JetStreamOnHoverInteractionFormatted("E",_powerState.ToString()));
            main.SetIcon(HandReticle.IconType.Info);

            if (GameInput.GetButtonDown(GameInput.Button.Exit))
            {
                if (_powerState != FCSPowerStates.Powered)
                {
                    _mono.ActivateTurbine();
                }
                else
                {
                    _mono.DeActivateTurbine();
                }
            }
        }

        public void OnHandClick(GUIHand hand)
        {

        }

        public void ChangePowerState(FCSPowerStates powerState)
        {
            _powerState = powerState;
        }
    }
}
