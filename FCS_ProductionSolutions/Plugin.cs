﻿using BepInEx;
using BepInEx.Logging;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Buildable;
using FCSCommon.Utilities;
using HarmonyLib;
using Nautilus.Handlers;
using System.Reflection;

namespace FCS_ProductionSolutions;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
[BepInDependency("FCS_AlterraHub")]
public class Plugin : BaseUnityPlugin
{
    public new static ManualLogSource Logger { get; private set; }

    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

    /// <summary>
    /// Configuration of the Production Solutions Mod (For use in-game menu)
    /// </summary>
    internal static Config Configuration { get; } = OptionsPanelHandler.RegisterModOptions<Config>();

    public static IModSettingsBase ModSettings => new ModSettings();

    private void Awake()
    {
        // set project-scoped logger instance
        Logger = base.Logger;
        MaterialHelpers.GetIngameObjects();

        //Register mod pack
        FCSModsAPI.PublicAPI.RegisterModPack(PluginInfo.PLUGIN_NAME, Assembly.GetExecutingAssembly(), ModSettings.AssetBundleName);
        FCSModsAPI.PublicAPI.AddStoreCategory(PluginInfo.PLUGIN_GUID, "ProductionSolutionsIcon_W", "Production Solutions", PDAPages.ProductionSolutions);
        StartCoroutine(MaterialHelpers.GetGameBaseMaterial(() =>
        {
            QuickLogger.Info($"Started patching [{PluginInfo.PLUGIN_NAME}]. Version: {QuickLogger.GetAssemblyVersion(Assembly)}");
            // Initialize custom prefabs
            InitializePrefabs();
        }));

        // register harmony patches, if there are any
        Harmony.CreateAndPatchAll(Assembly, $"{PluginInfo.PLUGIN_GUID}");
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void InitializePrefabs()
    {
        //Add mod and patch.
        FCSModsAPI.PublicAPI.RegisterMod(PluginInfo.PLUGIN_NAME, "IC", new IonCubeGeneratorBuildable());
    }
}