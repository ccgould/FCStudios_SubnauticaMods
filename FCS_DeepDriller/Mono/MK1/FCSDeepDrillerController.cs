using System;
using System.Collections;
using System.Collections.Generic;
using FCS_DeepDriller.Attachments.MK1;
using FCS_DeepDriller.Buildable.MK1;
using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Enumerators;
using FCS_DeepDriller.Helpers;
using FCS_DeepDriller.Managers;
using FCSCommon.Enums;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_DeepDriller.Mono.MK1
{
    [RequireComponent(typeof(WeldablePoint))]
    internal class FCSDeepDrillerController : MonoBehaviour, IConstructable, IProtoEventListener
    {
        #region Private Members
        private DeepDrillerSaveDataEntry _saveData;
        private Constructable _buildable;
        private PrefabIdentifier _prefabId;
        private BatteryAttachment _batteryAttachment;
        private List<TechType> _bioData = new List<TechType>();
        private bool _sendToExStorage;
        private bool _invalidPlacement;
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private DeepDrillerSaveDataEntry _data;
        private string _currentBiome;

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
        internal FCSDeepDrillerHealthHandler HealthManager { get; private set; }
        internal int ExtendStateHash { get; private set; }
        internal int ShaftStateHash { get; private set; }
        internal int BitSpinState { get; private set; }
        internal int BitDamageState { get; private set; }
        internal int ScreenStateHash { get; private set; }
        internal BatteryAttachment BatteryController { get; private set; }
        internal OreGenerator OreGenerator { get; private set; }
        internal bool IsInitialized { get; set; }
        internal DeepDrillerComponentManager ComponentManager { get; private set; }

#if USE_ExStorageDepot
        internal ExStorageDepotController ExStorageDepotController { get; set; }
#endif

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            if (IsBeingDeleted) return;

            if (_runStartUpOnEnable)
            {

                if (!IsInitialized)
                {
                    Initialize();
                }

                if (_data == null)
                {
                    ReadySaveData();
                }

                if (_fromSave)
                {
                    DeepDrillerModuleContainer.SetModules(_data.Modules);
                    DeepDrillerContainer.LoadItems(_data.Items);

                    if (QPatch.Configuration.AllowDamage)
                    {
                        StartCoroutine(SetHeath());
                    }

                    PowerManager.LoadData(_data);

                    if (_data.IsFocused)
                    {
                        OreGenerator.SetIsFocus(_data.IsFocused);
                    }

                    _batteryAttachment.GetController().LoadData(_data.PowerData);

                    _currentBiome = _data.Biome;

                    _fromSave = false;
                }
                
                StartCoroutine(DropLegs());
                StartCoroutine(TryGetLoot());
                _runStartUpOnEnable = false;
            }
        }

        private IEnumerator SetHeath()
        {
            while (HealthManager.GetHealth() != _data.Health)
            {
                HealthManager?.SetHealth(_data.Health);
                yield return null;
            }
        }

        #endregion

        #region IConstructable
        public bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;

            if (IsInitialized == false)
            {
                return true;
            }

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

            if (constructed)
            {

                var seaBase = gameObject?.transform?.parent?.gameObject;

                if (seaBase != null)
                {
                    QuickLogger.Debug($"Base Name: {seaBase.name}", true);
                    if (seaBase.name.StartsWith("Base", StringComparison.OrdinalIgnoreCase))
                    {
                        QuickLogger.Debug("Is a base");
                        _invalidPlacement = true;
                    }
                }

                if (isActiveAndEnabled)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                    }

                    _currentBiome = BiomeManager.GetBiome();
                    StartCoroutine(DropLegs());
                    StartCoroutine(TryGetLoot());

                }
                else
                {
                    _runStartUpOnEnable = true;
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

            if (QPatch.Configuration.AllowDamage)
            {
                _saveData.Health = HealthManager.GetHealth();
            }

            _saveData.PowerData = PowerManager.SaveData();
            _saveData.FocusOre = OreGenerator.GetFocus();
            _saveData.IsFocused = OreGenerator.GetIsFocused();
            _saveData.Biome = _currentBiome;
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
            _fromSave = true;
        }
        
        #endregion

        #region Private Methods
        private void Initialize()
        {
            QuickLogger.Debug($"Initializing");

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

            _prefabId = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();

            if (_prefabId == null)
            {
                QuickLogger.Error("Prefab Identifier Component was not found");
            }

            if (_buildable == null)
            {
                _buildable = GetComponentInParent<Constructable>();
            }

            PowerManager = gameObject.AddComponent<FCSDeepDrillerPowerHandler>();
            PowerManager.Initialize(this);
            PowerManager.OnPowerUpdate += OnPowerUpdate;

            if (QPatch.Configuration.AllowDamage)
            {
                HealthManager = gameObject.AddComponent<FCSDeepDrillerHealthHandler>();
                HealthManager.Initialize(this);
                HealthManager.SetHealth(100);
                HealthManager.OnDamaged += OnDamaged;
                HealthManager.OnRepaired += OnRepaired;
                QuickLogger.Debug($"=============================================== Made Health {HealthManager.GetHealth()}=============================");
            }


            OreGenerator = gameObject.AddComponent<OreGenerator>();
            OreGenerator.Initialize(this);
            OreGenerator.OnAddCreated += OreGeneratorOnAddCreated;

            DeepDrillerContainer = new FCSDeepDrillerContainer();
            DeepDrillerContainer.Setup(this);

            DeepDrillerModuleContainer = new FCSDeepDrillerModuleContainer();
            DeepDrillerModuleContainer.Setup(this);

            AnimationHandler = gameObject.AddComponent<FCSDeepDrillerAnimationHandler>();
            AnimationHandler.Initialize(this);

            LavaPitHandler = gameObject.AddComponent<FCSDeepDrillerLavaPitHandler>();
            LavaPitHandler.Initialize(this);

            UpdateSystemLights(PowerManager.GetPowerState());

            IsInitialized = true;

            QuickLogger.Debug($"Initializing Completed");
        }

        private void UpdateLegState(bool isExtended)
        {
            AnimationHandler.SetBoolHash(ExtendStateHash, isExtended);
        }

        private void OnDamaged()
        {
            QuickLogger.Debug("OnDamaged", true);
            AnimationHandler.SetBoolHash(BitDamageState, true);
            AnimationHandler.SetIntHash(ShaftStateHash, 2);
            UpdateSystemLights(PowerManager.GetPowerState());
        }

        private void OnRepaired()
        {
            QuickLogger.Debug("OnRepaired", true);

            AnimationHandler.SetBoolHash(BitDamageState, false);

            if (PowerManager.GetPowerState() == FCSPowerStates.Tripped) return;

            UpdateSystemLights(PowerManager.GetPowerState());

            if (isActiveAndEnabled)
            {
                StartCoroutine(DropLegs());
            }
            else
            {
                _runStartUpOnEnable = true;
            }
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

            if (QPatch.Configuration.AllowDamage)
            {
                if (HealthManager.IsDamagedFlag())
                {
                    //MaterialHelpers.ChangeEmissionColor("DeepDriller_BaseColor_BaseColor",gameObject, new Color(1, 1f, 1f));
                    MaterialHelpers.ReplaceEmissionTexture("DeepDriller_BaseColor_BaseColor", "DeepDriller_Emissive_Error", gameObject, QPatch.GlobalBundle);
                    return;
                }
                if (value == FCSPowerStates.Unpowered || value == FCSPowerStates.Tripped && !HealthManager.IsDamagedFlag())
                {
                    //MaterialHelpers.ChangeEmissionColor("DeepDriller_BaseColor_BaseColor", gameObject, new Color(0.9803922f, 0.6313726f, 0.007843138f));

                    MaterialHelpers.ReplaceEmissionTexture("DeepDriller_BaseColor_BaseColor", "DeepDriller_Emissive_Off",
                        gameObject, QPatch.GlobalBundle);
                }
                else if (value == FCSPowerStates.Powered && !HealthManager.IsDamagedFlag())
                    //MaterialHelpers.ChangeEmissionColor("DeepDriller_BaseColor_BaseColor", gameObject, new Color(0.08235294f, 1f, 1f));

                    MaterialHelpers.ReplaceEmissionTexture("DeepDriller_BaseColor_BaseColor", "DeepDriller_Emissive_On",
                        gameObject, QPatch.GlobalBundle);
            }
            else
            {
                if (value == FCSPowerStates.Unpowered || value == FCSPowerStates.Tripped)
                {
                    MaterialHelpers.ReplaceEmissionTexture("DeepDriller_BaseColor_BaseColor", "DeepDriller_Emissive_Off",
                        gameObject, QPatch.GlobalBundle);
                }
                else if (value == FCSPowerStates.Powered)
                    //MaterialHelpers.ChangeEmissionColor("DeepDriller_BaseColor_BaseColor", gameObject, new Color(0.08235294f, 1f, 1f));

                    MaterialHelpers.ReplaceEmissionTexture("DeepDriller_BaseColor_BaseColor", "DeepDriller_Emissive_On",
                        gameObject, QPatch.GlobalBundle);
            }

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

            if (_sendToExStorage)
            {
                DeepDrillerContainer.SendToExStorage(type.ToInventoryItem());
            }
            else
            {
                DeepDrillerContainer.AddItem(type.ToPickupable());
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _data = Mod.GetDeepDrillerSaveData(id);
        }

        private IEnumerator TryGetLoot()
        {
            QuickLogger.Debug("In TryGetLoot");

            if (OreGenerator == null)
            {
                QuickLogger.Error("OreGenerator is null");
                yield return null;
            }

            while (OreGenerator.AllowedOres.Count <= 0)
            {
                var loot = GetBiomeData(_currentBiome,transform);

                if (loot != null)
                {
                    OreGenerator.AllowedOres = loot;
                    ConnectDisplay();
                }

                yield return null;
            }
        }
        
        private void ConnectDisplay()
        {
            if (DisplayHandler != null) return;
            QuickLogger.Debug($"Creating Display");
            DisplayHandler = gameObject.AddComponent<FCSDeepDrillerDisplay>();
            DisplayHandler.Setup(this);

            //TODO: SetFocus Ore
            DisplayHandler.UpdateListItems(_data?.FocusOre ?? TechType.None);
        }

        #endregion

        #region Internal Methods
        internal void PowerOffDrill()
        {
            if (PowerManager.GetPowerState() == FCSPowerStates.Tripped) return;
            UpdateLegState(false);
            PowerManager.SetPowerState(FCSPowerStates.Tripped);
        }

        internal bool IsInvalidPlacement()
        {
            return _invalidPlacement;
        }

        internal void PowerOnDrill()
        {
            if (_invalidPlacement) return;
            
            if (QPatch.Configuration.AllowDamage)
            {
                if (HealthManager.IsDamagedFlag())
                {
                    return;
                }
            }

            if (!PowerManager.IsPowerAvailable())
            {
                return;
            }
            
            PowerManager.SetPowerState(FCSPowerStates.Powered);

            if (DisplayHandler != null)
            {
                DisplayHandler.UpdateListItems(GetFocusedOre());
            }
        }

        internal IEnumerator DropLegs()
        {
            QuickLogger.Debug("Attempting to Extend legs");

            while (!AnimationHandler.GetBoolHash(ExtendStateHash))
            {
                UpdateLegState(true);
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

        internal List<TechType> GetBiomeData(string biome = null, Transform tr = null)
        {
            if (_bioData?.Count <= 0 && tr != null || biome != null)
            {
                var data = BiomeManager.FindBiomeLoot(tr,biome);

                if (data != null)
                {
                    _bioData = data;
                }
            }

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
