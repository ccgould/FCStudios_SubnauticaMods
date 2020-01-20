using FCS_AIMarineTurbine.Mono;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FCS_AIMarineTurbine.Buildable
{
    internal partial class AIWindSurferBuildable : Craftable
    {

        private static readonly AIWindSurferBuildable Singleton = new AIWindSurferBuildable();

        public AIWindSurferBuildable() : base("AIWindSurfer", "AI Wind Surfer", "Wind Turbine from the guyes you know")
        {
        }

        public static void PatchSMLHelper()
        {
            if (!Singleton.GetPrefabs())
            {
                throw new FileNotFoundException($"Failed to retrieve the {Singleton.FriendlyName} prefab from the asset bundle");
            }
            Singleton.Patch();
            CraftDataHandler.SetCraftingTime(Singleton.TechType, 5);
            CraftTreeHandler.AddTabNode(CraftTree.Type.Constructor, "FCStudios", "FCStudios Mods", ImageUtils.LoadSpriteFromFile($"./QMods/AIMarineTurbine/Assets/Default.png"));

            //CraftTreePatcher.customTabs.Add(new CustomCraftTab("FCStudios", "FCStudios Mods", CraftScheme.Constructor, new Atlas.Sprite(ImageUtils.LoadTextureFromFile($"./QMods/AIMarineTurbine/Assets/Default.png"))));

            //CraftTreePatcher.customNodes.Add(new CustomCraftNode(Singleton.TechType, CraftScheme.Constructor, "FCStudios/WindSurferTurbine"));
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = null;

            try
            {
                QuickLogger.Debug("Making GameObject");

                QuickLogger.Debug("Instantiate GameObject");

                prefab = GameObject.Instantiate(_Prefab);

                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = prefab.GetComponentsInChildren<Renderer>();
                skyApplier.anchorSky = Skies.Auto;

                var shader = Shader.Find("MarmosetUBER");

                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    foreach (Material material in renderer.materials)
                    {
                        material.shader = shader;
                    }
                }

                prefab.EnsureComponent<VFXSurface>();
                prefab.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;

                Rigidbody rb = prefab.EnsureComponent<Rigidbody>();
                rb.angularDrag = 1f;
                rb.mass = 12000f;
                rb.useGravity = false;
                rb.centerOfMass = new Vector3(-0.1f, 0.8f, -1.7f);
                //rb.isKinematic = true;

                // Add fabricating animation
                var fabricatingA = prefab.AddComponent<VFXFabricating>();
                //fabricatingA.localMinY = -0.1f;
                //fabricatingA.localMaxY = 0.6f;
                //fabricatingA.posOffset = new Vector3(0f, 0f, 0f);
                //fabricatingA.eulerOffset = new Vector3(0f, 0f, 0f);
                //fabricatingA.scaleFactor = 1.0f;

                WorldForces forces = prefab.EnsureComponent<WorldForces>();
                forces.aboveWaterDrag = 0f;
                forces.aboveWaterGravity = 3f;
                forces.handleDrag = true;
                forces.handleGravity = true;
                forces.underwaterDrag = 0f;
                forces.underwaterGravity = -10f;


                PrefabIdentifier prefabID = prefab.AddComponent<PrefabIdentifier>();
                prefabID.ClassId = this.ClassID;

                prefab.AddComponent<AIWindSuferController>();
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
            }

            return prefab;
        }

        public override string AssetsFolder { get; } = $"FCSAIMarineTurbine/Assets";
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
                    new Ingredient(TechType.TitaniumIngot, 1),
                    new Ingredient(TechType.CopperWire, 1),
                    new Ingredient(TechType.ComputerChip, 1),
                    new Ingredient(TechType.AdvancedWiringKit, 1),
                    new Ingredient(TechType.Glass, 1),
                    new Ingredient(TechType.FiberMesh, 2),
                    new Ingredient(TechType.Lubricant, 2),
                }
            };
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
                    new Ingredient(TechType.TitaniumIngot, 1),
                    new Ingredient(TechType.CopperWire, 1),
                    new Ingredient(TechType.ComputerChip, 1),
                    new Ingredient(TechType.AdvancedWiringKit, 1),
                    new Ingredient(TechType.Glass, 1),
                    new Ingredient(TechType.FiberMesh, 2),
                    new Ingredient(TechType.Lubricant, 2),
                }
            };
#endif

            QuickLogger.Debug($"Created Ingredients");
            return customFabRecipe;
        }

        public override TechGroup GroupForPDA { get; } = TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.ExteriorModule;
        public override CraftTree.Type FabricatorType { get; } = CraftTree.Type.Constructor;
        public override string[] StepsToFabricatorTab { get; } = new[] { "FCStudios" };
    }
}
