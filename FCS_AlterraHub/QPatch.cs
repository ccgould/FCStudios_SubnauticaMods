using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.AlterraHubDepot.Buildable;
using FCS_AlterraHub.Mods.AlterraHubDepot.Spawnable;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Spawnables;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Mods.OreConsumer.Buildable;
using FCS_AlterraHub.Mods.OreConsumer.Spawnable;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Structs;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using HarmonyLib;
using QModManager.API.ModLoading;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;
using UWE;
using Object = UnityEngine.Object;

namespace FCS_AlterraHub
{
    /*
     * AlterraHub is a mod that allows you to purchase or craft FCStudios object in the Subnautica world.
     * This mod acts as a hub for the base and also allows other mods to connect to one another
     */
    [QModCore]
    public class QPatch
    {
        private static string PdaEntryMessage => $"Please open your AlterraHub PDA to read this data entry ({Configuration.FCSPDAKeyCode}). Make sure you have completed the Alterra Hub Station mission to do so.";
        public static Config Configuration { get;} = OptionsPanelHandler.Main.RegisterModOptions<Config>();
        public static EncyclopediaConfig EncyclopediaConfig { get;} = OptionsPanelHandler.Main.RegisterModOptions<EncyclopediaConfig>();
        public static AssetBundle GlobalBundle { get; set; }

        private static readonly FieldInfo CachedEnumString_valueToString = typeof(CachedEnumString<PingType>).GetField("valueToString", BindingFlags.NonPublic | BindingFlags.Instance);

        //internal static OreConsumerFragment OreConsumerFragment { get; } = new OreConsumerFragment();
        //internal static AlterraHubDepotFragment AlterraHubDepotFragment { get; } = new AlterraHubDepotFragment();


        [QModPatch]
        public static void Patch()
        {
            LanguageHandler.SetLanguageLine($"Ency_{Mod.AlterraHubDepotClassID}", Mod.AlterraHubDepotFriendly);
            LanguageHandler.SetLanguageLine($"EncyDesc_{Mod.AlterraHubDepotClassID}", PdaEntryMessage);
            LanguageHandler.SetLanguageLine($"Ency_{Mod.OreConsumerClassID}", Mod.OreConsumerFriendly);
            LanguageHandler.SetLanguageLine($"EncyDesc_{Mod.OreConsumerClassID}", PdaEntryMessage);

            //QModServices.Main.AddCriticalMessage($"Power Loss Over Distance Result: {MathHelpers.PowerLossOverDistance(379)}");

            Mod.CollectKnownDevices();

            QuickLogger.Info($"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");
            GlobalBundle = FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(Mod.AssetBundleName);

            Mod.LoadAudioFiles();
            
            QuickLogger.DebugLogsEnabled = Configuration.EnableDebugLogs;
            
            //Load Prefabs
            AlterraHub.GetPrefabs();
            
            //Patch all spawnables
            PatchSpawnables();

            //Patch all the buildables
            PatchBuildables();

            //Patch all Additional Store Items
            PatchAdditionalStoreItems();

            //Run Harmony Patches
            var harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "com.alterrahub.fcstudios");

            //Create AlterraHubStation PingType
            var type = Type.GetType("SubnauticaMap.PingMapIcon, SubnauticaMap", false, false);
            if (type != null)
            {
                var pingOriginal = AccessTools.Method(type, "Refresh");
                var pingPrefix = new HarmonyMethod(AccessTools.Method(typeof(PingMapIcon_Patch), "Prefix"));
                harmony.Patch(pingOriginal, pingPrefix);
            }

