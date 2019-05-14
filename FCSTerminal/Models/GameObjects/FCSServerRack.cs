using FCSTerminal.Configuration;
using FCSTerminal.Logging;
using FCSTerminal.Models.Controllers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System.Collections.Generic;
using System.IO;
using FCSCommon.Extensions;
using UnityEngine;

namespace FCSTerminal.Models.GameObjects
{
    /// <summary>
    /// A class that defines a FCS Server Rack
    /// This is a build able prefab that uses SMLHelper v2 Buildable Class
    /// </summary>
    public class FCSServerRack : Buildable
    {
        private Constructable _constructable;

        #region Public Methods  
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="classId"></param>
        /// <param name="friendlyName"></param>
        /// <param name="description"></param>
        public FCSServerRack(string classId, string friendlyName, string description) : base(classId, friendlyName, description)
        {
            Information.ASSETSFOLDER = Path.Combine("QMods", AssetsFolder);
        }

        /// <summary>
        /// The <see cref="GameObject"/> of this model
        /// </summary>
        /// <returns></returns>
        public override GameObject GetGameObject()
        {
            Log.Info($"// == Making {Information.ModRackFriendly} GameObject == //");

            Log.Info($"// == Instantiate {Information.ModRackFriendly} GameObject == //");

            GameObject prefab = GameObject.Instantiate(LoadItems.FcsServerRackPrefab);

            CreateBuildableShaders(prefab);

            Log.Info("Adding Constructible");

            // Add constructible
            _constructable = GameObjectExtensions.GetOrAddComponent<Constructable>(prefab);
            _constructable.allowedOnWall = false;
            _constructable.allowedOnGround = true;
            _constructable.allowedInSub = false;
            _constructable.allowedInBase = true;
            _constructable.allowedOnCeiling = false;
            _constructable.allowedOutside = true;
            _constructable.model = prefab.FindChild("model");
            _constructable.techType = TechType;

            Log.Info("GetOrAdd TechTag");
            // Allows the object to be saved into the game 
            //by setting the TechTag and the PrefabIdentifier 
            GameObjectExtensions.GetOrAddComponent<TechTag>(prefab).type = this.TechType;

            Log.Info("GetOrAdd PrefabIdentifier");

            GameObjectExtensions.GetOrAddComponent<PrefabIdentifier>(prefab).ClassId = this.ClassID;

            GameObjectExtensions.GetOrAddComponent<BoxCollider>(prefab);

            Log.Info("Add GameObject CustomBatteryController");

            GameObjectExtensions.GetOrAddComponent<FCSServerRackController>(prefab);


            Log.Info("Made GameObject");

            return prefab;

        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Registers the prefab
        /// </summary>
        public void Register()
        {
            Log.Info("Set GameObject Tag");

            var techTag = LoadItems.FcsServerRackPrefab.AddComponent<TechTag>();
            techTag.type = TechType;

            Log.Info("Set GameObject Bounds");

            // Add constructable bounds
            LoadItems.FcsServerRackPrefab.AddComponent<ConstructableBounds>().bounds = new OrientedBounds(new Vector3(-0.1f, -0.1f, 0f), new Quaternion(0, 0, 0, 0), new Vector3(0.9f, 0.5f, 0f));

            Log.Info("Destroy GameObject Rigid body");

            var rb = LoadItems.FcsServerRackPrefab.GetComponent<Rigidbody>();
            MonoBehaviour.DestroyImmediate(rb);
        }

        /// <summary>
        /// Creates the shader needed for the building animation
        /// </summary>
        /// <param name="prefab"></param>
        private void CreateBuildableShaders(GameObject prefab)
        {
            //========== Allows the building animation and material colors ==========// 

            Shader shader = Shader.Find("MarmosetUBER");
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (renderer.name != "Glass")
                {
                    renderer.material.shader = shader;
                }
            }

            SkyApplier skyApplier = GameObjectExtensions.GetOrAddComponent<SkyApplier>(prefab);
            skyApplier.renderers = renderers;
            skyApplier.anchorSky = Skies.Auto;

            //========== Allows the building animation and material colors ==========// 
        }
        #endregion

        #region Overrides

        /// <summary>
        /// An override of the GetBlueprintsRecipe for creating the build recipe
        /// </summary>
        /// <returns></returns>
        protected override TechData GetBlueprintRecipe()
        {
            Log.Info("Creating FCS Terminal Server Rack recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.Titanium, 3),
                    new Ingredient(TechType.Battery, 1),
                    new Ingredient(TechType.WiringKit, 1),
                    new Ingredient(TechType.Quartz, 1),
                }
            };
            Log.Info("Created Ingredients for FCS Terminal Server Rack");
            return customFabRecipe;
        }

        // == Overridden properties == //
        public override TechGroup GroupForPDA { get; } = TechGroup.Miscellaneous;
        public override TechCategory CategoryForPDA { get; } = TechCategory.Misc;
        public override string AssetsFolder { get; } = Path.Combine(Information.ModName, "Assets");
        public override string IconFileName { get; } = "Default.png";
        public override string HandOverText { get; } = "FCS Server Rack";
        // == Overridden properties == //

        #endregion
    }
}
