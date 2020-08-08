using System;
using System.Collections;
using System.Collections.Generic;
using FCS_DeepDriller.Buildable.MK1;
using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Managers;
using FCS_DeepDriller.Mono.MK1;
using FCSCommon.Controllers;
using FCSCommon.Enums;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Extensions;
using FCSTechFabricator.Managers;
using UnityEngine;

namespace FCS_DeepDriller.Mono.MK2
{
    [RequireComponent(typeof(WeldablePoint))]
    internal class FCSDeepDrillerController : MonoBehaviour, IConstructable, IProtoEventListener
    {
        #region Private Members
        private DeepDrillerSaveDataEntry _saveData;
        private Constructable _buildable;
        private PrefabIdentifier _prefabId;
        private List<TechType> _bioData = new List<TechType>();
        private bool _sendToExStorage;
        private bool _invalidPlacement;
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private DeepDrillerSaveDataEntry _data;
        private string _currentBiome;
        private GameObject _laser;

        internal string CurrentBiome
        {
            get => _currentBiome;
            set => _currentBiome = value;
        }


        #endregion

        #region Internal Properties
        internal bool IsBeingDeleted { get; set; }
        internal FCSDeepDrillerAnimationHandler AnimationHandler { get; private set; }
        internal FCSDeepDrillerLavaPitHandler LavaPitHandler { get; private set; }
        internal FCSDeepDrillerContainer DeepDrillerContainer { get; private set; }
        internal bool IsConstructed { get; private set; }  //=> _buildable != null && _buildable.constructed;
        internal AudioManager AudioManager { get; private set; }
        internal FCSDeepDrillerPowerHandler PowerManager { get; private set; }
        internal FCSDeepDrillerDisplay DisplayHandler { get; private set; }
        internal FCSDeepDrillerHealthHandler HealthManager { get; private set; }
        internal int ExtendStateHash { get; private set; }
        internal int ShaftStateHash { get; private set; }
        internal int BitSpinState { get; private set; }
        internal int BitDamageState { get; private set; }
        internal int ScreenStateHash { get; private set; }
        internal int SolarStateHash { get; private set; }
        internal OreGenerator OreGenerator { get; private set; }
        internal bool IsInitialized { get; set; }
        internal FCSDeepDrillerOilHandler OilHandler { get; set; }
        internal LaserManager LaserManager { get; private set; }
        internal DumpContainer PowercellDumpContainer { get; set; }
        internal DumpContainer OilDumpContainer { get; set; }
        public PowerRelay PowerRelay { get; private set; }
        public ColorManager ColorManager { get; private set; }
        public UpgradeManager UpgradeManager { get; private set; }

#if USE_ExStorageDepot
        internal ExStorageDepotController ExStorageDepotController { get; set; }
#endif

        #endregion

        #region Unity Methods

        private void OnDestroy()
        {
            IsBeingDeleted = true;
        }

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

                if (_fromSave && _data != null)
                {
                    if (QPatch.Configuration.AllowDamage)
                    {
                        StartCoroutine(SetHeath());
                    }

                    PowerManager.LoadData(_data);
                    DeepDrillerContainer.LoadData(_data.Items);
                    if (_data.IsFocused)
                    {
                        OreGenerator.SetIsFocus(_data.IsFocused);
                        OreGenerator.Load(_data.FocusOres);
                    }

                    ColorManager.SetMaskColorFromSave(_data.BodyColor.Vector4ToColor());
                    CurrentBiome = _data.Biome;
                    OilHandler.SetOilTimeLeft(_data.OilTimeLeft);
                    
                    _fromSave = false;
                }

                var powerRelay = gameObject.AddComponent<PowerRelay>();
                PowerManager.SetPowerRelay(powerRelay);

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

            if (DeepDrillerContainer.HasItems())
            {
                reason = FCSDeepDrillerBuildable.RemoveAllItems();
                return false;
            }

            return true;
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

                    CurrentBiome = BiomeManager.GetBiome();
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
            _saveData.BodyColor = ColorManager.GetMaskColor().ColorToVector4();
            _saveData.PowerState = PowerManager.GetPowerState();
            _saveData.PullFromRelay = PowerManager.GetPullFromPowerRelay();
            _saveData.Items = DeepDrillerContainer.SaveData();

            if (QPatch.Configuration.AllowDamage)
            {
                _saveData.Health = HealthManager.GetHealth();
            }

