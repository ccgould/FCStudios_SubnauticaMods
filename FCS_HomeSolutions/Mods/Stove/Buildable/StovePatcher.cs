using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.Stove.Mono;
using FCSCommon.Utilities;
using SMLHelper.Crafting;
using SMLHelper.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif


namespace FCS_HomeSolutions.Mods.Stove.Buildable
{
    internal partial class StoveBuildable : SMLHelper.Assets.Buildable
    {
        internal const string StoveClassID = "FCSStove";
        internal const string StoveFriendly = "Stove";
        internal const string StoveDescription = "Cook a simple meal or an entire banquet";
        internal const string StovePrefabName = "FCS_Stove";
        internal static string StoveKitClassID = $"{StoveClassID}_Kit";
        internal const string StoveTabID = "STV";

        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
        public override string AssetsFolder { get; } = Mod.GetAssetPath();

        public StoveBuildable() : base(StoveClassID, StoveFriendly, StoveDescription)
        {

            OnStartedPatching += () =>
            {
                var stoveKit = new FCSKit(StoveKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                stoveKit.Patch();
            };
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, StoveKitClassID.ToTechType(), 10000, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(FCSAssetBundlesService.PublicAPI.GetPrefabByName(StovePrefabName, FCSAssetBundlesService.PublicAPI.GlobalBundleName));

                prefab.name = this.PrefabFileName;
                
                var center = new Vector3(0f, 0.728528f, 0.04556149f);
                var size = new Vector3(1.099533f, 1.161895f, 1.035657f);

                GameObjectHelpers.AddConstructableBounds(prefab,size,center);

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
                constructable.rotationEnabled = true;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.AddComponent<TechTag>().type = TechType;

                prefab.SetActive(false);
                var storageContainer = prefab.AddComponent<FCSStorage>();
                //storageContainer.Initialize(ClassID);
                storageContainer.Initialize(16,4,4,"Stove",ClassID);

                storageContainer.enabled = true;
                prefab.SetActive(true);
                
                prefab.AddComponent<StoveController>();
                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);

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
                    new Ingredient(StoveKitClassID.ToTechType(),1)
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
