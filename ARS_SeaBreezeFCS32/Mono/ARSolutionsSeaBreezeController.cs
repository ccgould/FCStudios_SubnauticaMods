using ARS_SeaBreezeFCS32.Buildables;
using ARS_SeaBreezeFCS32.Configuration;
using ARS_SeaBreezeFCS32.Interfaces;
using ARS_SeaBreezeFCS32.Model;
using FCSCommon.Converters;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using FCSCommon.Interfaces;
using FCSTechFabricator.Components;
using FCSTechFabricator.Interfaces;
using UnityEngine;

namespace ARS_SeaBreezeFCS32.Mono
{
    public partial class ARSolutionsSeaBreezeController : MonoBehaviour, IFridgeContainer, IProtoEventListener, IConstructable,IRenameable
    {
        #region Private Members
        private Constructable _buildable;
        public PrefabIdentifier PrefabId { get; private set; }

        private readonly string _saveDirectory = Path.Combine(SaveUtils.GetCurrentSaveDataDir(), "ARSSeaBreezeFCS32");
        private string SaveFile => Path.Combine(_saveDirectory, PrefabId.Id + ".json");
        private ARSolutionsSeaBreezeContainer _fridgeContainer;
        private ARSolutionsSeaBreezeFreonContainer _freonContainer;
        private bool _deconstructionAllowed = true;
        private float _maxTime;
        private float currentTime;
        #endregion

        #region  Public Properties

        /// <summary>
        /// Is the FridgeContainer is full
        /// </summary>
        public bool IsFull => _fridgeContainer.IsFull;

        /// <summary>
        /// Number of Items in the Fridge
        /// </summary>
        public int NumberOfItems => _fridgeContainer.NumberOfItems;

        public int FreeSpace => QPatch.Configuration.StorageLimit - NumberOfItems;

        public void OpenStorage() => _fridgeContainer.OpenStorage();
        public void AttemptToTakeItem(TechType techType) => _fridgeContainer.AttemptToTakeItem(techType);

        /// <summary>
        /// Items in the fridge
        /// </summary>
        List<EatableEntities> IFridgeContainer.FridgeItems => _fridgeContainer?.FridgeItems;

        public Dictionary<TechType, int> TrackedItems => _fridgeContainer?.TrackedItems;

        /// <summary>
        /// If the gameobject is costructed
        /// </summary>
        public bool IsConstructed => _buildable != null && _buildable.constructed;

        public Action<int, int> OnContainerUpdate
        {
            get => _fridgeContainer.OnContainerUpdate;
            set => _fridgeContainer.OnContainerUpdate = value;
        }

        #endregion

        #region internal Properties
        //internal ARSolutionsSeaBreezePowerManager PowerManager { get; private set; }
        internal ARSolutionsSeaBreezeAnimationManager AnimationManager { get; private set; }

        private string _currentTimeHMS { get; set; }
        internal bool CoolantIsDone => currentTime <= 0;
        internal ARSolutionsSeaBreezeDisplay Display { get; private set; }
        internal NameController NameController { get; private set; }

        private bool _runTimer;
        private bool _doOnce;
        private bool _initialized;
        private bool _runStartUpOnEnable;
        private SaveData _savedData;

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!_initialized)
                {
                    Initialized();
                }
                
                if (Display != null)
                {
                    Display.Setup(this);
                    var numberOfItems = _fridgeContainer.NumberOfItems;
                    Display.OnContainerUpdate(numberOfItems, QPatch.Configuration.StorageLimit);
                    Display.OnLabelChanged(NameController.GetCurrentName(), NameController);
                    _runStartUpOnEnable = false;
                }


