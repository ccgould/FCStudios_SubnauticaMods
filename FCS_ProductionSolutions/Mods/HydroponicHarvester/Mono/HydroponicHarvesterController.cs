using System.Collections.Generic;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.HydroponicHarvester.Buildable;
using FCS_ProductionSolutions.Mods.HydroponicHarvester.Enumerators;
using FCS_ProductionSolutions.Structs;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.HydroponicHarvester.Mono
{
    internal class HydroponicHarvesterController : FcsDevice, IFCSSave<SaveData>, IHandTarget
    {
        private bool _isFromSave;
        private bool _runStartUpOnEnable;
        private HydroponicHarvesterDataEntry _savedData;
        private EffectsManager _effectsManager;
        private bool _isInBase;
        internal DisplayManager DisplayManager;
        private InterfaceInteraction _interactionHelper;
        private const float powerUsage = 0.85f;
        public override StorageType StorageType => StorageType.Harvester;
        public EffectsManager EffectsManager => _effectsManager;
        public GrowBedManager GrowBedManager { get; set; }
        public override bool IsOperational => IsOperationalCheck();
        //private Dictionary<TechType, PickReturnsData> _pickData => WorldHelpers.PickReturns;
        //private List<FCSDNASampleData> _knownTechType => Mod.GetHydroponicKnownTech();

        #region Unity Methods

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, HydroponicHarvesterPatch.HydroponicHarvesterModTabID, Mod.ModPackID);
            Manager.AlertNewFcsStoragePlaced(this);
            DisplayManager?.UpdateUI();
        }

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (_isFromSave)
                {
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }

                    _colorManager.LoadTemplate(_savedData.ColorTemplate);
                    DisplayManager.SetSpeedGraphic(_savedData.HarvesterSpeedMode);
                    if (_savedData.SetBreaker)
                    {
                        EffectsManager.SetBreaker(true);
                        EffectsManager.TurnOffLights();
                        DisplayManager.SetLightGraphicOff();
                    }
                    
                    _isInBase = _savedData.IsInBase;

                    //QuickLogger.Debug($"=============== Attempting Harvester {UnitID} Cleaning ===============");
                    ////Check if any ghost objects and clean
                    //foreach (Plantable plantable in gameObject.GetComponentsInChildren<Plantable>(true))
                    //{
                    //    Destroy(plantable.gameObject);
                    //    QuickLogger.Debug($"Destroyed Item {plantable.plantTechType.AsString()}");

                    //}
                    //QuickLogger.Debug($"=============== Attempting Harvester {UnitID} Cleaning Completed ===============");

                    GrowBedManager.Load(_savedData);
                }

                _runStartUpOnEnable = false;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            IPCMessage -= OnIpcMessage;
            if (GrowBedManager?.ItemsContainer != null)
            {
                GrowBedManager.ItemsContainer.onAddItem -= UpdateTerminals;
                GrowBedManager.ItemsContainer.onRemoveItem -= UpdateTerminals;
                GrowBedManager.ItemsContainer.onRemoveItem -= GrowBedManager.OnItemPulledFromStorage;
            }
        }

        #endregion

        public override float GetPowerUsage()
        {
            if (GrowBedManager == null || GrowBedManager.IsFull || !GrowBedManager.HasSeeds()) return 0;

            switch (GrowBedManager.GetCurrentSpeedMode())
            {
                case HarvesterSpeedModes.Off:
                    return 0;
                case HarvesterSpeedModes.Max:
                    return powerUsage * 4;
                case HarvesterSpeedModes.High:
                    return powerUsage * 3;
                case HarvesterSpeedModes.Low:
                    return powerUsage * 2;
                case HarvesterSpeedModes.Min:
                    return powerUsage;
                default:
                    return 0f;
            }
        }

        private bool IsOperationalCheck()
        {
            //Returning true until further notice
            return true;
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetHydroponicHarvesterSaveData(GetPrefabID());
        }

        public override void Initialize()
        {
            if (IsInitialized) return;
            
            if (GrowBedManager == null)
            {
                GrowBedManager = gameObject.GetComponent<GrowBedManager>();
                GrowBedManager.Initialize(this);
                GrowBedManager.ItemsContainer.onAddItem += UpdateTerminals;
                GrowBedManager.ItemsContainer.onRemoveItem += UpdateTerminals;
                GrowBedManager.ItemsContainer.onRemoveItem += GrowBedManager.OnItemPulledFromStorage;
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol);
            }

            if (EffectsManager == null)
            {
                _effectsManager = new EffectsManager(this);
                InvokeRepeating(nameof(CheckSystem), .5f, .5f);
            }

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<DisplayManager>();
                DisplayManager.Setup(this);
            }
            
