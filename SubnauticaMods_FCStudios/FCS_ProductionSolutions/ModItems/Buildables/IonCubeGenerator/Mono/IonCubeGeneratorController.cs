using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.ModItems.FCSPDA.Mono;
using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Enumerators;
using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Interfaces;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono
{
    internal class IonCubeGeneratorController : FCSDevice
    {

        private const float CubeEnergyCost = 1500f;
        private IonCubeGenSpeedModes _currentMode = StartingMode;
        private PowerRelay _connectedRelay = null;
        private float CubeCreationTime = CubeEnergyCost;
        private float EnergyConsumptionPerSecond = 0f;
        private readonly IList<float> _progress = new List<float>(new[] { -1f, -1f, -1f });
        private PowerRelay ConnectedRelay
        {
            get
            {
                while (_connectedRelay == null)
                    UpdatePowerRelay();

                return _connectedRelay;
            }
        }
        private ICubeContainer _cubeContainer;

        internal const float ProgressComplete = 100f;
        internal const IonCubeGenSpeedModes StartingMode = IonCubeGenSpeedModes.Off;
        internal const float StartUpComplete = 4f;
        internal const float CooldownComplete = 19f;
        public bool IsFull => _cubeContainer.IsFull;

        private float AvailablePower => ConnectedRelay.GetPower();

        public bool PauseUpdates { get; set; } = false;

        public IonCubeGenSpeedModes CurrentSpeedMode
        {
            get => _currentMode;
            set
            {
                IonCubeGenSpeedModes previousMode = _currentMode;
                _currentMode = value;
                if (_currentMode != IonCubeGenSpeedModes.Off)
                {
                    CubeCreationTime = Convert.ToSingle(_currentMode);
                    EnergyConsumptionPerSecond = CubeEnergyCost / CubeCreationTime;

                    if (previousMode == IonCubeGenSpeedModes.Off)
                        TryStartingNextCube();
                }
                else // Off State
                {
                    EnergyConsumptionPerSecond = 0f;
                }
            }
        }

        public float StartUpProgress
        {
            get => _progress[(int)CubePhases.StartUp];
            set => _progress[(int)CubePhases.StartUp] = value;
        }

        public float GenerationProgress
        {
            get => _progress[(int)CubePhases.Generating];
            set => _progress[(int)CubePhases.Generating] = value;
        }

        public float CoolDownProgress
        {
            get => _progress[(int)CubePhases.CoolDown];
            set => _progress[(int)CubePhases.CoolDown] = value;
        }

        public float StartUpPercent => Mathf.Max(0f, StartUpProgress / StartUpComplete);
        public float GenerationPercent => Mathf.Max(0f, GenerationProgress / CubeEnergyCost);
        public float CoolDownPercent => Mathf.Max(0f, CoolDownProgress / CooldownComplete);

        public int NumberOfCubes
        {
            get => _cubeContainer.NumberOfCubes;
            set => _cubeContainer.NumberOfCubes = value;
        }

        public bool NotAllowToGenerate => PauseUpdates || !IsConstructed || CurrentSpeedMode == IonCubeGenSpeedModes.Off;

        #region Unity methods

        public new void Awake()
        {
            //if (_saveData == null)
            //{
            //    ReadySaveData();
            //}

            if (_cubeContainer == null)
            {
                _cubeContainer = new CubeGeneratorContainer(this);
            }

            var pulleyController = gameObject.EnsureComponent<CubeGeneratorPulleyController>();
            pulleyController.Initialize(this);

            var fxController = gameObject.EnsureComponent<CubeGeneratorFxController>();
            fxController.Initialize(this);

            UnitID = "IC001";

            var interaction = gameObject.GetComponent<HoverInteraction>();
            interaction.onSettingsKeyPressed += onSettingsKeyPressed;

            UpdatePowerRelay();
        }

        private void ReadySaveData()
        {
            string id = (base.GetComponentInParent<PrefabIdentifier>() ?? base.GetComponent<PrefabIdentifier>()).Id;
            //this._saveData = Mod.GetIonCubeGeneratorSaveData(id);
        }

        private void Start()
        {
            UpdatePowerRelay();
        }

        private void Update()
        {
            if (NotAllowToGenerate)
                return;
                     
            float energyToConsume = EnergyConsumptionPerSecond * DayNightCycle.main.deltaTime;
            bool requiresEnergy = GameModeUtils.RequiresPower();
            bool hasPowerToConsume = !requiresEnergy || (AvailablePower >= energyToConsume);

            if (!hasPowerToConsume)
                return;

            if (CoolDownProgress >= CooldownComplete)
            {
                // Logger.Log(Logger.Level.Debug, "IonCube Generator - Finished Cooldown", showOnScreen: true);

                PauseUpdates = true;
                _cubeContainer.NumberOfCubes++;
                // Finished cool down - See if the next cube can be started                
                TryStartingNextCube();

                PauseUpdates = false;
            }
            else if (CoolDownProgress >= 0f)
            {
                // Is currently cooling down
                CoolDownProgress = Mathf.Min(CooldownComplete, CoolDownProgress + DayNightCycle.main.deltaTime);
            }
            else if (GenerationProgress >= CubeEnergyCost)
            {
                // Logger.Log(Logger.Level.Debug, "IonCube Generator - Cooldown", showOnScreen: true);

                // Finished generating cube - Start cool down
                CoolDownProgress = 0f;
            }
            else if (GenerationProgress >= 0f)
            {
                if (requiresEnergy)
                    ConnectedRelay.ConsumeEnergy(energyToConsume, out float amountConsumed);

                // Is currently generating cube
                GenerationProgress = Mathf.Min(CubeEnergyCost, GenerationProgress + energyToConsume);
            }
            else if (StartUpProgress >= StartUpComplete)
            {
                // Logger.Log(Logger.Level.Debug, "IonCube Generator - Generating", showOnScreen: true);
                // Finished start up - Start generating cube
                GenerationProgress = 0f;
            }
            else if (StartUpProgress >= 0f)
            {
                // Is currently in start up routine
                StartUpProgress = Mathf.Min(StartUpComplete, StartUpProgress + DayNightCycle.main.deltaTime);
            }
        }

        #endregion

        private void TryStartingNextCube()
        {
            // Logger.Log(Logger.Level.Debug, "Trying to start another cube", showOnScreen: true);

            if (CurrentSpeedMode == IonCubeGenSpeedModes.Off)
                return;// Powered off, can't start a new cube

            if (StartUpProgress < 0f || // Has not started a cube yet
                CoolDownProgress == CooldownComplete) // Has finished a cube
            {
                if (!_cubeContainer.IsFull)
                {
                    // Logger.Log(Logger.Level.Debug, "IonCube Generator - Start up", showOnScreen: true);
                    CoolDownProgress = -1f;
                    GenerationProgress = -1f;
                    StartUpProgress = 0f;
                }
                else
                {
                    // Logger.Log(Logger.Level.Debug, "Cannot start another cube, container is full", showOnScreen: true);
                }
            }
            else
            {
                // Logger.Log(Logger.Level.Debug, "Cannot start another cube, another cube is currently in progress", showOnScreen: true);
            }
        }

        private void UpdatePowerRelay()
        {
            PowerRelay relay = PowerSource.FindRelay(transform);
            if (relay != null && relay != _connectedRelay)
            {
                _connectedRelay = relay;
                // Logger.Log(Logger.Level.Debug, "PowerRelay found at last!");
            }
            else
            {
                _connectedRelay = null;
            }
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        public override bool IsDeconstructionObstacle()
        {
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;
            if (constructed)
            {
                if (base.isActiveAndEnabled)
                {
                    if (!this.IsInitialized)
                    {
                        this.Initialize();
                    }
                    this.IsInitialized = true;
                    return;
                }
                _runStartUpOnEnable = true;
            }
        }

        public void OpenStorage()
        {
            _cubeContainer.OpenStorage();
        }

        internal void OnAddItemEvent(InventoryItem item)
        {
            Buildable.deconstructionAllowed = false;
        }

        internal void OnRemoveItemEvent(InventoryItem item)
        {
            Buildable.deconstructionAllowed = _cubeContainer.NumberOfCubes == 0;
            TryStartingNextCube();
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {

        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {

        }

        internal void onSettingsKeyPressed()
        {
            QuickLogger.Debug("Opening Settings",true);
            FCSPDAController.Main.OpenDeviceUI("uGUI_Ioncube", this);
        }

        internal bool IsCrafting()
        {
            return GenerationProgress > -1f && GenerationProgress < CubeEnergyCost;
        }
    }
}