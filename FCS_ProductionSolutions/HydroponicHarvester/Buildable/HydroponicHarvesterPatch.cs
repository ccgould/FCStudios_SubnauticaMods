using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Spawnables;
using FCS_AlterraHub.Structs;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.HydroponicHarvester.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace FCS_ProductionSolutions.HydroponicHarvester.Buildable
{
    internal class HydroponicHarvesterPatch : SMLHelper.V2.Assets.Buildable
    {
        public HydroponicHarvesterPatch() : base(Mod.HydroponicHarvesterModClassName, Mod.HydroponicHarvesterModFriendlyName, Mod.HydroponicHarvesterModDescription)
        {
            OnFinishedPatching += () =>
            {
                var hydroponicHarvesterKit = new FCSKit(Mod.HydroponicHarvesterKitClassID, Mod.HydroponicHarvesterModFriendlyName, Path.Combine(AssetsFolder, $"{ClassID}.png"));
                hydroponicHarvesterKit.Patch();
                FCSAlterraHubService.PublicAPI.CreateStoreEntry(TechType, hydroponicHarvesterKit.TechType, 196875, StoreCategory.Production);
                FCSAlterraHubService.PublicAPI.RegisterEncyclopediaEntry(TechType,new List<FcsEntryData>
                {
                    new FcsEntryData
                    {
                        key = "HydroHarvester",
                        unlocked = true,
                        path = "fcs",
                        timeCapsule = false,
                        nodes = new []{ "fcs"},
                        Description = "The hydroponic Harvester .....",
                        Title = "Hydroponic Harvester",
                        Verbose = true
                    },
                    new FcsEntryData
                    {
                        key = "HydroHarvester1",
                        unlocked = true,
                        path = "fcs",
                        timeCapsule = false,
                        nodes = new []{ "fcs"},
                        Description = "The hydroponic Harvester .....",
                        Title = "Hydroponic Harvester 1",
                        Verbose = false
                    }
                });

            };
        }
        public override TechGroup GroupForPDA => TechGroup.Miscellaneous;
        public override TechCategory CategoryForPDA => TechCategory.Misc;
        public override string AssetsFolder => Mod.GetAssetPath();



        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(ModelPrefab.HydroponicHarvesterPrefab);

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
                constructable.allowedOnConstructables = true;
                constructable.model = model;
                constructable.techType = TechType;

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = ClassID;

                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<HydroponicHarvesterController>();

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