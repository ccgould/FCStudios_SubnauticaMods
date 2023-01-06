using System;
using System.Collections.Generic;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Buildable;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Models;
using FCS_ProductionSolutions.Mods.DeepDriller.Managers;
using FCSCommon.Utilities;
using UnityEngine;
using UWE;

namespace FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono
{
    internal class FCSDeepDrillerController : DrillSystem, IFCSSave<SaveData>, IHandTarget
    {
        #region Private Members

        private DeepDrillerSaveDataEntry _saveData;
        private bool _allPlateFormsFound;
        private bool PlatformLegsExtended;
        private const int Segments = 50;
        private LineRenderer _line;
        private const float LerpSpeed = 10f;
        private bool _isRangeVisible;
        private float _currentDistance;

        private readonly List<PistonBobbing> _pistons = new();
        //public override bool AllowsTransceiverPulling { get; } = true;

        #endregion

        #region Internal Properties

        public override bool CanBeSeenByTransceiver { get; set; }
        internal FCSDeepDrillerLavaPitHandler LavaPitHandler { get; private set; }
        public override bool IsConstructed { get; set; }
        public FCSDeepDrillerPowerHandler DeepDrillerPowerManager { get; set; }
        internal FCSDeepDrillerDisplay DisplayHandler { get; private set; }
        internal DumpContainer PowercellDumpContainer { get; set; }
        public FCSDeepDrillerUpgradeManager UpgradeManager { get; private set; }
        public FCSDeepDrillerTransferManager TransferManager { get; set; }
        public override bool IsVisible => IsConstructed && IsInitialized;
        public override int MaxItemAllowForTransfer { get; } = 6;

        private List<TechType> Resources => BiomeManager.Resources;

        public override TechType[] AllowedTransferItems { get; } =
        {
            TechType.Lubricant
        };

        internal override bool UseOnScreenUi => false;

        #endregion

        #region Unity Methods

