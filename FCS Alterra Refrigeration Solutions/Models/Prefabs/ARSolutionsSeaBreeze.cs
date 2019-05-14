using FCS_Alterra_Refrigeration_Solutions.Configuration;
using FCS_Alterra_Refrigeration_Solutions.Logging;
using FCS_Alterra_Refrigeration_Solutions.Models.Components;
using FCSCommon.Extensions;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FCS_Alterra_Refrigeration_Solutions.Models.Prefabs
{
    /// <summary>
    /// A class that resembles a Alterra Refrigeration Solutions Prefab
    /// </summary>
    public class ARSolutionsSeaBreeze : Buildable
    {
        #region Private Members 

        private Constructable _constructable;

        #endregion

        #region Constructor
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="classId"></param>
        /// <param name="friendlyName"></param>
        /// <param name="description"></param>
        public ARSolutionsSeaBreeze(string classId, string friendlyName, string description) : base(classId, friendlyName, description)
        {
            Log.Info("Creating Seabreeze .....");
            Information.ASSETSFOLDER = Path.Combine("QMods", AssetsFolder);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Registers the sea breeze
        /// </summary>
        public void RegisterItem()
        {
            Log.Info("Loading Prefab");

            SmallLocker = Resources.Load<GameObject>("Submarine/Build/SmallLocker");

            Log.Info("Set GameObject Tag");

            var techTag = LoadItems.FcsARSolutionPrefab.AddComponent<TechTag>();
            techTag.type = TechType;

            Log.Info("Set GameObject Bounds");

            // Add constructible bounds
            LoadItems.FcsARSolutionPrefab.AddComponent<ConstructableBounds>().bounds = new OrientedBounds(new Vector3(-0.1f, -0.1f, 0f), new Quaternion(0, 0, 0, 0), new Vector3(0.9f, 0.5f, 0f));


            Log.Info("Destroy GameObject Rigid body");

            var rb = LoadItems.FcsARSolutionPrefab.GetComponent<Rigidbody>();
            MonoBehaviour.DestroyImmediate(rb);
        }

        public GameObject SmallLocker { get; set; }

        #endregion

        #region Overrides

        public override GameObject GetGameObject()
        {

            GameObject prefab = null;

            try
            {
                Log.Info("Making GameObject");
                Log.Info("Instantiate GameObject");

                var customprefab = GameObject.Instantiate(LoadItems.FcsARSolutionPrefab);

                Log.Info("Getting Storage Container");

                //prefab = GameObject.Instantiate(this.SmallLocker);


                GameObject originalPrefab = Resources.Load<GameObject>("Submarine/Build/Locker");
                prefab = GameObject.Instantiate(originalPrefab);

                prefab.name = "SeaBreeze";

                var container = prefab.GetComponent<StorageContainer>();
                container.width = 6;
                container.height = 8;
                container.container.Resize(6, 6);
                container.storageLabel = "Sea Breeze";
                container.hoverText = "Open Sea Breeze FCS32";
                var meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>();
                
                foreach (var meshRenderer in meshRenderers)
                {
                    GameObject.DestroyImmediate(meshRenderer);
                }

                //var prefabText = prefab.GetComponentInChildren<Text>();
                //var label = prefab.FindChild("Label");


                var model = prefab.FindChild("model");

                GameObject.DestroyImmediate(model);


                //GameObject.DestroyImmediate(label);

                //var meshColiBoxColliders = container.GetComponentsInChildren<BoxCollider>();
                //foreach (var collider in meshColiBoxColliders)
                //{
                //    GameObject.DestroyImmediate(collider);
                //}

                //var meshRigidbody = container.GetComponentsInChildren<Rigidbody>();
                //foreach (var rigidbody in meshRigidbody)
                //{
                //    GameObject.DestroyImmediate(rigidbody);
                //}


                customprefab.transform.SetParent(prefab.transform, false);


                //========== Allows the building animation and material colors ==========// 

                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = customprefab.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    renderer.material.shader = shader;
                }

                ApplyMaterials(customprefab);
                SkyApplier skyApplier = customprefab.GetOrAddComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;

                //========== Allows the building animation and material colors ==========// 

                Log.Info("Adding Constructible");

                // Add constructible
                _constructable = prefab.GetOrAddComponent<Constructable>();
                _constructable.allowedOnWall = false;
                _constructable.allowedOnGround = true;
                _constructable.allowedInSub = true;
                _constructable.allowedInBase = true;
                _constructable.allowedOnCeiling = false;
                _constructable.allowedOutside = true;
                _constructable.model = customprefab.FindChild("model");
                _constructable.techType = TechType;
                
                Log.Info("GetOrAdd TechTag");

                // Allows the object to be saved into the game 
                //by setting the TechTag and the PrefabIdentifier 
                prefab.GetOrAddComponent<TechTag>().type = this.TechType;

                Log.Info("GetOrAdd PrefabIdentifier");

                prefab.GetOrAddComponent<PrefabIdentifier>().ClassId = this.ClassID;

                prefab.GetOrAddComponent<BoxCollider>();

                Log.Info($"Add Component {nameof(ARSolutionsSeaBreezeController)}");

                var controller = prefab.GetOrAddComponent<ARSolutionsSeaBreezeController>();



                //customprefab.transform.position = Vector3.zero;

                Log.Info("Made GameObject");


            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

            return prefab;
        }

        private static void ApplyMaterials(GameObject seaBreeze)
        {

            Log.Info("Applying Shaders");
            Shader shader = Shader.Find("MarmosetUBER");

            MeshRenderer mainBaseBodyRenderer = seaBreeze.FindChild("model").FindChild("Decals").GetComponent<MeshRenderer>();


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

        public override string IconFileName { get; } = "Default.png";

        public override string AssetsFolder { get; } = $"{Information.ModName}/Assets";

        protected override TechData GetBlueprintRecipe()
        {
            Log.Info("Creating Sea Breeze recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.ComputerChip, 1),
                    new Ingredient(TechType.CopperWire, 2),
                    new Ingredient(TechType.Titanium, 3),
                    new Ingredient(TechType.AdvancedWiringKit, 1),
                    new Ingredient(TechType.Glass, 1),
                    new Ingredient(TechType.Magnetite, 1)
                }
            };
            Log.Info("Created Ingredients for the SeaBreeze");
            return customFabRecipe;
        }

        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorPieces;

        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorPiece;

        public override string HandOverText { get; } = "Click to open";

        Component CopyComponent(Component original, GameObject destination)
        {
            System.Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            // Copied fields can be restricted with BindingFlags
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy;
        }
        #endregion
    }
}