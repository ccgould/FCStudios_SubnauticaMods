using System;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.DeepDriller.Buildable;
using FCS_ProductionSolutions.DeepDriller.Managers;
using FCS_ProductionSolutions.DeepDriller.Models;
using FCSCommon.Controllers;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_ProductionSolutions.Buildable;
using UnityEngine;


namespace FCS_ProductionSolutions.DeepDriller.Mono
{
    internal class FCSDeepDrillerController : FcsDevice, IFCSSave<SaveData>, IHandTarget
    {
        #region Private Members

        private DeepDrillerSaveDataEntry _saveData;
        private Constructable _buildable;
        private List<TechType> _bioData = new List<TechType>();
        private bool _runStartUpOnEnable;
        private bool _allPlateFormsFound;
        private bool PlatformLegsExtended;
        private const int Segments = 50;
        private LineRenderer _line;
        private const float LerpSpeed = 10f;
        private bool _isRangeVisible;
        private float _currentDistance;
        private bool _noBiomeMessageSent;
        private bool _biomeFoundMessageSent;
        private List<PistonBobbing> _pistons = new List<PistonBobbing>();
        private StringBuilder _sb;
        private bool _isBreakSet;
        private AudioSource _audio;
        private bool _wasPlaying;
        private AudioLowPassFilter _lowPassFilter;

        internal string CurrentBiome { get; set; }
        internal bool IsFromSave { get; set; }

        #endregion

        #region Internal Properties

        public override bool CanBeSeenByTransceiver { get; set; }
        internal bool IsBeingDeleted { get; set; }
        internal FCSDeepDrillerAnimationHandler AnimationHandler { get; private set; }
        internal FCSDeepDrillerLavaPitHandler LavaPitHandler { get; private set; }
        internal FCSDeepDrillerContainer DeepDrillerContainer { get; private set; }
        public override bool IsConstructed { get; set; }
        public  FCSDeepDrillerPowerHandler DeepDrillerPowerManager { get;  set; }
        internal FCSDeepDrillerDisplay DisplayHandler { get; private set; }
        internal FCSDeepDrillerOreGenerator OreGenerator { get; private set; }
        internal FCSDeepDrillerOilHandler OilHandler { get; set; }
        internal DumpContainer PowercellDumpContainer { get; set; }
        internal DumpContainer OilDumpContainer { get; set; }
        public FCSDeepDrillerUpgradeManager UpgradeManager { get; private set; }
        public FCSDeepDrillerTransferManager TransferManager { get; set; }
        public override bool IsVisible => IsConstructed && IsInitialized;
        public override int MaxItemAllowForTransfer { get; } = 6;
        public override TechType[] AllowedTransferItems { get; } =
        {
            TechType.Lubricant
        };

        #endregion

        #region Unity Methods

        private void Start()
        {
            DisplayHandler?.UpdateUnitID();
            UpdateEmission();
        }

        public override void OnDestroy()
        {
            Manager?.UnRegisterDevice(this);
            base.OnDestroy();
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

                if (_saveData == null)
                {
                    ReadySaveData();
                }

                if (IsFromSave && _saveData != null)
                {
                    DeepDrillerPowerManager.LoadData(_saveData);

                    DeepDrillerContainer.LoadData(_saveData.Items);
                    if (_saveData.IsFocused)
                    {
                        OreGenerator.SetIsFocus(_saveData.IsFocused);
                        OreGenerator.Load(_saveData.FocusOres);
                    }
                    FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.DeepDrillerMk3TabID, Mod.ModName);
                    _colorManager.ChangeColor(_saveData.Body.Vector4ToColor());
                    _colorManager.ChangeColor(_saveData.Sec.Vector4ToColor(), ColorTargetMode.Secondary);
                    CurrentBiome = _saveData.Biome;
                    OilHandler.SetOilTimeLeft(_saveData.OilTimeLeft);
                    
                    UpgradeManager.Load(_saveData?.Upgrades);
                    OreGenerator.SetBlackListMode(_saveData.IsBlackListMode);
                    _isRangeVisible = _saveData.IsRangeVisible;
                    _isBreakSet = _saveData.IsBrakeSet;
                }

