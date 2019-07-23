using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Enumerators;
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

        internal void Save(DeepDrillerSaveData saveDataList)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_saveData == null)
            {
                _saveData = new DeepDrillerSaveDataEntry() { Id = id };
            }

            _saveData.Id = id;
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

        private void Initialize()
        {
            ExtendStateHash = Animator.StringToHash("Extend");

            ShaftStateHash = Animator.StringToHash("ShaftState");

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
            _oreGenerator.Start(1, 2);// TODO replace with 5,8
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

        private void OnPowerUpdate(bool value)
        {
            _oreGenerator.SetAllowTick(value);
        }

        public FCSDeepDrillerDisplay DisplayHandler { get; private set; }

        #endregion

        #region IProtoEventListener
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
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            var data = Mod.GetDeepDrillerSaveData(id);
        }
        #endregion

        private void OreGeneratorOnAddCreated(string type)
        {
            QuickLogger.Debug($"In OreGeneratorOnOnAddCreated {type}");
            DeepDrillerContainer.AddItem(type.ToPickupable());
        }

        internal void PowerOffDrill()
        {
            AnimationHandler.SetBoolHash(ExtendStateHash, false);
            PowerManager.SetPowerState(FCSPowerStates.Tripped);
        }

        internal void StopDrill()
        {
            throw new System.NotImplementedException();
        }

        internal void PowerOnDrill()
        {
            AnimationHandler.SetBoolHash(ExtendStateHash, true);
            PowerManager.SetPowerState(FCSPowerStates.Powered);
        }

        internal void RemoveAttachment()
        {
            PowerManager.SetModule(DeepDrillModules.None);
        }

        internal void AddAttachment(DeepDrillModules module)
        {
            PowerManager.SetModule(module);
        }

        internal bool IsModuleRemovable()
        {
            //TODO Set the Condition for this to work
            return true;
        }
    }
}
