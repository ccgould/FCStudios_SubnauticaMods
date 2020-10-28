using System;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Spawnables;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.MatterAnalyzer.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace FCS_ProductionSolutions.MatterAnalyzer.Buildable
{
    internal class MatterAnalyzerPatch : SMLHelper.V2.Assets.Buildable
    {
        public MatterAnalyzerPatch() : base(Mod.MatterAnalyzerClassName, Mod.MatterAnalyzerFriendlyName, Mod.MatterAnalyzerDescription)
        {
            OnFinishedPatching += () =>
            {
                var matterAnalyzerKit = new FCSKit(Mod.MatterAnalyzerKitClassID, Mod.MatterAnalyzerFriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                matterAnalyzerKit.Patch();
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, matterAnalyzerKit.TechType, 30000f, StoreCategory.Production);

            };
        }
        public override TechGroup GroupForPDA => TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA => TechCategory.InteriorModule;
        public override string AssetsFolder => Mod.GetAssetPath();
        
        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.MatterAnalyzerPrefab);

                var size = new Vector3(1.353966f, 2.503282f, 1.006555f);
                var center = new Vector3(0.006554961f, 1.394679f, 0.003277525f);

                GameObjectHelpers.AddConstructableBounds(prefab, size, center);

                var model = prefab.FindChild("model");

                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;
                //========== Allows the building animation and material colors ==========// 

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();

                constructable.allowedOutside = true;
                constructable.allowedInBase = true;
                constructable.allowedOnGround = true;
                constructable.allowedOnWall = false;
                constructable.rotationEnabled = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedInSub = true;
                constructable.allowedOnConstructables = false;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<MatterAnalyzerController>();

                //Apply the glass shader here because of autosort lockers for some reason doesnt like it.
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModName);
                return prefab;

            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return null;
        }


#if SUBNAUTICA
        protected override TechData GetBlueprintRecipe()
        {
            return Mod.HydroponicHarvesterIngredients;
        }
#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
            return Mod.HydroponicHarvesterIngredients;
        }
#endif
    }
}