using ExStorageDepot.Configuration;
using ExStorageDepot.Mono;
using FCSCommon.Helpers;

namespace ExStorageDepot.Buildable
{
    using FCSCommon.Extensions;
    using FCSCommon.Utilities;
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Crafting;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    internal partial class ExStorageDepotBuildable : Buildable
    {

        internal static readonly ExStorageDepotBuildable Singleton = new ExStorageDepotBuildable();
        public override TechGroup GroupForPDA { get; } = TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.ExteriorModule;

        public override TechType RequiredForUnlock { get; } = TechType.PowerCell;

        public override string AssetsFolder { get; } = $"{Mod.ModFolderName}/Assets";

        public ExStorageDepotBuildable() : base(Mod.ClassID, Mod.ModFriendly, Mod.ModDesc)
        {
            OnFinishedPatching += AdditionalPatching;
        }

        internal static void PatchHelper()
        {
            if (!Singleton.GetPrefabs())
            {
                throw new FileNotFoundException($"Failed to retrieve the {Singleton.FriendlyName} prefab from the asset bundle");
            }

            Singleton.Patch();
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = null;

            try
            {
                prefab = GameObject.Instantiate(_prefab);
                prefab.AddComponent<BoxCollider>();
                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                // Add constructible
                var constructable = prefab.EnsureComponent<Constructable>();
                constructable.allowedOnWall = false;
                constructable.allowedOnGround = true;
                constructable.allowedInSub = false;
                constructable.allowedInBase = QPatch.Config.AllowInBase;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = true;
                constructable.model = prefab.FindChild("model");
                constructable.techType = TechType;
                constructable.rotationEnabled = true;
                constructable.allowedOnConstructables = Player.main.GetDepth() > 1;
                
                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Global;

                var center = new Vector3(0f, 1.579518f,0f);
                var size = new Vector3(2.669801f, 2.776958f, 2.464836f);

                if (!QPatch.Config.AllowInBase) //Adjust to fit inside
                {
                    GameObjectHelpers.AddConstructableBounds(prefab, size, center);
                }

                prefab.AddComponent<TechTag>().type = this.TechType;
                prefab.AddComponent<PrefabIdentifier>().ClassId = this.ClassID;
                prefab.AddComponent<FMOD_CustomLoopingEmitter>();
                prefab.AddComponent<ExStorageDepotController>();
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return prefab;
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
                    new Ingredient(Mod.ExStorageKitClassID.ToTechType(), 1)
                }
            };

            QuickLogger.Debug($"Created Ingredients");

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
                    new Ingredient(Mod.ExStorageKitClassID.ToTechType(), 1)
                }
            };

            QuickLogger.Debug($"Created Ingredients");

            return customFabRecipe;
        }
#endif
    }
}
