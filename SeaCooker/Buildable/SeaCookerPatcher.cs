using AE.SeaCooker.Configuration;
using AE.SeaCooker.Managers;
using AE.SeaCooker.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSCommon.Extensions;
using SMLHelper.V2.Crafting;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AE.SeaCooker.Buildable
{
    using SMLHelper.V2.Assets;
    using System;
    internal partial class SeaCookerBuildable : Buildable
    {
        private static readonly SeaCookerBuildable Singleton = new SeaCookerBuildable();

        //FCSAESeaCooker
        public SeaCookerBuildable() : base(Mod.ClassID, Mod.FriendlyName, Mod.Description)
        {
            OnFinishedPatching += AdditionalPatching;
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = null;

            try
            {

                prefab = GameObject.Instantiate(_prefab);

                var size = new Vector3(1.217227f, 1.23226f, 0.5353913f);
                var center = new Vector3(0f, 0.1884735f, 0.2995481f);

                GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                prefab.name = this.PrefabFileName;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return prefab;
        }

        public override string AssetsFolder { get; } = $"{Mod.ModName}/Assets";
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
                    new Ingredient(Mod.SeaCookerKitClassID.ToTechType(), 1)
                }
            };
            return customFabRecipe;
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
                    new Ingredient(Mod.SeaCookerKitClassID.ToTechType(), 1)
                }
            };
            return customFabRecipe;
        }
#endif

        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;

        internal static void PatchSMLHelper()
        {
            if (!Singleton.GetPrefabs())
            {
                throw new FileNotFoundException($"Failed to retrieve the {Singleton.FriendlyName} prefab from the asset bundle");
            }

            Register();
            
            Singleton.Patch();
        }

        private static void Register()
        {
            if (_prefab != null)
            {
                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = _prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Far;

                var model = _prefab.FindChild("model");

                SkyApplier skyApplier = _prefab.AddComponent<SkyApplier>();
                skyApplier.renderers = model.GetComponentsInChildren<MeshRenderer>();
                skyApplier.anchorSky = Skies.Auto;
                
                //========== Allows the building animation and material colors ==========// 

                QuickLogger.Debug("Adding Constructible");

                // Add constructible
                var constructable = _prefab.AddComponent<Constructable>();
                constructable.allowedOnWall = true;
                constructable.allowedOnGround = false;
                constructable.allowedInSub = true;
                constructable.allowedInBase = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = false;
                constructable.model = model;
                constructable.techType = Singleton.TechType;

                PrefabIdentifier prefabID = _prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = Singleton.ClassID;

                _prefab.AddComponent<AnimationManager>();
                _prefab.AddComponent<TechTag>().type = Singleton.TechType;
                _prefab.AddComponent<PlayerInteraction>();
                _prefab.AddComponent<FMOD_CustomLoopingEmitter>();
                _prefab.AddComponent<SeaCookerController>();
            }
        }
    }
}
