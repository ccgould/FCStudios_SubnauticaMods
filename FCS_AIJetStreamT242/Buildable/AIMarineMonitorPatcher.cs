using FCSAlterraIndustrialSolutions.Models.Controllers;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FCS_AIMarineTurbine.Buildable
{
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Crafting;

    internal partial class AIMarineMonitorPatcher : Buildable
    {
        private static readonly AIMarineMonitorPatcher Singleton = new AIMarineMonitorPatcher();
        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
        public override string AssetsFolder { get; } = $"FCSAIMarineTurbine/Assets";
        public static void PatchSMLHelper()
        {
            if (!Singleton.GetPrefabs())
            {
                throw new FileNotFoundException($"Failed to retrieve the {Singleton.FriendlyName} prefab from the asset bundle");
            }

            Singleton.Patch();
        }
        //public static JetStreamT242Config JetStreamT242Config { get; set; }

        public AIMarineMonitorPatcher() : base("AIMarineMonitor",
            "AI Marine Monitor",
            "Why go outside and get wet? Get your turbine status and control your turbine from inside!")
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

                SkyApplier skyApplier = prefab.GetOrAddComponent<SkyApplier>();
                skyApplier.renderers = model.GetComponentsInChildren<MeshRenderer>();
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                QuickLogger.Debug("Adding Constructible");

                // Add constructible
                var constructable = prefab.GetOrAddComponent<Constructable>();
                constructable.allowedOnWall = true;
                constructable.allowedOnGround = false;
                constructable.allowedInSub = false;
                constructable.allowedInBase = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = false;
                constructable.model = prefab.FindChild("model");
                constructable.rotationEnabled = false;
                constructable.techType = TechType;

                prefab.GetOrAddComponent<AIMarineMonitorController>();

                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = this.ClassID;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return prefab;
        }

        protected override TechData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.WiringKit, 1),
                    new Ingredient(TechType.ComputerChip, 1),
                    new Ingredient(TechType.Battery, 1),
                    new Ingredient(TechType.Glass, 1)
                }
            };
            QuickLogger.Debug($"Created Ingredients");
            return customFabRecipe;
        }
    }
}
