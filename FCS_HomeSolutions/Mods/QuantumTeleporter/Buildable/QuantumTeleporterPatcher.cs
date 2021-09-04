using System;
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
    internal partial class QuantumTeleporterBuildable : SMLHelper.V2.Assets.Buildable
    {
        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;

        internal const string QuantumTeleporterClassID = "QuantumTeleporter";
        internal const string QuantumTeleporterFriendly = "Quantum Teleporter";

        internal const string QuantumTeleporterDescription = "Teleport to other Quantum Teleporter units inside your base or to an entirely different base.";

        internal const string QuantumTeleporterPrefabName = "QuantumTeleporter";
        internal static string QuantumTeleporterKitClassID = $"{QuantumTeleporterClassID}_Kit";
        private readonly GameObject _prefab;
        internal const string QuantumTeleporterTabID = "QT";


        public QuantumTeleporterBuildable() : base(QuantumTeleporterClassID, QuantumTeleporterFriendly, QuantumTeleporterDescription)
        {
            _prefab = ModelPrefab.GetPrefab(QuantumTeleporterPrefabName);
            OnStartedPatching += () =>
            {
                var quantumTeleporterKit = new FCSKit(QuantumTeleporterKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                quantumTeleporterKit.Patch();
            };
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, QuantumTeleporterKitClassID.ToTechType(), 750000, StoreCategory.Home);
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
                constructable.allowedInSub = true;
                constructable.allowedInBase = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = true;
                constructable.model = model;
                constructable.rotationEnabled = true;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<FMOD_CustomLoopingEmitter>();
                prefab.AddComponent<QuantumTeleporterController>();

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
                    new Ingredient(QuantumTeleporterKitClassID.ToTechType(), 1)
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
