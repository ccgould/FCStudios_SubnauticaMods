using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
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
    internal partial class QuantumTeleporterVehiclePadBuildable : SMLHelper.V2.Assets.Buildable
    {
        public override TechGroup GroupForPDA { get; } = TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.ExteriorModule;

        internal const string QuantumTeleporterVehiclePadClassID = "QuantumTeleporterVehiclePad";
        internal const string QuantumTeleporterVehiclePadFriendly = "Quantum Teleporter Vehicle Pad";

        internal const string QuantumTeleporterVehiclePadDescription = "Creates a Quantum Teleportation destination when teleporting a vehicle via the FCStudios PDA.";

        internal const string QuantumTeleporterVehiclePadPrefabName = "FCS_QuantumVehiclePad";
        internal static string QuantumTeleporterVehiclePadKitClassID = $"{QuantumTeleporterVehiclePadClassID}_Kit";
        private readonly GameObject _prefab;
        internal const string QuantumTeleporterVehiclePadTabID = "QVP";


        public QuantumTeleporterVehiclePadBuildable() : base(QuantumTeleporterVehiclePadClassID, QuantumTeleporterVehiclePadFriendly, QuantumTeleporterVehiclePadDescription)
        {
            _prefab = ModelPrefab.GetPrefab(QuantumTeleporterVehiclePadPrefabName);
            OnStartedPatching += () =>
            {
                var QuantumTeleporterVehiclePadKit = new FCSKit(QuantumTeleporterVehiclePadKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                QuantumTeleporterVehiclePadKit.Patch();
            };
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, QuantumTeleporterVehiclePadKitClassID.ToTechType(), 1000000, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                prefab.name = this.PrefabFileName;
                var center = new Vector3(0f, 1.433978f, 0f);
                var size = new Vector3(2.274896f, 2.727271f, 2.069269f);
                GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                var model = prefab.FindChild("model");

                SkyApplier skyApplier = prefab.AddComponent<SkyApplier>();
                skyApplier.renderers = model.GetComponentsInChildren<MeshRenderer>();
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                QuickLogger.Debug("Adding Constructible");

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();
                constructable.allowedOnWall = false;
                constructable.allowedOnGround = true;
                constructable.allowedInSub = false;
                constructable.allowedInBase = false;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = true;
                constructable.model = model;
                constructable.rotationEnabled = true;
                constructable.placeDefaultDistance = 5;
                constructable.placeMinDistance = 5;
                constructable.placeMaxDistance = 10;
                constructable.techType = TechType;

                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Global;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<FMOD_CustomLoopingEmitter>();
                prefab.AddComponent<QuantumTeleporterVehiclePadController>();

                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }

        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            gameObject.Set(GetGameObject());
            yield break;
        }

        public override string AssetsFolder { get; } = Mod.GetAssetPath();
        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(QuantumTeleporterVehiclePadKitClassID.ToTechType(), 1)
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