using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSDemo.Configuration;
using FCSDemo.Model;
using Mono;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSDemo.Buildables
{
    internal class FCSDemoBuidable : Buildable
    {
        private GameObject _prefab;

        public FCSDemoBuidable(ModEntry entry) : base(entry.ClassID, entry.FriendlyName,Mod.Description)
        {
            _prefab = entry.Prefab;
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                if (QPatch.Configuration.UseCustomBoundingBox)
                {
                    var center = QPatch.Configuration.BoundingCenter;
                    var size = QPatch.Configuration.BoundingSize;
                    GameObjectHelpers.AddConstructableBounds(prefab, size.ToVector3(), center.ToVector3());
                }
                
                var model = prefab.FindChild("model");
                
                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;
                
                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Far;

                //========== Allows the building animation and material colors ==========// 

                QuickLogger.Info("Loading Configurations",true);

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();

                constructable.allowedOutside = QPatch.Configuration.AllowedOutside;
                constructable.allowedInBase = QPatch.Configuration.AllowedInBase;
                constructable.allowedOnGround = QPatch.Configuration.AllowedOnGround;
                constructable.allowedOnWall = QPatch.Configuration.AllowedOnWall;
                constructable.rotationEnabled = QPatch.Configuration.RotationEnabled;
                constructable.allowedOnCeiling = QPatch.Configuration.AllowedOnCeiling;
                constructable.allowedInSub = QPatch.Configuration.AllowedInSub;
                constructable.allowedOnConstructables = QPatch.Configuration.AllowedOnConstructables;

                constructable.placeMaxDistance = QPatch.Configuration.PlaceMaxDistance;//7f;
                constructable.placeMinDistance = QPatch.Configuration.PlaceMinDistance; //5f;
                constructable.placeDefaultDistance = QPatch.Configuration.PlaceDefaultDistance; //6f;
                constructable.model = model;
                constructable.techType = TechType;

                QuickLogger.Info("Loaded Configurations",true);

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;
                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<FCSDemoController>();
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModName);

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
            
        }

        public static void PatchHelper()
        {

            Register();

        }

        public override TechGroup GroupForPDA => TechGroup.Miscellaneous;
        public override TechCategory CategoryForPDA => TechCategory.Misc;
        private string _assetFolder => Mod.GetAssetFolder();
        public override string AssetsFolder => _assetFolder;

    }
}