                StartCoroutine(TryGetLoot());
                _runStartUpOnEnable = false;
            }
        }

        private void Update()
        {
            if (_line == null) return;
            if (_isRangeVisible)
            {
                CreatePoints(Mathf.Clamp(_currentDistance + DayNightCycle.main.deltaTime * LerpSpeed * 1, 0, QPatch.Configuration.DDDrillAlterraStorageRange));
            }
            else
            {
                for (int i = 0; i < _line.positionCount; i++)
                {
                    if (_line.GetPosition(i) != Vector3.zero)
                        _line.SetPosition(i, Vector3.zero);
                    _currentDistance = 0f;
                }
            }

            if (_audio != null && _audio.isPlaying)
            {

                if (_audio.isPlaying && Mathf.Approximately(Time.timeScale, 0f))
                {
                    _audio.Pause();
                    _wasPlaying = true;
                }

                if (_wasPlaying && Time.timeScale > 0)
                {
                    _audio.Play();
                    _wasPlaying = false;
                }
            }

            if (_lowPassFilter != null)
            {
                _lowPassFilter.cutoffFrequency = Player.main.IsUnderwater() || 
                                                 Player.main.IsInBase() || 
                                                 Player.main.IsInSub() ||
                                                 Player.main.inSeamoth || 
                                                 Player.main.inExosuit ? 1566f : 22000f;
            }
        }

        #endregion

        #region IConstructable
        public override bool CanDeconstruct(out string reason)
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

        public override void OnConstructedChanged(bool constructed)
        { 
            QuickLogger.Info("In Constructed Changed");

            IsConstructed = constructed;

            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                    }

                    CurrentBiome = BiomeManager.GetBiome();
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

        public void Save(SaveData saveDataList, ProtobufSerializer serializer = null)
        {
            if (!IsInitialized || _colorManager == null || DeepDrillerPowerManager == null ||
                DeepDrillerContainer == null || OreGenerator == null || OilHandler == null || 
                UpgradeManager == null || TransferManager == null)
            {
                QuickLogger.Error($"Failed to save driller {GetPrefabID()}");
                return;
            }

            if (_saveData == null)
            {
                _saveData = new DeepDrillerSaveDataEntry();
            }


            QuickLogger.Message($"SaveData = {_saveData}", true);

            _saveData.Id = GetPrefabID();
            _saveData.Body = _colorManager.GetColor().ColorToVector4();
            _saveData.Sec = _colorManager.GetSecondaryColor().ColorToVector4();

            _saveData.PowerState = DeepDrillerPowerManager.GetPowerState();
            _saveData.PullFromRelay = DeepDrillerPowerManager.GetPullFromPowerRelay();
            _saveData.PowerData = DeepDrillerPowerManager.SaveData();

            _saveData.Items = DeepDrillerContainer.SaveData();

            _saveData.FocusOres = OreGenerator.GetFocuses();
            _saveData.IsFocused = OreGenerator.GetIsFocused();
            _saveData.IsBlackListMode = OreGenerator.GetInBlackListMode();

            _saveData.Biome = CurrentBiome;

            _saveData.OilTimeLeft = OilHandler.GetOilTimeLeft();

            _saveData.Upgrades = UpgradeManager.Save();

            _saveData.IsRangeVisible = _isRangeVisible;

            _saveData.IsBrakeSet = _isBreakSet;

            _saveData.AllowedToExport = TransferManager.IsAllowedToExport();
            saveDataList.DeepDrillerMk2Entries.Add(_saveData);

        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info("Saving Drills");
                Mod.Save(serializer);
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            IsFromSave = true;
        }

        #endregion

        #region Private Methods
        
        public override void Initialize()
        {
            QuickLogger.Debug($"Initializing");

            _sb = new StringBuilder();

            _audio = gameObject.GetComponent<AudioSource>();
            _lowPassFilter = gameObject.GetComponent<AudioLowPassFilter>();



            //InvokeRepeating(nameof(UpdateDrillShaftSate), 1, 1);
            //foreach (Transform child in gameObject.transform.FirstOrDefault(x => x.name == "hover_pistons_pumps").transform)
            //{
                
            //}
            _pistons.Add(gameObject.transform.FirstOrDefault(x => x.name == "hover_piston_pump_01").gameObject.AddComponent<PistonBobbing>());
            _pistons.Add(gameObject.transform.FirstOrDefault(x => x.name == "hover_piston_pump_02").gameObject.AddComponent<PistonBobbing>());
            _pistons.Add(gameObject.transform.FirstOrDefault(x => x.name == "hover_piston_pump_03").gameObject.AddComponent<PistonBobbing>());
            _pistons.Add(gameObject.transform.FirstOrDefault(x => x.name == "hover_piston_pump_04").gameObject.AddComponent<PistonBobbing>());
            _pistons.Add(gameObject.transform.FirstOrDefault(x => x.name == "hover_piston_pump_05").gameObject.AddComponent<PistonBobbing>());
            _pistons.Add(gameObject.transform.FirstOrDefault(x => x.name == "hover_piston_pump_06").gameObject.AddComponent<PistonBobbing>());

            _pistons[1].Invert = true;
            _pistons[3].Invert = true;
            _pistons[5].Invert = true;



            if (OilHandler == null)
            {
                OilHandler = gameObject.AddComponent<FCSDeepDrillerOilHandler>();
                OilHandler.Initialize(this);
            }

            if (_buildable == null)
            {
                _buildable = GetComponentInParent<Constructable>();
            }

            OreGenerator = gameObject.AddComponent<FCSDeepDrillerOreGenerator>();
            OreGenerator.Initialize(this);
            OreGenerator.OnAddCreated += OreGeneratorOnAddCreated;

            if (DeepDrillerPowerManager == null)
            {
                DeepDrillerPowerManager = gameObject.AddComponent<FCSDeepDrillerPowerHandler>();
                DeepDrillerPowerManager.Initialize(this);
                var powerRelay = gameObject.AddComponent<PowerRelay>();
                DeepDrillerPowerManager.SetPowerRelay(powerRelay);
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial,ModelPrefab.SecondaryMaterial);
            }

            DeepDrillerContainer = new FCSDeepDrillerContainer();
            DeepDrillerContainer.Setup(this);


            AnimationHandler = gameObject.AddComponent<FCSDeepDrillerAnimationHandler>();
            AnimationHandler.Initialize(this);

            //LavaPitHandler = gameObject.AddComponent<FCSDeepDrillerLavaPitHandler>();
            //LavaPitHandler.Initialize(this);

            if (OilDumpContainer == null)
            {
                OilDumpContainer = gameObject.AddComponent<DumpContainer>();
                OilDumpContainer.Initialize(transform,
                    FCSDeepDrillerBuildable.OilDropContainerTitle(),
                    OilHandler, 4, 4);
            }

            if (PowercellDumpContainer == null)
            {
                PowercellDumpContainer = gameObject.AddComponent<DumpContainer>();
                PowercellDumpContainer.Initialize(transform,
                    FCSDeepDrillerBuildable.PowercellDumpContainerTitle(),
                    DeepDrillerPowerManager, 1, 1);
            }

            if (TransferManager == null)
            {
                TransferManager = gameObject.AddComponent<FCSDeepDrillerTransferManager>();
                TransferManager.Initialize(this);
            }

            if (UpgradeManager == null)
            {
                UpgradeManager = gameObject.AddComponent<FCSDeepDrillerUpgradeManager>();
                UpgradeManager.Initialize(this);
            }
            
            _line = gameObject.GetComponent<LineRenderer>();
            _line.SetVertexCount(Segments + 1);
            _line.useWorldSpace = false;

            OnGenerate();

            InvokeRepeating(nameof(UpdateDrillShaftState), .5f, .5f);

            IsInitialized = true;

            QuickLogger.Debug($"Initializing Completed");
        }

        private void UpdateDrillShaftState()
        {
            if(!IsConstructed || !IsInitialized) return;

            if (OreGenerator.GetIsDrilling())
            {
                ChangePistonState();
                AnimationHandler.DrillState(true);
                if(!_audio.isPlaying)
                    _audio.Play();
            }
            else
            {
                ChangePistonState(false);
                AnimationHandler.DrillState(false);
                if(_audio.isPlaying)
                    _audio.Stop();
            }
        }

        private void ChangePistonState(bool isRunning = true)
        {
            foreach (var piston in _pistons)
            {
                piston.SetState(isRunning);
            }
        }

        private void OreGeneratorOnAddCreated(TechType type)
        {
            //if (TransferManager.IsAllowedToExport())
            //{
            //    var result = TransferManager.TransferToExStorage(type);
            //    if (result)
            //    {
            //        return;
            //    }
            //}
            DeepDrillerContainer.AddItemToContainer(type);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetDeepDrillerMK2SaveData(id);
        }
        
        private IEnumerator TryGetLoot()
        {
            QuickLogger.Debug("In TryGetLoot");

            if (OreGenerator == null)
            {
                QuickLogger.Error("OreGenerator is null");
                yield return null;
            }


            while (OreGenerator?.AllowedOres.Count <= 0)
            {
                if (string.IsNullOrEmpty(CurrentBiome))
                {
                    if (!_noBiomeMessageSent)
                    {
                        QuickLogger.Info($"No biome Found trying to find biome");
                        _noBiomeMessageSent = true;
                    }

                    CurrentBiome = BiomeManager.GetBiome(gameObject.transform);
                }
                else
                {
                    if (!_biomeFoundMessageSent)
                    {
                        QuickLogger.Info($"biome Found: {CurrentBiome}");
                        _biomeFoundMessageSent = true;
                    }

                    var loot = GetBiomeData(CurrentBiome, transform);

                    if (loot != null)
                    {
                        OreGenerator.AllowedOres = loot;
                        ConnectDisplay();
                    }
                }
                yield return null;
            }
            yield return 0;
        }

        private void ConnectDisplay()
        {
            if (DisplayHandler != null) return;
            QuickLogger.Debug($"Creating Display");
            DisplayHandler = gameObject.AddComponent<FCSDeepDrillerDisplay>();
            DisplayHandler.Setup(this);
            DisplayHandler.RefreshStorageAmount();
            DisplayHandler.UpdateListItemsState(_saveData?.FocusOres ?? new HashSet<TechType>());
            DisplayHandler?.UpdateUnitID();

            if (_saveData != null)
            {
                if (_saveData.AllowedToExport)
                {
                    TransferManager?.Toggle();
                }

                if (_saveData != null)
                {
                    DisplayHandler.LoadFromSave(_saveData);
                }
            }
            
        }

        private void OnGenerate()
        {
            if (PlatformLegsExtended) return;

            QuickLogger.Debug("OnGenerate", true);
            if (!_allPlateFormsFound)
            {
                var pillar6 = GameObjectHelpers.FindGameObject(gameObject, "DrillSupportLeg")?.AddComponent<Pillar>();
                pillar6?.Instantiate(this, true);
                _allPlateFormsFound = true;
            }
        }
        
        private void CreatePoints(float radius)
        {
            _currentDistance = radius;
            float x;
            float y;
            float z;

            float angle = 20;

            for (int i = 0; i < (Segments + 1); i++)
            {
                x = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
                z = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;

                _line.SetPosition(i, new Vector3(x, 0, z));


                angle += (360f / Segments);
            }
        }
        
        private void UpdateEmission()
        {
            MaterialHelpers.ChangeEmissionColor(ModelPrefab.EmissionControllerMaterial, gameObject,
                _isBreakSet ? Color.red : Color.cyan);
        }

        #endregion

        #region Internal Methods

        public override bool IsUnderWater()
        {
            return GetDepth() >= 0.63f;
        }

        internal float GetDepth()
        {
#if SUBNAUTICA
            return gameObject == null ? 0f : Ocean.main.GetDepthOf(gameObject);
#elif BELOWZERO
            return gameObject == null ? 0f : Ocean.GetDepthOf(gameObject);
#endif
        }

        internal List<TechType> GetBiomeData(string biome = null, Transform tr = null)
        {
            if (_bioData?.Count <= 0 && tr != null || biome != null)
            {
                var data = BiomeManager.FindBiomeLoot(tr, biome);

                if (data != null)
                {
                    _bioData = data;
                    _bioData.Add(Mod.GetSandBagTechType());
                }
            }

            return _bioData;
        }

        public override bool IsOperational => DeepDrillerPowerManager.HasEnoughPowerToOperate() &&
                   DeepDrillerPowerManager.GetPowerState() == FCSPowerStates.Powered &&
                   OilHandler.HasOil() && !DeepDrillerContainer.IsFull && !_isBreakSet;
        
        internal bool GetIsRangeVisible()
        {
            return _isRangeVisible;
        }

        internal bool IsBreakerSet()
        {
            return _isBreakSet;
        }

        internal void ToggleRangeView()
        {
            _isRangeVisible = !_isRangeVisible;
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        internal bool IsBreakSet()
        {
            return _isBreakSet;
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        public void EmptyDrill()
        {
            DeepDrillerContainer.Clear();
        }

        public void OnHandHover(GUIHand hand)
        {
            if (DeepDrillerPowerManager == null || !DeepDrillerPowerManager.IsInitialized || DisplayHandler == null || DisplayHandler.IsInteraction()) return;

            if (IsConstructed && IsInitialized)
            {
                _sb.Clear();
                _sb.Append(UnitID);
                _sb.Append(Environment.NewLine);
                _sb.Append(AuxPatchers.PressKeyToOperate(GameInput.GetBindingName(GameInput.Button.Exit, GameInput.BindingSet.Primary), Mod.DeepDrillerMk3FriendlyName));
                _sb.Append(Environment.NewLine);
                _sb.Append(Language.main.GetFormat<int, int>("ThermalPlantStatus", Mathf.RoundToInt(DeepDrillerPowerManager.GetSourcePower(DeepDrillerPowerSources.Thermal)), Mathf.RoundToInt(DeepDrillerPowerManager.GetSourcePowerCapacity(DeepDrillerPowerSources.Thermal))));
                _sb.Append(Environment.NewLine);
                _sb.Append(Language.main.GetFormat<int, int, int>("SolarPanelStatus", Mathf.RoundToInt(DeepDrillerPowerManager.GetRechargeScalar() * 100f), Mathf.RoundToInt(DeepDrillerPowerManager.GetSourcePower(DeepDrillerPowerSources.Solar)), Mathf.RoundToInt(DeepDrillerPowerManager.GetSourcePowerCapacity(DeepDrillerPowerSources.Solar))));

                HandReticle.main.SetInteractText(_sb.ToString(), false, HandReticle.Hand.None);
                HandReticle.main.SetIcon(HandReticle.IconType.Info, 1f);

                if (GameInput.GetButtonDown(GameInput.Button.Exit))
                {
                    _isBreakSet ^= true;
                    UpdateEmission();
                }
            }
        }

        public void OnHandClick(GUIHand hand)
        {

        }

        public override bool CanBeStored(int amount, TechType techType)
        {
            var result = OilHandler.CanBeStored(amount, techType);
            QuickLogger.Debug($"Drill can be stored result: {result}",true);
            return result;
        }

        public override bool AddItemToContainer(InventoryItem item)
        {
            return OilHandler.AddItemToContainer(item);
        }

        #endregion
    }
}