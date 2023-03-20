using BepInEx;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Configuation;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.Spawnables.DebitCard.Spawnable;
using FCS_AlterraHub.ModItems.TestObject.Buildable;
using FCSCommon.Utilities;
using HarmonyLib;
using SMLHelper.Handlers;
using System.Reflection;

namespace FCS_AlterraHub;

/*
 * AlterraHub is a mod that allows you to purchase or craft FCStudios object in the Subnautica world.
 * This mod acts as a hub for the base and also allows other mods to connect to one another
 */
[BepInPlugin(GUID, MODNAME, VERSION)]
public class Main : BaseUnityPlugin
{
    #region [Declarations]

    public const string
        MODNAME = "AlterraHub",
        AUTHOR = "FieldCreatorsStudios",
        GUID = AUTHOR + "_" + MODNAME,
        VERSION = "1.0.0.0";

    /// <summary>
    /// Configuration of the AlterraHub Mod (For use in-game menu)
    /// </summary>
    public static Config Configuration { get; } = OptionsPanelHandler.RegisterModOptions<Config>();

    public static IModSettingsBase ModSettings => new ModSettings();

    #endregion

    private void Awake()
    {
        LanguageService.AdditionalPatching();


        MaterialHelpers.GetIngameObjects();

        StartCoroutine(MaterialHelpers.GetGameBaseMaterial(() =>
        {
            QuickLogger.Info($"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");

            //Register mod pack
            FCSModsAPI.PublicAPI.RegisterModPack(MODNAME, Assembly.GetExecutingAssembly(), ModSettings.AssetBundleName);

            ////Add mod           
            //FCSModsAPI.PublicAPI.RegisterMod(MODNAME,new TestBuildable());


            // FCSModsAPI.PublicAPI.RegisterMod(MODNAME,
            //new DebitCardSpawnable(new Models.Structs.FCSModItemSettings(MODNAME)
            //{
            //    AllowedInBase = true,
            //    AllowedOnBase = true,
            //    AllowedOnCeiling= true,
            //    AllowedOnConstructables = true,
            //    AllowedOnGround = true,
            //    AllowOnRigidBody = true,
            //    AllowedOutside = true,
            //    AllowedOnWall= true,
            //    RotationEnabled= true,
            //    DrawTime = 0.5f,
            //    DropTime = 1f,
            //    HolsterTime= 0.35f,
            //    CellLevel = LargeWorldEntity.CellLevel.Global,
            //    ClassId = "DebitCard",
            //    FriendlyName = "Alterra Debit Card",
            //    Description = LanguageService.DebitCardDescription,
            //    PrefabName = "CreditCard"
            //}));

            QuickLogger.Info($"Finished patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");
        }));

        //Harmony
        var harmony = new Harmony($"com.{MODNAME.ToLower()}.fcstudios");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}
