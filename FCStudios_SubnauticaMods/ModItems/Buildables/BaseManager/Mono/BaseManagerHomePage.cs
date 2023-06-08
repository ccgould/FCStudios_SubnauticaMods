using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono.GUI.Dialogs;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono;
internal class BaseManagerHomePage : Page
{
    public override PDAPages PageType => PDAPages.None;
    
    [SerializeField]
    private TMP_Text clock;
    [SerializeField]
    private TMP_Text baseName;
    [SerializeField]
    private TMP_Text daysPassedValue;
    [SerializeField]
    private TMP_Text hullStrengthValue;
    [SerializeField]
    private TMP_Text temperatureValue;
    [SerializeField]
    private TMP_Text basePowerLabel;
    [SerializeField]
    private Transform grid;
    [SerializeField]
    private GameObject basePowerListItemPrefab;
    private HabitatManager _baseManager;

    private void Update()
    {
        if (!Player.main.IsInBase()) return;
        var subRoot = Player.main.GetCurrentSub();
        daysPassedValue.text = DayNightCycle.main.GetDay().ToString("F2");
        hullStrengthValue.text = subRoot.gameObject.GetComponent<BaseHullStrength>()?.GetTotalStrength().ToString("F2") ?? "0";
        temperatureValue.text = Language.main.GetFormat("ThermalPlantCelsius", subRoot.GetTemperature());
        basePowerLabel.text = GetPowerInfo();
    }

    private string GetPowerInfo()
    {
        SubRoot currentSub = Player.main.currentSub;
        if (currentSub != null && currentSub.isBase)
        {
            PowerRelay powerRelay = currentSub.powerRelay;
            if (powerRelay != null)
            {
               var power = Mathf.RoundToInt(powerRelay.GetPower());
               var maxPower = Mathf.RoundToInt(powerRelay.GetMaxPower());
               var status = powerRelay.GetPowerStatus();
                return $"BASE POWER {power}/{maxPower}";
            }
        }

        return "BASE POWER 000/000";
    }

    public void OnBulkHeadToggleChanged(bool value)
    {
        //HabitatService.main.GetPlayersCurrentBase();

        var subRoot = Player.main.GetCurrentSub();

       var bulkHeads =  subRoot.GetAllComponentsInChildren<BulkheadDoor>();

        foreach (var bulkhead in bulkHeads)
        {
            bulkhead.SetState(value);
        }
    }

    public void OnFilterationMachineToggle(bool value)
    {
       var geo =  Player.main.GetCurrentSub().GetAllComponentsInChildren<BaseFiltrationMachineGeometry>();

        QuickLogger.Debug($"Geo: {geo.Count()}", true);

        foreach (var filtrationMachine in geo)
        {
            var comp = filtrationMachine.GetModule();
            if(!value)
            {
                comp.CancelInvoke("UpdateFiltering");
                filtrationMachine.workSound.Stop();
                filtrationMachine.vfxController.Stop(1);
            }
            else
            {
                comp.InvokeRepeating("UpdateFiltering", 1f, 1f);
                filtrationMachine.workSound.Play();
                filtrationMachine.vfxController.Play(1);
            }
        }
    }

    public void OnRenameBaseButtonClicked()
    {
        uGUI.main.userInput.RequestString(LanguageService.ChangeBaseName(), LanguageService.Submit(), _baseManager.GetBaseName(), 20, (s) =>
        {
            _baseManager.SetBaseName(s);
            UpdateBaseName();
        });


    }

    private void UpdateBaseName()
    {
        if (string.IsNullOrEmpty(_baseManager.GetBaseName()))
        {
            baseName.text = _baseManager.GetBaseFriendlyName();
            clock.text = string.Empty;
        }
        else
        {
            baseName.text = _baseManager.GetBaseName();
            clock.text = _baseManager.GetBaseFriendlyName();
        }
    }

    public void OnCancelScansClicked()
    {
        if (!Player.main.IsInBase()) return;
        var scanners = Player.main.GetCurrentSub().GetComponentsInChildren<uGUI_MapRoomScanner>();

        foreach (var scanner in scanners)
        {
            scanner.OnCancelScan();
        }
    }

    public override void OnBackButtonClicked()
    {

    }

    public override void Enter(object arg = null)
    {
        base.Enter(arg);
        RefreshPage();
    }

    private void RefreshPage()
    {
        _baseManager = HabitatService.main.GetPlayersCurrentBase();

        UpdateBaseName();

        foreach (Transform item in grid)
        {
            Destroy(item.gameObject);
        }

        var powerSources = new Dictionary<string, List<PowerSource>>();


        GetPowerSource<BaseNuclearReactor>(powerSources, "Nuclear Reactor");
        GetPowerSource<SolarPanel>(powerSources, "Solar Panel");
        GetPowerSource<BaseBioReactor>(powerSources, "Bio Reactor");
        GetPowerSource<ThermalPlant>(powerSources, "Thermal Plant");

        foreach (var powerSource in powerSources)
        {
            var h = GameObject.Instantiate(basePowerListItemPrefab, grid, false);
            var controller = h.GetComponent<BasePowerInfoListItem>();
            controller.Initialize(powerSource.Key, powerSource.Value);
        }
    }

    public void OnToggleLightOn(bool value)
    {
        var subroot = Player.main.GetCurrentSub();
        if (subroot != null)
        {
            subroot.ForceLightingState(value);
        }
    }

    public void OnExternalLightsToggle(bool value)
    {
        var floodLights = Player.main.GetCurrentSub().GetComponentsInChildren<TechLight>();

        QuickLogger.Debug($"Flood Lights: {floodLights.Count()}", true);

        foreach (var item in floodLights)
        {
            if(item.constructable.constructed)
            {

                if (value)
                {
                    item.InvokeRepeating("UpdatePower", 0f, TechLight.updateInterval);
                }
                else
                {
                    item.CancelInvoke("UpdatePower");
                }

                item.SetLightsActive(value);
            }
        }
    }

    public void OnAutoDimLightsToggle(bool value)
    {
        var subroot = Player.main.GetCurrentSub();
        if (subroot != null)
        {
            subroot.subWarning = value;
        }
    }

    public void OnDimLightSliderChanged(float value)
    {

    }

    

    private static void GetPowerSource<T>(Dictionary<string, List<PowerSource>> powerSources, string key) where T :  Component
    {
        foreach (T nr in Player.main.GetCurrentSub().GetAllComponentsInChildren<T>())
        {
            var powerSource = nr.GetComponent<PowerSource>();
            if (powerSources.ContainsKey(key))
            {
                powerSources[key].Add(powerSource);
            }
            else
            {
                powerSources.Add(key, new List<PowerSource> { powerSource });
            }
        }
    }
}
