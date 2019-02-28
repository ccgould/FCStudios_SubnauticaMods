using FCSPowerStorage.Configuration;
using FCSPowerStorage.Helpers;
using FCSTerminal.Logging;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace FCSPowerStorage.Model
{
    public class CustomBattery : Buildable
    {
        #region Private Members 

        /// <summary>
        /// The ID of this object
        /// </summary>
        private readonly string _fcsBatteryId;
        private Constructable _constructable;
        public GameObject FCSPowerStoragePrefab;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor for the CustomBatteryClass
        /// </summary>
        /// <param name="classId">The ID for the battery</param>
        /// <param name="prefabFileName">The name of the Prefab</param>
        /// <param name="techType">The TechType of the prefab</param>
        public CustomBattery(string classId, string prefabFileName) : base(classId, prefabFileName, Information.PowerStorageDef)
        {
            _fcsBatteryId = classId;

            //Create techType
            Log.Info("Creating FCS Battery Storage .....");
            Log.Info(_fcsBatteryId);

            Information.ASSETSFOLDER = Path.Combine("QMods", AssetsFolder);
        }


        #endregion

        #region Public Methods

        public void RegisterFCSPowerStorage()
        {
            Log.Info("Loading Prefab");

            FCSPowerStoragePrefab = AssetHelper.Asset.LoadAsset<GameObject>("Power_Storage");


            Log.Info("Update large world entity");

            Log.Info("Set GameObject Tag");

            var techTag = FCSPowerStoragePrefab.AddComponent<TechTag>();
            techTag.type = TechType;

            Log.Info("Set GameObject Bounds");

            // Add constructable bounds
            FCSPowerStoragePrefab.AddComponent<ConstructableBounds>().bounds = new OrientedBounds(new Vector3(-0.1f, -0.1f, 0f), new Quaternion(0, 0, 0, 0), new Vector3(0.9f, 0.5f, 0f));

            //var collider = ServerRackPrefab.AddComponent<BoxCollider>();
            //collider.size = new Vector3(0.02f, 0.06f, 0.02f);

            Log.Info("Destroy GameObject Rigid body");

            var rb = FCSPowerStoragePrefab.GetComponent<Rigidbody>();
            MonoBehaviour.DestroyImmediate(rb);
        }

        #endregion

        #region Overrides

        public override GameObject GetGameObject()
        {
            Log.Info("Making GameObject");

            Log.Info("Instantiate GameObject");

            GameObject prefab = GameObject.Instantiate(FCSPowerStoragePrefab);



            //========== Allows the building animation and material colors ==========// 

            Shader shader = Shader.Find("MarmosetUBER");
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material.shader = shader;

                if (renderer.material.name == "Power_Storage_Details_Albedo")
                {
                    Log.Info("Found Material");
                    renderer.material.color = Color.black;
                }
            }

            SkyApplier skyApplier = prefab.GetOrAddComponent<SkyApplier>();
            skyApplier.renderers = renderers;
            skyApplier.anchorSky = Skies.Auto;

            //========== Allows the building animation and material colors ==========// 


            Log.Info("Adding Constructible");

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

            Log.Info("GetOrAdd TechTag");
            // Allows the object to be saved into the game 
            //by setting the TechTag and the PrefabIdentifier 
            prefab.GetOrAddComponent<TechTag>().type = this.TechType;

            Log.Info("GetOrAdd PrefabIdentifier");

            prefab.GetOrAddComponent<PrefabIdentifier>().ClassId = this.ClassID;

            prefab.GetOrAddComponent<BoxCollider>();

            Log.Info("Add GameObject CustomBatteryController");

            prefab.GetOrAddComponent<CustomBatteryController>();


            Log.Info("Made GameObject");

            return prefab;
        }

        public override string IconFileName { get; } = "Default.png";

        public override string AssetsFolder { get; } = @"FCSPowerStorage/Assets";

        protected override TechData GetBlueprintRecipe()
        {
            Log.Info("Creating FCS Battery Storage recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.HydrochloricAcid, 6),
                    new Ingredient(TechType.Titanium, 7),
                    new Ingredient(TechType.Battery, 1),
                    new Ingredient(TechType.WiringKit, 1),
                    new Ingredient(TechType.Quartz, 1),

                }
            };
            Log.Info("Created Ingredients for FCS Power Storage");
            return customFabRecipe;
        }

        public override TechGroup GroupForPDA { get; } = TechGroup.Miscellaneous;

        public override TechCategory CategoryForPDA { get; } = TechCategory.Misc;

        public override string HandOverText { get; } = "Click to open";

        #endregion

    }

}