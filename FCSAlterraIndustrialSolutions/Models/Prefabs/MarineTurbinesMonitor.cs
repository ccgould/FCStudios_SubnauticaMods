using FCSAlterraIndustrialSolutions.Configuration;
using FCSAlterraIndustrialSolutions.Logging;
using FCSAlterraIndustrialSolutions.Models.Controllers;
using FCSCommon.Extensions;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System;
using System.Collections.Generic;
using FCSCommon.Helpers;
using UnityEngine;

namespace FCSAlterraIndustrialSolutions.Models.Prefabs
{
    public class MarineTurbinesMonitor : Buildable
    {
        #region Public Methods

        public MarineTurbinesMonitor(string classId, string friendlyName, string description) : base(classId, friendlyName, description)
        {
        }

        public void Register()
        {

            Log.Info("Set GameObject Tag");

            var techTag = LoadItems.MarineTurbinesMonitorPrefab.AddComponent<TechTag>();

            techTag.type = TechType;

            Log.Info("Set GameObject Bounds");

            // Add constructible bounds
            LoadItems.MarineTurbinesMonitorPrefab.AddComponent<ConstructableBounds>();
        }
        #endregion

        #region Overrides
        public override GameObject GetGameObject()
        {
            GameObject prefab = null;

            try
            {
                Log.Info("Making GameObject");

                Log.Info("Instantiate GameObject");

                prefab = GameObject.Instantiate(LoadItems.MarineTurbinesMonitorPrefab);

                prefab.name = Information.MarineTurbinesMonitorName;

                //========== Allows the building animation and material colors ==========// 

                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    renderer.material.shader = shader;
                }

                //ApplyMaterials(prefab);

                SkyApplier skyApplier = prefab.GetOrAddComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                Log.Info("Adding Constructible");

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


                Log.Info("GetOrAdd TechTag");

                // Allows the object to be saved into the game 
                //by setting the TechTag and the PrefabIdentifier 
                prefab.GetOrAddComponent<TechTag>().type = this.TechType;

                Log.Info("GetOrAdd PrefabIdentifier");

                prefab.GetOrAddComponent<PrefabIdentifier>().ClassId = this.ClassID;

                prefab.GetOrAddComponent<BoxCollider>();

                Log.Info($"Add Component {nameof(MarineMonitorController)}");

                prefab.GetOrAddComponent<MarineMonitorController>();

                //Apply the shaders
                ApplyShaders(prefab);

                Log.Info("Made GameObject");


            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

            return prefab;
        }

        protected override TechData GetBlueprintRecipe()
        {
            Log.Info($"Creating {Information.MarineTurbinesMonitorFriendly} recipe...");
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
            Log.Info($"Created Ingredients for FCS {Information.MarineTurbinesMonitorFriendly}");
            return customFabRecipe;
        }
        #endregion

        #region Private Methods
        private void ApplyShaders(GameObject prefab)
        {
            //Use this to do the Emission
            Shader shader = Shader.Find("MarmosetUBER");
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    material.shader = shader;

                    if (material.name.StartsWith("FCS_TurbinesMonitor"))
                    {
                        //material.EnableKeyword("MARMO_SPECMAP");
                        material.EnableKeyword("_EMISSION");
                        material.EnableKeyword("MARMO_EMISSION");
                        //material.EnableKeyword("_METALLICGLOSSMAP");

                        material.SetVector("_EmissionColor", new Color(0f, 1.437931f, 1.5f, 1.0f) * 1.0f);
                        material.SetTexture("_Illum", MaterialHelpers.FindTexture2D("FCS_TurbinesMonitor_llum_Emission", LoadItems.ASSETBUNDLE));
                        material.SetVector("_Illum_ST", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
                    }

                    if (material.name.StartsWith("FCS_SUBMods_GlobalDecals"))
                    {
                        material.EnableKeyword("_ZWRITE_ON");
                        material.EnableKeyword("MARMO_ALPHA");
                        material.EnableKeyword("MARMO_ALPHA_CLIP");
                    }
                }
            }
        }
        #endregion

        #region Public Overrides
        public override string IconFileName { get; } = "MarineTurbineMonitor.png";
        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;
        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;
        public override string AssetsFolder { get; } = $"{Information.ModName}/Assets";
        #endregion

    }
}