            Mod.AlterraHubStationPingType = WorldHelpers.CreatePingType("AlterraHubStation", "AlterraHubStation",
                ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), "AlterraHubPing.png")));


            //if (QModServices.Main.ModPresent("EasyCraft"))
            //    EasyCraft_API.Init(harmony);

            //FCSAlterraHubService.PublicAPI.RegisterEncyclopediaEntry(EncyclopediaConfig.EncyclopediaEntries);
            
            //Register debug commands
            ConsoleCommandsHandler.Main.RegisterConsoleCommands(typeof(DebugCommands));

            VoiceNotificationSystem.RegisterVoice(Path.Combine(Mod.GetAssetPath(),"Audio", "ElectricalBoxesNeedFixing.mp3"),string.Empty);
        }

        private static void PatchAdditionalStoreItems()
        {
            //Electronics
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.ReactorRod, TechType.ReactorRod, 5, 445000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.ReactorRod);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.AdvancedWiringKit, TechType.AdvancedWiringKit, 5, 372500, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.AdvancedWiringKit);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.WiringKit, TechType.WiringKit, 5, 115000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.WiringKit);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.PrecursorIonPowerCell, TechType.PrecursorIonPowerCell, 5, 425000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.PrecursorIonPowerCell);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.PowerCell, TechType.PowerCell, 5, 35000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.PowerCell);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.PrecursorIonBattery, TechType.PrecursorIonBattery, 5, 210000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.PrecursorIonBattery);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.Battery, TechType.Battery, 5, 16250, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.Battery);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.CopperWire, TechType.CopperWire, 5, 23750, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.CopperWire);

            //Advanced Materials
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.Polyaniline, TechType.Polyaniline, 5, 88750, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.Polyaniline);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.Aerogel, TechType.Aerogel, 5, 133000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.Aerogel);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.AramidFibers, TechType.AramidFibers, 5, 95000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.AramidFibers);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.Benzene, TechType.Benzene, 5, 75000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.Benzene);
            FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType.HydrochloricAcid, TechType.HydrochloricAcid, 5, 10000, StoreCategory.Misc);
            StoreInventorySystem.AddInvalidReturnItem(TechType.HydrochloricAcid);
        }

        public static FCSHUD HUD { get; set; }
        public static bool IsDockedVehicleStorageAccessInstalled { get; set; }
        //internal static DummyFragment DummyFragment { get; } = new DummyFragment("OreConsumerFragmenta", "Ore Consumer Fragment", "Fragment of an Ore Consumer Machine.");
        private static void PatchSpawnables()
        {
            ////Patch Bio Fuel
            //var bioFuelSpawnable = new BioFuelSpawnable();
            //bioFuelSpawnable.Patch();

            //Patch Debit Card
            var debitCardSpawnable = new DebitCardSpawnable();
            debitCardSpawnable.Patch();
            Mod.DebitCardTechType = debitCardSpawnable.TechType;

            var oreConsumerFragment = new OreConsumerFragment();
            oreConsumerFragment.Patch();
            Mod.OreConsumerFragmentTechType = oreConsumerFragment.TechType;

            var alterraHubDepotFragment = new AlterraHubDepotFragment();
            alterraHubDepotFragment.Patch();
            Mod.AlterraHubDepotFragmentTechType = alterraHubDepotFragment.TechType;

            var keyCardSpawnable = new KeyCardSpawnable();
            keyCardSpawnable.Patch();

            //DummyFragment.Patch();


            //var dummyObject = new DummyObject("DummyObject","Dummy Object","Dummy");
            //dummyObject.Patch();

        }
        
        private static void PatchBuildables()
        {
            //Patch OreConsumer
            var oreConsumer = new OreConsumerPatcher();
            oreConsumer.Patch();

            var alterraHubDepotPatcher = new AlterraHubDepotPatcher();
            alterraHubDepotPatcher.Patch();
        }

        public static class PingMapIcon_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(object __instance)
            {
                FieldInfo field = __instance.GetType().GetField("ping");
                PingInstance ping = field.GetValue(__instance) as PingInstance;
                if (ping.pingType == Mod.AlterraHubStationPingType)
                {
                    FieldInfo field2 = __instance.GetType().GetField("icon");
                    uGUI_Icon icon = field2.GetValue(__instance) as uGUI_Icon;
                    icon.sprite = SpriteManager.Get(SpriteManager.Group.Pings, "AlterraHubStation");
                    icon.color = Color.black;
                    RectTransform rectTransform = icon.rectTransform;
                    rectTransform.sizeDelta = Vector2.one * 28f;
                    rectTransform.localPosition = Vector3.zero;
                    return false;
                }
                return true;
            }
        }
    }

    //internal class DummyFragment : Spawnable
    //{
    //    public override WorldEntityInfo EntityInfo => new WorldEntityInfo() { cellLevel = LargeWorldEntity.CellLevel.Medium, classId = ClassID, localScale = Vector3.one, prefabZUp = false, slotType = EntitySlot.Type.Small, techType = TechType };

    //    public DummyFragment(string classId, string friendlyName, string description) : base(classId, friendlyName, description)
    //    {
    //        OnFinishedPatching += () =>
    //        {
    //            CoordinatedSpawnsHandler.RegisterCoordinatedSpawns(new List<SpawnInfo>
    //            {
    //                //AlterraDepot
    //                new SpawnInfo(TechType, new Vector3(0, 0, 0)),
    //                new SpawnInfo(TechType, new Vector3(0, 1, 0)),
    //                new SpawnInfo(TechType, new Vector3(0, 2, 0)),
    //            });
    //        };
    //    }

    //    public override GameObject GetGameObject()
    //    {

    //        try
    //        {
    //            var prefab = GameObject.Instantiate(AlterraHub.OreConsumerFragPrefab);

    //            PrefabIdentifier prefabIdentifier = prefab.EnsureComponent<PrefabIdentifier>();
    //            prefabIdentifier.ClassId = this.ClassID;
    //            prefab.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
    //            prefab.EnsureComponent<TechTag>().type = this.TechType;

    //            var rb = prefab.GetComponentInChildren<Rigidbody>();

    //            if (rb == null)
    //            {
    //                rb = prefab.EnsureComponent<Rigidbody>();
    //                rb.isKinematic = true;
    //            }

    //            Pickupable pickupable = prefab.EnsureComponent<Pickupable>();
    //            pickupable.isPickupable = false;

    //            ResourceTracker resourceTracker = prefab.EnsureComponent<ResourceTracker>();
    //            resourceTracker.prefabIdentifier = prefabIdentifier;
    //            resourceTracker.techType = this.TechType;
    //            resourceTracker.overrideTechType = TechType.Fragment;
    //            resourceTracker.rb = rb;
    //            resourceTracker.pickupable = pickupable;
    //            return prefab;
    //        }
    //        catch (Exception e)
    //        {
    //            Console.WriteLine(e);
    //            throw;
    //        }
    //    }

    //    public override IEnumerator GetGameObjectAsync(IOut<GameObject> oreconsumerFrag)
    //    {
    //        oreconsumerFrag.Set(GetGameObject());
    //        yield break;
    //    }
    //}

    //internal class DummyObject : Buildable
    //{
    //    protected override TechData GetBlueprintRecipe()
    //    {
    //        QuickLogger.Debug($"Creating recipe...");
    //        // Create and associate recipe to the new TechType
    //        var customFabRecipe = new TechData()
    //        {
    //            craftAmount = 1,
    //            Ingredients = new List<Ingredient>()
    //            {
    //                new Ingredient(TechType.Titanium,1)
    //            }
    //        };
    //        return customFabRecipe;
    //    }

    //    public override TechGroup GroupForPDA => TechGroup.Personal;
    //    public override TechCategory CategoryForPDA => TechCategory.Tools;
    //    public override TechType RequiredForUnlock => QPatch.DummyFragment.TechType;
    //    public override string DiscoverMessage => $"{FriendlyName} Unlocked!";
    //    public override bool AddScannerEntry => true;
    //    public override int FragmentsToScan => 3;
    //    public override float TimeToScanFragment => 5f;
    //    public override bool DestroyFragmentOnScan => true;
        
    //    public DummyObject(string classId, string friendlyName, string description) : base(classId, friendlyName, description)
    //    {
    //    }

    //    public override GameObject GetGameObject()
    //    {

    //        var gameObject = Object.Instantiate(AlterraHub.OreConsumerFragPrefab);

    //        var prefabIdentifier = gameObject.EnsureComponent<PrefabIdentifier>();
    //        prefabIdentifier.ClassId = ClassID;
    //        gameObject.EnsureComponent<TechTag>().type = TechType;

    //        return gameObject;
    //    }
    //}
}
