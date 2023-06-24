using FCS_AlterraHub.API;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.ModItems.Spawnables.PaintTool.Mono.UI;
using FCSCommon.Helpers;
using Nautilus.Assets.Gadgets;
using System.Collections;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.Spawnables.PaintTool.Items;
internal class PaintToolSpawnable : FCSSpawnableModBase
{
    public static TechType PatchedTechType { get; private set; }

    public PaintToolSpawnable() : base(PluginInfo.PLUGIN_NAME, "FCS_PaintTool", FileSystemHelper.ModDirLocation, "PaintTool", "Paint Tool")
    {
        OnStartRegister += () =>
        {
            _customPrefab.SetEquipment(EquipmentType.Hand).WithQuickSlotType(QuickSlotType.Selectable);
        };


        OnFinishRegister += () =>
        {
            var bundleName = FCSModsAPI.PublicAPI.GetModBundleName(PluginInfo.PLUGIN_NAME, ClassID);
            PatchedTechType = TechType;
            FCSPDAController.AddAdditionalPage<uGUI_PaintTool>(TechType, FCSAssetBundlesService.PublicAPI.GetPrefabByName("uGUI_PaintTool", bundleName, FileSystemHelper.ModDirLocation, false));
        };
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
        yield break;
    }
}
