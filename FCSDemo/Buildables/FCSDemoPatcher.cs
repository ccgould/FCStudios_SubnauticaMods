using System;
using System.Collections.Generic;
using System.IO;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSDemo.Configuration;
using FCSTechFabricator.Components;
using FCSTechFabricator.Extensions;
using Mono;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSDemo.Buildables
{
    internal partial class FCSDemoBuidable : Buildable
    {
        private static readonly FCSDemoBuidable Singleton = new FCSDemoBuidable();
        public FCSDemoBuidable() : base(Mod.ClassID, Mod.FriendlyName, Mod.Description)
        {
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);
                prefab.name = this.PrefabFileName;

                if (QPatch.Configuration.Config.UseCustomBoundingBox)
                {
                    var center = QPatch.Configuration.Config.BoundingCenter;
                    var size = QPatch.Configuration.Config.BoundingSize;
                    GameObjectHelpers.AddConstructableBounds(prefab, size.ToVector3(), center.ToVector3());
                }
                
                //Add the FCSTechFabricatorTag component
                prefab.AddComponent<FCSTechFabricatorTag>();

                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
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
                    new Ingredient(TechType.Titanium, 1)
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
                    new Ingredient(Mod.FCSDemoKitClassID.ToTechType(), 1)
                }
            };
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(_assetFolder, $"{ClassID}.png"));
        }
#endif
        internal static void Register()
        {
            var model = _prefab.FindChild("model");


            //========== Allows the building animation and material colors ==========// 
            Shader shader = Shader.Find("MarmosetUBER");
            Renderer[] renderers = _prefab.GetComponentsInChildren<Renderer>();
            SkyApplier skyApplier = _prefab.EnsureComponent<SkyApplier>();

            foreach (Renderer renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    material.shader = shader;
                }
            }


            skyApplier.renderers = renderers;
            skyApplier.anchorSky = Skies.Auto;


            // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
            var lwe = _prefab.AddComponent<LargeWorldEntity>();
            lwe.cellLevel = LargeWorldEntity.CellLevel.Far;

            //========== Allows the building animation and material colors ==========// 

            // Add constructible
            var constructable = _prefab.AddComponent<Constructable>();

            constructable.allowedOutside = QPatch.Configuration.Config.AllowedOutside;
            constructable.allowedInBase = QPatch.Configuration.Config.AllowedInBase;
            constructable.allowedOnGround = QPatch.Configuration.Config.AllowedOnGround;
            constructable.allowedOnWall = QPatch.Configuration.Config.AllowedOnWall;
            constructable.rotationEnabled = QPatch.Configuration.Config.RotationEnabled;
            constructable.allowedOnCeiling = QPatch.Configuration.Config.AllowedOnCeiling;
            constructable.allowedInSub = QPatch.Configuration.Config.AllowedInSub;
            constructable.allowedOnConstructables = QPatch.Configuration.Config.AllowedOnConstructables;

            constructable.placeMaxDistance = 7f;
            constructable.placeMinDistance = 5f;
            constructable.placeDefaultDistance = 6f;
            constructable.model = model;
            constructable.techType = Singleton.TechType;

            PrefabIdentifier prefabID = _prefab.AddComponent<PrefabIdentifier>();
            prefabID.ClassId = Singleton.ClassID;

            _prefab.AddComponent<TechTag>().type = Singleton.TechType;
            _prefab.AddComponent<FCSDemoController>();
        }

        public static void PatchHelper()
        {
            if (!Singleton.GetPrefabs())
            {
                throw new FileNotFoundException($"Failed to retrieve the {Singleton.FriendlyName} prefab from the asset bundle");
            }

            Register();
            Singleton.Patch();
        }

        public override TechGroup GroupForPDA => TechGroup.Miscellaneous;
        public override TechCategory CategoryForPDA => TechCategory.Misc;
        private string _assetFolder => Mod.GetAssetFolder();
        public override string AssetsFolder => _assetFolder;

    }
}
