using FCS_AlterraHub.API;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCSCommon.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_EnergySolutions.ModItems.Buildables.JetStream.Buildables;
internal class UniversalChargerBuildable : FCSBuildableModBase
{
    internal static List<string> ucPowercellSlots = new()
    {
        "UCPowerCellCharger1",
        "UCPowerCellCharger2",
        "UCPowerCellCharger3",
        "UCPowerCellCharger4",
        "UCPowerCellCharger5",
        "UCPowerCellCharger6",
        "UCPowerCellCharger7",
        "UCPowerCellCharger8",
        "UCPowerCellCharger9",
        "UCPowerCellCharger10",
    };

    internal static List<string> ucBatterySlots = new()
    {
        "UCBatteryCharger1",
        "UCBatteryCharger2",
        "UCBatteryCharger3",
        "UCBatteryCharger4",
        "UCBatteryCharger5",
        "UCBatteryCharger6",
        "UCBatteryCharger7",
        "UCBatteryCharger8",
        "UCBatteryCharger9",
        "UCBatteryCharger10",
    };

    public static TechType PatchedTechType { get; private set; }
    public static string PatchedClassID { get; private set; }

    public UniversalChargerBuildable() : base(PluginInfo.PLUGIN_NAME, "UniversalCharger", FileSystemHelper.ModDirLocation, "UniversalCharger", "Universal Charger")
    {
        OnStartRegister += () =>
        {

            foreach (var e in ucPowercellSlots)
            {
                Equipment.slotMapping.Add(e, EquipmentType.PowerCellCharger);
            }

            foreach (var e in ucBatterySlots)
            {
                Equipment.slotMapping.Add(e, EquipmentType.BatteryCharger);
            }


            var kit = new FCSKit(_classID, _friendlyName, PluginInfo.PLUGIN_NAME);
            kit.PatchSMLHelper();
            _kitTechType = kit.TechType;

            var bundleName = FCSModsAPI.PublicAPI.GetModBundleName(PluginInfo.PLUGIN_NAME, ClassID);

            PatchedTechType = TechType;
            PatchedClassID = ClassID;
            //FCSPDAController.AddAdditionalPage<uGUI_SolarCluster>(TechType, FCSAssetBundlesService.PublicAPI.GetPrefabByName("uGUI_SolarCluster", bundleName, FileSystemHelper.ModDirLocation, false));
            FCSModsAPI.PublicAPI.CreateStoreEntry(TechType, _kitTechType, 1, _settings.ItemCost, StoreCategory.Energy);
        };
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
        //var taskResult = CraftData.GetPrefabForTechTypeAsync(TechType.SolarPanel);
        //yield return taskResult;

        //PowerRelay solarPowerRelay = taskResult.GetResult().GetComponent<PowerRelay>();

        //var pFX = prefab.GetComponent<PowerFX>();
        //pFX.vfxPrefab = solarPowerRelay.powerFX.vfxPrefab

        yield return null;
    }
}
