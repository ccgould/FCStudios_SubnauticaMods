using FCSCommon.Utilities;
using FCSPowerStorage.Configuration;
using FCSPowerStorage.Helpers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System.IO;
using System.Linq;
using UnityEngine;


namespace FCSPowerStorage.Model
{
    /// <summary>
    /// A Class that defines a CustomBattery for the FCS Power Storage Mod
    /// </summary>
    public class CustomBattery : Buildable
    {
        #region Private Members 

        /// <summary>
        /// The ID of this object
        /// </summary>
        private readonly string _fcsBatteryId;
        private Constructable _constructable;

        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor for the CustomBatteryClass
        /// </summary>
        /// <param name="classId">The ID for the battery</param>
        /// <param name="prefabFileName">The name of the Prefab</param>
        /// <param name="techType">The TechType of the prefab</param>
        public CustomBattery(string classId, string prefabFileName) : base(classId, prefabFileName, LoadItems.ModStrings.Description)
        {
            _fcsBatteryId = classId;

            //Create techType
            QuickLogger.Debug("Creating FCS Battery Storage .....");
            QuickLogger.Debug(_fcsBatteryId);

            Information.ASSETSFOLDER = Path.Combine("QMods", AssetsFolder);
        }
        #endregion

        #region Public Properties

        public GameObject FCSPowerStoragePrefab { get; set; }

        #endregion

        #region Public Methods
        /// <summary>
        /// Registers the power storage
        /// </summary>
        public void RegisterFCSPowerStorage()
        {
            QuickLogger.Debug("Loading Prefab");

            FCSPowerStoragePrefab = AssetHelper.Asset.LoadAsset<GameObject>("Power_Storage");

            QuickLogger.Debug("Set GameObject Tag");

            var techTag = FCSPowerStoragePrefab.AddComponent<TechTag>();

            techTag.type = TechType;

            QuickLogger.Debug("Set GameObject Bounds");

            // Add constructible bounds
            FCSPowerStoragePrefab.AddComponent<ConstructableBounds>().bounds = new OrientedBounds(new Vector3(-0.1f, -0.1f, 0f), new Quaternion(0, 0, 0, 0), new Vector3(0.9f, 0.5f, 0f));


            //Log.Info("Destroy GameObject Rigid body");

            //var rb = FCSPowerStoragePrefab.GetComponent<Rigidbody>();
            //MonoBehaviour.DestroyImmediate(rb);
        }

        #endregion

        #region Overrides

        public override GameObject GetGameObject()
        {
            QuickLogger.Debug("Making GameObject");
            QuickLogger.Debug("Instantiate GameObject");

            GameObject prefab = GameObject.Instantiate(FCSPowerStoragePrefab);



            //========== Allows the building animation and material colors ==========// 

            Shader shader = Shader.Find("MarmosetUBER");
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material.shader = shader;

                if (renderer.material.name == "Power_Storage_Details_Albedo")
                {
                    QuickLogger.Debug("Found Material");
                    renderer.material.color = Color.black;
                }
            }

            SkyApplier skyApplier = prefab.GetOrAddComponent<SkyApplier>();
            skyApplier.renderers = renderers;
            skyApplier.anchorSky = Skies.Auto;

            //========== Allows the building animation and material colors ==========// 


            QuickLogger.Debug("Adding Constructible");

            // Add constructible
            _constructable = prefab.GetOrAddComponent<Constructable>();
            _constructable.allowedOnWall = true;
            _constructable.allowedOnGround = false;
            _constructable.allowedInSub = true;
            _constructable.allowedInBase = true;
            _constructable.allowedOnCeiling = false;
            _constructable.allowedOutside = false;
            _constructable.model = prefab.FindChild("model");
            _constructable.techType = TechType;

            QuickLogger.Debug("GetOrAdd TechTag");
            // Allows the object to be saved into the game 
            //by setting the TechTag and the PrefabIdentifier 
            prefab.GetOrAddComponent<TechTag>().type = this.TechType;

            QuickLogger.Debug("GetOrAdd PrefabIdentifier");

            prefab.GetOrAddComponent<PrefabIdentifier>().ClassId = this.ClassID;

            prefab.GetOrAddComponent<BoxCollider>();

            QuickLogger.Debug("Add GameObject CustomBatteryController");

            prefab.GetOrAddComponent<CustomBatteryController>();

            QuickLogger.Debug("Made GameObject");

            return prefab;
        }

        public override string IconFileName { get; } = "Default.png";

        public override string AssetsFolder { get; } = @"FCSPowerStorage/Assets";

        protected override TechData GetBlueprintRecipe()
        {
            QuickLogger.Debug("Creating FCS Battery Storage recipe...");
            // Create and associate recipe to the new TechType



            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = LoadItems.BatteryConfiguration.ConvertToIngredients().ToList()
            };
            QuickLogger.Debug("Created Ingredients for FCS Power Storage");
            return customFabRecipe;
        }



        public override TechGroup GroupForPDA { get; } = TechGroup.InteriorModules;

        public override TechCategory CategoryForPDA { get; } = TechCategory.InteriorModule;

        public override string HandOverText { get; } = "Click to open";

        #endregion

    }

}