#if DEBUG
            QuickLogger.Debug($"Initialized Harvester {GetPrefabID()}");
#endif

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
            MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseLightsEmissiveController, gameObject, 4f);
            
            IPCMessage += OnIpcMessage;

            var canvas = gameObject.GetComponentInChildren<Canvas>();
            _interactionHelper = GameObjectHelpers.FindGameObject(gameObject, "Canvas2").AddComponent<InterfaceInteraction>();
            //InvokeRepeating(nameof(CheckIfWorldSettled), 1f, 1f);
            IsInitialized = true;
        }
        
        private void UpdateTerminals(InventoryItem item)
        {
            BaseManager.GlobalNotifyByID("DTC", "ItemUpdateDisplay");
        }

        private void OnIpcMessage(string message)
        {
            if (message.Equals("UpdateDNA"))
            {
                DisplayManager.LoadKnownSamples();
                QuickLogger.Debug("Loading DNA Samples");
            }
        }

        private void CheckSystem()
        {
            _effectsManager?.ToggleLightsByDistance();
        }

        public override IFCSStorage GetStorage()
        {
            return GrowBedManager;
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {GetPrefabID()}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {GetPrefabID()}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");

            if (_savedData == null)
            {
                ReadySaveData();
            }

            _isFromSave = true;
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;

            if (GrowBedManager != null)
            {
                var hasItems = GrowBedManager.HasItems();
                if (hasItems)
                {
                    reason = $"Cannot destroy harvester {UnitID} because it has plants inside";
                    return false;
                }
            }

            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;

            if (constructed)
            {
                if (Player.main != null)
                {
                    _isInBase = Player.main.IsInBase();
                }

                if (isActiveAndEnabled)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                    }

                    IsInitialized = true;
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new HydroponicHarvesterDataEntry();
            }

            _savedData.ID = GetPrefabID();
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
            _savedData.IsInBase = _isInBase;
            _savedData.HarvesterSpeedMode = GrowBedManager.GetCurrentSpeedMode();
            _savedData.SetBreaker = EffectsManager.GetBreakerState();
            GrowBedManager.Save(_savedData);
            newSaveData.HydroponicHarvesterEntries.Add(_savedData);
            QuickLogger.Debug($"Saving ID {_savedData.ID}", true);
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }
        
        public bool IsInBase()
        {
            return _isInBase;
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        public override Pickupable RemoveItemFromContainer(TechType techType)
        {
            QuickLogger.Debug("Hydroponics Remove From Container",true);

            GrowBedManager.TakeItemFromContainer(techType,GrowBedManager.GetSlotByItem(techType),false);
            var itemTask = new TaskResult<InventoryItem>();
            CouroutineManager.WaitCoroutine(techType.ToInventoryItem(itemTask));
            return itemTask.Get().item;
        }

        public override void OnHandHover(GUIHand hand)
        {
            if (!IsInitialized || !IsConstructed || _interactionHelper.IsInRange) return;
            base.OnHandHover(hand);

            var data = new string[]{};
            data.HandHoverPDAHelperEx(GetTechType());
        }
        public void OnHandClick(GUIHand hand)
        {
            
        }
    }
}