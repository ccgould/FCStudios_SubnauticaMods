using FCS_DeepDriller.Attachments;
using FCS_DeepDriller.Buildable;
using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Enumerators;
using FCS_DeepDriller.Helpers;
using FCS_DeepDriller.Managers;
using FCS_DeepDriller.Mono.Handlers;
using FCSAlterraIndustrialSolutions.Models.Controllers.Logic;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Models.Components;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using FCSCommon.Utilities.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_DeepDriller.Mono
{
    [RequireComponent(typeof(WeldablePoint))]
    internal class FCSDeepDrillerController : MonoBehaviour, IConstructable, IProtoEventListener
    {
        #region Private Members
        private DeepDrillerSaveDataEntry _saveData;
        private string _currentBiome;
        private string CurrentBiome
        {
            get => _currentBiome;
            set
            {
                _currentBiome = value;
                if (value != BiomeManager.NoneString && !string.IsNullOrEmpty(value))
                {
                    ConnectDisplay();
                }
            }
        }
        private Constructable _buildable;
        private PrefabIdentifier _prefabId;
        private bool _initialized;
        private BatteryAttachment _batteryAttachment;
        private List<TechType> _bioData = new List<TechType>();
        #endregion

        #region Internal Properties
        internal bool IsBeingDeleted { get; set; }
        internal FCSDeepDrillerAnimationHandler AnimationHandler { get; private set; }
        internal FCSDeepDrillerLavaPitHandler LavaPitHandler { get; private set; }
        internal FCSDeepDrillerContainer DeepDrillerContainer { get; private set; }
        internal FCSDeepDrillerModuleContainer DeepDrillerModuleContainer { get; private set; }
        internal bool IsConstructed { get; private set; }  //=> _buildable != null && _buildable.constructed;
        internal FCSDeepDrillerPowerHandler PowerManager { get; private set; }
        internal FCSDeepDrillerDisplay DisplayHandler { get; private set; }
        internal HealthController HealthManager { get; private set; }
        internal int ExtendStateHash { get; private set; }
        internal int ShaftStateHash { get; private set; }
        internal int BitSpinState { get; private set; }
        internal int BitDamageState { get; private set; }
        internal int ScreenStateHash { get; private set; }
        internal BatteryAttachment BatteryController { get; private set; }
        internal OreGenerator OreGenerator { get; private set; }
        internal VFXManager VFXManagerHandler { get; private set; }
        public DeepDrillerComponentManager ComponentManager { get; private set; }

        #endregion

        #region Unity Methods
        private void Update()
        {
            if (HealthManager != null)
            {
                HealthManager.HealthChecks();
            }
        }
        #endregion

        #region IConstructable
        public bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;

            if (DeepDrillerModuleContainer.IsEmpty() && DeepDrillerContainer.IsEmpty())
            {
                return true;
            }

            reason = FCSDeepDrillerBuildable.RemoveAllItems();
            return false;
        }

        public void OnConstructedChanged(bool constructed)
        {
            QuickLogger.Info("In Constructed Changed");

            IsConstructed = constructed;

            if (IsBeingDeleted) return;

            if (constructed)
            {
                if (!_initialized)
                {
                    Initialize();
                }
            }
        }
        #endregion

        #region IProtoEventListener

        internal void Save(DeepDrillerSaveData saveDataList)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_saveData == null)
            {
                _saveData = new DeepDrillerSaveDataEntry();
            }

            _saveData.Id = id;
            _saveData.PowerState = PowerManager.GetPowerState();
            _saveData.Modules = DeepDrillerModuleContainer.GetCurrentModules();
            _saveData.Items = DeepDrillerContainer.GetItems();
            _saveData.Health = HealthManager.GetHealth();
            _saveData.PowerData = PowerManager.SaveData();
            _saveData.FocusOre = OreGenerator.GetFocus();
            _saveData.IsFocused = OreGenerator.GetIsFocused();
            _saveData.Biome = BiomeManager.GetBiome();
            saveDataList.Entries.Add(_saveData);
        }
        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info("Saving Drills");
                Mod.SaveDeepDriller();
            }
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            var data = Mod.GetDeepDrillerSaveData(id);

            CurrentBiome = data.Biome;
            OreGenerator.AllowedOres = GetBiomeData();
            OreGenerator.SetFocus(data.FocusOre);
            DeepDrillerModuleContainer.SetModules(data.Modules);
            DeepDrillerContainer.LoadItems(data.Items);
            HealthManager.SetHealth(data.Health);
            PowerManager.LoadData(data);

            if (data.IsFocused)
            {
                OreGenerator.SetIsFocus(data.IsFocused);
            }

            _batteryAttachment.GetController().LoadData(data.PowerData);

            QuickLogger.Debug("Updating ListItems");

            StartCoroutine(ForceSelection());

            if (PowerManager.GetPowerState() == FCSPowerStates.Powered && !AnimationHandler.GetBoolHash(ExtendStateHash))
            {
                StartCoroutine(DropLegs());
            }

        }

        private IEnumerator ForceSelection()
        {
            int i = 0;
            while (i < 5)
            {
                DisplayHandler.UpdateListItems(GetFocusedOre());
                i++;
                yield return null;
            }
        }

        private void UpdateLegState(bool value)
        {
            AnimationHandler.SetBoolHash(ExtendStateHash, value);
        }

        #endregion

        #region Private Methods

        private void Initialize()
        {

            ComponentManager = new DeepDrillerComponentManager();

            _batteryAttachment = new BatteryAttachment();
            _batteryAttachment.GetGameObject(this);
            _batteryAttachment.GetController().OnBatteryAdded += OnBatteryAdded;
            _batteryAttachment.GetController().OnBatteryRemoved += OnBatteryRemoved;

            BatteryController = _batteryAttachment;

            var solarAttachment = new SolarAttachment();
            solarAttachment.GetGameObject(this);

            if (!ComponentManager.FindAllComponents(this, solarAttachment.GetSolarAttachment(), _batteryAttachment.GetBatteryAttachment(), null))
            {
                QuickLogger.Error("Couldn't find all components");
                return;
            }

            ComponentManager.Setup();

            ExtendStateHash = Animator.StringToHash("Extend");

            ShaftStateHash = Animator.StringToHash("ShaftState");

            ScreenStateHash = Animator.StringToHash("ScreenState");

            BitSpinState = Animator.StringToHash("BitSpinState");

            BitDamageState = Animator.StringToHash("BitDamageState");

            TechTypeHelper.Initialize();

            _prefabId = GetComponentInParent<PrefabIdentifier>();

            if (_prefabId == null)
            {
                QuickLogger.Error("Prefab Identifier Component was not found");
            }

            if (_buildable == null)
            {
                _buildable = GetComponentInParent<Constructable>();
            }

            var uniqueLiveMixingData = new CustomLiveMixinData.UniqueLiveMixinData();
            var liveMixinData = uniqueLiveMixingData.Create(100, true, true, true);

            HealthManager = gameObject.AddComponent<HealthController>();

            OreGenerator = gameObject.AddComponent<OreGenerator>();
            OreGenerator.Initialize(this);
            OreGenerator.OnAddCreated += OreGeneratorOnAddCreated;

            UpdateCurrentBiome();

            if (CurrentBiome != BiomeManager.NoneString && !string.IsNullOrEmpty(CurrentBiome))
            {
                OreGenerator.AllowedOres = GetBiomeData();
            }

            var liveMixin = gameObject.AddComponent<LiveMixin>();

            HealthManager.Initialize(liveMixin, liveMixinData);
            HealthManager.FullHealth();
            HealthManager.OnDamaged += OnDamaged;
            HealthManager.OnRepaired += OnRepaired;

            DeepDrillerContainer = new FCSDeepDrillerContainer();
            DeepDrillerContainer.Setup(this);

            DeepDrillerModuleContainer = new FCSDeepDrillerModuleContainer();
            DeepDrillerModuleContainer.Setup(this);

            //VFXManagerHandler = gameObject.AddComponent<VFXManager>();
            //VFXManagerHandler.Initialize(this);

            PowerManager = gameObject.AddComponent<FCSDeepDrillerPowerHandler>();
            PowerManager.Initialize(this);
            PowerManager.OnPowerUpdate += OnPowerUpdate;

            AnimationHandler = gameObject.AddComponent<FCSDeepDrillerAnimationHandler>();
            AnimationHandler.Initialize(this);

            LavaPitHandler = gameObject.AddComponent<FCSDeepDrillerLavaPitHandler>();
            LavaPitHandler.Initialize(this);
            LavaPitHandler.OnLavaRaised += OnLavaRaised;


            UpdateSystemLights(PowerManager.GetPowerState());

            _initialized = true;
        }

        private void OnLavaRaised(bool value)
        {
            //VFXManagerHandler.UpdateVFX();
        }

        private void OnDamaged()
        {
            AnimationHandler.SetBoolHash(BitDamageState, true);
            AnimationHandler.SetIntHash(ShaftStateHash, 2);
            UpdateSystemLights(PowerManager.GetPowerState());
        }

        private void OnRepaired()
        {
            AnimationHandler.SetBoolHash(BitDamageState, false);
            UpdateSystemLights(PowerManager.GetPowerState());
        }

        private void OnBatteryRemoved(Pickupable obj)
        {
            PowerManager.RemoveBattery(obj);
        }

        private void OnBatteryAdded(Pickupable obj, string slot)
        {
            PowerManager.AddBattery(obj, slot);
        }

        private void OnPowerUpdate(FCSPowerStates value)
        {
            UpdateDrillShaftSate(value);
            UpdateSystemLights(value);
            QuickLogger.Debug($"PowerState Changed to: {value}", true);
        }

        private void UpdateSystemLights(FCSPowerStates value)
        {
            QuickLogger.Debug($"Changing System Lights", true);

            if (HealthManager.IsDamagedFlag)
            {
                MaterialHelpers.ReplaceEmissionTexture("DeepDriller_BaseColor_BaseColor", "DeepDriller_Emissive_Error", gameObject, QPatch.GlobalBundle);
                return;
            }

            if (value == FCSPowerStates.Powered && !HealthManager.IsDamagedFlag)
                MaterialHelpers.ReplaceEmissionTexture("DeepDriller_BaseColor_BaseColor", "DeepDriller_Emissive_On",
                    gameObject, QPatch.GlobalBundle);
            else if (value == FCSPowerStates.Unpowered || value == FCSPowerStates.Tripped && !HealthManager.IsDamagedFlag)
                MaterialHelpers.ReplaceEmissionTexture("DeepDriller_BaseColor_BaseColor", "DeepDriller_Emissive_Off",
                    gameObject, QPatch.GlobalBundle);
        }

        private void UpdateDrillShaftSate(FCSPowerStates value)
        {
            switch (value)
            {
                case FCSPowerStates.None:
                    break;
                case FCSPowerStates.Powered:
                    AnimationHandler.SetBoolHash(BitSpinState, true);
                    AnimationHandler.SetIntHash(ShaftStateHash, 1);
                    break;
                case FCSPowerStates.Unpowered:
                    AnimationHandler.SetBoolHash(BitSpinState, false);
                    break;
                case FCSPowerStates.Tripped:
                    AnimationHandler.SetBoolHash(BitSpinState, false);
                    AnimationHandler.SetIntHash(ShaftStateHash, 2);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        private void OreGeneratorOnAddCreated(TechType type)
        {
            QuickLogger.Debug($"In OreGeneratorOnOnAddCreated {type}");
            DeepDrillerContainer.AddItem(type.ToPickupable());
        }

        private void UpdateCurrentBiome()
        {
            QuickLogger.Debug($"Attempting to find biome || CB {CurrentBiome}");
            CurrentBiome = BiomeManager.GetBiome();
        }

        private void ConnectDisplay()
        {
            if (DisplayHandler != null) return;
            DisplayHandler = gameObject.AddComponent<FCSDeepDrillerDisplay>();
            DisplayHandler.Setup(this);
        }

        #endregion

        #region Internal Methods
        internal void PowerOffDrill()
        {
            if (PowerManager.GetPowerState() == FCSPowerStates.Tripped) return;
            UpdateLegState(false);
            PowerManager.SetPowerState(FCSPowerStates.Tripped);
        }

        internal void PowerOnDrill()
        {
            if (HealthManager.IsDamagedFlag || !PowerManager.IsPowerAvailable()) return;

            UpdateLegState(true);
            PowerManager.SetPowerState(FCSPowerStates.Powered);
        }

        internal IEnumerator DropLegs()
        {
            QuickLogger.Debug("Attempting to Extend legs");

            int i = 1;
            while (!AnimationHandler.GetBoolHash(ExtendStateHash))
            {
                PowerOnDrill();
                QuickLogger.Debug($"Attempting to extend legs attempt ({i++})");
                yield return null;
            }
        }

        internal void RemoveAttachment(DeepDrillModules module)
        {
            if (module == DeepDrillModules.Focus)
            {
                OreGenerator.RemoveFocus();
            }

            if (module == DeepDrillModules.Solar)
            {
                PowerManager.RemoveSolar();
            }

            ComponentManager.HideAttachment(module);
        }

        internal List<TechType> GetBiomeData()
        {
            if (_bioData.Count == 0)
            {
                _bioData = BiomeManager.GetBiomeData(CurrentBiome);
            }

            QuickLogger.Debug($"BioData Count = {_bioData.Count}");

            return _bioData;
        }

        internal void AddAttachment(DeepDrillModules module)
        {
            ComponentManager.ShowAttachment(module);
        }

        internal void SetOreFocus(TechType techType)
        {
            OreGenerator.SetFocus(techType);
        }

        internal bool GetFocusedState()
        {
            return OreGenerator.GetIsFocused();
        }

        internal TechType GetFocusedOre()
        {
            return OreGenerator.GetFocus();
        }
        #endregion
    }
}
