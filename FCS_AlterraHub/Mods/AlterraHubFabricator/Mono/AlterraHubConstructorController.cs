using System;
using System.Collections.Generic;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.AlterraHubConstructor.Buildable;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCSCommon.Interfaces;
using FCSCommon.Utilities;

namespace FCS_AlterraHub.Mods.AlterraHubFabricator.Mono
{
    internal class AlterraHubConstructorController : FcsDevice, IFCSSave<SaveData>
    {
        private PortManager _portManager;
        private bool _runStartUpOnEnable;
        private AlterraHubConstructorEntry _savedData;
        private bool _isFromSave;
        private List<TechType> _storage = new();
        public StorageContainer Storage;

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetAlterraHubConstructorEntrySaveData(GetPrefabID());
        }

        public  void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, AlterraHubFabricatorPatcher.TabID, Mod.ModPackID);


            if (Manager is not null)
            {
                _portManager = Manager.Habitat.GetComponentInChildren<PortManager>();
                _portManager.RegisterConstructor(this);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (_portManager != null)
            {
                _portManager.UnRegisterConstructor(this);
            }
        }

        public override void Initialize()
        {
            IsInitialized = true;
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug($"In OnProtoSerialize -  Ore Consumer");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {GetPrefabID()}");
                Mod.Save();
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
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new AlterraHubConstructorEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
            _savedData.BaseId = BaseId;
            QuickLogger.Debug($"Saving ID {_savedData.Id}", true);
            newSaveData.AlterraHubConstructorEntries.Add(_savedData);
        }

        public bool ShipItems(List<CartItem> pendingItem)
        {
            try
            {
                foreach (CartItem item in pendingItem)
                {
                    for (int i = 0; i < item.ReturnAmount; i++)
                    {

#if SUBNAUTICA_STABLE
                        Storage.container.UnsafeAdd(item.ReceiveTechType.ToInventoryItemLegacy());
#else
                        StartCoroutine(item.ReceiveTechType.AddTechTypeToContainerUnSafe(Storage.container));
#endif
                    }
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                return false;
            }

            return true;
        }
    }
}