            _saveData.PowerData = PowerManager.SaveData();
            _saveData.FocusOres = OreGenerator.GetFocuses();
            _saveData.IsFocused = OreGenerator.GetIsFocused();
            _saveData.Biome = CurrentBiome;
            _saveData.OilTimeLeft = OilHandler.GetOilTimeLeft();
            _saveData.SolarExtended = PowerManager.IsSolarExtended();
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

            ExtendStateHash = Animator.StringToHash("LegState");

            ShaftStateHash = Animator.StringToHash("ShaftState");

            ScreenStateHash = Animator.StringToHash("ScreenState");

            BitSpinState = Animator.StringToHash("BitSpinState");

            BitDamageState = Animator.StringToHash("BitDamageState");

            SolarStateHash = Animator.StringToHash("SolarState");
            
            _prefabId = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();

            InvokeRepeating(nameof(UpdateDrillShaftSate),1,1);

            if (OilHandler == null)
            {
                OilHandler = gameObject.AddComponent<FCSDeepDrillerOilHandler>();
                OilHandler.Initialize(this);
            }

            if (_prefabId == null)
            {
                QuickLogger.Error("Prefab Identifier Component was not found");
            }

            if (_buildable == null)
            {
                _buildable = GetComponentInParent<Constructable>();
            }

            OreGenerator = gameObject.AddComponent<OreGenerator>();
            OreGenerator.Initialize(this);
            OreGenerator.OnAddCreated += OreGeneratorOnAddCreated;


            PowerManager = gameObject.AddComponent<FCSDeepDrillerPowerHandler>();
            PowerManager.Initialize(this);
            PowerManager.OnPowerUpdate += OnPowerUpdate;

            if (LaserManager == null)
            {
                LaserManager = new LaserManager();
                LaserManager.Setup(this);
            }

            if (ColorManager == null)
            {
                ColorManager = gameObject.AddComponent<ColorManager>();
                ColorManager.Initialize(gameObject, FCSDeepDrillerBuildable.BodyMaterial);
            }

            AudioManager = new AudioManager(gameObject.GetComponent<FMOD_CustomLoopingEmitter>());
            
            HealthManager = gameObject.AddComponent<FCSDeepDrillerHealthHandler>();
            HealthManager.Initialize(this);
            HealthManager.SetHealth(100);
            HealthManager.OnDamaged += OnDamaged;
            HealthManager.OnRepaired += OnRepaired;
            
            DeepDrillerContainer = new FCSDeepDrillerContainer();
            DeepDrillerContainer.Setup(this);


            AnimationHandler = gameObject.AddComponent<FCSDeepDrillerAnimationHandler>();
            AnimationHandler.Initialize(this);

            LavaPitHandler = gameObject.AddComponent<FCSDeepDrillerLavaPitHandler>();
            LavaPitHandler.Initialize(this);

            if (OilDumpContainer == null)
            {
                OilDumpContainer = gameObject.AddComponent<DumpContainer>();
                OilDumpContainer.Initialize(transform, 
                    FCSDeepDrillerBuildable.OilDropContainerTitle(), 
                    FCSDeepDrillerBuildable.NotAllowedItem(), 
                    FCSDeepDrillerBuildable.StorageFull(), 
                    OilHandler, 4, 4);
            }

            if (PowercellDumpContainer == null)
            {
                PowercellDumpContainer = gameObject.AddComponent<DumpContainer>();
                PowercellDumpContainer.Initialize(transform,
                    FCSDeepDrillerBuildable.PowercellDumpContainerTitle(),
                    FCSDeepDrillerBuildable.NotAllowedItem(),
                    FCSDeepDrillerBuildable.StorageFull(),
                    PowerManager, 1, 1);
            }

            if(UpgradeManager == null)
            {
                UpgradeManager = gameObject.AddComponent<UpgradeManager>();
                UpgradeManager.Initialize(this);
            }


            UpdateSystemLights(PowerManager.GetPowerState());

            IsInitialized = true;

            QuickLogger.Debug($"Initializing Completed");
        }



