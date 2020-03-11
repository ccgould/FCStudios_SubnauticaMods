using System;
using System.Collections.Generic;
using System.IO;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using GasPodCollector.Configuration;
using GasPodCollector.Mono;
using GasPodCollector.Mono.Managers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
using AnimationManager = FCSCommon.Controllers.AnimationManager;

namespace GasPodCollector.Buildables
{
    internal partial class GaspodCollectorBuildable : Buildable
    {
        private string _assetFolder => Mod.GetAssetFolder();

        public override TechGroup GroupForPDA => TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.ExteriorModule;
        public override string AssetsFolder => _assetFolder;

        public GaspodCollectorBuildable() : base(Mod.ClassID, Mod.FriendlyName, Mod.Description)
        {
            OnFinishedPatching += () =>
            {
                AdditionalPatching();
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);
                prefab.name = this.PrefabFileName;

                var center = new Vector3(0f, 0.2518765f, 0f);
                var size = new Vector3(0.5021304f, 0.5062426f, 0.5044461f);

                GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                //Add the FCSTechFabricatorTag component
                prefab.AddComponent<FCSTechFabricatorTag>();

                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return null;
        }

#if SUBNAUTICA
        protected override TechData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(Mod.GaspodCollectorKitClassID.ToTechType(), 1)
                }
            };
            return customFabRecipe;
        }

        protected override Atlas.Sprite GetItemSprite()
        {
            return new Atlas.Sprite(ImageUtils.LoadTextureFromFile(Path.Combine(_assetFolder, $"{ClassID}.png")));
        }
#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(Mod.GaspodCollectorKitClassID.ToTechType(), 1)
                }
            };
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(_assetFolder, $"{ClassID}.png"));
        }
#endif

        internal void Register()
        {
            if (GetPrefabs())
            {
                var model = _prefab.FindChild("model");

                SkyApplier skyApplier = _prefab.AddComponent<SkyApplier>();
                skyApplier.renderers = model.GetComponentsInChildren<MeshRenderer>();
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                // Add constructible
                var constructable = _prefab.AddComponent<Constructable>();
                constructable.allowedOutside = true;
                constructable.forceUpright = true;
                constructable.placeMaxDistance = 7f;
                constructable.placeMinDistance = 5f;
                constructable.placeDefaultDistance = 6f;
                constructable.model = model;
                constructable.techType = TechTypeID;

                PrefabIdentifier prefabID = _prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = Mod.ClassID;

                Rigidbody component = _prefab.EnsureComponent<Rigidbody>();
                component.mass = 400f;
                component.angularDrag = 1f;
                component.drag = 1f;
                component.isKinematic = false;
                component.freezeRotation = false;
                component.detectCollisions = true;
                component.useGravity = false;

                _prefab.AddComponent<Stabilizer>().uprightAccelerationStiffness = 0.3f;
                _prefab.AddComponent<TechTag>().type = TechTypeID;
                _prefab.AddComponent<FMOD_CustomLoopingEmitter>();
                _prefab.AddComponent<GaspodManager>();
                _prefab.AddComponent<GaspodCollectorStorage>();
                _prefab.AddComponent<AnimationManager>();
                _prefab.AddComponent<GasopodCollectorDisplayManager>();
                _prefab.AddComponent<GaspodCollectorController>();
                _prefab.AddComponent<GasopdCollectorPowerManager>();
            }
        }
    }
}