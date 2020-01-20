using FCSAlterraIndustrialSolutions.Models.Controllers;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSCommon.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using FCS_AIMarineTurbine.Configuration;
using UnityEngine;

namespace FCS_AIMarineTurbine.Buildable
{
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Crafting;

    internal partial class AIMarineMonitorBuildable : Buildable
    {
        private static readonly AIMarineMonitorBuildable Singleton = new AIMarineMonitorBuildable();
        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
        public override string AssetsFolder { get; } = $"{Mod.ModFolderName}/Assets";

        public static void PatchSMLHelper()
        {
            if (!Singleton.GetPrefabs())
            {
                throw new FileNotFoundException($"Failed to retrieve the {Singleton.FriendlyName} prefab from the asset bundle");
            }
            
            Singleton.Patch();
        }

        public AIMarineMonitorBuildable() : base(Mod.MarineMonitorClassID,Mod.MarineMonitorFriendlyName,Mod.MarineMonitorDescriptription)
        {
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = null;

            try
            {
                QuickLogger.Debug("Making GameObject");

                QuickLogger.Debug("Instantiate GameObject");

                prefab = GameObject.Instantiate(_prefab);

                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Far;

                var model = prefab.FindChild("model");

                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    renderer.material.shader = shader;
                }

                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = model.GetComponentsInChildren<MeshRenderer>();
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                QuickLogger.Debug("Adding Constructible");

                // Add constructible
                var constructable = prefab.EnsureComponent<Constructable>();
                constructable.allowedOnWall = true;
                constructable.allowedOnGround = false;
                constructable.allowedInSub = false;
                constructable.allowedInBase = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = false;
                constructable.model = prefab.FindChild("model");
                constructable.rotationEnabled = false;
                constructable.techType = TechType;

                var center = new Vector3(0.06065065f, 0.02289772f, 0.06869301f);
                var size = new Vector3(2.071494f, 1.235519f, 0.1364295f);
                GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                prefab.EnsureComponent<AIMarineMonitorController>();

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = this.ClassID;
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
                    new Ingredient(Mod.MarineMontiorKitClassID.ToTechType(), 1)
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
                    new Ingredient(Mod.MarineMontiorKitClassID.ToTechType(), 1)
                }
            };
            QuickLogger.Debug($"Created Ingredients");
            return customFabRecipe;
        }
#endif
    }
}
