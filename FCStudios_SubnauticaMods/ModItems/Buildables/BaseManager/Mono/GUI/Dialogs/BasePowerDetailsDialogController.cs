using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono.GUI.Dialogs;
internal class BasePowerDetailsDialogController : Page
{
    [SerializeField]
    private Transform powerProductersGrid;
    [SerializeField]
    private Transform powerConsumersGrid;
    [SerializeField]
    private TMP_Text basePowerInfo;
    [SerializeField]
    private GameObject basePowerListItemPrefab;
    public override PDAPages PageType => PDAPages.None;

    private static List<FCSPowerInterface> _powerConsumers { get; set; } = new();

    private void Update()
    {
        if (!Player.main.IsInBase()) return;
        //var subRoot = Player.main.GetCurrentSub();
        basePowerInfo.text = GetPowerInfo();
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

    public override void Enter(object arg = null)
    {
        RefreshPage();
        base.Enter(arg);
    }

    private void RefreshPage()
    {
        RefreshProducers();
        RefreshConsumers();
    }

    private void RefreshProducers()
    {
        var powerSources = new Dictionary<string, List<PowerSource>>();

        var basePowerRelay = Player.main.GetCurrentSub()?.GetComponent<BasePowerRelay>();

        
        foreach (PowerSource item in basePowerRelay.inboundPowerSources)
        {
            var techType = UWE.Utils.GetComponentInHierarchy<TechTag>(item.gameObject)?.type ?? FindDeviceName(item);
            var name = techType.AsString();
            
            if(powerSources.ContainsKey(name))
            {
                powerSources[name].Add(item);
            }
            else
            {
                powerSources.Add(name, new List<PowerSource>() { 
                    item
                });
            }
        }

        foreach (var powerSource in powerSources)
        {
            var h = GameObject.Instantiate(basePowerListItemPrefab, powerProductersGrid, false);
            var controller = h.GetComponent<BasePowerInfoListItem>();
            controller.Initialize(powerSource.Key, powerSource.Value);
        }
    }

    private TechType FindDeviceName(PowerSource item)
    {
        //Because this device doesnt Have a TechTag we cant find the TechType so
        //we will attemp to find it defining known classes        

        if (UWE.Utils.GetComponentInHierarchy<BaseBioReactor>(item.gameObject)  || 
            UWE.Utils.GetComponentInHierarchy<BaseBioReactorGeometry>(item.gameObject))
        {
            return TechType.Bioreactor;
        }

        if (UWE.Utils.GetComponentInHierarchy<BaseNuclearReactor>(item.gameObject) ||
            UWE.Utils.GetComponentInHierarchy<BaseNuclearReactorGeometry>(item.gameObject))
        {
            return TechType.NuclearReactor;
        }

        return TechType.None;
    }

    private void RefreshConsumers()
    {


        foreach (Transform item in powerConsumersGrid)
        {
            Destroy(item);
        }

        var dic = new Dictionary<string, List<FCSPowerInterface>>();
        var dicChargers = new Dictionary<string, List<Charger>>();

        var chargers = Player.main.GetCurrentSub()?.GetComponentsInChildren<Charger>();

        foreach (Charger charger in chargers)
        {
            var name = charger.GetComponent<TechTag>()?.type.AsString() ?? "None";

            if (dic.ContainsKey(name))
            {
                dicChargers[name].Add(charger);
            }
            else
            {
                dicChargers.Add(name, new List<Charger>() { charger });
            }
        }

        foreach (var item in _powerConsumers)
        {
            if(dic.ContainsKey(item.DeviceFriendlyName))
            {
                dic[item.DeviceFriendlyName].Add(item);
            }
            else
            {
                dic.Add(item.DeviceFriendlyName, new List<FCSPowerInterface>() { item});
            }
        }


        foreach (var consumer in dic)
        {
            var h = GameObject.Instantiate(basePowerListItemPrefab, powerConsumersGrid, false);
            var controller = h.GetComponent<BasePowerInfoListItem>();
            controller.Initialize(consumer.Key, consumer.Value);
        }

        foreach (var consumer in dicChargers)
        {
            var h = GameObject.Instantiate(basePowerListItemPrefab, powerConsumersGrid, false);
            var controller = h.GetComponent<BasePowerInfoListItem>();
            controller.Initialize(consumer.Key, consumer.Value);
        }
    }

    private static void GetPowerSource<T>(Dictionary<string, List<PowerSource>> powerSources, string key) where T : Component
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

    public override void OnBackButtonClicked()
    {
        throw new System.NotImplementedException();
    }

    public void Enter()
    {
        Enter(null);
    }

    public override void Exit()
    {
        base.Exit();
        ClearLists();
    }

    private void ClearLists()
    {
        foreach (Transform item in powerProductersGrid)
        {
            Destroy(item.gameObject);
        }

        foreach (Transform item in powerConsumersGrid)
        {
            Destroy(item.gameObject);
        }
    }

    public static void Register(FCSPowerInterface powerInterface)
    {
        _powerConsumers.Add(powerInterface);
    }

    public static void UnRegister(FCSPowerInterface powerInterface)
    {
        _powerConsumers.Remove(powerInterface);
    }
}