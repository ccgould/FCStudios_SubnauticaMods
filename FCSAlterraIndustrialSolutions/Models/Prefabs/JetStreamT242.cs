using FCSAlterraIndustrialSolutions.Configuration;
using FCSAlterraIndustrialSolutions.Logging;
using FCSAlterraIndustrialSolutions.Models.Controllers;
using FCSCommon.Components;
using FCSCommon.Extensions;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FCSAlterraIndustrialSolutions.Models.Prefabs
{
    public class JetStreamT242 : Buildable
    {
        public JetStreamT242(string classId, string friendlyName, string description) : base(classId, friendlyName, description)
        {
        }

        internal void Register()
        {

            Log.Info("Set GameObject Tag");

            var techTag = LoadItems.JetStreamT242Prefab.AddComponent<TechTag>();

            techTag.type = TechType;

            Log.Info("Set GameObject Bounds");

            // Add constructible bounds
            LoadItems.JetStreamT242Prefab.AddComponent<ConstructableBounds>();


            //Log.Info("Destroy GameObject Rigid body");

            //var rb = LoadItems.JetStreamT242Prefab.GetComponent<Rigidbody>();
            //MonoBehaviour.DestroyImmediate(rb);
        }

        #region Overrides
        public override GameObject GetGameObject()
        {
            GameObject prefab = null;

            try
            {
                Log.Info("Making GameObject");

                Log.Info("Instantiate GameObject");

                prefab = GameObject.Instantiate(LoadItems.JetStreamT242Prefab);

                prefab.name = Information.JetStreamTurbineName;

                //========== Allows the building animation and material colors ==========// 

                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    foreach (Material material in renderer.materials)
                    {
                        renderer.material.shader = shader;

                        if (renderer.material.name.StartsWith("Mat_Color"))
                        {
                            material.DisableKeyword("_EMISSION");
                        }
                    }
                }

                //ApplyMaterials(prefab);

                // Add large world entity ALLOWS YOU TO SAVE ON TERRAIN
                var lwe = prefab.AddComponent<LargeWorldEntity>();
                lwe.cellLevel = LargeWorldEntity.CellLevel.Far;

                SkyApplier skyApplier = prefab.GetOrAddComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                Log.Info("Adding Constructible");

                // Add constructible
                var constructable = prefab.GetOrAddComponent<Constructable>();
                constructable.allowedOnWall = false;
                constructable.allowedOnGround = true;
                constructable.allowedInSub = false;
                constructable.allowedInBase = false;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = true;
                constructable.model = prefab.FindChild("model");
                constructable.rotationEnabled = true;
                constructable.techType = TechType;

                prefab.AddComponent<PowerPlug>();


                prefab.AddComponent<LiveMixin>();

                Log.Info("GetOrAdd TechTag");

                // Allows the object to be saved into the game 
                //by setting the TechTag and the PrefabIdentifier 
                prefab.GetOrAddComponent<TechTag>().type = this.TechType;

                Log.Info("GetOrAdd PrefabIdentifier");

                prefab.GetOrAddComponent<PrefabIdentifier>().ClassId = this.ClassID;


                Log.Info($"Add Component {nameof(JetStreamT242Controller)}");

                prefab.GetOrAddComponent<JetStreamT242Controller>();

                Log.Info($"Add Component {nameof(BeaconController)}");

                prefab.GetOrAddComponent<BeaconController>();

                Log.Info($"Add Component {nameof(DamagedMaterialController)}");

                prefab.GetOrAddComponent<DamagedMaterialController>();

                Log.Info("Made GameObject");


            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

            return prefab;
        }

        private static void ApplyMaterials(GameObject prefab)
        {

            Log.Info("Applying Shaders");
            Shader shader = Shader.Find("MarmosetUBER");

            MeshRenderer mainBaseBodyRenderer = prefab.FindChild("model").FindChild("Decals").GetComponent<MeshRenderer>();


            // == Decals == //
            var decals = mainBaseBodyRenderer.materials[0];


            // == Set Public Materials == //


            // == MAIN BASE BODY == //
            decals.shader = shader;
            decals.EnableKeyword("_ZWRITE_ON");
            decals.EnableKeyword("MARMO_ALPHA");
            decals.EnableKeyword("MARMO_ALPHA_CLIP");

            Log.Info("Shaders Set");

        }

        protected override TechData GetBlueprintRecipe()
        {
            Log.Info($"Creating {Information.JetStreamTurbineFriendly} recipe...");
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
            Log.Info($"Created Ingredients for FCS {Information.JetStreamTurbineFriendly}");
            return customFabRecipe;
        }
        #endregion

        #region Public Overrides
        public override string IconFileName { get; } = "Default.png";
        public override TechGroup GroupForPDA { get; } = TechGroup.ExteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.ExteriorModule;
        public override string AssetsFolder { get; } = $"{Information.ModName}/Assets";
        #endregion

    }
}