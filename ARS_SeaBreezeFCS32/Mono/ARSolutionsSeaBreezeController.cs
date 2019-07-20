using ARS_SeaBreezeFCS32.Interfaces;
using ARS_SeaBreezeFCS32.Model;
using FCSCommon.Converters;
using FCSCommon.Extensions;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Utility;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ARS_SeaBreezeFCS32.Mono
{
    public partial class ARSolutionsSeaBreezeController : IFridgeContainer, IProtoTreeEventListener, IConstructable
    {
        #region Private Members
        private Constructable _buildable;
        public PrefabIdentifier PrefabId { get; private set; }

        private readonly string _saveDirectory = Path.Combine(SaveUtils.GetCurrentSaveDataDir(), "ARSSeaBreezeFCS32");
        private string SaveFile => Path.Combine(_saveDirectory, PrefabId.Id + ".json");
        private ARSolutionsSeaBreezeContainer _fridgeContainer;
        private ARSolutionsSeaBreezeFreonContainer _freonContainer;
        private bool _deconstructionAllowed = true;
        private ARSolutionsSeaBreezeDisplay _display;
        private float _maxTime;
        private float currentTime;
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

        private string _currentTimeHMS { get; set; }
        internal bool CoolantIsDone => currentTime <= 0;
        private bool _runTimer;
        private bool _doOnce;

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

            PrefabId = GetComponentInParent<PrefabIdentifier>();
            if (PrefabId == null)
            {
                QuickLogger.Error("Prefab Identifier Component was not found");
            }

            _fridgeContainer = new ARSolutionsSeaBreezeContainer(this);
            _freonContainer = new ARSolutionsSeaBreezeFreonContainer(this);
            _freonContainer.OnPDAClosedAction += OnPdaClosedAction;
            _freonContainer.OnPDAOpenedAction += OnPdaOpenedAction;

            AnimationManager = GetComponentInParent<ARSolutionsSeaBreezeAnimationManager>();
            if (AnimationManager == null)
            {
                QuickLogger.Error("Animation Manager Component was not found");
            }

            InvokeRepeating("UpdateFridgeCooler", 1, 0.5f);
        }
        private void Update()
        {
            if (!Mathf.Approximately(currentTime, 0))
            {
                UpdateTimer();
                UpdateDisplayTimer(_currentTimeHMS);
            }
            else
            {
                if (_freonContainer.CheckIfFreonAvailable()) return;
                //QuickLogger.Debug("Setting timer to zero", true);
                UpdateDisplayTimer(TimeConverters.SecondsToHMS(0));
            }
        }

        private void OnDestroy()
        {
            PowerManager.OnPowerOutage -= OnPowerOutage;
            PowerManager.OnPowerResume -= OnPowerResume;
            _freonContainer.OnPDAClosedAction -= OnPdaClosedAction;
            _freonContainer.OnPDAOpenedAction -= OnPdaOpenedAction;
        }
        #endregion

        #region Internal Methods
        internal void UpdateDisplayTimer(string filterRemainingTime)
        {
            if (_display == null) return;
            _display.UpdateTimer(filterRemainingTime);
        }

        internal void StartTimer()
        {
            _runTimer = true;
        }

        internal void StopTimer()
        {
            _runTimer = false;
        }

        internal void ResetTime()
        {
            currentTime = _maxTime;
        }

        internal void SetMaxTime(float time)
        {
            _maxTime = time;
        }

        internal void InitializeTimer(float freonTime)
        {
            SetMaxTime(freonTime);
            ResetTime();
            StartTimer();
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
            _freonContainer.OpenStorage();
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
            //Disabled because filters where removed from the mod
            //AnimationManager.ToggleDriveState();
        }

        internal void UpdateFridgeCooler()
        {
            if (_freonContainer == null) return;

            //QuickLogger.Debug($"GOS {_freonContainer.GetOpenState()} || GIPA {PowerManager.GetIsPowerAvailable()} || CID {CoolantIsDone}");

            if (!_freonContainer.GetOpenState() && PowerManager.GetIsPowerAvailable() &&
                !PowerManager.GetHasBreakerTripped() && !CoolantIsDone)
            {
                _fridgeContainer.CoolItems();
                StartTimer();
                return;
            }

            _fridgeContainer.DecayItems();
            StopTimer();
        }

        private void OnPdaClosedAction()
        {
            //Disabled because filters where removed from the mod
            //AnimationManager.ToggleDriveState();
        }

        private void OnPowerResume()
        {
            //QuickLogger.Debug("In OnPowerResume", true);
            //UpdateFridgeCooler();
        }

        private void OnPowerOutage()
        {
            //QuickLogger.Debug("In OnPowerOutage", true);
            //UpdateFridgeCooler();
        }

        private void UpdateTimer()
        {
            if (!_runTimer) return;

            if (currentTime > 0f)
            {
                currentTime -= DayNightCycle.main.deltaTime;
                _currentTimeHMS = TimeConverters.SecondsToHMS(currentTime);
            }
            else if (currentTime <= 0f && !_doOnce)
            {
                _doOnce = true;
                _currentTimeHMS = TimeConverters.SecondsToHMS(0);
                currentTime = 0f;
            }
        }
        #endregion

        #region IPhotoTreeEventListener
        public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {
            QuickLogger.Debug($"Saving {PrefabId.Id} Data");

            if (!Directory.Exists(_saveDirectory))
                Directory.CreateDirectory(_saveDirectory);

            var saveData = new SaveData
            {
                HasBreakerTripped = PowerManager.GetHasBreakerTripped(),
                FridgeContainer = _fridgeContainer.GetSaveData(),
                FreonCount = _freonContainer.GetFreonCount(),
                RemainingTime = currentTime
            };

            var output = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            File.WriteAllText(SaveFile, output);

            QuickLogger.Debug($"Saved {PrefabId.Id} Data");
        }

        public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("// ****************************** Load Data *********************************** //");

            if (PrefabId != null)
            {
                QuickLogger.Info($"Loading SeaBreezeFCS32 {PrefabId.Id}");

                SaveData savedData = null;
                if (File.Exists(SaveFile))
                {
                    string savedDataJson = File.ReadAllText(SaveFile).Trim();

                    //LoadData
                    savedData = JsonConvert.DeserializeObject<SaveData>(savedDataJson);
                    _fridgeContainer.LoadFoodItems(savedData.FridgeContainer);
                    currentTime = savedData.RemainingTime;
                    PowerManager.SetHasBreakerTripped(savedData.HasBreakerTripped);
                    _freonContainer.LoadFreon(savedData);
                }
            }
            else
            {
                QuickLogger.Error("PrefabIdentifier is null");
            }
            QuickLogger.Debug("// ****************************** Loaded Data *********************************** //");
        }
        #endregion
    }
}
