using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.DisplayBoard.Buildable;
using FCS_HomeSolutions.Mods.FireExtinguisherRefueler.Buildable;
using FCSCommon.Utilities;
using HarmonyLib;
using SMLHelper.V2.Json.ExtensionMethods;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.DisplayBoard.Mono
{
    internal class DisplayBoardController : FcsDevice,IFCSSave<SaveData> ,IHandTarget
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private DisplayBoardDataEntry _savedData;
        private GameObject _display;
        private IEnumerable<GameObject> _screens;
        private HashSet<DisplayScreenController> _trackedScreens = new();

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, DisplayBoardBuildable.DisplayBoardTabID, Mod.ModPackID);
            TurnOnDevice();
            if (Manager != null)
            {
                Manager.OnPowerStateChanged += status =>
                {
                    if (status == PowerSystem.Status.Offline)
                    {
                        TurnOffDevice();
                    }
                    else
                    {
                        TurnOnDevice();
                    }
                };
            }

        }

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (_isFromSave)
                {
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }

                    if (_savedData != null)
                    { 
                        _colorManager.LoadTemplate(_savedData.ColorTemplate);
                        _trackedScreens.ElementAt(0).Load(_savedData.Display1);
                        _trackedScreens.ElementAt(1).Load(_savedData.Display2);
                        _trackedScreens.ElementAt(2).Load(_savedData.Display3);
                        _trackedScreens.ElementAt(3).Load(_savedData.Display4);
                        _isFromSave = false;
                    }
                }
                _runStartUpOnEnable = false;
            }
        }

        public override void Initialize()
        {
            QuickLogger.Info("Initializing", true);

            if (_colorManager == null)
            {
                QuickLogger.Info($"Creating Color Component", true);
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol);
            }

            _display = GameObjectHelpers.FindGameObject(gameObject, "Screens");
            _screens = GameObjectHelpers.FindGameObjects(_display, "Screen_",SearchOption.StartsWith);
            foreach (GameObject child in _screens)
            {
                var screen = child.EnsureComponent<DisplayScreenController>();
                screen.Initialize(this);
                _trackedScreens.Add(screen);
            }


            InvokeRepeating(nameof(GetData),1,1);
            
            IsInitialized = true;
            QuickLogger.Info("Initialized", true);
        }

        private void GetData()
        {

        }

        public override void TurnOnDevice()
        {
            if (Manager?.GetPowerState() != PowerSystem.Status.Offline)
            {
                _display?.SetActive(true);
            }
        }

        public override void TurnOffDevice()
        {
            if (Manager?.GetPowerState() == PowerSystem.Status.Offline)
            {
                _display?.SetActive(false);
            }
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new DisplayBoardDataEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
            _savedData.Display1 = _trackedScreens.ElementAt(0).Save();
            _savedData.Display2 = _trackedScreens.ElementAt(1).Save();
            _savedData.Display3 = _trackedScreens.ElementAt(2).Save();
            _savedData.Display4 = _trackedScreens.ElementAt(3).Save();
            QuickLogger.Debug($"Saving ID {_savedData.Id}");
            newSaveData.DisplayBoardDataEntries.Add(_savedData);
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {GetPrefabID()}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {GetPrefabID()}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");

            if (_savedData == null)
            {
                ReadySaveData();
            }

            if (!IsInitialized)
            {
                Initialize();
            }

            _isFromSave = true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;

            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                    }
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = String.Empty;
            return true;
        }

        public override void  OnHandHover(GUIHand hand)
        {
            if (!IsInitialized || !IsConstructed) return;
        }

        public void OnHandClick(GUIHand hand)
        {

        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.DisplayBoardEntrySaveData(GetPrefabID());
        }
    }   

    internal enum DisplayMode
    {
        None = -1,
        Time = 0,
        CustomText = 1,
        BaseHullStrength = 2,
        Temperature = 3,
        Biome = 4,
    }
}
