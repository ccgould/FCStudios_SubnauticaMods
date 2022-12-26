using System;
using System.Text;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_LifeSupportSolutions.Buildable;
using FCS_LifeSupportSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Mods.OxygenTank.Mono
{
    internal class BaseOxygenTankController : FcsDevice, IFCSSave<SaveData>, IHandTarget
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private BaseOxygenTankEntry _savedData;
        private AudioManager _audioManager;
        private OxygenTankAttachPoint _oxygenAttachPoint;
        private StringBuilder _sb = new StringBuilder();

        public override bool IsOperational => IsConstructed && Manager != null && IsInitialized;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.BaseOxygenTankTabID, Mod.ModPackID);
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
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

                    _colorManager.LoadTemplate(_savedData.ColorTemplate);

                    if (!string.IsNullOrEmpty(_savedData.ParentID))
                        _oxygenAttachPoint.parentPipeUID = _savedData.ParentID;
                }

                _runStartUpOnEnable = false;
            }
        }

        public override void Initialize()
        {
            _oxygenAttachPoint = gameObject.EnsureComponent<OxygenTankAttachPoint>();

            if (_audioManager == null)
            {
                _audioManager = new AudioManager(gameObject.EnsureComponent<FMOD_CustomLoopingEmitter>());
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial, ModelPrefab.SecondaryMaterial);
            }

            IsInitialized = true;
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

            _isFromSave = true;
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
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

                    IsInitialized = true;
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
            else
            {
                if (_oxygenAttachPoint != null)
                    _oxygenAttachPoint.SetParent(null);
            }
        }

        public IPipeConnection GetRootOxygenProvider()
        {
            return _oxygenAttachPoint.GetParent()?.GetRoot();
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new BaseOxygenTankEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
            _savedData.ParentID = _oxygenAttachPoint.parentPipeUID;
            QuickLogger.Debug($"Saving ID {_savedData.Id}");
            newSaveData.BaseOxygenTankEntries.Add(_savedData);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetOxygenTankSaveData(GetPrefabID());
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }

        public override void OnHandHover(GUIHand hand)
        {
            if (!IsConstructed || !IsInitialized || Manager == null) return;
            HandReticle main = HandReticle.main;

            main.SetIcon(HandReticle.IconType.Info);

            var requiredTankCount = Manager.GetRequiredTankCount(Main.Configuration.SmallBaseOxygenHardcore);
            _sb.Clear();
            if (_oxygenAttachPoint.allowConnection && _oxygenAttachPoint.parentPipeUID != null)
                _sb.Append($"Pipe Connected");
            else if (_oxygenAttachPoint.allowConnection)
                _sb.Append($"Searching for Oxygen Pipe");
            else
                _sb.Append($"Pipe Connection Disabled.");
            _sb.Append(Environment.NewLine);
            _sb.Append($"Active Tanks: {Manager.ActiveBaseOxygenTankCount}, Required Tank Count: {requiredTankCount}");
            main.SetTextRaw(HandReticle.TextType.Use, $"{Mod.BaseOxygenTankFriendly} - {UnitID}");
            main.SetTextRaw(HandReticle.TextType.UseSubscript, _sb.ToString());
        }

        public void OnHandClick(GUIHand hand)
        {
            _oxygenAttachPoint.allowConnection = !_oxygenAttachPoint.allowConnection;
            if (_oxygenAttachPoint.parentPipeUID != null)
                _oxygenAttachPoint.SetParent(null);
        }
    }
}