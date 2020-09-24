using System;
using System.Collections.Generic;
using DataStorageSolutions.Abstract;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Model;
using FCSCommon.Components;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Abstract;
using FCSTechFabricator.Components;
using FCSTechFabricator.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace DataStorageSolutions.Mono
{
    internal class DSSItemDisplayController : DataStorageSolutionsController, IFCSStorage
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private SaveDataEntry _savedData;
        private uGUI_Icon _icon;
        private Text _amount;
        private TechType _currentTechType;
        private InterfaceButton _button;
        public Action<int, int> OnContainerUpdate { get; set; }
        public int GetContainerFreeSpace => 1;
        public bool IsFull => false;

        public override void Save(SaveData save)
        {
            if (!IsInitialized || !IsConstructed) return;

            var id = GetPrefabID();

            if (_savedData == null)
            {
                _savedData = new SaveDataEntry();
            }

            _savedData.ID = id;
            _savedData.ItemDisplayItem = _currentTechType;
            save.Entries.Add(_savedData);
        }
        
        public override BaseManager Manager { get; set; }
        
        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (!IsInitialized)
            {
                Initialize();
            }

            if (_fromSave)
            {
                if (_savedData == null)
                {
                    ReadySaveData();
                }

                _currentTechType = _savedData.ItemDisplayItem;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetSaveData(GetPrefabID());
        }

        public override void Initialize()
        {
            if (DumpContainer == null)
            {
                DumpContainer = new DumpContainer();
                DumpContainer.Initialize(transform, "Item Display Receptical", AuxPatchers.NotAllowed(), AuxPatchers.CannotBeStored(), this, 1, 1);
            }

            var icon = GameObjectHelpers.FindGameObject(gameObject, "Icon");
            _icon = icon?.AddComponent<uGUI_Icon>();
            _button = InterfaceHelpers.CreateButton(icon, "IconClick", InterfaceButtonMode.Background,
                OnButtonClick, Color.white, Color.gray, 5.0f);


            _amount = GameObjectHelpers.FindGameObject(gameObject, "Text")?.GetComponent<Text>();

            var addBTN = GameObjectHelpers.FindGameObject(gameObject, "AddBTN");
            InterfaceHelpers.CreateButton(addBTN, "AddBTN", InterfaceButtonMode.Background,
                OnButtonClick, Color.gray, Color.white, 5.0f, AuxPatchers.ColorPage());

            var deleteBTN = GameObjectHelpers.FindGameObject(gameObject, "DeleteBTN");
            InterfaceHelpers.CreateButton(deleteBTN, "DeleteBTN", InterfaceButtonMode.Background,
                OnButtonClick, Color.gray, Color.white, 5.0f, AuxPatchers.ColorPage());

            FindManager();

            InvokeRepeating(nameof(UpdateScreen),1f,1f);

            IsInitialized = true;
        }

        private void OnButtonClick(string arg1, object arg2)
        {
            switch (arg1)
            {
                case "AddBTN":
                    DumpContainer?.OpenStorage();
                    break;
                case "IconClick":
                    Manager.RemoveItemFromBaseAndGivePlayer(_currentTechType);
                    break;
                case "DeleteBTN":
                    _icon.gameObject.SetActive(false);
                    _currentTechType = TechType.None;
                    _amount.text = "";
                    break;
            }
        }

        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;

            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    Initialize();
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");

            if (_savedData == null)
            {
                ReadySaveData();
            }
            _fromSave = true;
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                var id = GetPrefabID();
                QuickLogger.Info($"Saving {id}");
                Mod.Save();
                QuickLogger.Info($"Saved {id}");
            }
        }
        
        public override void UpdateScreen()
        {
            if(_icon == null || _currentTechType == TechType.None)return;
            _icon.sprite = SpriteManager.Get(_currentTechType);
            _amount.text = $"x{Manager.GetItemCount(_currentTechType)}";
            _button.TextLineOne = string.Format(AuxPatchers.TakeFormatted(), Language.main.Get(_currentTechType));
            _icon.gameObject.SetActive(true);
        }

        internal void FindManager(BaseManager managers = null)
        {
            Manager = managers ?? BaseManager.FindManager(gameObject);
        }

        public bool CanBeStored(int amount, TechType techType)
        {
            return true;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            return true;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            _currentTechType = pickupable.GetTechType();
            Player.main.GetPDA().Close();
            return false;
        }

        public bool IsAllowedToRemoveItems()
        {
            return false;
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            return null;
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            return null;
        }

        public bool ContainsItem(TechType techType)
        {
            return false;
        }
    }
}
