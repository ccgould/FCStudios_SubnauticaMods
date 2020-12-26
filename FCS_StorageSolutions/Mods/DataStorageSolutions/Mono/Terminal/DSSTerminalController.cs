using System;
using System.Collections.Generic;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Helpers;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class DSSTerminalController : FcsDevice, IFCSSave<SaveData>, IFCSDumpContainer
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private DSSTerminalDataEntry _saveData;
        private DumpContainerSimplified _dumpContainer;
        private DSSTerminalDisplayManager _display;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.DSSTabID, Mod.ModName);
            _display.Setup(this);
            _dumpContainer.Initialize(transform, $"Add item to base: {Manager.GetBaseName()}", this, 6, 8, gameObject.name);
        }

        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (!IsInitialized)
            {
                Initialize();
            }

            if (_saveData == null)
            {
                ReadySaveData();
            }

            if (_fromSave)
            {
                _colorManager.ChangeColor(_saveData.Body.Vector4ToColor());
                _colorManager.ChangeColor(_saveData.SecondaryBody.Vector4ToColor(), ColorTargetMode.Secondary);
                _fromSave = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetDSSTerminalSaveData(id);
        }

        public override void Initialize()
        {
            if (_colorManager == null)
            {
                _colorManager = gameObject.EnsureComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial, ModelPrefab.SecondaryMaterial);
            }

            if (_dumpContainer == null)
            {
                _dumpContainer = gameObject.EnsureComponent<DumpContainerSimplified>();
            }

            if (_display == null)
            {
                _display = gameObject.EnsureComponent<DSSTerminalDisplayManager>();
                
            }

            IsInitialized = true;

            QuickLogger.Debug($"Initialized");
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.DSSTerminalFriendlyName}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {Mod.DSSTerminalFriendlyName}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_saveData == null)
            {
                _saveData = new DSSTerminalDataEntry();
            }
            _saveData.ID = id;
            _saveData.Body = _colorManager.GetColor().ColorToVector4();
            _saveData.SecondaryBody = _colorManager.GetSecondaryColor().ColorToVector4();



            newSaveData.DSSTerminalDataEntries.Add(_saveData);
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
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
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public void OpenStorage()
        {
            _dumpContainer.OpenStorage();
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {

            QuickLogger.Debug($"Checking if allowed {_dumpContainer.GetItemCount() + 1}",true);

            //TODO Check filter first


            int availableSpace = 0;
            foreach (IDSSRack baseRack in Manager.BaseRacks)
            {
                availableSpace += baseRack.GetFreeSpace();
            }

            var result = availableSpace >= _dumpContainer.GetItemCount() + 1;
            QuickLogger.Debug($"Allowed result: {result}", true);
            return result;
        }

        public override bool AddItemToContainer(InventoryItem item)
        {
            try
            {
                var result = TransferHelpers.AddItemToNetwork(item, Manager);
                if (!result)
                {
                    PlayerInteractionHelper.GivePlayerItem(item);
                }
            }
            catch (Exception e)
            {
                QuickLogger.Debug(e.Message,true);
                QuickLogger.Debug(e.StackTrace);
                PlayerInteractionHelper.GivePlayerItem(item);
                return false;
            }
            return true;
        }
    }
}