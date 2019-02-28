using FCSPowerStorage.Configuration;
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

        // This is the original fabricator prefab.
        private static readonly GameObject OriginalBattery = Resources.Load<GameObject>("WorldEntities/Tools/PowerCell");

        private readonly float _currentPower;

        /// <summary>
        /// The ID of this object
        /// </summary>
        private readonly string _fcsBatteryId;

        private readonly TechType _fcsBatteryStorageTechType;
        private CustomBatterySaveData _saveData = new CustomBatterySaveData();
        private Constructable _constructable;

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
            Log.Info("Creating FCS Battery Storage craft tree...");
            Log.Info(_fcsBatteryId);
        }


        #endregion

        #region Public Methods

        public void RegisterFCSPowerStorage()
        {
            //base.InvokeRepeating("UpdatePowerRelay", 0, 1);
            Log.Info("Getting Save Data FCS Battery Storage recipe...");

            if (_saveData != null && _saveData.PowerCellType != TechType.None)
            {
                //charge = _saveData.PowerCellCharge;
                TechType = _saveData.PowerCellType;
            }




        }

        #endregion

        #region Private Methods

        public string GetSaveDataPath()
        {
            var saveFile = Path.Combine("FCSPowerStorage", ClassID + ".json");
            return saveFile;
        }

        private CustomBatterySaveData CreateSaveData()
        {
            var saveData = new CustomBatterySaveData
            {
                PowerCellType = TechType,
                //PowerCellCharge = charge
            };

            return saveData;
        }

        #endregion

        #region Overrides

        public override GameObject GetGameObject()
        {
            Log.Info("Making GameObject");

            GameObject prefab = GameObject.Instantiate(OriginalBattery);

            prefab.name = ClassID;

            GameObject model = prefab.FindChild("engine_power_cell_01");

            // Add constructible
            _constructable = prefab.AddComponent<Constructable>();
            _constructable.allowedOnWall = true;
            _constructable.allowedOnGround = true;
            _constructable.allowedInSub = false;
            _constructable.allowedInBase = true;
            _constructable.allowedOnCeiling = false;
            _constructable.allowedOutside = true;
            _constructable.model = model;
            _constructable.techType = TechType;

            // Update large world entity
            var lwe = prefab.GetComponent<LargeWorldEntity>();
            lwe.cellLevel = LargeWorldEntity.CellLevel.Near;

            var techTag = prefab.GetComponent<TechTag>();
            techTag.type = TechType;


            // Add constructible bounds
            var bounds = prefab.AddComponent<ConstructableBounds>();

            var pickupable = prefab.GetComponent<Pickupable>();
            MonoBehaviour.DestroyImmediate(pickupable);

            var rb = prefab.GetComponent<Rigidbody>();
            MonoBehaviour.DestroyImmediate(rb);



            Log.Info(prefab.ToString());

            Log.Info("Made GameObject");

            prefab.name = _fcsBatteryId;

            GameObject.Destroy(prefab.GetComponent<Battery>());

            var customBatteryController = prefab.AddComponent<CustomBatteryController>();
            customBatteryController.constructable = _constructable;

            // Update prefab ID
            var prefabId = prefab.GetComponent<PrefabIdentifier>();
            prefabId.ClassId = _fcsBatteryId;
            prefabId.name = this.PrefabFileName;

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
                    new Ingredient(TechType.PowerCell, 6),
                    new Ingredient(TechType.ComputerChip, 1),
                    new Ingredient(TechType.Titanium, 4),
                    new Ingredient(TechType.JeweledDiskPiece, 1)
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




