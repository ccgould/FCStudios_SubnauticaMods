using System;
using System.Collections;
using System.Collections.Generic;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.DeepDriller.Configuration;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Buildable;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono;
using FCS_ProductionSolutions.Mods.DeepDriller.Interfaces;
using FCS_ProductionSolutions.Mods.DeepDriller.Managers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.DeepDriller.LightDuty.Mono
{
    internal class DeepDrillerLightDutyController : FcsDevice, IFCSSave<SaveData>, IHandTarget, IDeepDrillerController
    {
        public FCSDeepDrillerContainer DeepDrillerContainer { get; set; }
        public DumpContainer OilDumpContainer { get; set; }
        private List<TechType> _bioData = new List<TechType>();


        public bool IsBreakSet()
        {
            return false;
        }

        public bool IsPowerAvailable()
        {
            return PowerManager?.IsPowerAvailable() ?? false;
        }

        private DeepDrillerLightDutySaveDataEntry _saveData;
        private bool _isFromSave;
        private bool _runStartUpOnEnable;
        private PingInstance _ping;
        private bool _noBiomeMessageSent;
        private bool _biomeFoundMessageSent;
        private bool _isBreakSet;
        private bool _wasPlaying;
        private AudioSource _audio;
        private AudioLowPassFilter _lowPassFilter;
        internal FCSDeepDrillerOilHandler OilHandler { get; set; }

        public string CurrentBiome { get; set; }
        public PowerManager PowerManager { get; private set; }

        public override void Initialize()
        {
            if (IsInitialized) return;

            _audio = gameObject.GetComponent<AudioSource>();
            _lowPassFilter = gameObject.GetComponent<AudioLowPassFilter>();

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol);
                _colorManager.ChangeColor(new ColorTemplate());
            }

            DeepDrillerContainer = new FCSDeepDrillerContainer();

            OreGenerator = gameObject.AddComponent<FCSDeepDrillerOreGenerator>();
            OreGenerator.Initialize(this);
            OreGenerator.OnAddCreated += OreGeneratorOnAddCreated;

            if (OilDumpContainer == null)
            {
                OilDumpContainer = gameObject.AddComponent<DumpContainer>();
                OilDumpContainer.Initialize(transform,
                    FCSDeepDrillerBuildable.OilDropContainerTitle(),
                    OilHandler, 4, 4);
            }

            if (OilHandler == null)
            {
                OilHandler = gameObject.AddComponent<FCSDeepDrillerOilHandler>();
                OilHandler.Initialize(this);
            }

            if (PowerManager == null)
            {
                PowerManager = gameObject.AddComponent<PowerManager>();
            }
            
            _ping = WorldHelpers.CreateBeacon(gameObject, QPatch.DeepDrillerPingType, "");


            IsInitialized = true;
        }

        private void OreGeneratorOnAddCreated(TechType type)
        {
            DeepDrillerContainer.AddItemToContainer(type);
        }

        public FCSDeepDrillerOreGenerator OreGenerator { get; set; }

        #region Unity Methods

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.DeepDrillerMk3TabID, Mod.ModPackID);
            if (_saveData == null)
            {
                ReadySaveData();
            }

            LoadSave();
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetDeepDrillerLightDutySaveData(id);
        }

        private void LoadSave()
        {
            if (_isFromSave && _saveData != null)
            {
                PowerManager.LoadData(_saveData);

                DeepDrillerContainer.LoadData(_saveData.Items);
                _colorManager.LoadTemplate(_saveData.ColorTemplate);
                CurrentBiome = _saveData.Biome;
                OilHandler.SetOilTimeLeft(_saveData.OilTimeLeft);

                _isBreakSet = _saveData.IsBrakeSet;
                if (!string.IsNullOrWhiteSpace(_saveData.BeaconName))
                {
                    SetPingName(_saveData.BeaconName);
                }
                ToggleVisibility(_saveData.IsPingVisible);
            }
        }

        internal void ToggleVisibility(bool value = false)
        {
            _ping.SetVisible(value);
            PingManager.NotifyVisible(_ping);
        }

        public void SetPingName(string beaconName)
        {
            _ping.SetLabel(beaconName);
            PingManager.NotifyRename(_ping);
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

                StartCoroutine(TryGetLoot());
                _runStartUpOnEnable = false;
            }
        }

        public bool IsBeingDeleted { get; set; }

        private void Update()
        {
            if (_audio != null && _audio.isPlaying)
            {

                _audio.volume = QPatch.Configuration.MasterDeepDrillerVolume;

                if (!QPatch.Configuration.DDMK3FxAllowed)
                {
                    _audio.Stop();
                    return;
                }


                if (_audio.isPlaying && WorldHelpers.CheckIfPaused())
                {
                    _audio.Pause();
                    _wasPlaying = true;
                }

                if (_wasPlaying && !WorldHelpers.CheckIfPaused())
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


            if (WorldHelpers.CheckIfInRange(gameObject, Player.main.gameObject, 5f) && IsOperational && !IsBreakSet())
            {
                MainCameraControl.main.ShakeCamera(.3f);
            }
        }

        #endregion

        #region IProtoEventListener

        public void Save(SaveData saveDataList, ProtobufSerializer serializer = null)
        {
            if (!IsInitialized || _colorManager == null)
            {
                QuickLogger.Error($"Failed to save driller {GetPrefabID()}");
                return;
            }

            if (_saveData == null)
            {
                _saveData = new DeepDrillerLightDutySaveDataEntry();
            }


            QuickLogger.Message($"SaveData = {_saveData}", true);

            _saveData.Id = GetPrefabID();
            _saveData.ColorTemplate = _colorManager.SaveTemplate();

            _saveData.PowerState = PowerManager.GetPowerState();
            _saveData.PowerData = PowerManager.SaveData();

            _saveData.Items = DeepDrillerContainer.SaveData();
            _saveData.Biome = CurrentBiome;

            _saveData.OilTimeLeft = OilHandler.GetOilTimeLeft();
            _saveData.BeaconName = _ping.GetLabel();
            _saveData.IsPingVisible = _ping.visible;
            saveDataList.DeepDrillerLightDutyEntries.Add(_saveData);

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
            _isFromSave = true;
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

                    var loot = Helpers.Helpers.GetBiomeData(ref _bioData, CurrentBiome, transform);

                    if (loot != null)
                    {
                        OreGenerator.AllowedOres = loot;
                        //ConnectDisplay();
                    }
                }
                yield return null;
            }
            yield return 0;
        }
        
        public void OnHandClick(GUIHand hand)
        {
            //IMplement ui opening on click
        }

    }

    internal class PowerManager : FCSPowerManager, IPowerManager
    {
        private FCSPowerStates _powerState;

        public bool IsPowerAvailable()
        {
            return true;
        }

        public override FCSPowerStates GetPowerState()
        {
            return _powerState;
        }

        public DeepDrillerPowerData SaveData()
        {
            return null;
        }

        public void LoadData(DeepDrillerLightDutySaveDataEntry saveData)
        {
            throw new NotImplementedException();
        }

    }
}