                if (_savedData != null)
                {
                    _fridgeContainer.LoadFoodItems(_savedData.FridgeContainer);
                    //currentTime = _savedData.RemainingTime;
                    //PowerManager.SetHasBreakerTripped(savedData.HasBreakerTripped);
                    //_freonContainer.LoadFreon(savedData);
                    //Display.OnContainerUpdate(_fridgeContainer.NumberOfItems, QPatch.Configuration.Config.StorageLimit);
                    NameController.SetCurrentName(_savedData.UnitName);
                    Display?.OnLabelChanged(_savedData.UnitName, NameController);
                }

                _runStartUpOnEnable = false;
            }
        }


        private void Awake()
        {

        }

        private void Initialized()
        {
            //DO NOT REMOVE AWAKE ITS NEEDED BY COOKER
            PrefabId = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            
            if (PrefabId == null)
            {
                QuickLogger.Error("Prefab Identifier Component was not found");
            }

            if (_buildable == null)
            {
                _buildable = GetComponentInParent<Constructable>();
            }

            //Power Manager has been disabled in this version

            //PowerManager = gameObject.EnsureComponent<ARSolutionsSeaBreezePowerManager>();
            //if (PowerManager != null)
            //{
            //    PowerManager.Initialize(this);
            //    PowerManager.OnPowerOutage += OnPowerOutage;
            //    PowerManager.OnPowerResume += OnPowerResume;
            //}
            //else
            //{
            //    QuickLogger.Error("Power Manager Component was not found");
            //}
            
            _fridgeContainer = new ARSolutionsSeaBreezeContainer(this);
            //_freonContainer = new ARSolutionsSeaBreezeFreonContainer(this);
            //_freonContainer.OnPDAClosedAction += OnPdaClosedAction;
            //_freonContainer.OnPDAOpenedAction += OnPdaOpenedAction;

            if (NameController == null)
            {
                NameController = gameObject.EnsureComponent<NameController>();
                NameController.Initialize(ARSSeaBreezeFCS32Buildable.Submit(), Mod.FriendlyName);
            }
            
            AnimationManager = GetComponentInParent<ARSolutionsSeaBreezeAnimationManager>();

            if (AnimationManager == null)
            {
                QuickLogger.Error("Animation Manager Component was not found");
            }
            
            QuickLogger.Debug("Setting Name");

            NameController.SetCurrentName(Mod.GetNewSeabreezeName());
            NameController.OnLabelChanged += OnLabelChangedMethod;
            //InvokeRepeating("UpdateFridgeCooler", 1, 0.5f);

            if(Display == null)
                Display = gameObject.AddComponent<ARSolutionsSeaBreezeDisplay>();


            _initialized = true;

            QuickLogger.Info("Initialized");
        }

        private void OnLabelChangedMethod(string arg1, NameController arg2)
        { 
            OnLabelChanged?.Invoke(arg1,arg2);
        }


        private void Update()
        {
            if (!_initialized) return;


            //if (!Mathf.Approximately(currentTime, 0))
            //{
            //    UpdateTimer();
            //    UpdateDisplayTimer(_currentTimeHMS);
            //}
            //else
            //{
            //    if (_freonContainer.CheckIfFreonAvailable()) return;
            //    //QuickLogger.Debug("Setting timer to zero", true);
            //    UpdateDisplayTimer(TimeConverters.SecondsToHMS(0));
            //}
        }

        private void OnDestroy()
        {
            if (!_initialized) return;
            //PowerManager.OnPowerOutage -= OnPowerOutage;
            //PowerManager.OnPowerResume -= OnPowerResume;
            //_freonContainer.OnPDAClosedAction -= OnPdaClosedAction;
            //_freonContainer.OnPDAOpenedAction -= OnPdaOpenedAction;
        }

        #endregion

        #region Internal Methods
        internal void UpdateDisplayTimer(string filterRemainingTime)
        {
            if (Display == null) return;
            Display.UpdateTimer(filterRemainingTime);
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
            QuickLogger.Debug("OnConstructionChanged");
            if (constructed)
            {
                QuickLogger.Debug("Constructed");

                if (isActiveAndEnabled)
                {
                    if (!_initialized)
                    {
                        Initialized();
                    }
                    
                    if (Display != null)
                    {
                        Display.Setup(this);
                        Display.OnContainerUpdate(_fridgeContainer.NumberOfItems, QPatch.Configuration.StorageLimit);
                        Display.OnLabelChanged(NameController.GetCurrentName(), NameController);
                        _runStartUpOnEnable = false;
                    }
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
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

        public string GetPrefabID()
        {
            if(PrefabId == null)
                PrefabId = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();

            return PrefabId?.Id;
        }

        public bool AddItemToFridge(InventoryItem item, out string reason)
        {
            reason = string.Empty;
            if (_fridgeContainer != null)
            {
                if (!_fridgeContainer.IsFull)
                {
                    if (!_fridgeContainer.IsAllowedToAdd(item.item, true))
                    {
                        reason = ARSSeaBreezeFCS32Buildable.ItemNotAllowed();
                        return false;
                    }
                    _fridgeContainer.AddItemFromExternal(item);
                }
                else
                {
                    reason = ARSSeaBreezeFCS32Buildable.SeaBreezeFull();
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check to see if the amount if items <see cref="int"/> can be stored in the SeaBreeze Unit.
        /// </summary>
        /// <param name="amount">The amount of items to check against the container</param>
        /// <returns></returns>
        public bool CanBeStored(int amount)
        {
            return _fridgeContainer != null && _fridgeContainer.HasRoomFor(amount);
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
            //if (_freonContainer == null) return;

            //QuickLogger.Debug($"GOS {_freonContainer.GetOpenState()} || GIPA {PowerManager.GetIsPowerAvailable()} || CID {CoolantIsDone}");

            //if (!_freonContainer.GetOpenState() && PowerManager.GetIsPowerAvailable() &&
            //    !PowerManager.GetHasBreakerTripped() && !CoolantIsDone)
            //{
            //    //_fridgeContainer.CoolItems();
            //    StartTimer();
            //    return;
            //}

            //_fridgeContainer.DecayItems();
            //StopTimer();
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
        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug($"Saving {PrefabId.Id} Data");

            if (!Directory.Exists(_saveDirectory))
                Directory.CreateDirectory(_saveDirectory);

            var saveData = new SaveData
            {
                //HasBreakerTripped = PowerManager.GetHasBreakerTripped(),
                FridgeContainer = _fridgeContainer.GetSaveData(),
                RemainingTime = currentTime,
                UnitName = NameController.GetCurrentName()
            };

            var output = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            File.WriteAllText(SaveFile, output);

            QuickLogger.Debug($"Saved {PrefabId.Id} Data");
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("// ****************************** Load Data *********************************** //");

            PrefabId = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();

            if (PrefabId == null)
            {
                QuickLogger.Error("Prefab Identifier Component was not found");
            }

            if (PrefabId != null)
            {
                QuickLogger.Info($"Loading SeaBreezeFCS32 {PrefabId.Id}");

                if (File.Exists(SaveFile))
                {
                    string savedDataJson = File.ReadAllText(SaveFile).Trim();

                    //LoadData
                    _savedData = JsonConvert.DeserializeObject<SaveData>(savedDataJson);

                }
            }
            else
            {
                QuickLogger.Error("PrefabIdentifier is null");
            }

            QuickLogger.Debug("// ****************************** Loaded Data *********************************** //");
        }
        #endregion

        internal void SetDeconstructionAllowed(bool value)
        {
            _deconstructionAllowed = value;
        }

        public void RenameDevice(string newName)
        {
            if (NameController != null)
            {
                NameController.SetCurrentName(newName);
            }
        }

        public string GetDeviceName()
        {
            return NameController != null ? NameController.GetCurrentName() : string.Empty;
        }

        public void SetNameControllerTag(object obj)
        {
            if (NameController != null)
            {
                NameController.Tag = obj;
            }
        }

        public Action<string, NameController> OnLabelChanged { get; set; }
    }
}
