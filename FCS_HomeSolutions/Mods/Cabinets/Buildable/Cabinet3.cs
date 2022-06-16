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
using FCS_HomeSolutions.Mods.Cabinets.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;

#endif

namespace FCS_HomeSolutions.Mods.Cabinets.Buildable
{
    internal class Cabinet3Buildable : SMLHelper.V2.Assets.Buildable
    {
        private readonly GameObject _prefab;
        internal const string Cabinet3ClassID = "CabinetTall";
        internal const string Cabinet3Friendly = "Tall Vertical Cabinet";
        internal const string Cabinet3Description = "A stylish furniture piece for storage and decoration";
        internal const string Cabinet3PrefabName = "FCS_Cabinet_03";
        internal static string Cabinet3KitClassID = $"{Cabinet3ClassID}_Kit";

        public Cabinet3Buildable() : base(Cabinet3ClassID, Cabinet3Friendly, Cabinet3Description)
        {
            _prefab = ModelPrefab.GetPrefabFromGlobal(Cabinet3PrefabName);

            OnStartedPatching += () =>
            {
                var miniFountainKit = new FCSKit(Cabinet3KitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                miniFountainKit.Patch();
            };
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, Cabinet3KitClassID.ToTechType(), 6000, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }

#if SUBNAUTICA_STABLE
        public override GameObject GetGameObject()
        {
            GameObject prefab =null;
            try
            {
                prefab = GameObject.Instantiate(_prefab);

                prefab.name = this.PrefabFileName;

                var center = new Vector3(0f, 1.43428f, 0f);
                var size = new Vector3(0.7484856f, 2.553802f, 0.6422229f);

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
                constructable.allowedOutside = false;
                constructable.allowedOnConstructables = true;
                constructable.rotationEnabled = true;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                UWEHelpers.CreateStorageContainer(prefab, prefab.FindChild("StorageRoot"), ClassID, "Storage", 3, 6);

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<CabinetController>();
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }
            return prefab;
        }
#else
        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            GameObject prefab = null;
            try
            {
                prefab = GameObject.Instantiate(_prefab);

                prefab.name = this.PrefabFileName;

                var center = new Vector3(0f, 1.43428f, 0f);
                var size = new Vector3(0.7484856f, 2.553802f, 0.6422229f);

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
                constructable.allowedOutside = false;
                constructable.allowedOnConstructables = true;
                constructable.rotationEnabled = true;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                UWEHelpers.CreateStorageContainer(prefab, prefab.FindChild("StorageRoot"), ClassID, "Storage", 3, 6);

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<CabinetController>();
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }
            gameObject.Set(prefab);
            yield break;
        }
#endif

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
                    new Ingredient(Cabinet3KitClassID.ToTechType(), 1)
                }
            };
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }

        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
    }
}
