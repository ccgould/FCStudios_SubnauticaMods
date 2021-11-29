using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.Cabinets.Buildable;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif
namespace FCS_HomeSolutions.Mods.QuantumTeleporter.Buildable
{
    internal partial class QuantumPowerBankChargerBuildable : SMLHelper.V2.Assets.Buildable
    {
        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;

        internal const string QuantumPowerBankChargerClassID = "QuantumPowerBankCharger";
        internal const string QuantumPowerBankChargerFriendly = "Quantum Power Bank CHarger";

        internal const string QuantumPowerBankChargerDescription = "Charge your Quantum Power Banks with raw power and those extra ion cubes you have laying around.";

        internal const string QuantumPowerBankChargerPrefabName = "FCS_QuantumPowerBankCharger";
        internal static string QuantumPowerBankChargerKitClassID = $"{QuantumPowerBankChargerClassID}_Kit";
        private readonly GameObject _prefab;
        internal const string QuantumPowerBankChargerTabID = "QPBC";


        public QuantumPowerBankChargerBuildable() : base(QuantumPowerBankChargerClassID, QuantumPowerBankChargerFriendly, QuantumPowerBankChargerDescription)
        {
            _prefab = ModelPrefab.GetPrefab(QuantumPowerBankChargerPrefabName);
            OnStartedPatching += () =>
            {
                var QuantumPowerBankChargerKit = new FCSKit(QuantumPowerBankChargerKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                QuantumPowerBankChargerKit.Patch();
            };
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, QuantumPowerBankChargerKitClassID.ToTechType(), 750000, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                prefab.name = this.PrefabFileName;
                var center = new Vector3(0f, -0.08046275f, 0.3177613f);
                var size = new Vector3(0.7822189f, 1.67095f, 0.5188269f);
                GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                var model = prefab.FindChild("model");

                SkyApplier skyApplier = prefab.AddComponent<SkyApplier>();
                skyApplier.renderers = model.GetComponentsInChildren<MeshRenderer>();
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                QuickLogger.Debug("Adding Constructible");

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();
                constructable.allowedOnWall = true;
                constructable.allowedOnGround = false;
                constructable.allowedInSub = true;
                constructable.allowedInBase = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = false;
                constructable.model = model;
                constructable.rotationEnabled = false;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.AddComponent<TechTag>().type = TechType;
                var controller = prefab.AddComponent<QuantumPowerBankChargerController>();
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", ClassID);

                //var storage = prefab.AddComponent<FCSStorage>();
                //storage.Initialize(1,2,1,"Quantum Power Bank Charger",ClassID);
                //controller._storage = storage;

                controller._storage = UWEHelpers.CreateStorageContainer(prefab,
                    GameObjectHelpers.FindGameObject(prefab, "StorageRoot"), ClassID, "Quantum Power Bank Charger", 2,
                    1);

                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }

        public override string AssetsFolder { get; } = Mod.GetAssetPath();
        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(QuantumPowerBankChargerKitClassID.ToTechType(), 1)
                }
            };
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
