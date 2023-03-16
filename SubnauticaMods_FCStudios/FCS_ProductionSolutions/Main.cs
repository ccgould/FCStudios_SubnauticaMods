

using BepInEx;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Configuation;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Structs;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Buildable;
using FCSCommon.Utilities;
using SMLHelper.Handlers;
using System.Reflection;
using Config = FCS_ProductionSolutions.Configuration.Config;

namespace FCS_ProductionSolutions;

[BepInPlugin(GUID, MODNAME, VERSION)]
internal class Main : BaseUnityPlugin
{
    #region [Declarations]

    public const string
        MODNAME = "ProductionSolutions",
        AUTHOR = "FieldCreatorsStudios",
        GUID = AUTHOR + "_" + MODNAME,
        VERSION = "1.0.0.0";

    /// <summary>
    /// Configuration of the Production Solutions Mod (For use in-game menu)
    /// </summary>
    internal static Config Configuration { get; } = OptionsPanelHandler.RegisterModOptions<Config>();

    public static IModSettingsBase ModSettings => new ModSettings();

    #endregion

    private void Awake()
    {
        MaterialHelpers.GetIngameObjects();

        StartCoroutine(MaterialHelpers.GetGameBaseMaterial(() =>
        {
            QuickLogger.Info($"Started patching [{MODNAME}]. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");

            //Register mod pack
            FCSModsAPI.PublicAPI.RegisterModPack(MODNAME, Assembly.GetExecutingAssembly(), ModSettings.AssetBundleName);


            //Add mod and patch.
            FCSModsAPI.PublicAPI.RegisterMod(MODNAME, new IonCubeGeneratorBuildable());
        }));
    }
}