        private void UpdateLegState(bool isExtended)
        {
            AnimationHandler.SetIntHash(ExtendStateHash, isExtended ? 1 : 2);
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

        private void OnPowerUpdate(FCSPowerStates value)
        {
            UpdateSystemLights(value);
            QuickLogger.Debug($"PowerState Changed to: {value}", true);
        }

        private void UpdateSystemLights(FCSPowerStates value)
        {
            return;
            QuickLogger.Debug($"Changing System Lights", true);

            if (QPatch.Configuration.AllowDamage)
            {
                if (HealthManager.IsDamagedFlag())
                {
                    //MaterialHelpers.ChangeEmissionColor("DeepDriller_BaseColor_BaseColor",gameObject, new Color(1, 1f, 1f));
                    MaterialHelpers.ReplaceEmissionTexture("DeepDriller_BaseColor_BaseColor", "DeepDriller_Emissive_Error", gameObject, FCSDeepDrillerBuildable.AssetBundle);
                    return;
                }
                if (value == FCSPowerStates.Unpowered || value == FCSPowerStates.Tripped && !HealthManager.IsDamagedFlag())
                {
                    //MaterialHelpers.ChangeEmissionColor("DeepDriller_BaseColor_BaseColor", gameObject, new Color(0.9803922f, 0.6313726f, 0.007843138f));

                    MaterialHelpers.ReplaceEmissionTexture("DeepDriller_BaseColor_BaseColor", "DeepDriller_Emissive_Off",
                        gameObject, FCSDeepDrillerBuildable.AssetBundle);
                }
                else if (value == FCSPowerStates.Powered && !HealthManager.IsDamagedFlag())
                    //MaterialHelpers.ChangeEmissionColor("DeepDriller_BaseColor_BaseColor", gameObject, new Color(0.08235294f, 1f, 1f));

                    MaterialHelpers.ReplaceEmissionTexture("DeepDriller_BaseColor_BaseColor", "DeepDriller_Emissive_On",
                        gameObject, FCSDeepDrillerBuildable.AssetBundle);
            }
            else
            {
                if (value == FCSPowerStates.Unpowered || value == FCSPowerStates.Tripped)
                {
                    MaterialHelpers.ReplaceEmissionTexture("DeepDriller_BaseColor_BaseColor", "DeepDriller_Emissive_Off",
                        gameObject, FCSDeepDrillerBuildable.AssetBundle);
                }
                else if (value == FCSPowerStates.Powered)
                    //MaterialHelpers.ChangeEmissionColor("DeepDriller_BaseColor_BaseColor", gameObject, new Color(0.08235294f, 1f, 1f));

                    MaterialHelpers.ReplaceEmissionTexture("DeepDriller_BaseColor_BaseColor", "DeepDriller_Emissive_On",
                        gameObject, FCSDeepDrillerBuildable.AssetBundle);
            }
        }

        private void UpdateDrillShaftSate()
        {
            if(IsOperational())
            {
                LasersVisibility();
                AnimationHandler.DrillState(true);
                AudioManager.PlayAudio();
            }
            else
            {
                LasersVisibility(false);
                AnimationHandler.DrillState(false);
                AudioManager.StopAudio();
            }
        }

        private void LasersVisibility(bool isVisible = true)
        {
            if (isVisible)
            {
                LaserManager.ChangeLasersState();
            }
            else
            {
                LaserManager.ChangeLasersState(false);
            }
        }

        private void OreGeneratorOnAddCreated(TechType type)
        {
            //TODO Fill in code for  OreGeneratorOnAddCreated(TechType type) to send items
            DeepDrillerContainer.AddItemToContainer(type);
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
                var loot = GetBiomeData(CurrentBiome, transform);

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
            DisplayHandler.UpdateBiome(CurrentBiome);
            DisplayHandler.OnIsFocusedChanged(_data.IsFocused);
            DisplayHandler.UpdateListItemsState(_data?.FocusOres ?? new HashSet<TechType>());
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
                DisplayHandler.UpdateListItemsState(GetFocusedOres());
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

        internal void SetOreFocus(TechType techType)
        {
            OreGenerator.AddFocus(techType);
        }

        internal bool GetFocusedState()
        {
            return OreGenerator.GetIsFocused();
        }

        internal HashSet<TechType> GetFocusedOres()
        {
            return OreGenerator.GetFocusedOres();
        }

        internal bool IsOperational()
        {
            return PowerManager.IsPowerAvailable() && 
                   PowerManager.GetPowerState() == FCSPowerStates.Powered &&
                   OilHandler.HasOil() && 
                   !HealthManager.IsDamagedFlag();
        }

        #endregion
    }
}
