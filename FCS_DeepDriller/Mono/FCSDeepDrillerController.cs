using FCS_DeepDriller.Attachments;
using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Enumerators;
using FCS_DeepDriller.Helpers;
using FCS_DeepDriller.Managers;
using FCS_DeepDriller.Mono.Handlers;
using FCSAlterraIndustrialSolutions.Models.Controllers.Logic;
using FCSCommon.Extensions;
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

        private DeepDrillerSaveDataEntry _saveData;
        private string _currentBiome;
        private HealthController _healthSystem;
        private OreGenerator _oreGenerator;
        private Constructable _buildable;
        private PrefabIdentifier _prefabId;
        private bool _initialized;
        private BatteryAttachment _batteryAttachment;
        private List<TechType> _bioData = new List<TechType>();


        internal bool IsBeingDeleted { get; set; }
        internal FCSDeepDrillerAnimationHandler AnimationHandler { get; private set; }
        public FCSDeepDrillerLavaPitHandler LavaPitHandler { get; private set; }
        internal FCSDeepDrillerContainer DeepDrillerContainer { get; private set; }
        internal FCSDeepDrillerModuleContainer DeepDrillerModuleContainer { get; private set; }
        internal bool IsConstructed { get; private set; }  //=> _buildable != null && _buildable.constructed;
        internal FCSDeepDrillerPowerHandler PowerManager { get; private set; }
        internal int ExtendStateHash { get; private set; }
        internal int ShaftStateHash { get; private set; }
        internal int BitSpinState { get; private set; }
        public int BitDamageState { get; private set; }
        public int ScreenStateHash { get; private set; }
        internal BatteryAttachment BatteryController { get; private set; }


        #region IConstructable
        public bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        public void OnConstructedChanged(bool constructed)
        {
            QuickLogger.Info("In Constructed Changed");

            IsConstructed = constructed;

            if (IsBeingDeleted) return;

            if (constructed)
            {
                _currentBiome = BiomeManager.GetBiome();

                if (!_initialized)
                {
                    Initialize();
                }


                DisplayHandler = gameObject.AddComponent<FCSDeepDrillerDisplay>();
                DisplayHandler.Setup(this);
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
            _saveData.Health = _healthSystem.GetHealth();
            _saveData.PowerData = PowerManager.SaveData();
            _saveData.FocusOre = _oreGenerator.GetFocus();
            _saveData.IsFocused = _oreGenerator.GetIsFocused();
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

            DeepDrillerModuleContainer.SetModules(data.Modules);
            DeepDrillerContainer.LoadItems(data.Items);
            _healthSystem.SetHealth(data.Health);
            PowerManager.LoadData(data);
            _oreGenerator.SetFocus(data.FocusOre);

            if (data.IsFocused)
            {
                _oreGenerator.ToggleFocus();
            }

            _batteryAttachment.GetController().LoadData(data.PowerData);
            DisplayHandler.UpdateListItems(data.FocusOre);
        }

        private void UpdateLegState(bool value)
        {
            AnimationHandler.SetBoolHash(ExtendStateHash, value);
        }

        #endregion

        private void Initialize()
        {

            _batteryAttachment = new BatteryAttachment();
            _batteryAttachment.GetGameObject(this);
            _batteryAttachment.GetController().OnBatteryAdded += OnBatteryAdded;
            _batteryAttachment.GetController().OnBatteryRemoved += OnBatteryRemoved;

            BatteryController = _batteryAttachment;

            var solarAttachment = new SolarAttachment();
            solarAttachment.GetGameObject(this);

            //var focusAttachment = new FocusAttachment();
            //focusAttachment.GetGameObject(this);

            if (!DeepDrillerComponentManager.FindAllComponents(this, solarAttachment.GetSolarAttachment(), _batteryAttachment.GetBatteryAttachment(), null))
            {
                QuickLogger.Error("Couldn't find all components");
                return;
            }

            DeepDrillerComponentManager.Setup();

            ExtendStateHash = Animator.StringToHash("Extend");

            ShaftStateHash = Animator.StringToHash("ShaftState");

            ScreenStateHash = Animator.StringToHash("ScreenState");

            BitSpinState = Animator.StringToHash("BitSpinState");

            BitDamageState = Animator.StringToHash("BitDamageState");


            TechTypeHelpers.Initialize();

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

            _healthSystem = gameObject.AddComponent<HealthController>();

            _oreGenerator = gameObject.AddComponent<OreGenerator>();
            _oreGenerator.Initialize(1, 2);// TODO replace with 5,8
            _oreGenerator.OnAddCreated += OreGeneratorOnAddCreated;
            _oreGenerator.AllowedOres = GetBiomeData();

            var liveMixin = gameObject.AddComponent<LiveMixin>();

            _healthSystem.Startup(liveMixin, liveMixinData);
            _healthSystem.FullHealth();

            DeepDrillerContainer = new FCSDeepDrillerContainer();
            DeepDrillerContainer.Setup(this);

            DeepDrillerModuleContainer = new FCSDeepDrillerModuleContainer();
            DeepDrillerModuleContainer.Setup(this);

            PowerManager = gameObject.AddComponent<FCSDeepDrillerPowerHandler>();
            PowerManager.Initialize(this);
            PowerManager.OnPowerUpdate += OnPowerUpdate;

            AnimationHandler = gameObject.AddComponent<FCSDeepDrillerAnimationHandler>();
            AnimationHandler.Initialize(this);

            LavaPitHandler = gameObject.AddComponent<FCSDeepDrillerLavaPitHandler>();
            LavaPitHandler.Initialize(this);

            _initialized = true;
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
            _oreGenerator.SetAllowTick(value);
            UpdateDrillShaftSate(value);
            LavaPitHandler.RestTime();
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

        public FCSDeepDrillerDisplay DisplayHandler { get; private set; }
        private void OreGeneratorOnAddCreated(TechType type)
        {
            QuickLogger.Debug($"In OreGeneratorOnOnAddCreated {type}");
            DeepDrillerContainer.AddItem(type.ToPickupable());
        }

        internal void PowerOffDrill()
        {
            if (PowerManager.GetPowerState() != FCSPowerStates.Tripped)
            {
                UpdateLegState(false);
                PowerManager.SetPowerState(FCSPowerStates.Tripped);
                AnimationHandler.SetBoolHash(ScreenStateHash, false);
            }
        }

        internal void StopDrill()
        {
            throw new System.NotImplementedException();
        }

        internal void PowerOnDrill()
        {
            UpdateLegState(true);
            PowerManager.SetPowerState(FCSPowerStates.Powered);
            AnimationHandler.SetBoolHash(ScreenStateHash, true);
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
                _oreGenerator.RemoveFocus();
            }
            DeepDrillerComponentManager.HideAttachment(module);
        }

        internal void AddAttachment(DeepDrillModules module)
        {
            DeepDrillerComponentManager.ShowAttachment(module);
        }

        internal List<TechType> GetBiomeData()
        {
            if (_bioData.Count == 0)
            {
                _bioData = BiomeManager.GetBiomeData(_currentBiome);
            }

            QuickLogger.Debug($"BioData Count = {_bioData.Count}");

            return _bioData;
        }

        internal void SetOreFocus(TechType techType)
        {
            _oreGenerator.SetFocus(techType);
        }

        internal void ToggleFocus()
        {
            _oreGenerator.ToggleFocus();
        }

        internal bool GetFocusedState()
        {
            return _oreGenerator.GetIsFocused();
        }

        internal TechType GetFocusedOre()
        {
            return _oreGenerator.GetFocus();
        }
    }
}
