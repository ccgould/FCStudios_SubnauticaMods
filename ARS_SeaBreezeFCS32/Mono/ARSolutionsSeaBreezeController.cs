using ARS_SeaBreezeFCS32.Interfaces;
using ARS_SeaBreezeFCS32.Model;
using FCSCommon.Enums;
using FCSCommon.Extensions;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Utility;
using System;
using System.Collections.Generic;
using System.IO;

namespace ARS_SeaBreezeFCS32.Mono
{
    internal partial class ARSolutionsSeaBreezeController : IFridgeContainer, IProtoTreeEventListener, IConstructable
    {
        #region Private Members
        private Constructable _buildable;
        private PrefabIdentifier _prefabId;
        private readonly string _saveDirectory = Path.Combine(SaveUtils.GetCurrentSaveDataDir(), "ARSSeaBreezeFCS32");
        private string SaveFile => Path.Combine(_saveDirectory, _prefabId.Id + ".json");
        private ARSolutionsSeaBreezeContainer _fridgeContainer;
        private ARSolutionsSeaBreezeFilterContainer _filterContainer;
        private bool _deconstructionAllowed = true;
        private ARSolutionsSeaBreezeDisplay _display;
        #endregion

        #region Public Properties

        /// <summary>
        /// Is the FridgeContainer is full
        /// </summary>
        public bool IsFull => _fridgeContainer.IsFull;
        /// <summary>
        /// Number of Items in the Fridge
        /// </summary>
        public int NumberOfItems => _fridgeContainer.NumberOfItems;
        /// <summary>
        /// Items in the fridge
        /// </summary>
        public List<EatableEntities> FridgeItems => _fridgeContainer?.FridgeItems;
        /// <summary>
        /// If the gameobject is costructed
        /// </summary>
        public bool IsConstructed => _buildable != null && _buildable.constructed;
        #endregion

        #region internal Properties
        internal ARSolutionsSeaBreezePowerManager PowerManager { get; private set; }
        internal ARSolutionsSeaBreezeAnimationManager AnimationManager { get; private set; }

        internal Action OnMonoUpdate;
        private FilterState _prevFilterState;

        #endregion

        #region Unity Methods
        public override void Awake()
        {
            base.Awake();

            if (_buildable == null)
            {
                _buildable = GetComponentInParent<Constructable>();
            }

            PowerManager = gameObject.GetOrAddComponent<ARSolutionsSeaBreezePowerManager>();
            if (PowerManager != null)
            {
                PowerManager.Initialize(this);
                PowerManager.OnPowerOutage += OnPowerOutage;
                PowerManager.OnPowerResume += OnPowerResume;
            }
            else
            {
                QuickLogger.Error("Power Manager Component was not found");
            }

            _prefabId = GetComponentInParent<PrefabIdentifier>();
            if (_prefabId == null)
            {
                QuickLogger.Error("Prefab Identifier Component was not found");
            }

            _fridgeContainer = new ARSolutionsSeaBreezeContainer(this);
            _filterContainer = new ARSolutionsSeaBreezeFilterContainer(this);
            _filterContainer.OnPDAClosedAction += OnPdaClosedAction;
            _filterContainer.OnPDAOpenedAction += OnPdaOpenedAction;

            AnimationManager = GetComponentInParent<ARSolutionsSeaBreezeAnimationManager>();
            if (AnimationManager == null)
            {
                QuickLogger.Error("Animation Manager Component was not found");
            }

            InvokeRepeating("UpdateFridgeCooler", 1, 1);

        }

        private void OnPowerResume()
        {
            QuickLogger.Debug("In OnPowerResume", true);
            UpdateFridgeCooler();
        }

        private void OnPowerOutage()
        {
            QuickLogger.Debug("In OnPowerOutage", true);
            UpdateFridgeCooler();
        }

        private void Update()
        {
            OnMonoUpdate?.Invoke();
        }

        #endregion

        #region Public Methods  

        public bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            if (_deconstructionAllowed) return true;
            reason = "Sea Breeze is not empty";
            return false;
        }

        public void OnConstructedChanged(bool constructed)
        {
            if (constructed)
            {
                QuickLogger.Debug("Constructed", true);

                if (_display == null)
                {
                    _display = gameObject.AddComponent<ARSolutionsSeaBreezeDisplay>();
                    _display.Setup(this);
                }

            }
        }

        public void OnAddItemEvent(InventoryItem item)
        {
            if (_buildable == null) return;
            _deconstructionAllowed = false;
            _display.ItemModified<string>(null);
        }

        public void OnRemoveItemEvent(InventoryItem item)
        {
            _deconstructionAllowed = _fridgeContainer.NumberOfItems == 0;
            _display.ItemModified<string>(null);
        }

        public int GetTechTypeAmount(TechType techType)
        {
            return _fridgeContainer.GetTechTypeAmount(techType);
        }

        /// <summary>
        /// Opens the filter container
        /// </summary>
        public void OpenFilterContainer()
        {
            _filterContainer.OpenStorage();
        }

        /// <summary>
        /// Opens the fridge container
        /// </summary>
        public void OpenStorage()
        {
            _fridgeContainer.OpenStorage();
        }

        #endregion

        #region Private Methods
        private void OnPdaOpenedAction()
        {
            AnimationManager.ToggleDriveState();
        }

        private void UpdateFridgeCooler()
        {

            if (_filterContainer == null) return;

            //QuickLogger.Debug($"GetFilterState {_filterContainer.GetFilterState()} || IsPowerAvaliable {PowerManager.IsPowerAvaliable} || GetOpenState {_fridgeContainer.GetOpenState()}", true);

            if (_prevFilterState == _filterContainer.GetFilterState()) return;

            if (!_filterContainer.GetOpenState() && PowerManager.GetIsPowerAvailable() &&
                _filterContainer.GetFilterState() == FilterState.Filtering)
            {
                _fridgeContainer.CoolItems();
                _prevFilterState = _filterContainer.GetFilterState();
                return;
            }

            _fridgeContainer.DecayItems();
            _prevFilterState = _filterContainer.GetFilterState();
        }

        private void OnPdaClosedAction()
        {
            AnimationManager.ToggleDriveState();
        }
        #endregion

        #region IPhotoTreeEventListener
        public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {
            QuickLogger.Debug($"Saving {_prefabId.Id} Data");

            if (!Directory.Exists(_saveDirectory))
                Directory.CreateDirectory(_saveDirectory);

            var saveData = new SaveData
            {
                //HasBreakerTripped = PowerManager.GetHasBreakerTripped(),
                FridgeContainer = _fridgeContainer.GetFridgeItems()

            };

            var output = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            File.WriteAllText(SaveFile, output);

            QuickLogger.Debug($"Saved {_prefabId.Id} Data");
        }

        public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("// ****************************** Load Data *********************************** //");

            if (_prefabId != null)
            {
                QuickLogger.Info($"Loading SeaBreezeFCS32 {_prefabId.Id}");

                if (File.Exists(SaveFile))
                {
                    string savedDataJson = File.ReadAllText(SaveFile).Trim();

                    //LoadData
                    var savedData = JsonConvert.DeserializeObject<SaveData>(savedDataJson);
                    _fridgeContainer.LoadFoodItems(savedData.FridgeContainer);
                }
            }
            else
            {
                QuickLogger.Error("PrefabIdentifier is null");
            }
            QuickLogger.Debug("// ****************************** Loaded Data *********************************** //");
        }
        #endregion

        public void UpdateDisplayTimer(string filterRemainingTime)
        {
            if (_display == null) return;
            _display.UpdateTimer(filterRemainingTime);
        }
    }
}
