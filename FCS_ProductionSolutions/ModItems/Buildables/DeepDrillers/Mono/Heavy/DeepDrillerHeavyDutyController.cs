using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Structs;
using FCS_AlterraHub.ModItems.Buildables.OreCrusher.Mono;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Models;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Base.Interface;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Services;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UWE;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Heavy;
internal class DeepDrillerHeavyDutyController : MonoBehaviour, IDrillSystem
{
    private DeepDrillerOperatorController _controller;
    [SerializeField] FCSDeepDrillerOilHandler oilHandler;
    [SerializeField] FCSDeepDrillerOreGenerator oreGenerator;
    [SerializeField] GameObject oilMesh;
    [SerializeField] AnimationCurve oilLevel;
    [SerializeField] MotorHandler drillBit;
    [SerializeField] PistonBobbing[] pistons;
    [SerializeField] StorageContainer storageContainer;
    [SerializeField] PrefabIdentifier prefabIden;
    [SerializeField] DDPlatformController platformController;


    public string UnitID => "DD0001";

    public GameObject GameObject => gameObject;

    public DeepDrillerHeavyDutyController()
    {
        
    }


    public void OnEnable()
    {
        MaterialHelpers.ApplyGlassShaderTemplate(gameObject, "_glass", Plugin.ModSettings.ModPackID);
        DeepDrillerManager.main.RegisterDrill(this);
    }

    private void Update()
    {
        if(IsOperational())
        {
            foreach(var p in pistons)
            {
                p.SetState(true);
            }

            drillBit.StartMotor();
        }
        else
        {
            foreach (var p in pistons)
            {
                p.SetState(false);
            }

            drillBit.StopMotor();
        }

        oilMesh.transform.localPosition = new Vector3(oilMesh.transform.localPosition.x, oilLevel.Evaluate(oilHandler.GetOilPercent()), oilMesh.transform.localPosition.z);
    }

    internal void Initialize(DeepDrillerOperatorController controller)
    {
        _controller = controller;
        oilHandler.SetDrillSystem(this);
        oreGenerator.Initialize(this);
        storageContainer.enabled = false;
    }

    internal FCSDeepDrillerContainer GetStorage()
    {
        return oreGenerator.GetStorage();
    }

    public int GetOresPerDayCountInt()
    {
        return 100;
    }

    public IEnumerable<UpgradeFunction> GetUpgrades()
    {
        return null;
    }

    public bool IsBreakSet()
    {
        return false;
    }

    public string GetOilLevel()
    {
        return oilHandler.GetOilPercent().ToString("P1");
    }

    public float GetOilPercent()
    {
        return oilHandler.GetOilPercent();
    }

    public bool IsOperational()
    {
        return oilHandler.HasOil();
    }

    public string GetPrefabID()
    {
        return GameObject.GetComponent<PrefabIdentifier>()?.Id;
    }


    public void AddItemToContainer(TechType techType)
    {
        _controller.AddItemToContainer(techType);
    }

    public void RemoveItemFromContainer(TechType techType)
    {
        
    }

    public DDPlatformController GetPlatformController()
    {
        return platformController;
    }
}
