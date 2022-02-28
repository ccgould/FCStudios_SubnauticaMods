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
using FCS_ProductionSolutions.Mods.DeepDriller.LightDuty.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_ProductionSolutions.Mods.DeepDriller.LightDuty.Buildable
{
    internal partial class DeepDrillerLightDutyBuildable : SMLHelper.V2.Assets.Buildable
    {
        private readonly GameObject _prefab;
        public override TechGroup GroupForPDA { get; } = TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.ExteriorModule;

        public override string AssetsFolder => Mod.GetAssetFolder();

        internal const string DeepDrillerLightDutyTabID = "DDL";
        internal const string DeepDrillerLightDutyFriendlyName = "Deep Driller Light Duty";
        internal const string DeepDrillerLightDutyModName = "DeepDrillerLightDuty";
        internal static string DeepDrillerLightDutyKitClassID => $"{DeepDrillerLightDutyClassName}_Kit";
        internal const string DeepDrillerLightDutyClassName = "DeepDrillerLightDuty";
        internal const string DeepDrillerLightDutyPrefabName = "FCS_DeepDrillerLightDuty";
        internal const string DeepDrillerLightDutyDescription = "LLight Duty Automated drill platform suitable for all biomes. Requires lubricant and external power.";



        public DeepDrillerLightDutyBuildable() : base(DeepDrillerLightDutyClassName, DeepDrillerLightDutyFriendlyName, DeepDrillerLightDutyDescription)
        {
            _prefab = ModelPrefab.GetPrefab(DeepDrillerLightDutyPrefabName);

            OnFinishedPatching += () =>
            {
                var deepDrillerLightDutyKit = new FCSKit(DeepDrillerLightDutyKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                deepDrillerLightDutyKit.Patch();
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, deepDrillerLightDutyKit.TechType, 250000, StoreCategory.Production);
            };
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = null;

            try
            {
                prefab = GameObject.Instantiate<GameObject>(_prefab);

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
                constructable.model = prefab.FindChild("model");
                constructable.techType = TechType;
                constructable.rotationEnabled = true;
                constructable.forceUpright = true;
                constructable.placeDefaultDistance = 10f;
                constructable.placeMaxDistance = 10f;

                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Global;

                var center = new Vector3(0f, 1.880612f, 0f);
                var size = new Vector3(1.729009f, 3.01031f, 1.815033f);

                GameObjectHelpers.AddConstructableBounds(prefab, size,center);

                prefab.AddComponent<PrefabIdentifier>().ClassId = this.ClassID;
                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<DeepDrillerLightDutyController>();

                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID/*,1f,2,.2f*/);

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
                    new Ingredient(DeepDrillerLightDutyKitClassID.ToTechType(), 1)
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
