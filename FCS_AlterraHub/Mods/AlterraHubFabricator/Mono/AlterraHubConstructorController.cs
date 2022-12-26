using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Model.Converters;
using FCS_AlterraHub.Mods.AlterraHubConstructor.Buildable;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mods.AlterraHubFabricator.Mono
{
    internal class AlterraHubConstructorController : FcsDevice, IFCSSave<SaveData>
    {
        private PortManager _portManager;
        private bool _runStartUpOnEnable;
        private AlterraHubConstructorEntry _savedData;
        
        private bool _isFromSave;
        public StorageContainer Storage;
        private Text _amount;
        private Text _shippingInfo;
        private Image _icon;
        private FMOD_CustomLoopingEmitter _machineSound;
        private IEnumerable<CartItemSaveData> _pendingItems;
        private float _requestedTime;
        private float _totalTime;
        private float _countDown;

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

            Storage.container.onAddItem += item =>
            {
                UpdateTotals();
            };

            Storage.container.onRemoveItem += item =>
            {
                UpdateTotals();

                if (!Storage.container.Any())
                {
                    _icon.fillAmount = 0;
                }
            };

            Storage.container.isAllowedToAdd += (pickupable, verbose) => false;
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
                    _requestedTime = _savedData.RequestedTime;
                    _totalTime = _savedData.TotalTime;
                    _countDown = _savedData.CountDown;

                    if (_savedData.PendingItems?.Any() ?? false)
                    {
                        StartCoroutine(PerformShipping(_savedData.PendingItems, _savedData.PendingItems.Count() * 3.0f));
                    }

                }

                _runStartUpOnEnable = false;
            }
        }

        private void UpdateTotals()
        {
            if(_amount == null) return;
            _amount.text = $"Total: {Storage?.container?.count ?? 0}";
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
            if (IsInitialized) return;

            if (_machineSound == null)
            {
                _machineSound = FModHelpers.CreateCustomLoopingEmitter(gameObject, "water_filter_loop", "event:/sub/base/water_filter_loop");
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol);
            }

            _amount = GameObjectHelpers.FindGameObject(gameObject, "Amount")?.GetComponent<Text>();
            _shippingInfo = GameObjectHelpers.FindGameObject(gameObject, "ShippingInfo")?.GetComponent<Text>();
            _icon = GameObjectHelpers.FindGameObject(gameObject, "iconFill")?.GetComponent<Image>();

            UpdateTotals();

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
            _savedData.PendingItems = _pendingItems;
            _savedData.RequestedTime =  _requestedTime;
            _savedData.TotalTime =  _totalTime;
            _savedData.CountDown =  _countDown;
        QuickLogger.Debug($"Saving ID {_savedData.Id}", true);
            newSaveData.AlterraHubConstructorEntries.Add(_savedData);
        }

        public bool ShipItems(IEnumerable<CartItemSaveData> pendingItem)
        {

            try
            {
                StartCoroutine(PerformShipping(pendingItem,pendingItem.Count() * 3.0f));
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                return false;
            }

            return true;
        }

        private void ToggleEffectsAndSound(bool isRunning)
        {
            if (_machineSound == null) return;

            if (isRunning)
            {
                if (!_machineSound._playing) _machineSound.Play();
            }
            else
            {
                if (_machineSound._playing) _machineSound.Stop();
            }
        }

        private IEnumerator PerformShipping(IEnumerable<CartItemSaveData> pendingItem, float time = 3)
        {
            _pendingItems = pendingItem;
            //to whatever you want
            _requestedTime = time;
            _totalTime = 0;
            _countDown = time;
            Storage.enabled = false;
            ToggleEffectsAndSound(true);
            while (_totalTime <= time)
            {
                _icon.fillAmount = _totalTime / time;
                _totalTime += Time.deltaTime;
                _countDown -= Time.deltaTime;
                _shippingInfo.text = TimeConverters.SecondsToHMS(_countDown);
                yield return null;
            }

            foreach (var item in pendingItem)
            {
                for (int i = 0; i < item.ReturnAmount; i++)
                {

                    StartCoroutine(item.ReceiveTechType.AddTechTypeToContainerUnSafe(Storage.container));
                }
            }

            _pendingItems = null;
            Storage.enabled = true;
            ToggleEffectsAndSound(false);
        }
    }
}
