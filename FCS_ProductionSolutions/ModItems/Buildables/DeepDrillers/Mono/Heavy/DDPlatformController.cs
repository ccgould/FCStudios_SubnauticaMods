using FCS_AlterraHub.Core.Helpers;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Base.Interface;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Heavy.Enumerators;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Heavy.Struct;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Heavy.UGUI;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Spawnables;
using FCSCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UWE;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Heavy;
internal class DDPlatformController : MonoBehaviour
{
    public GameObject[] Ports = new GameObject[4];

    internal HolographIconType GetPlatformType()
    {
        if (gameObject.GetComponentInChildren<DeepDrillerOperatorController>())
        {
            return HolographIconType.Operator;
        }

        if (gameObject.GetComponentInChildren<DeepDrillerHeavyDutyController>())
        {
            return HolographIconType.Drill;
        }

        return HolographIconType.None;
    }

    internal ConnectedDrillData ConnectionData { get; set; }

    public void AddNewPlatForm(Transform target, DeepDrillerOperatorController operatorController, GameObject go)
    {
        // Instantiate the prefab with a random rotation 2 meters in front of the player camera:
        //var go = GameObject.Instantiate(prefab);
        go.transform.SetParent(target);
        go.transform.localPosition = new Vector3(0f, 0f, 0f);
        go.transform.SetParent(operatorController.DrillsGroupLocation.transform, true);
        go.transform.localRotation = Quaternion.identity;
        var controller = go.GetComponent<DeepDrillerHeavyDutyController>();
        controller.Initialize(operatorController);
        MaterialHelpers.ApplyGlassShaderTemplate(go, "_glass", Plugin.ModSettings.ModPackID);
    }

    internal IDrillSystem GetMountedDevice()
    {
        return gameObject.GetComponentInChildren<IDrillSystem>();
    }

    internal void LoadFromSave(KeyValuePair<string, ConnectedDrillData> drillSaveData)
    {
        QuickLogger.Debug($"Loading Drill: {drillSaveData.Key}");
    }
}