        internal override void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, FCSDeepDrillerBuildable.DeepDrillerMk3TabID,
                Mod.ModPackID);
            DisplayHandler?.UpdateUnitID();
            base.Start();
        }

        internal override void LoadSave()
        {
            if (_isFromSave && _saveData != null)
            {
                DeepDrillerPowerManager.LoadData(_saveData);

                DeepDrillerContainer.LoadData(_saveData.Items);
                if (_saveData.IsFocused)
                {
                    OreGenerator.SetIsFocus(_saveData.IsFocused);
                    OreGenerator.Load(_saveData.FocusOres);
                }

                _colorManager.LoadTemplate(_saveData.ColorTemplate);
                CurrentBiome = _saveData.Biome;
                OilHandler.SetOilTimeLeft(_saveData.OilTimeLeft);

                UpgradeManager.Load(_saveData?.Upgrades);
                OreGenerator.SetBlackListMode(_saveData.IsBlackListMode);
                _isRangeVisible = _saveData.IsRangeVisible;
                _isBreakSet = _saveData.IsBrakeSet;
                if (!string.IsNullOrWhiteSpace(_saveData.BeaconName))
                {
                    SetPingName(_saveData.BeaconName);
                }

                ToggleVisibility(_saveData.IsPingVisible);
                UpdateEmission();
            }
        }

        internal override void Update()
        {
            if (_line == null) return;


            if (_isRangeVisible)
            {
                CreatePoints(Mathf.Clamp(_currentDistance + DayNightCycle.main.deltaTime * LerpSpeed * 1, 0,
                    Main.Configuration.DDDrillAlterraStorageRange));
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

            base.Update();
        }

        #endregion

        #region IProtoEventListener

        public override void Save(SaveData saveDataList, ProtobufSerializer serializer = null)
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
            _saveData.ColorTemplate = _colorManager.SaveTemplate();

            _saveData.PowerState = DeepDrillerPowerManager.GetPowerState();
            _saveData.PullFromRelay = DeepDrillerPowerManager.GetPullFromPowerRelay();
            _saveData.PowerData = DeepDrillerPowerManager.SaveData();

            _saveData.Items = DeepDrillerContainer.SaveData();

            _saveData.FocusOres = OreGenerator.GetFocusedOres();
            _saveData.IsFocused = OreGenerator.GetIsFocused();
            _saveData.IsBlackListMode = OreGenerator.GetInBlackListMode();

            _saveData.Biome = CurrentBiome;

            _saveData.OilTimeLeft = OilHandler.GetOilTimeLeft();

            _saveData.Upgrades = UpgradeManager.Save();

            _saveData.IsRangeVisible = _isRangeVisible;

            _saveData.BeaconName = _ping.GetLabel();
            _saveData.IsPingVisible = _ping.visible;

            _saveData.IsBrakeSet = _isBreakSet;

            _saveData.AllowedToExport = TransferManager.IsAllowedToExport();
            saveDataList.DeepDrillerMk2Entries.Add(_saveData);
        }

        #endregion

        #region Private Methods

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

        internal override void ConnectDisplay()
        {
            if (DisplayHandler != null) return;
            QuickLogger.Debug($"Creating Display");
            DisplayHandler = gameObject.AddComponent<FCSDeepDrillerDisplay>();
            DisplayHandler.Setup(this);
            DisplayHandler.RefreshStorageAmount();
            DisplayHandler.UpdateListItemsState(_saveData?.FocusOres ?? new HashSet<TechType>());
            DisplayHandler?.UpdateUnitID();
            DisplayHandler?.UpdatePingToggleState(_ping?.visible ?? false);
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

        #endregion

        #region Internal Methods

        public override void Initialize()
        {
            base.Initialize();

            _pistons.Add(gameObject.transform.FirstOrDefault(x => x.name == "hover_piston_pump_01").gameObject
                .AddComponent<PistonBobbing>());
            _pistons.Add(gameObject.transform.FirstOrDefault(x => x.name == "hover_piston_pump_02").gameObject
                .AddComponent<PistonBobbing>());
            _pistons.Add(gameObject.transform.FirstOrDefault(x => x.name == "hover_piston_pump_03").gameObject
                .AddComponent<PistonBobbing>());
            _pistons.Add(gameObject.transform.FirstOrDefault(x => x.name == "hover_piston_pump_04").gameObject
                .AddComponent<PistonBobbing>());
            _pistons.Add(gameObject.transform.FirstOrDefault(x => x.name == "hover_piston_pump_05").gameObject
                .AddComponent<PistonBobbing>());
            _pistons.Add(gameObject.transform.FirstOrDefault(x => x.name == "hover_piston_pump_06").gameObject
                .AddComponent<PistonBobbing>());

            _pistons[1].Invert = true;
            _pistons[3].Invert = true;
            _pistons[5].Invert = true;

            if (DeepDrillerPowerManager == null)
            {
                DeepDrillerPowerManager = gameObject.AddComponent<FCSDeepDrillerPowerHandler>();
                DeepDrillerPowerManager.Initialize(this);
                var powerRelay = gameObject.AddComponent<PowerRelay>();
                DeepDrillerPowerManager.SetPowerRelay(powerRelay);
                PowerManager = DeepDrillerPowerManager;
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

            AnimationHandler = gameObject.EnsureComponent<FCSDeepDrillerAnimationHandler>();
            AnimationHandler.Initialize(new AnimationCurve(new Keyframe(0, 1.686938f), new Keyframe(1, -0.581f)));

            OnGenerate();

            InvokeRepeating(nameof(UpdatePistonState), .5f, .5f);

            IsInitialized = true;

            QuickLogger.Debug($"Initializing Completed");
        }

        private void UpdatePistonState()
        {
            if (!IsConstructed || !IsInitialized) return;

            if (OreGenerator.GetIsDrilling())
            {
                ChangePistonState();
            }
            else
            {
                ChangePistonState(false);
            }
        }

        internal override void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetDeepDrillerMK2SaveData(id);
        }

        public override List<PistonBobbing> GetPistons()
        {
            return _pistons;
        }

        public override bool IsOperational => DeepDrillerPowerManager.HasEnoughPowerToOperate() &&
                                              DeepDrillerPowerManager.GetPowerState() == FCSPowerStates.Powered &&
                                              OilHandler.HasOil() && !DeepDrillerContainer.IsFull && !_isBreakSet;

        internal bool GetIsRangeVisible()
        {
            return _isRangeVisible;
        }

        internal void ToggleRangeView()
        {
            _isRangeVisible = !_isRangeVisible;
        }

        internal override bool IsPowerAvailable()
        {
            return DeepDrillerPowerManager?.IsPowerAvailable() ?? false;
        }

        public override IEnumerable<UpgradeFunction> GetUpgrades()
        {
            return UpgradeManager.Upgrades;
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        public override void OnHandHover(GUIHand hand)
        {
            if (DeepDrillerPowerManager == null || !DeepDrillerPowerManager.IsInitialized || DisplayHandler == null ||
                DisplayHandler.IsInteraction()) return;

            base.OnHandHover(hand);

            if (IsConstructed && IsInitialized)
            {
                _sb.Clear();
                _sb.Append(UnitID);
                _sb.Append(Environment.NewLine);
                _sb.Append(AuxPatchers.PressKeyToOperate(
                    GameInput.GetBindingName(GameInput.Button.Exit, GameInput.BindingSet.Primary),
                    FCSDeepDrillerBuildable.DeepDrillerMk3FriendlyName));
                _sb.Append(Environment.NewLine);
                _sb.Append(Language.main.GetFormat<int, int>("ThermalPlantStatus",
                    Mathf.RoundToInt(DeepDrillerPowerManager.GetSourcePower(DeepDrillerPowerSources.Thermal)),
                    Mathf.RoundToInt(DeepDrillerPowerManager.GetSourcePowerCapacity(DeepDrillerPowerSources.Thermal))));
                _sb.Append(Environment.NewLine);
                _sb.Append(Language.main.GetFormat<int, int, int>("SolarPanelStatus",
                    Mathf.RoundToInt(DeepDrillerPowerManager.GetRechargeScalar() * 100f),
                    Mathf.RoundToInt(DeepDrillerPowerManager.GetSourcePower(DeepDrillerPowerSources.Solar)),
                    Mathf.RoundToInt(DeepDrillerPowerManager.GetSourcePowerCapacity(DeepDrillerPowerSources.Solar))));

                _sb.Append(AlterraHub.ViewInPDA());
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, _sb.ToString());
                HandReticle.main.SetIcon(HandReticle.IconType.Info, 1f);
            }
        }

        public void OnHandClick(GUIHand hand)
        {
        }

        public override bool CanBeStored(int amount, TechType techType)
        {
            var result = OilHandler.CanBeStored(amount, techType);
            QuickLogger.Debug($"Drill can be stored result: {result}", true);
            return result;
        }

        public override bool AddItemToContainer(InventoryItem item)
        {
            return OilHandler.AddItemToContainer(item);
        }

        public override IFCSStorage GetStorage()
        {
            //Getting Storage of the Oil Handler for the Transceiver
            return OilHandler;
        }

        public override TechType GetRandomTechTypeFromDevice()
        {
            if (!IsConstructed || !IsInitialized || DeepDrillerContainer == null || !DeepDrillerContainer.HasItems())
                return TechType.None;

            return DeepDrillerContainer.GetRandomItem();
        }

        public override Pickupable RemoveItemFromDevice(TechType techType)
        {
            return RemoveItemFromContainer(techType);
        }

        public override Pickupable RemoveItemFromContainer(TechType techType)
        {
            var result = DeepDrillerContainer.OnlyRemoveItemFromContainer(techType);

            if (!result) return null;
            var itemTask = new TaskResult<InventoryItem>();
            CoroutineManager.WaitCoroutine(techType.ToInventoryItem(itemTask));

            return itemTask.Get().item;
        }

        #endregion
    }
}