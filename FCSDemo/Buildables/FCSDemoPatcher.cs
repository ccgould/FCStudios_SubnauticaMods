using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCSCommon.Utilities;
using FCSDemo.Configuration;
using FCSDemo.Model;
using FCSDemo.Mono;
using SMLHelper.Assets;
using SMLHelper.Crafting;
using SMLHelper.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.Crafting.TechData;
using Sprite = Atlas.Sprite;

#endif

namespace FCSDemo.Buildables
{
    internal class FCSDemoBuidable : Buildable
    {
        private GameObject _prefab;
        public override string IconFileName { get; }

        public FCSDemoBuidable(ModEntry entry) : base(entry.ClassID, entry.FriendlyName,Mod.Description)
        {
            if (!string.IsNullOrEmpty(entry.IconName))
            {
                IconFileName = entry.IconName;
            }
            
            _prefab = entry.Prefab;
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                if (Main.Configuration.UseCustomBoundingBox.Value)
                {
                    var center = Main.Configuration.BoundingCenter;
                    var size = Main.Configuration.BoundingSize;
                    GameObjectHelpers.AddConstructableBounds(prefab, size.Value, center.Value);
                }
                
                var model = prefab.FindChild("model");

                //========== Allows the building animation and material colors ==========// 
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

                constructable.allowedOutside = Main.Configuration.AllowedOutside.Value;
                constructable.allowedInBase = Main.Configuration.AllowedInBase.Value;
                constructable.allowedOnGround = Main.Configuration.AllowedOnGround.Value;
                constructable.allowedOnWall = Main.Configuration.AllowedOnWall.Value;
                constructable.rotationEnabled = Main.Configuration.RotationEnabled.Value;
                constructable.allowedOnCeiling = Main.Configuration.AllowedOnCeiling.Value;
                constructable.allowedInSub = Main.Configuration.AllowedInSub.Value;
                constructable.allowedOnConstructables = Main.Configuration.AllowedOnConstructables.Value;

                constructable.placeMaxDistance = Main.Configuration.PlaceMaxDistance.Value;//7f;
                constructable.placeMinDistance = Main.Configuration.PlaceMinDistance.Value; //5f;
                constructable.placeDefaultDistance = Main.Configuration.PlaceDefaultDistance.Value; //6f;
                constructable.model = model;
                constructable.techType = TechType;

                QuickLogger.Info("Loaded Configurations",true);

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;
                prefab.AddComponent<TechTag>().type = TechType;
                model.FindChild("mesh").AddComponent<ColorCustomizer>();
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
