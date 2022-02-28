using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Buildable
{
    internal partial class FCSDeepDrillerBuildable : SMLHelper.V2.Assets.Buildable
    {
        public override TechGroup GroupForPDA { get; } = TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.ExteriorModule;

        public override string AssetsFolder => Mod.GetAssetFolder();

        internal const string DeepDrillerMk3TabID = "DD";
        internal const string DeepDrillerMk3FriendlyName = "Deep Driller Heavy Duty";
        internal const string DeepDrillerMk3ModName = "DeepDrillerMK3";
        internal static string DeepDrillerMk3KitClassID => $"{DeepDrillerMk3ClassName}_Kit";
        internal const string DeepDrillerMk3ClassName = "DeepDrillerMk3";
        internal const string DeepDrillerMk3PrefabName = "DeepDrillerMK3";
        internal const string DeepDrillerMk3Description = "Heavy Duty Automated drill platform suitable for all biomes with integrated solar and thermal generators. Requires lubricant.";

        public FCSDeepDrillerBuildable() : base(DeepDrillerMk3ClassName, DeepDrillerMk3FriendlyName, DeepDrillerMk3Description)
        {
            OnFinishedPatching += () =>
            {
                var deepDrillerMk2Kit = new FCSKit(DeepDrillerMk3KitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                deepDrillerMk2Kit.Patch();
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, deepDrillerMk2Kit.TechType, 700000, StoreCategory.Production);
                AdditionalPatching();
            };
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = null;

            try
            {
                prefab = GameObject.Instantiate<GameObject>(ModelPrefab.DeepDrillerPrefab);

                //========== Allows the building animation and material colors ==========// 
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = prefab.GetComponentsInChildren<Renderer>();
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                // Add constructible
                var constructable = prefab.EnsureComponent<Constructable>();
                constructable.allowedOnWall = false;
                constructable.allowedOnGround = true;
                constructable.allowedInSub = false;
                constructable.allowedInBase = false;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = true;
                constructable.model = prefab.FindChild("DrillModel");
                constructable.techType = TechType;
                constructable.rotationEnabled = true;
                constructable.forceUpright = true;
                constructable.placeDefaultDistance = 10f;
                constructable.placeMaxDistance = 10f;

                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Global;

                //var center = new Vector3(-2.384186e-07f, 2.500637f, -0.007555246f);
                //var size = new Vector3(5.605133f, 8.229565f, 5.689059f);

                var center = new Vector3(-0.0168829f, 3.009828f, 0.03002357f);
                var size = new Vector3(4.291656f, 5.013103f, 4.075207f);

                GameObjectHelpers.AddConstructableBounds(prefab, size,center);

                prefab.AddComponent<PrefabIdentifier>().ClassId = this.ClassID;
                prefab.AddComponent<TechTag>().type = TechTypeID;
                prefab.AddComponent<FCSDeepDrillerController>();

                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);

            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return prefab;
        }

        protected override RecipeData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(AsyncExtensions.ToTechType(DeepDrillerMk3KitClassID), 1)
                }
            };
            QuickLogger.Debug($"Created Ingredients");
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, $"{ClassID}.png"));
        }
    }
}
