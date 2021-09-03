using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Global.Spawnables;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.ModManagers;
using FCS_HomeSolutions.Mods.MiniFountainFilter.Mono;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using RecipeData = SMLHelper.V2.Crafting.TechData;
using Sprite = Atlas.Sprite;
#endif

namespace FCS_HomeSolutions.Mods.MiniFountainFilter.Buildables
{
    internal partial class MiniFountainFilterBuildable : Buildable
    {
        internal const string MiniFountainFilterClassID = "MiniFountainFilter";
        internal const string MiniFountainFilterFriendly = "Mini Filter & Fountain";
        internal const string MiniFountainFilterDescription = "A smaller water filtration system for your base or cyclops.";
        internal const string MiniFountainFilterPrefabName = "FCS_MiniFountainFilter";
        internal static string MiniFountainFilterKitClassID = $"{MiniFountainFilterClassID}_Kit";
        private readonly GameObject _prefab;
        internal const string MiniFountainFilterTabID = "MFF";
        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
        public override string AssetsFolder { get; } = Mod.GetAssetPath();

        public MiniFountainFilterBuildable() : base(MiniFountainFilterClassID, MiniFountainFilterFriendly, MiniFountainFilterDescription)
        {
            _prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName(MiniFountainFilterPrefabName, FCSAssetBundlesService.PublicAPI.GlobalBundleName);

            OnStartedPatching += () =>
            {
                var miniFountainKit = new FCSKit(MiniFountainFilterKitClassID, FriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                miniFountainKit.Patch();
            };
            OnFinishedPatching += () =>
            {
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, MiniFountainFilterKitClassID.ToTechType(), 45000, StoreCategory.Home);
                FCSAlterraHubService.PublicAPI.RegisterPatchedMod(ClassID);
                AdditionalPatching();
            };
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                prefab.name = this.PrefabFileName;

                var center = new Vector3(-0.0748806f, 0.2327834f, 0.2067378f);
                var size = new Vector3(0.7395439f, 1.192087f, 0.410716f);

                GameObjectHelpers.AddConstructableBounds(prefab,size,center);

                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Far;

                var model = prefab.FindChild("model");

                SkyApplier skyApplier = prefab.AddComponent<SkyApplier>();
                skyApplier.renderers = model.GetComponentsInChildren<MeshRenderer>();
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                QuickLogger.Debug("Adding Constructible");

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();
                constructable.allowedOnWall = true;
                constructable.allowedOnGround = false;
                constructable.allowedInSub = true;
                constructable.allowedInBase = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = false;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.AddComponent<AnimationManager>();
                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<FMOD_CustomLoopingEmitter>();
                prefab.AddComponent<MiniFountainFilterController>();

                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
                MaterialHelpers.ApplyParticlesUber(prefab, "bubble_01_sheet_premul_underwaterCamera", 1f, 0f, 1f, 10f, 3f, false, Vector4.zero, new Vector4(1f, 1f, 1f, .5f));
                MaterialHelpers.ApplyParticlesUber(prefab, "bubble_02_sheet_add_underwaterLit", 1f, 0f, 1f, 10f, 3f, false, Vector4.zero, new Vector4(1f, 1f, 1f, 10f));
                MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, prefab, Color.cyan);
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
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(MiniFountainFilterKitClassID.ToTechType(), 1)
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
