using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Buildable;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono;
using FCS_ProductionSolutions.Mods.DeepDriller.Patchers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.Mods.DeepDriller.Managers
{
    internal abstract class DrillSystem : FcsDevice
    {
        internal FCSDeepDrillerAnimationHandler AnimationHandler { get; set; }
        internal FCSDeepDrillerContainer DeepDrillerContainer { get; set; }
        internal FCSDeepDrillerOreGenerator OreGenerator { get; set; }
        internal DumpContainer OilDumpContainer { get; set; }
        internal FCSDeepDrillerOilHandler OilHandler { get; set; }
        internal string CurrentBiome { get; set; }
        internal bool IsBeingDeleted { get; set; }
        internal FCSPowerManager PowerManager { get; private protected set; }
        protected StringBuilder _sb =  new();
        public override bool IsOperational => PowerManager.HasEnoughPowerToOperate() &&
                                              PowerManager.GetPowerState() == FCSPowerStates.Powered &&
                                              OilHandler.HasOil() && !DeepDrillerContainer.IsFull && !_isBreakSet;
        protected PingInstance _ping;
        protected bool _isBreakSet;
        private AudioSource _audio;
        private AudioLowPassFilter _lowPassFilter;
        private bool _noBiomeMessageSent;
        private bool _biomeFoundMessageSent;
        private List<TechType> _bioData = new();
        private bool _runStartUpOnEnable;
        private bool _wasPlaying;
        protected bool _isFromSave;
        private ISaveDataEntry _saveData;
        internal abstract bool UseOnScreenUi { get; }
        public Action<PowercellData> OnBatteryLevelChange { get; set; }
        public Action<float> OnOilLevelChange { get; set; }

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
        
        private void UpdateDrillShaftState()
        {
            if (!IsConstructed || !IsInitialized) return;

            if (GetIsDrilling())
            {
                AnimationHandler.DrillState(true);
                PlaySFX();
            }
            else
            {
                AnimationHandler.DrillState(false);
                StopSFX();

            }
        }

        private void OreGeneratorOnAddCreated(TechType type)
        {
            DeepDrillerContainer.AddItemToContainer(type);
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

                    var loot = Helpers.Helpers.GetBiomeData(ref _bioData, CurrentBiome, transform);

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

        internal virtual void Update()
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

        protected void UpdateEmission()
        {
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject,
                _isBreakSet ? Color.red : Color.cyan);
        }


        internal virtual void ConnectDisplay(){}

        internal virtual void Start()
        {
            InvokeRepeating(nameof(UpdateDrillShaftState), .5f, .5f);
            UpdateEmission();
            if (_saveData == null)
            {
                ReadySaveData();
            }

            LoadSave();
        }


        public override void OnDestroy()
        {
            Manager?.UnRegisterDevice(this);
            base.OnDestroy();
            IsBeingDeleted = true;
        }

        internal abstract void ReadySaveData();
        
        internal abstract void LoadSave();

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

            if (OilHandler == null)
            {
                OilHandler = gameObject.AddComponent<FCSDeepDrillerOilHandler>();
                OilHandler.Initialize(this);
            }

            if (OilDumpContainer == null)
            {
                OilDumpContainer = gameObject.AddComponent<DumpContainer>();
                OilDumpContainer.Initialize(transform,
                    FCSDeepDrillerBuildable.OilDropContainerTitle(),
                    OilHandler, 4, 4);
            }


            _ping = WorldHelpers.CreateBeacon(gameObject, QPatch.DeepDrillerPingType, "");

            IsInitialized = true;
        }

        internal bool IsBreakSet()
        {
            return _isBreakSet;
        }

        internal void StopSFX()
        {
            if (_audio.isPlaying)
                _audio.Stop();
        }
        
        internal void PlaySFX()
        {
            if (!_audio.isPlaying)
                _audio.Play();
        }

        internal void SetPingName(string beaconName)
        {
            _ping.SetLabel(beaconName);
            PingManager.NotifyRename(_ping);
        }

        internal void EmptyDrill()
        {
            DeepDrillerContainer.Clear();
        }

        internal bool GetIsDrilling()
        {
            return OreGenerator.GetIsDrilling();
        }

        internal virtual bool IsPowerAvailable()
        {
            return PowerManager?.IsPowerAvailable() ?? false;
        }

        public virtual IEnumerable<UpgradeFunction> GetUpgrades()
        {
            return null;
        }

        public virtual List<PistonBobbing> GetPistons()
        {
            return null;
        }

        internal void ToggleVisibility(bool value = false)
        {
            _ping.SetVisible(value);
            PingManager.NotifyVisible(_ping);
        }

        internal void ToggleBeacon()
        {
            _ping.enabled = !_ping.enabled;
        }

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

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }

        #region IProtoEventListener

        public abstract void Save(SaveData saveDataList, ProtobufSerializer serializer = null);

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

        public override void OnHandHover(GUIHand hand)
        {
            base.OnHandHover(hand);

            if (hand.IsTool())
            {
                _sb.Clear();
                _sb.Append(AlterraHub.PleaseClearHands());
                HandReticle.main.SetInteractText(_sb.ToString(), AlterraHub.ViewInPDA(), false, false, HandReticle.Hand.None);
                HandReticle.main.SetIcon(HandReticle.IconType.HandDeny);
                return;
            }


            if (IsConstructed && IsInitialized)
            {
                _sb.Clear();
                _sb.Append(UnitID);
                _sb.Append(Environment.NewLine);
                _sb.Append(AuxPatchers.PressKeyToOperate(GameInput.GetBindingName(GameInput.Button.Exit, GameInput.BindingSet.Primary), FCSDeepDrillerBuildable.DeepDrillerMk3FriendlyName));


                if (UseOnScreenUi)
                {
                    _sb.Append(Environment.NewLine);
                    _sb.Append($"Press: {GameInput.GetBindingName(GameInput.Button.AltTool, GameInput.BindingSet.Primary)} to open interface."); 

                    if (GameInput.GetButtonDown(GameInput.Button.AltTool))
                    {
                        DeepDrillerHUD.Main.Show(this);
                    }
                }


                HandReticle.main.SetInteractText(_sb.ToString(), AlterraHub.ViewInPDA(), false, false, HandReticle.Hand.None);
                HandReticle.main.SetIcon(HandReticle.IconType.Info, 1f);

                if (GameInput.GetButtonDown(GameInput.Button.Exit))
                {
                    _isBreakSet ^= true;
                    UpdateEmission();
                }



            }
        }

        public float GetPowerCharge()
        {
            return PowerManager.GetDevicePowerCharge();
        }

        public PowercellData GetBatteryPowerData()
        {
            return PowerManager?.GetBatteryPowerData();
        }

        public float GetOilPercentage()
        {
            return OilHandler?.GetOilPercent() ?? 0f;
        }

        public string GetOresPerDayCount()
        {
            return OreGenerator?.GetItemsPerDay();
        }

        public string GetPowerUsageAmount()
        {
            return PowerManager?.GetPowerUsage().ToString();
        }

        public bool GetBeaconState()
        {
            return _ping?.visible ?? false;
        }

        public string GetPingName()
        {
            return _ping?.GetLabel();
        }
    }
}
