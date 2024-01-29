using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Models.Enumerators;
using FCS_EnergySolutions.Configuration;
using FCSCommon.Utilities;
using System.Collections.Generic;
using System;
using UnityEngine;
using static FCS_EnergySolutions.Configuration.SaveData;

namespace FCS_EnergySolutions.ModItems.Buildables.JetStream.Mono;
internal class JetStreamT242PowerManager :MonoBehaviour
{
    [SerializeField] private PowerSource _powerSource;
    [SerializeField] private JetStreamController _mono;
    private FCSPowerStates _powerState;
    private float _energyPerSec;
    private string _curBiome;
    private float _temperature;
    private bool _isInitialized;
    private float _time;
    [SerializeField] private const float MaxThermalEnergyPerSecond = 1.6500001f;
    [SerializeField] private float MaxTurbineEnergyPerSecond = 2.66f;
    [SerializeField] private const float AddPowerInterval = 2f;

    internal bool ProducingPower
    {
        get
        {
            var value = _mono != null && _mono.IsConstructed && _powerState == FCSPowerStates.Powered;
            return value;
        }
    }

    private void Awake()
    {
        InvokeRepeating("QueryTemperature", UnityEngine.Random.value, 10f);
    }

    internal PowerSource GetPowerSource()
    {
        return _powerSource;
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
        if (WorldHelpers.CheckIfPaused())
        {
            return;
        }

        if (UWEHelpers.RequiresPower())
        {
            _time -= DayNightCycle.main.deltaTime;
            var addPowerInterval = AddPowerInterval * DayNightCycle.main.dayNightSpeed;

            if (_time <= 0)
            {
                var thermalPower = MaxThermalEnergyPerSecond * addPowerInterval * Mathf.Clamp01(Mathf.InverseLerp(25f, 100f, this._temperature));
                QuickLogger.Debug($"Turbine Thermal Power: {thermalPower}");
                _energyPerSec = MaxTurbineEnergyPerSecond + thermalPower;
                _powerSource.AddEnergy(_energyPerSec, out float num2);
                _time = 1f;
            }

        }
    }

    private void QueryTemperature()
    {
        WaterTemperatureSimulation main = WaterTemperatureSimulation.main;
        if (main)
        {
            this._temperature = Mathf.Max(this._temperature, main.GetTemperature(base.transform.position));
        }
    }

    private float WaterMultiplier()
    {
        return (1f + (Mathf.PerlinNoise(0f, Time.time * 0.05f) * 1.5f));
    }

    internal float GetBiomeData(string biome)
    {
        if (Plugin.Configuration.JetStreamT242BiomeSpeeds == null) return 0;

        QuickLogger.Debug($"Finding the speed for the biome {biome}", true);

        foreach (KeyValuePair<string, float> biomeSpeed in Plugin.Configuration.JetStreamT242BiomeSpeeds)
        {
            if (biome.Trim().StartsWith(biomeSpeed.Key, StringComparison.OrdinalIgnoreCase))
            {
                QuickLogger.Debug($"Found Speed: {biomeSpeed.Value}", true);
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
        QuickLogger.Debug($"Current Biome: {_curBiome}", true);
        return _curBiome;
    }

    public void LoadFromSave(JetStreamT242SaveData savedData)
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

    public void Save(JetStreamT242SaveData savedData)
    {
        savedData.PowerState = _powerState;
        savedData.StoredPower = GetStoredPower();
    }

    //public void OnHandHover(GUIHand hand)
    //{
    //    var techType = _mono.GetTechType();

    //    if (!_mono.IsUpright())
    //    {
    //        var data = new[]
    //        {
    //                AuxPatchers.NotOnPlatform()
    //            };
    //        data.HandHoverPDAHelperEx(techType, HandReticle.IconType.HandDeny);
    //        return;
    //    }


    //    if (!_mono.IsUnderWater())
    //    {
    //        var data = new[]
    //        {
    //                AuxPatchers.NotUnderWater(),
    //                AuxPatchers.NotUnderWaterDesc()
    //            };
    //        data.HandHoverPDAHelperEx(techType, HandReticle.IconType.HandDeny);
    //        return;
    //    }

    //    var data1 = new[]
    //    {
    //            Language.main.GetFormat(AuxPatchers.JetStreamOnHover(),
    //                _mono.UnitID,Mathf.RoundToInt(this._powerSource.GetPower()),
    //                Mathf.RoundToInt(this._powerSource.GetMaxPower()),
    //                (_energyPerSec * 60).ToString("N1")),
    //            AuxPatchers.JetStreamOnHoverInteractionFormatted("E", _powerState.ToString())

    //        };
    //    data1.HandHoverPDAHelperEx(techType);

    //    if (GameInput.GetButtonDown(GameInput.Button.Exit))
    //    {
    //        if (_powerState != FCSPowerStates.Powered)
    //        {
    //            _mono.ActivateTurbine();
    //        }
    //        else
    //        {
    //            _mono.DeActivateTurbine();
    //        }
    //    }

    //    if (Input.GetKeyDown(FCS_AlterraHub.Main.Configuration.PDAInfoKeyCode))
    //    {
    //        //TODO V2 FIx
    //        //FCSPDAController.Main.OpenEncyclopedia(_mono.GetTechType());
    //    }
    //}


    public void ChangePowerState(FCSPowerStates powerState)
    {
        _powerState = powerState;
    }

    internal FCSPowerStates GetPowerState()
    {
        return _powerState;
    }

    internal void ToggleDevice()
    {
        if (_powerState != FCSPowerStates.Powered)
        {
            _mono.TurnOnDevice();
        }
        else
        {
            _mono.TurnOffDevice();
        }
    }
}