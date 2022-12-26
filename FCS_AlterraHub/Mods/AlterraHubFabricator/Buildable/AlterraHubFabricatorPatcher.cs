using System;
using System.Collections;
using SMLHelper.V2.Crafting;
using System.IO;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.AlterraHubFabricator.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Utility;
using UnityEngine;
using System.Collections.Generic;

#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_AlterraHub.Mods.AlterraHubConstructor.Buildable
{
    internal class AlterraHubFabricatorPatcher : SMLHelper.V2.Assets.Buildable
    {
        private readonly GameObject _prefab;
        public const string TabID = "AHC";
        public override TechGroup GroupForPDA => TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.InteriorModule;
        public override string AssetsFolder => Mod.GetAssetPath();

        public static float OreProcessingTime { get; set; } = 90;

        internal const string AlterraHubConstructorClassID = "AlterraHubConstructor";
        internal const string AlterraHubConstructorFriendly = "AlterraHub Constructor";
        internal const string AlterraHubConstructorDescription = "N/A";
        internal const string AlterraHubConstructorPrefabName = "FCS_AlterraHubFabricator";

        public override string DiscoverMessage => $"{FriendlyName} Unlocked!";
        public override List<TechType> CompoundTechsForUnlock => GetUnlocks();

        private List<TechType> GetUnlocks()
        {
            var list = new List<TechType>();

            foreach (var ingredient in this.GetBlueprintRecipe().Ingredients)
            {
                list.Add(ingredient.techType);
            }

            return list;
        }

        public AlterraHubFabricatorPatcher() : base(AlterraHubConstructorClassID, AlterraHubConstructorFriendly, AlterraHubConstructorDescription)
        {
            _prefab = AlterraHub.GetPrefab(AlterraHubConstructorPrefabName);
        }

#if SUBNAUTICA_STABLE
        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                var center = new Vector3(0f, 0.1045737f, 0.3921592f);
                var size = new Vector3(0.8473471f, 1.675344f, 0.3643239f);

                GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                var model = prefab.FindChild("model");

                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;
                //========== Allows the building animation and material colors ==========// 

                var lw = prefab.AddComponent<LargeWorldEntity>();
                lw.cellLevel = LargeWorldEntity.CellLevel.Global;

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();

                constructable.allowedOutside = false;
                constructable.allowedInBase = true;
                constructable.allowedOnGround = false;
                constructable.allowedOnWall = true;
                constructable.rotationEnabled = false;
                constructable.allowedOnCeiling = false;
                constructable.allowedInSub = true;
                constructable.allowedOnConstructables = false;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.AddComponent<TechTag>().type = TechType;
                var controller = prefab.AddComponent<AlterraHubConstructorController>();
                var storage = UWEHelpers.CreateStorageContainer(prefab, null, ClassID, "AlterraHub Constructor", 6, 8);
                
                controller.Storage = storage;
                
                return prefab;

            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return null;
        }
#else
        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
                var prefab = GameObject.Instantiate(_prefab);

                var center = new Vector3(0f, 3.477769f, 0f);
                var size = new Vector3(6.227153f, 6.457883f, 5.26316f);

            GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                var model = prefab.FindChild("model");

                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;
                //========== Allows the building animation and material colors ==========// 

                var lw = prefab.AddComponent<LargeWorldEntity>();
                lw.cellLevel = LargeWorldEntity.CellLevel.Global;

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();

                constructable.allowedOutside = true;
                constructable.allowedInBase = false;
                constructable.allowedOnGround = true;
                constructable.allowedOnWall = false;
                constructable.rotationEnabled = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedInSub = false;
                constructable.allowedOnConstructables = false;
                constructable.model = model;
                constructable.techType = TechType;
                constructable.forceUpright = true;
                constructable.placeDefaultDistance = 5;
                constructable.placeMinDistance = 5;
                constructable.placeMaxDistance = 10;

            PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<AlterraHubConstructorController>();

                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
            gameObject.Set(prefab);
            yield break;
        }
#endif

        protected override RecipeData GetBlueprintRecipe()
        {
            return new RecipeData
            {
                craftAmount = 1,
                Ingredients =
                {
                    new Ingredient(TechType.Glass, 2),
                    new Ingredient(TechType.PlasteelIngot, 2),
                    new Ingredient(TechType.AdvancedWiringKit, 1),
                    new Ingredient(TechType.Silicone, 2),
                    new Ingredient(TechType.Lubricant, 1)
                }
            };
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
