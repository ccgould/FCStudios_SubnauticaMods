using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Heavy.UGUI;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.uGUI;
using FCSCommon.Helpers;
using FMOD;
using Nautilus.Handlers;
using Nautilus.Utility;
using System.Collections;
using System.IO;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Buildables;
internal class DeepDrillerOperatorBuildable : FCSBuildableModBase
{
    //private EnumBuilder<PingType> _drillPing;
    internal static GameObject DeepDrillerPrefab { get; private set; }

    public static TechType PatchedTechType { get; set; }
    public DeepDrillerOperatorBuildable() : base(PluginInfo.PLUGIN_NAME, "DeepDrillerOperator", FileSystemHelper.ModDirLocation, "DeepDrillerOperator", "Deep Driller Operator")
    {
        OnStartRegister += () =>
        {
            //_drillPing = EnumHandler.AddEntry<PingType>("DeepDriller")
            //.WithIcon(ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"DeepDriller_ping.png")));


            var kit = new FCSKit(_classID, _friendlyName, PluginInfo.PLUGIN_NAME);
            kit.PatchSMLHelper();
            _kitTechType = kit.TechType;

            var bundleName = FCSModsAPI.PublicAPI.GetModBundleName(PluginInfo.PLUGIN_NAME, ClassID);

            //MODE mode = MODE.DEFAULT | MODE.ACCURATETIME | MODE.LOOP_NORMAL | MODE._3D | MODE._3D_LINEARROLLOFF;
            //var drillSound = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName("fcsproductionsolutionsbundle", FileSystemHelper.ModDirLocation).LoadAsset<AudioClip>("soE");
            //var sound = AudioUtils.CreateSound(drillSound, mode);
            //CustomSoundHandler.RegisterCustomSound("DrillSound", sound, "bus:/master/SFX_for_pause/PDA_pause/all/SFX/reverbsend");

            //sound.set3DMinMaxDistance(1, 20);
                
            DeepDrillerPrefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("DeepDrillerHeavyDuty", bundleName, FileSystemHelper.ModDirLocation, true);

            PatchedTechType = TechType;
            FCSPDAController.AddAdditionalPage<uGUI_DDHeavyHomePage>(TechType, FCSAssetBundlesService.PublicAPI.GetPrefabByName("uGUI_DeepDrillerOperator", bundleName, FileSystemHelper.ModDirLocation, false));
            FCSModsAPI.PublicAPI.CreateStoreEntry(TechType, _kitTechType, 1, _settings.ItemCost, StoreCategory.Production);
        };
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
        //var ping = prefab.GetComponent<PingInstance>();
        //ping.SetType(_drillPing);
        return null;
    }
}
