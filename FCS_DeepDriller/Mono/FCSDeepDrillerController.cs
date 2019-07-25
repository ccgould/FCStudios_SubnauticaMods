using FCS_DeepDriller.Attachments;
using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Enumerators;
using FCS_DeepDriller.Helpers;
using FCS_DeepDriller.Managers;
using FCS_DeepDriller.Mono.Handlers;
using FCSAlterraIndustrialSolutions.Models.Controllers.Logic;
using FCSCommon.Extensions;
using FCSCommon.Models;
using FCSCommon.Models.Components;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using FCSCommon.Utilities.Enums;
using UnityEngine;

namespace FCS_DeepDriller.Mono
{
    [RequireComponent(typeof(WeldablePoint))]
    internal class FCSDeepDrillerController : MonoBehaviour, IConstructable, IProtoEventListener
    {

        private DeepDrillerSaveDataEntry _saveData;
        private string _currentBiome;
        private PowerController _powerSystem;
        private HealthController _healthSystem;
        private OreGenerator _oreGenerator;
        private Constructable _buildable;
        private PrefabIdentifier _prefabId;
        private bool _initialized;


        internal bool IsBeingDeleted { get; set; }
        internal FCSDeepDrillerAnimationHandler AnimationHandler { get; private set; }

        internal FCSDeepDrillerContainer DeepDrillerContainer { get; private set; }
        internal FCSDeepDrillerModuleContainer DeepDrillerModuleContainer { get; private set; }
        internal bool IsConstructed { get; private set; }  //=> _buildable != null && _buildable.constructed;
        internal FCSDeepDrillerPowerHandler PowerManager { get; private set; }
        internal int ExtendStateHash { get; private set; }
        internal int ShaftStateHash { get; private set; }
        public BatteryAttachment BatteryController { get; private set; }

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
            saveDataList.Entries.Add(_saveData);
        }

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

        //TODO Set _OreGen new methods on save

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

            PowerManager.SetPowerState(data.PowerState);
            DeepDrillerModuleContainer.SetModules(data.Modules);
            DeepDrillerContainer.LoadItems(data.Items);
            _healthSystem.SetHealth(data.Health);
        }

        #endregion

        private void Initialize()
        {

            var batteryAttachment = new BatteryAttachment();
            batteryAttachment.GetGameObject(this);
            batteryAttachment.GetController().OnBatteryAdded += OnBatteryAdded;
            batteryAttachment.GetController().OnBatteryRemoved += OnBatteryRemoved;

            BatteryController = batteryAttachment;

            var solarAttachment = new SolarAttachment();
            solarAttachment.GetGameObject(this);

            var focusAttachment = new FocusAttachment();
            focusAttachment.GetGameObject(this);

            if (!DeepDrillerComponentManager.FindAllComponents(this, solarAttachment.GetSolarAttachment(), batteryAttachment.GetBatteryAttachment(), focusAttachment.GetFocusAttachment()))
            {
                QuickLogger.Error("Couldn't find all components");
                return;
            }

            DeepDrillerComponentManager.Setup();


            ExtendStateHash = Animator.StringToHash("Extend");

            ShaftStateHash = Animator.StringToHash("ShaftState");

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
            _oreGenerator.AllowedOres = BiomeManager.GetBiomeData(_currentBiome);

            var liveMixin = gameObject.AddComponent<LiveMixin>();

            _healthSystem.Startup(liveMixin, liveMixinData);
            _healthSystem.FullHealth();

            DeepDrillerContainer = new FCSDeepDrillerContainer();
            DeepDrillerContainer.Setup(this);

            DeepDrillerModuleContainer = new FCSDeepDrillerModuleContainer();
            DeepDrillerModuleContainer.Setup(this);

            _powerSystem = gameObject.AddComponent<PowerController>();
            _powerSystem.CreateBattery(100, 2000);

            PowerManager = gameObject.AddComponent<FCSDeepDrillerPowerHandler>();
            PowerManager.Initialize(this);
            PowerManager.OnPowerUpdate += OnPowerUpdate;

            AnimationHandler = gameObject.AddComponent<FCSDeepDrillerAnimationHandler>();
            AnimationHandler.Initialize(this);

            _initialized = true;
        }


        private void OnBatteryRemoved(Pickupable obj)
        {
            PowerManager.RemoveBattery(obj);
        }

        private void OnBatteryAdded(Pickupable obj)
        {
            PowerManager.AddBattery(obj);
        }

        private void OnPowerUpdate(FCSPowerStates value)
        {
            _oreGenerator.SetAllowTick(value);
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
                AnimationHandler.SetBoolHash(ExtendStateHash, false);
                PowerManager.SetPowerState(FCSPowerStates.Tripped);
            }
        }

        internal void StopDrill()
        {
            throw new System.NotImplementedException();
        }

        internal void PowerOnDrill()
        {
            if (PowerManager.GetPowerState() != FCSPowerStates.Powered)
            {
                AnimationHandler.SetBoolHash(ExtendStateHash, true);
                PowerManager.SetPowerState(FCSPowerStates.Powered);
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
            if (module == DeepDrillModules.Focus)
            {
                _oreGenerator.AddFocus();
            }
            DeepDrillerComponentManager.ShowAttachment(module);

        }
    }
}
