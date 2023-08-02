
using BepInEx;
using BepInEx.Logging;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Configuation;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Buildable;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Items.BaseTransmitter.Buildable;
using FCS_AlterraHub.ModItems.Buildables.DroneDepotPort.Buildable;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.Spawnables.DebitCard.Spawnable;
using FCS_AlterraHub.ModItems.Spawnables.PaintTool.Items;
using FCS_AlterraHub.ModItems.TestObject.Buildable;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using HarmonyLib;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;
using Nautilus.Json;
using Nautilus.Utility;
using System.Reflection;
using Unity.Audio;
using UnityEngine;
using static FCS_AlterraHub.Configuation.SaveData;

namespace FCS_AlterraHub;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
public class Plugin : BaseUnityPlugin
{
    public new static ManualLogSource Logger { get; private set; }

    /// <summary>
    /// Configuration of the AlterraHub Mod (For use in-game menu)
    /// </summary>
    internal static Config Configuration { get; } = OptionsPanelHandler.RegisterModOptions<Config>();

    public static IModSettingsBase ModSettings => new ModSettings();
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    internal static AlterraHubSaveData AlterraHubSaveData { get; private set; }

    private void Awake()
    {
        // set project-scoped logger instance
        Logger = base.Logger;

        LanguageService.AdditionalPatching();


        //Register Ccommands
        RegisterCommands();

        StartCoroutine(MaterialHelpers.GetGameBaseMaterial(() =>
        {
            QuickLogger.Info($"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");

            // Initialize custom prefabs
            InitializePrefabs();

            QuickLogger.Info($"Finished patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");
        }));

        FCSModsAPI.PublicAPI.AddStoreCategory(PluginInfo.PLUGIN_GUID, "HomeSolutionsIcon_W", "Alterra Hub", PDAPages.AlterraHub);

        AlterraHubSaveData = SaveDataHandler.RegisterSaveDataCache<AlterraHubSaveData>();

        AlterraHubSaveData.OnFinishedLoading += OnFinishedLoading;

        // register harmony patches, if there are any
        Harmony.CreateAndPatchAll(Assembly, $"{PluginInfo.PLUGIN_GUID}");

        LanguageHandler.SetLanguageLine("Subtitles_Mentus", "It Works");

        PDAHandler.AddLogEntry("Mentus", "Subtitles_Mentus", AssetBundleHelper.LoadAsset<AudioClip>(FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(FCSAssetBundlesService.PublicAPI.GlobalBundleName), "PDAMetious"));
        //Utils.PlayFMODAsset("")
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    public void OnFinishedLoading(object sender, JsonFileEventArgs e)
    {
        QuickLogger.Debug("Finished Loading", true);

        AlterraHubSaveData data = e.Instance as AlterraHubSaveData;

        if (data != null )
        {
            if(data.colorTemplate is not null)
            {
                ColorManager.colorTemplates = AlterraHubSaveData.colorTemplate;
            }
        }
    }

    private void InitializePrefabs()
    {
        //YeetKnifePrefab.Register();
        //Register mod pack
        FCSModsAPI.PublicAPI.RegisterModPack(PluginInfo.PLUGIN_NAME, Assembly.GetExecutingAssembly(), ModSettings.AssetBundleName,ModSaveManager.Save,ModSaveManager.LoadData);

        ////Add mod           
        //FCSModsAPI.PublicAPI.RegisterMod(MODNAME,new TestBuildable());

        //Add mod and patch.
        FCSModsAPI.PublicAPI.RegisterMod(PluginInfo.PLUGIN_NAME, "DP", new DroneDepotPortBuildable());

        //Add mod and patch.
        FCSModsAPI.PublicAPI.RegisterMod(PluginInfo.PLUGIN_NAME, "BMMR", new BaseManagerModuleRackBuildable());


        //Add mod and patch.
        FCSModsAPI.PublicAPI.RegisterMod(PluginInfo.PLUGIN_NAME, "BM", mod: new BaseManagerBuildable());

        //Add mod and patch.
        FCSModsAPI.PublicAPI.RegisterMod(PluginInfo.PLUGIN_NAME, "DC", new DebitCardSpawnable());

        //Add mod and patch.
        FCSModsAPI.PublicAPI.RegisterMod(PluginInfo.PLUGIN_NAME, "CGE", new ConnectionGridExtensionBuildable());

        //Add mod and patch.
        FCSModsAPI.PublicAPI.RegisterMod(PluginInfo.PLUGIN_NAME, "PC", new PaintCanSpawnable());

        //Add mod and patch.
        FCSModsAPI.PublicAPI.RegisterMod(PluginInfo.PLUGIN_NAME, "PT", new PaintToolSpawnable());

        //Add mod and patch.
        FCSModsAPI.PublicAPI.RegisterMod(PluginInfo.PLUGIN_NAME, "TO", new TestBuildable());

        var sinkDecoration = new CustomPrefab("MaterialsShowcaseDemo", "Materials Showcase Demo", "FCS Demo.");
        var model = FCSAssetBundlesService.InternalAPI.GetLocalPrefab("MaterialsShowcaseDemo");
        MaterialUtils.ApplySNShaders(model,1);
        sinkDecoration.SetGameObject(model);
        sinkDecoration.SetPdaGroupCategory(TechGroup.ExteriorModules, TechCategory.ExteriorModule);
        sinkDecoration.Register();
    }

    private static void RegisterCommands()
    {
        //Register Info commands
        ConsoleCommandsHandler.RegisterConsoleCommands(typeof(Commands));
    }
}